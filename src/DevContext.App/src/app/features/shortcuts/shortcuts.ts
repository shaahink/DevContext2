import { Component, HostListener, signal } from '@angular/core';

import { Button } from '../../ui/button/button';
import { KbdHint } from '../../ui/kbd-hint/kbd-hint';

interface ShortcutEntry {
  readonly keys: string;
  readonly description: string;
}

const SHORTCUTS: readonly ShortcutEntry[] = [
  { keys: 'Ctrl+K', description: 'Open command palette' },
  { keys: 'Ctrl+?', description: 'Show keyboard shortcuts' },
  { keys: 'Enter', description: 'Analyze repo (in source input)' },
  { keys: '\u2191\u2193', description: 'Navigate entries / palette items' },
  { keys: 'Esc', description: 'Close palette / sheet / drawer' },
  { keys: 'Depth slider', description: 'Adjust trace depth (1-10)' },
  { keys: 'Click node', description: 'Inspect node detail' },
  { keys: 'Click edge', description: 'Navigate to target node' },
];

@Component({
  selector: 'app-shortcuts',
  imports: [KbdHint, Button],
  template: `
    <app-button
      variant="ghost"
      size="sm"
      (click)="open.set(!open())"
      title="Keyboard shortcuts"
    >
      <kbd class="rounded border border-line bg-surface-2 px-1 py-0.5 font-mono text-[10px] text-ink-muted">?</kbd>
    </app-button>

    @if (open()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center" (click)="open.set(false)" (keydown.escape)="open.set(false)" tabindex="-1">
        <div class="absolute inset-0 bg-base/60"></div>
        <div class="relative w-full max-w-md rounded-lg border border-line bg-elevated shadow-2xl" (click)="$event.stopPropagation()" (keydown)="$event.stopPropagation()" tabindex="-1">
          <div class="flex items-center justify-between border-b border-line px-4 py-3">
            <span class="font-medium text-ink">Keyboard shortcuts</span>
            <app-button variant="ghost" size="sm" (click)="open.set(false)">
              <span class="text-ink-subtle">&times;</span>
            </app-button>
          </div>
          <div class="p-4 space-y-2">
            @for (s of shortcuts; track s.keys) {
              <div class="flex items-center justify-between text-xs">
                <span class="text-ink-muted">{{ s.description }}</span>
                <app-kbd-hint>{{ s.keys }}</app-kbd-hint>
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class Shortcuts {
  readonly open = signal(false);
  readonly shortcuts = SHORTCUTS;

  @HostListener('document:keydown', ['$event'])
  onKeydown(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key === '/') {
      e.preventDefault();
      this.open.set(!this.open());
    }
    if (e.key === 'Escape' && this.open()) {
      this.open.set(false);
    }
  }
}
