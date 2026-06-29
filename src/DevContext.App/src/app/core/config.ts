/**
 * Resolves the DevContext server base URL. In the Tauri shell the Rust side injects the sidecar
 * address on `globalThis.__DEVCONTEXT_SERVER__`; in the browser dev-server we fall back to the
 * server's stable local default.
 */
export function serverBaseUrl(): string {
  const injected = (globalThis as { __DEVCONTEXT_SERVER__?: string }).__DEVCONTEXT_SERVER__;
  return injected ?? 'http://127.0.0.1:5179';
}
