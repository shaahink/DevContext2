import { isStale, LatestGate, STALE } from './rpc-call';

interface Deferred<T> {
  readonly promise: Promise<T>;
  resolve(value: T): void;
  reject(err: unknown): void;
}

function deferred<T>(): Deferred<T> {
  let resolve!: (value: T) => void;
  let reject!: (err: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return { promise, resolve, reject };
}

describe('LatestGate', () => {
  it('returns the result when the call is still the latest', async () => {
    const gate = new LatestGate();
    const result = await gate.run('k', () => Promise.resolve('a'));
    expect(result).toBe('a');
  });

  it('drops a superseded result as STALE even when it resolves last', async () => {
    const gate = new LatestGate();
    const first = deferred<string>();
    const second = deferred<string>();

    const p1 = gate.run('k', () => first.promise);
    const p2 = gate.run('k', () => second.promise);

    second.resolve('second');
    first.resolve('first'); // arrives after being superseded — must not win

    expect(await p1).toBe(STALE);
    expect(await p2).toBe('second');
  });

  it('aborts the previous call signal when a new call starts', async () => {
    const gate = new LatestGate();
    const signals: AbortSignal[] = [];
    const hang = deferred<string>();

    void gate.run('k', (signal) => {
      signals.push(signal);
      return hang.promise;
    });
    void gate.run('k', (signal) => {
      signals.push(signal);
      return hang.promise;
    });

    expect(signals[0].aborted).toBe(true);
    expect(signals[1].aborted).toBe(false);
  });

  it('swallows errors from superseded calls', async () => {
    const gate = new LatestGate();
    const first = deferred<string>();

    const p1 = gate.run('k', () => first.promise);
    const p2 = gate.run('k', () => Promise.resolve('second'));

    first.reject(new Error('aborted transport'));

    expect(await p1).toBe(STALE); // resolves, never rejects
    expect(await p2).toBe('second');
  });

  it('propagates errors from the current call', async () => {
    const gate = new LatestGate();
    let caught: unknown = null;
    try {
      await gate.run('k', () => Promise.reject(new Error('boom')));
    } catch (err) {
      caught = err;
    }
    expect(caught instanceof Error && caught.message).toBe('boom');
  });

  it('cancel() aborts and invalidates the in-flight call', async () => {
    const gate = new LatestGate();
    const hang = deferred<string>();
    let signal!: AbortSignal;

    const p = gate.run('k', (s) => {
      signal = s;
      return hang.promise;
    });

    expect(gate.inFlight('k')).toBe(true);
    gate.cancel('k');
    expect(signal.aborted).toBe(true);
    expect(gate.inFlight('k')).toBe(false);

    hang.resolve('late');
    expect(await p).toBe(STALE);
  });

  it('cancelAll(prefix) only touches matching keys', async () => {
    const gate = new LatestGate();
    const hangA = deferred<string>();
    const hangB = deferred<string>();
    let signalA!: AbortSignal;
    let signalB!: AbortSignal;

    void gate.run('tab1:trace', (s) => ((signalA = s), hangA.promise));
    void gate.run('tab2:trace', (s) => ((signalB = s), hangB.promise));

    gate.cancelAll('tab1:');

    expect(signalA.aborted).toBe(true);
    expect(signalB.aborted).toBe(false);
  });

  it('keys are independent', async () => {
    const gate = new LatestGate();
    const slow = deferred<string>();

    const pa = gate.run('a', () => slow.promise);
    const pb = gate.run('b', () => Promise.resolve('b'));

    expect(await pb).toBe('b');
    slow.resolve('a');
    expect(await pa).toBe('a'); // 'b' never superseded 'a'
  });

  it('isStale narrows the union', async () => {
    const gate = new LatestGate();
    const result = await gate.run('k', () => Promise.resolve(42));
    if (!isStale(result)) {
      const n: number = result;
      expect(n).toBe(42);
    }
  });
});
