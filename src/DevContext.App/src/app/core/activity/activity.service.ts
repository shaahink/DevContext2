import { inject, Injectable, signal } from '@angular/core';

import { ToastService } from '../../ui/toast/toast';
import { OperationController, type OperationState } from './operation-controller';

export type ActivityState = 'idle' | 'busy' | 'error';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private readonly toast = inject(ToastService);
  private readonly _label = signal('');
  private readonly _stage = signal('');
  private readonly _percent = signal(0);
  private readonly _state = signal<ActivityState>('idle');
  private _controller: OperationController | null = null;

  readonly label = this._label.asReadonly();
  readonly stage = this._stage.asReadonly();
  readonly percent = this._percent.asReadonly();
  readonly state = this._state.asReadonly();

  start(label: string): OperationController {
    this._controller?.cancel();
    const ctrl = new OperationController();
    ctrl.onStateChange = (s: OperationState) => {
      switch (s) {
        case 'running': this._state.set('busy'); break;
        case 'cancelling': this._state.set('busy'); break;
        case 'done': this._state.set('idle'); break;
        case 'error': this._state.set('error'); break;
        case 'idle': this._state.set('idle'); break;
      }
    };
    this._controller = ctrl;
    this._label.set(label);
    this._stage.set('');
    this._percent.set(0);
    this._state.set('busy');
    return ctrl;
  }

  setProgress(stage: string, percent: number, message: string): void {
    this._stage.set(stage);
    this._percent.set(percent);
    this._label.set(message);
  }

  setError(message: string): void {
    this._label.set(message);
    this._state.set('error');
    this.toast.show(message, 'error');
    setTimeout(() => { if (this._state() === 'error') this._state.set('idle'); }, 5000);
  }

  clear(): void {
    this._state.set('idle');
    this._label.set('');
    this._stage.set('');
    this._percent.set(0);
  }

  /**
   * Runs a non-cancellable secondary op. Errors are surfaced (status bar + toast)
   * and swallowed — callers get `undefined` rather than a rejection, so their own
   * loading flags always clear. No view should re-swallow with an empty catch.
   */
  async runSecondary<T>(label: string, fn: () => Promise<T>): Promise<T | undefined> {
    this._label.set(label);
    this._state.set('busy');
    try {
      const result = await fn();
      this._state.set('idle');
      this._label.set('');
      return result;
    } catch (err) {
      this.setError(describeError(err));
      return undefined;
    }
  }

  get controller(): OperationController | null {
    return this._controller;
  }
}

function describeError(err: unknown): string {
  if (err instanceof Error && err.message) return err.message;
  return 'Something went wrong. Is the DevContext server running?';
}
