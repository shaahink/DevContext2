import { isTauri } from './tauri-env';

/**
 * Copies text to the clipboard via `tauri-plugin-clipboard-manager` when running in the
 * desktop shell — `navigator.clipboard` is flaky in WebView2 without OS focus (the exact
 * case that matters here: a keyboard shortcut fired while the window is merely app-focused
 * but the webview itself hasn't received focus). Falls back to `navigator.clipboard` in
 * `ng serve`/`dev:web`, where the plugin isn't available.
 */
export async function copyToClipboard(text: string): Promise<void> {
  if (isTauri()) {
    const { writeText } = await import('@tauri-apps/plugin-clipboard-manager');
    await writeText(text);
    return;
  }
  await navigator.clipboard?.writeText(text);
}
