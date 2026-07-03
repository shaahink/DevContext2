declare global {
  interface Window {
    __TAURI_INTERNALS__?: unknown;
  }
}

/**
 * True when running inside the Tauri desktop shell (vs. `ng serve`/`dev:web` in a plain
 * browser). `__TAURI_INTERNALS__` is always injected by the Tauri v2 webview, unlike the
 * legacy `window.__TAURI__` global, which only exists when `app.withGlobalTauri: true` is
 * set in `tauri.conf.json` (it isn't here — this app uses ESM plugin imports instead).
 */
export function isTauri(): boolean {
  return typeof window !== 'undefined' && window.__TAURI_INTERNALS__ !== undefined;
}
