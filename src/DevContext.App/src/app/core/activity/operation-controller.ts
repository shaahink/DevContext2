export type OperationState = 'idle' | 'running' | 'cancelling' | 'done' | 'error';

export class OperationController {
  private _state: OperationState = 'idle';
  private _cts: AbortController | null = null;
  private _cancelled = false;
  private _onStateChange?: (state: OperationState) => void;

  get state(): OperationState {
    return this._state;
  }

  set onStateChange(cb: (state: OperationState) => void) {
    this._onStateChange = cb;
  }

  cancel(): void {
    if (this._state === 'running') {
      this._cancelled = true;
      this._state = 'cancelling';
      this._onStateChange?.('cancelling');
      this._cts?.abort();
    }
  }

  async run<T>(fn: (ct: AbortSignal) => Promise<T>): Promise<T | undefined> {
    this._cts?.abort();
    this._cts = new AbortController();
    this._cancelled = false;
    this._state = 'running';
    this._onStateChange?.('running');

    try {
      const result = await fn(this._cts.signal);
      if (!this._cancelled) {
        this._state = 'done';
        this._onStateChange?.('done');
      }
      return result;
    } catch (err) {
      if (this._cancelled || (err instanceof DOMException && err.name === 'AbortError')) {
        this._state = 'idle';
        this._onStateChange?.('idle');
        return undefined;
      }
      this._state = 'error';
      this._onStateChange?.('error');
      throw err;
    }
  }

  reset(): void {
    this._cts?.abort();
    this._state = 'idle';
    this._onStateChange?.('idle');
  }
}
