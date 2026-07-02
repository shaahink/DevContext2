import { DestroyRef, inject, Injectable, signal } from '@angular/core';

import { DevContextApi } from '../data-access/devcontext-api';

@Injectable({ providedIn: 'root' })
export class ConnectionStore {
  private readonly api = inject(DevContextApi);
  private started = false;
  private intervalId: ReturnType<typeof setInterval> | undefined;

  private readonly _online = signal(false);
  private readonly _checked = signal(false);
  private readonly _version = signal('');

  readonly online = this._online.asReadonly();
  readonly checked = this._checked.asReadonly();
  readonly version = this._version.asReadonly();

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      clearInterval(this.intervalId);
    });
  }

  start(): void {
    if (this.started) return;
    this.started = true;
    void this.poll();
    this.intervalId = setInterval(() => void this.poll(), 5000);
  }

  private async poll(): Promise<void> {
    const res = await this.api.ping();
    this._online.set(res.ready);
    this._version.set(res.version);
    this._checked.set(true);
  }
}
