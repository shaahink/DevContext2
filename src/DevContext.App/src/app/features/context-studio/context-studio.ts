import { Component, computed, inject, signal } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

import type { ContextPackResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { type EntryVm } from '../../models/view-models';
import { SessionStore } from '../../state/session.store';
import { TrailStore } from '../../state/trail.store';
import { Icon } from '../../ui/icon/icon';
import { BudgetPanel } from './budget-panel';
import { totalCardTokens } from './card-tokens';
import { type ContextCard, CompositionView } from './composition-view';
import { packPreviewHtml } from './pack-preview';
import { type ContextCardSeed, ScopePicker, type ContextIntent, type OutputFormat } from './scope-picker';
import { type PackVerification, type SectionVerificationVm, VerificationPanel } from './verification-panel';

const INTENT_CARD_ORDER: Record<ContextIntent, readonly string[]> = {
  trace: ['flow', 'signatures', 'bodies', 'di_wiring', 'config', 'entities', 'contracts', 'tests', 'identity'],
  explain: ['identity', 'di_wiring', 'entities', 'contracts', 'signatures', 'bodies', 'flow', 'tests', 'config'],
  review: ['flow', 'bodies', 'signatures', 'di_wiring', 'entities', 'contracts', 'tests', 'config', 'identity'],
};

/** T5.6 — debounce between a pack-relevant change and the re-pack RPC. */
export const REPACK_DEBOUNCE_MS = 350;

@Component({
  selector: 'app-context-studio',
  imports: [ScopePicker, CompositionView, BudgetPanel, VerificationPanel, Icon],
  template: `
    <div class="flex h-full min-h-0">
      <app-scope-picker
        class="w-56 shrink-0 border-r border-line bg-surface"
        [entryGroups]="session.entryGroups()"
        [analyzed]="session.ready()"
        [isLibrary]="session.mapResponse()?.isLibrary ?? false"
        (cardsChange)="onCardsChange($event)"
        (trailSeedRequest)="onTrailSeed()"
        (omniboxCard)="onCardsChange([$event])"
      />

      <div class="flex min-w-0 flex-1 flex-col bg-base">
        <app-composition-view
          class="min-h-0 flex-1"
          [cards]="cards()"
          (cardToggleBody)="onToggleBody($event)"
          (cardRemove)="onRemove($event)"
          (cardReorder)="onReorder($event)"
          (cardRetry)="onRetry()"
        />

        <!-- D4.5 (L4): the LIVE pack preview — renders exactly what Copy/Save serve,
             recomputed by the same debounced repack every scope/budget/intent change
             already triggers. The core loop: see the context an agent would get, live. -->
        <section class="flex min-h-0 flex-col border-t border-line" [class.flex-1]="previewOpen()">
          <button
            type="button"
            class="flex shrink-0 items-center gap-2 px-3 py-1.5 text-2xs font-semibold uppercase tracking-wider text-ink-subtle hover:text-ink transition-colors"
            (click)="previewOpen.set(!previewOpen())"
          >
            <app-icon name="chevron-right" [size]="10" class="transition-transform" [class.rotate-90]="previewOpen()" />
            Live preview
            <span class="normal-case font-normal tracking-normal">— exactly what Copy copies</span>
            @if (packTotals(); as t) {
              <span class="ml-auto font-normal normal-case tracking-normal tabular-nums" [class.text-warn]="t.total > budgetTokens()">
                {{ t.total }} tok · allocated {{ t.allocated }} · budget {{ budgetTokens() }}
              </span>
            }
            @if (packPending()) {
              <span class="font-normal normal-case tracking-normal text-accent" [class.ml-auto]="!packTotals()">packing…</span>
            }
          </button>
          @if (previewOpen()) {
            <div class="code-block pack-preview min-h-0 flex-1 overflow-auto border-t border-line bg-surface px-3 py-2 transition-opacity" [class.opacity-50]="packPending()">
              @if (previewHtml(); as html) {
                <pre class="whitespace-pre-wrap font-mono text-2xs leading-relaxed text-ink-muted"><code [innerHTML]="html"></code></pre>
              } @else {
                <p class="py-4 text-center text-xs text-ink-subtle">Add cards from the scope picker — the assembled pack renders here as you shape it.</p>
              }
            </div>
          }
        </section>
      </div>

      <app-budget-panel
        class="w-48 shrink-0 border-l border-line bg-surface"
        [cards]="cards()"
        [omitted]="packOmitted()"
        [packPending]="packPending()"
        [exportReady]="exportReady()"
        [budget]="budgetTokens()"
        (budgetChange)="onBudgetChange($event)"
        [selectedIntent]="selectedIntent()"
        (selectedIntentChange)="onIntentChange($event)"
        [(selectedFormat)]="selectedFormat"
        [(showAllBodies)]="showAllBodies"
        (copyRequest)="onCopy()"
        (saveRequest)="onSave()"
        (globalBodiesChange)="onGlobalBodiesChange()"
      >
        <app-verification-panel
          [verification]="packVerification()"
          [verifying]="verifying()"
          (refreshRequest)="onVerifyRefresh()"
          (reanalyzeRequest)="onReanalyze()"
        />
      </app-budget-panel>
    </div>
  `,
  host: { class: 'h-full min-h-0' },
})
export class ContextStudio {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly trailStore = inject(TrailStore);

  protected readonly cards = signal<readonly ContextCard[]>([]);
  protected readonly selectedIntent = signal<ContextIntent>('trace');
  protected readonly selectedFormat = signal<OutputFormat>('markdown');
  protected readonly showAllBodies = signal(true);
  protected readonly budgetTokens = signal(4000);

  /** T5.6 (audit C1) — a budget change must re-pack, not silently serve the old bytes. */
  protected onBudgetChange(value: number): void {
    this.budgetTokens.set(value);
    this.schedulePack();
  }

  /** T5.6 — intent reorders the cards AND re-packs (the server honors card order + intent). */
  protected onIntentChange(intent: ContextIntent): void {
    this.selectedIntent.set(intent);
    this.sortByIntent(intent);
    this.schedulePack();
  }

  private sortByIntent(intent: ContextIntent): void {
    const order = INTENT_CARD_ORDER[intent];
    const idx = new Map(order.map((k, i) => [k, i]));
    this.cards.update((prev) =>
      [...prev].sort((a, b) => (idx.get(a.type) ?? 99) - (idx.get(b.type) ?? 99)),
    );
  }

  // Batch E (R2 §2.E item 2): the same function the budget panel uses. This was a second copy of the
  // reduce — identical today, which is exactly how two numbers for one stat start.
  protected readonly totalTokens = computed(() => totalCardTokens(this.cards()));

  /** T5.6 — THE pack. Exports serve exactly this or nothing; there is no client-side rebuild. */
  protected readonly serverPack = signal<string | null>(null);

  /** D4.5 (L4) — the server's own token accounting (was returned and dropped pre-D4.5). */
  protected readonly packTotals = signal<{ total: number; allocated: number } | null>(null);

  /** D4.5 (L4) — the live preview is open by default: the Studio's promised core loop. */
  protected readonly previewOpen = signal(true);

  /** D4.5 (L4) — the EXACT export string for the selected format. Copy/Save read THIS,
   * so "Copy copies what's shown" holds byte-for-byte (json's generatedAt included). */
  protected readonly previewText = computed(() => this.buildContext(this.selectedFormat()));

  private readonly sanitizer = inject(DomSanitizer);
  protected readonly previewHtml = computed(() => {
    const text = this.previewText();
    if (text === null) return null;
    return this.sanitizer.bypassSecurityTrustHtml(packPreviewHtml(text, this.selectedFormat()));
  });

  /** T5.1 (audit R1) — what the server cut, rendered in the budget panel. */
  protected readonly packOmitted = signal<readonly string[]>([]);

  /** T5.6 — true from a pack-relevant change until the re-pack lands; gates Copy/Save. */
  protected readonly packPending = signal(false);

  protected readonly exportReady = computed(() =>
    !this.packPending() && this.serverPack() !== null && this.cards().length > 0);

  /** Overridable in specs (0 = flush on the next macrotask). */
  protected packDebounceMs = REPACK_DEBOUNCE_MS;
  private packTimer: ReturnType<typeof setTimeout> | null = null;
  private packSeq = 0;

  /** T5.2 (audit R6) — the staleness ledger for the current pack; null until verified. */
  protected readonly packVerification = signal<PackVerification | null>(null);
  protected readonly verifying = signal(false);
  private verifySeq = 0;

  /** T5.2 — verify every unique focus the cards reference and merge per section. */
  private async verifyPack(): Promise<void> {
    const handle = this.session.handle();
    const focuses = [...new Set(this.cards().flatMap((c) => c.entryIds))];
    if (!handle || focuses.length === 0) {
      this.packVerification.set(null);
      return;
    }
    const seq = ++this.verifySeq;
    this.verifying.set(true);
    try {
      const results = await Promise.all(
        focuses.map((f) => this.api.verifyContext(handle, f, this.budgetTokens())),
      );
      if (seq !== this.verifySeq) return;
      const found = results.filter((r) => r.found);
      if (found.length === 0) {
        this.packVerification.set(null);
        return;
      }
      const byKey = new Map<string, { stale: boolean; filesChecked: number; changed: Map<string, SectionVerificationVm['changed'][number]> }>();
      for (const r of found) {
        for (const s of r.sections) {
          let agg = byKey.get(s.key);
          if (!agg) {
            agg = { stale: false, filesChecked: 0, changed: new Map() };
            byKey.set(s.key, agg);
          }
          agg.stale ||= s.stale;
          agg.filesChecked += s.filesChecked;
          for (const d of s.changed) {
            agg.changed.set(d.file, { file: d.file, status: d.status, lineDelta: d.lineDelta });
          }
        }
      }
      this.packVerification.set({
        anyStale: found.some((r) => r.anyStale),
        analyzedGitHead: found[0].analyzedGitHead,
        currentGitHead: found[0].currentGitHead,
        checkedAt: Date.now(),
        sections: [...byKey.entries()].map(([key, a]) => ({
          key,
          stale: a.stale,
          filesChecked: a.filesChecked,
          changed: [...a.changed.values()],
        })),
      });
    } catch {
      // Verification is advisory — a failed check must never block the Studio; the panel
      // simply disappears rather than claiming fresh OR stale without evidence.
      if (seq === this.verifySeq) this.packVerification.set(null);
    } finally {
      if (seq === this.verifySeq) this.verifying.set(false);
    }
  }

  protected onVerifyRefresh(): void {
    void this.verifyPack();
  }

  protected onReanalyze(): void {
    this.session.reAnalyze();
  }

  protected onCardsChange(seeds: readonly ContextCardSeed[]): void {
    const handle = this.session.handle();
    if (!handle) return;

    const entryMap = new Map<string, EntryVm>();
    for (const group of this.session.entryGroups()) {
      for (const e of group.entries) {
        entryMap.set(e.nodeId, e);
      }
    }

    const newCards: ContextCard[] = seeds.map((s) => ({
      id: crypto.randomUUID(),
      type: s.type,
      title: s.title,
      entryIds: s.entryIds,
      estimatedLines: s.estimatedLines,
      content: null,
      loading: true,
      bodyEnabled: this.showAllBodies(),
      serverTokens: null,
      sectionTokens: [],
      provenance: s.entryIds
        .map((id) => entryMap.get(id)?.provenance)
        .filter((p): p is string => !!p),
      sections: [],
      error: null,
    }));

    this.cards.update((prev) => [...prev, ...newCards]);

    this.schedulePack();
  }

  /** T5.6 (audit C1) — ONE re-pack path for every pack-relevant change (add/remove/reorder/
   * retry/budget/intent). Debounced; always sends the WHOLE card set, so the export can never
   * be a stale earlier batch (the pre-T5.6 pack held only the most recent add). config/tests
   * go to the server too — real sections since T4.3, the client stub filter is gone. */
  private schedulePack(immediate = false): void {
    if (this.packTimer !== null) clearTimeout(this.packTimer);
    this.packTimer = null;
    const handle = this.session.handle();
    if (!handle) return;
    if (this.cards().length === 0) {
      this.serverPack.set(null);
      this.packOmitted.set([]);
      this.packTotals.set(null);
      this.packVerification.set(null);
      this.packPending.set(false);
      return;
    }
    this.packPending.set(true);
    this.packTimer = setTimeout(() => {
      this.packTimer = null;
      void this.repack(handle);
    }, immediate ? 0 : this.packDebounceMs);
  }

  private async repack(handle: string): Promise<void> {
    const seq = ++this.packSeq;
    const specs = this.cards().map((c) => ({ type: c.type, title: c.title, entryIds: [...c.entryIds] }));
    try {
      const pack = await this.api.getContextPack(handle, specs, {
        budgetTokens: this.budgetTokens(),
        intent: this.selectedIntent(),
      });
      if (seq !== this.packSeq) return; // superseded by a newer re-pack

      // D4.5 (L4) — normalize the server's CRLF to LF at ingestion: ONE canonical byte
      // form for preview/Copy/Save. (A stray \r inside a preview span parses into an
      // extra newline — the HTML parser normalizes \r to \n but can't merge a CRLF pair
      // split across a tag boundary; the probe caught headings double-spacing.)
      this.serverPack.set(pack.assembledMarkdown ? pack.assembledMarkdown.replace(/\r\n/g, '\n') : null);
      this.packOmitted.set(pack.omitted);
      // D4.5 (L4) — surface the server's token truth in the preview header.
      this.packTotals.set(pack.assembledMarkdown
        ? { total: pack.totalTokens, allocated: pack.allocatedTokens }
        : null);

      // Correlate by (type, title) in order — duplicate specs consume response items in
      // sequence, so two cards sharing a type no longer clobber each other.
      const queues = new Map<string, ContextPackResponse['cards']>();
      for (const ci of pack.cards) {
        const key = `${ci.type} ${ci.title}`;
        const q = queues.get(key);
        if (q) q.push(ci);
        else queues.set(key, [ci]);
      }
      this.cards.update((prev) =>
        prev.map((c) => {
          const ci = queues.get(`${c.type} ${c.title}`)?.shift();
          // Not in the pack = dropped server-side (named in omitted[]); show it empty, not stale.
          if (!ci) return { ...c, loading: false, error: null, content: null, serverTokens: null, sections: [] };
          // T5.3/T5.5 — cards carry their sections' REAL content + provenance (T4.4 fields):
          // per-card copy, JSON export, file:line chips, and honest previews all read these.
          const sections = ci.sections.map((s) => ({
            key: s.key,
            tokens: s.tokens,
            content: s.content,
            sourceLocations: s.sourceLocations,
            verified: s.verified,
            approx: s.approx,
          }));
          // T5.5 (audit finding 40) — the preview is the sections' REAL text, never a
          // title echo ("Flow: /ProductList" told the reader nothing about the content).
          const preview = sections.map((s) => s.content).join('\n').trim();
          return {
            ...c,
            content: preview.length > 0 ? preview : null,
            loading: false,
            error: null,
            serverTokens: ci.tokens > 0 ? ci.tokens : null,
            estimatedLines: ci.tokens > 0 ? Math.max(1, Math.round(ci.tokens / 2.5)) : c.estimatedLines,
            sections,
          };
        }),
      );

      // T5.2 (audit R6) — every fresh pack gets a fresh staleness ledger, unprompted.
      void this.verifyPack();
    } catch (e) {
      if (seq !== this.packSeq) return;
      // T5.1 (audit R4) — a failed RPC must SAY so on the cards, not just stop the spinners.
      // T5.6 — and the stale pack must not survive as an exportable lie.
      const message = e instanceof Error ? e.message : 'Context pack request failed';
      this.serverPack.set(null);
      this.packOmitted.set([]);
      this.packTotals.set(null);
      this.packVerification.set(null);
      this.cards.update((prev) => prev.map((c) => ({ ...c, loading: false, error: message })));
    } finally {
      if (seq === this.packSeq) this.packPending.set(false);
    }
  }

  /** T5.1 (audit R4) — retry re-packs the whole set immediately. */
  protected onRetry(): void {
    if (!this.cards().some((c) => c.error !== null)) return;
    this.cards.update((prev) =>
      prev.map((c) => (c.error !== null ? { ...c, loading: true, error: null } : c)),
    );
    this.schedulePack(true);
  }

  protected onToggleBody(id: string): void {
    this.cards.update((prev) =>
      prev.map((c) => (c.id === id ? { ...c, bodyEnabled: !c.bodyEnabled } : c)),
    );
  }

  protected onGlobalBodiesChange(): void {
    const showAll = this.showAllBodies();
    this.cards.update((prev) =>
      prev.map((c) => ({ ...c, bodyEnabled: showAll })),
    );
  }

  protected onRemove(id: string): void {
    this.cards.update((prev) => prev.filter((c) => c.id !== id));
    this.schedulePack();
  }

  protected onTrailSeed(): void {
    const steps = this.trailStore.steps();
    if (steps.length === 0) return;
    const seeds: ContextCardSeed[] = [];
    const seen = new Set<string>();
    for (const step of steps) {
      if (step.kind === 'entry') {
        const found = this.findEntryByFocus(step.focus);
        if (found && !seen.has(found.nodeId)) {
          seen.add(found.nodeId);
          seeds.push({
            type: 'flow',
            title: `Flow: ${step.title}`,
            entryIds: [found.nodeId],
            estimatedLines: 15,
          });
        }
      }
    }
    if (seeds.length > 0) {
      this.onCardsChange(seeds);
    }
  }

  private findEntryByFocus(focus: string) {
    for (const group of this.session.entryGroups()) {
      for (const e of group.entries) {
        if (e.focus === focus) return e;
      }
    }
    return null;
  }

  protected onReorder(event: { fromIndex: number; toIndex: number }): void {
    this.cards.update((prev) => {
      const arr = [...prev];
      const [item] = arr.splice(event.fromIndex, 1);
      arr.splice(event.toIndex, 0, item);
      return arr;
    });
    // T5.6 — the server assembles in card order, so a reorder re-packs too.
    this.schedulePack();
  }

  /** D4.5 (L4) — Copy serves the preview's exact string (one computed, one truth). */
  protected onCopy(): void {
    const text = this.previewText();
    if (text === null) return;
    void navigator.clipboard.writeText(text);
  }

  protected onSave(): void {
    const format = this.selectedFormat();
    const text = this.previewText();
    if (text === null) return;
    const mime = format === 'plain' ? 'text/plain'
      : format === 'json' ? 'application/json'
      : 'text/markdown;charset=utf-8';
    const blob = new Blob([text], { type: mime });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = this.saveFileName(format);
    a.click();
    URL.revokeObjectURL(url);
  }

  /** T5.1 (audit R5) + T5.6 — `${repo}-context-${date}.{md|txt|json}`, never a hardcoded name. */
  protected saveFileName(format: OutputFormat): string {
    const label = this.session.summary()?.label ?? '';
    const repo = label.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '') || 'devcontext';
    const date = new Date().toISOString().slice(0, 10);
    const ext = format === 'plain' ? 'txt' : format === 'json' ? 'json' : 'md';
    return `${repo}-context-${date}.${ext}`;
  }

  /** T5.6 (audit C1) — ONE build path: exports are exactly the current server pack, or nothing
   * (Copy/Save are disabled via exportReady). The legacy client-side assembly — old header,
   * `<!-- context card -->` markers, estimate footers — is gone. Plain strips markdown SYNTAX
   * but keeps every line of content, so plain ≠ markdown while losing nothing. T5.3 (R8):
   * json is the STRUCTURED pack — cards with real section content, per-section provenance,
   * omissions, the staleness ledger, and the assembled markdown, for programmatic consumers. */
  private buildContext(format: OutputFormat): string | null {
    const pack = this.serverPack();
    if (!pack) return null;
    if (format === 'plain') {
      return pack
        .replace(/^#{1,6} /gm, '')
        .replace(/^_([^_].*)_$/gm, '$1')
        .replace(/^```.*$/gm, '')
        .replace(/\n{3,}/g, '\n\n')
        .trim();
    }
    if (format === 'json') {
      return JSON.stringify({
        repo: this.session.summary()?.label ?? null,
        generatedAt: new Date().toISOString(),
        budgetTokens: this.budgetTokens(),
        intent: this.selectedIntent(),
        omitted: this.packOmitted(),
        verification: this.packVerification(),
        cards: this.cards().map((c) => ({
          type: c.type,
          title: c.title,
          entryIds: c.entryIds,
          tokens: c.serverTokens,
          sections: c.sections,
        })),
        markdown: pack,
      }, null, 2);
    }
    return pack;
  }
}
