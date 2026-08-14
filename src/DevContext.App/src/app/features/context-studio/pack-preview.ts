import { highlightCSharp } from '../../core/code-highlight';
import type { OutputFormat } from '../../models/context-card';

/**
 * D4.5 (L4) — the live pack preview's renderer. The preview shows EXACTLY the string
 * Copy/Save serve (the studio's buildContext output); this helper only decorates it for
 * reading: markdown gets heading emphasis + Prism-highlighted C# fences, plain/json stay
 * verbatim. Pure and DOM-free; output is class-based HTML for [innerHTML] under the
 * global `.code-block` token styles.
 */
export function packPreviewHtml(text: string, format: OutputFormat): string {
  if (format !== 'markdown') return escapeHtml(text);

  const out: string[] = [];
  const lines = text.split('\n');
  let fence: { lang: string; buf: string[] } | null = null;
  for (const line of lines) {
    const open = /^```(\w*)\s*$/.exec(line);
    if (fence) {
      if (open) {
        // closing fence — emit the collected block
        const code = fence.buf.join('\n');
        const isCSharp = fence.lang === 'csharp' || fence.lang === 'cs';
        out.push(`<span class="pv-meta">\`\`\`${escapeHtml(fence.lang)}</span>`);
        out.push(isCSharp ? highlightCSharp(code) : escapeHtml(code));
        out.push('<span class="pv-meta">```</span>');
        fence = null;
      } else {
        fence.buf.push(line);
      }
      continue;
    }
    if (open) {
      fence = { lang: open[1], buf: [] };
      continue;
    }
    if (/^#{1,6} /.test(line)) out.push(`<span class="pv-h">${escapeHtml(line)}</span>`);
    else if (/^_[^_].*_$/.test(line)) out.push(`<span class="pv-meta">${escapeHtml(line)}</span>`);
    else out.push(escapeHtml(line));
  }
  // Unclosed fence at EOF — render what we collected, verbatim (honesty over tidiness).
  if (fence) {
    out.push(`<span class="pv-meta">\`\`\`${escapeHtml(fence.lang)}</span>`);
    out.push(escapeHtml(fence.buf.join('\n')));
  }
  return out.join('\n');
}

function escapeHtml(s: string): string {
  return s.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
}
