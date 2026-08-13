import { Component } from '@angular/core';

import type { TraceNodeVm } from '../../models/view-models';
import { SEAM_COLORS } from '../../models/seam-colors';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { EmptyState } from '../../ui/empty-state/empty-state';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { Icon } from '../../ui/icon/icon';
import { KindIcon } from '../../ui/kind-icon/kind-icon';
import { Meter } from '../../ui/meter/meter';
import { Panel } from '../../ui/panel/panel';
import { SeamChip } from '../../ui/seam-chip/seam-chip';
import { Segmented } from '../../ui/segmented/segmented';
import { Skeleton } from '../../ui/skeleton/skeleton';
import { Spinner } from '../../ui/spinner/spinner';
import { StatCell } from '../../ui/stat-cell/stat-cell';

const COLOR_TOKENS: readonly { name: string; cssVar: string }[] = [
  { name: 'base', cssVar: '--vibe-base' },
  { name: 'surface', cssVar: '--vibe-surface' },
  { name: 'surface-2', cssVar: '--vibe-surface-2' },
  { name: 'elevated', cssVar: '--vibe-elevated' },
  { name: 'line', cssVar: '--vibe-line' },
  { name: 'line-strong', cssVar: '--vibe-line-strong' },
  { name: 'ink', cssVar: '--vibe-ink' },
  { name: 'ink-muted', cssVar: '--vibe-ink-muted' },
  { name: 'ink-subtle', cssVar: '--vibe-ink-subtle' },
  { name: 'accent', cssVar: '--vibe-accent' },
  { name: 'accent-dim', cssVar: '--vibe-accent-dim' },
  { name: 'success', cssVar: '--vibe-success' },
  { name: 'warn', cssVar: '--vibe-warn' },
  { name: 'danger', cssVar: '--vibe-danger' },
  { name: 'info', cssVar: '--vibe-info' },
];

const MOCK_KINDS = ['HttpEndpoint', 'MessageConsumer', 'HostedService', 'ScheduledJob', 'DomainEventHandler', 'PublicApi'];

const MOCK_TRACE: TraceNodeVm = {
  id: 'n1',
  title: 'OrdersController.Post',
  kind: 'Method',
  seam: 'Entry',
  depth: 0,
  resolution: 'Semantic',
  truncated: false,
  omitted: 0,
  omittedNames: [],
  multiImplCount: 0,
  diHostCount: 0,
  testOnly: false,
  tags: [],
  children: [
    {
      id: 'n2',
      title: 'OrderService.Process',
      kind: 'Method',
      seam: 'Call',
      depth: 1,
      resolution: 'Semantic',
      truncated: false,
      omitted: 0,
      omittedNames: [],
      // M1.1 — a Resolves step DI can bind more than one way. The styleguide shows the
      // annotations so a reviewer can see what "honest about the wiring" looks like.
      multiImplCount: 3,
      diHostCount: 0,
      testOnly: true,
      tags: [],
      children: [
        {
          id: 'n3',
          title: '[Bus] PricingRequested',
          kind: 'Method',
          seam: 'Send',
          depth: 2,
          resolution: 'Syntactic',
          truncated: true,
          omitted: 2,
          omittedNames: ['PricingService.Handle', 'AuditService.Handle'],
          multiImplCount: 0,
          diHostCount: 0,
          testOnly: false,
          tags: [],
          children: [],
        },
      ],
    },
  ],
};

/**
 * Dev-only token sheet + component gallery (proposal W0.4). Every later stage proves
 * its visuals here before wiring into a real page. Not linked from any nav — reachable
 * only via `/styleguide` in dev mode (guarded by `isDevMode()` in app.config's route).
 */
@Component({
  selector: 'app-styleguide-page',
  imports: [Badge, Button, EmptyState, GraphCanvas, Icon, KindIcon, Meter, Panel, SeamChip, Segmented, Skeleton, Spinner, StatCell],
  template: `
    <div class="prose-zone mx-auto max-w-5xl px-6 py-8">
      <h1 class="mb-1 text-lg font-semibold text-ink">Styleguide</h1>
      <p class="mb-8 text-xs text-ink-subtle">
        Graphite design system (F proposal §4) — dev-only, not linked from any nav.
      </p>

      <h2 class="section-h -ml-2 mb-2 text-sm">Color</h2>
      <div class="mb-8 grid grid-cols-3 gap-2 sm:grid-cols-5">
        @for (t of colorTokens; track t.name) {
          <div class="hairline-parent relative overflow-hidden rounded border border-line">
            <div class="h-12" [style.background]="'var(' + t.cssVar + ')'"></div>
            <div class="px-2 py-1 text-2xs text-ink-muted">{{ t.name }}</div>
          </div>
        }
      </div>

      <h2 class="section-h -ml-2 mb-2 text-sm">Seam palette</h2>
      <div class="mb-8 flex flex-wrap gap-2">
        @for (seam of seamNames; track seam) {
          <app-seam-chip [seam]="seam" />
        }
      </div>

      <h2 class="section-h -ml-2 mb-2 text-sm">Type ramp</h2>
      <div class="mb-8 space-y-1">
        <p class="text-2xs text-ink">text-2xs — labels, meta (12px)</p>
        <p class="text-xs text-ink">text-xs — dense UI default (13px)</p>
        <p class="text-sm text-ink">text-sm — 14px UI base</p>
        <p class="text-base text-ink">text-base — section titles (15px)</p>
        <p class="text-lg font-semibold text-ink">text-lg — page titles</p>
        <p class="prose-zone">
          .prose-zone — where reading happens (LLM pane, insight detail, Home digest): 15px,
          1.6 leading, 68ch max width.
        </p>
        <p class="font-mono text-xs text-ink">font-mono — JetBrains Mono, node ids/routes/tokens</p>
      </div>

      <h2 class="section-h -ml-2 mb-2 text-sm">@layer components vocabulary</h2>
      <div class="mb-8 space-y-3">
        <div class="panel border border-line p-2">
          <p class="mb-2 text-2xs text-ink-subtle">.panel</p>
          <div class="list-row selected">selected .list-row</div>
          <div class="list-row">.list-row (hover me)</div>
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <span class="chip">.chip</span>
          <span class="chip active">.chip.active</span>
          <span class="kbd">Ctrl</span><span class="kbd">Shift</span><span class="kbd">L</span>
          <span class="text-2xs text-ink-subtle">.kbd</span>
        </div>
        <button type="button" class="section-h border border-line">▾ .section-h twisty</button>
        <div class="overlay-float w-fit p-3 text-xs">.overlay-float — the only shadowed thing</div>
        <div class="relative h-6 w-48 overflow-hidden rounded border border-line">
          <div class="hairline"></div>
          <span class="p-1 text-2xs text-ink-subtle">.hairline (animates)</span>
        </div>
        <app-skeleton width="12rem" height="0.875rem" />
      </div>

      <h2 class="section-h -ml-2 mb-2 text-sm">ui/ primitives</h2>
      <div class="mb-4 flex flex-wrap items-center gap-2">
        <app-badge>default</app-badge>
        <app-badge variant="accent">accent</app-badge>
        <app-badge variant="success">success</app-badge>
        <app-badge variant="warn">warn</app-badge>
        <app-badge variant="danger">danger</app-badge>
      </div>
      <div class="mb-4 flex flex-wrap items-center gap-2">
        <app-button variant="primary">Primary</app-button>
        <app-button variant="secondary">Secondary</app-button>
        <app-button variant="ghost">Ghost</app-button>
        <app-button variant="danger">Danger</app-button>
        <app-button [disabled]="true">Disabled</app-button>
      </div>
      <div class="mb-4 flex flex-wrap items-center gap-3">
        <app-spinner />
        <app-stat-cell [value]="94" label="entries" />
        <app-stat-cell value="89%" label="wired" />
      </div>
      <div class="mb-4 flex max-w-xs flex-col gap-2">
        <app-meter [value]="92" variant="success" />
        <app-meter [value]="42" variant="warn" />
        <app-meter [value]="12" variant="danger" />
      </div>
      <div class="mb-4 flex flex-wrap items-center gap-3">
        @for (kind of mockKinds; track kind) {
          <span class="flex items-center gap-1 text-2xs text-ink-muted">
            <app-kind-icon [kind]="kind" />
            {{ kind }}
          </span>
        }
      </div>
      <div class="mb-4 flex flex-wrap gap-2">
        @for (name of iconSample; track name) {
          <span class="flex flex-col items-center gap-1 rounded border border-line p-2 text-2xs text-ink-subtle">
            <app-icon [name]="name" [size]="16" />
            {{ name }}
          </span>
        }
      </div>
      <div class="mb-4 max-w-sm">
        <app-panel title="Panel with title">
          <p class="text-xs text-ink-muted">Panel body content.</p>
        </app-panel>
      </div>
      <div class="mb-4 h-24 max-w-sm border border-line">
        <app-empty-state title="No entries match the filter" icon="search">
          <button type="button" class="text-accent hover:underline">clear filters</button>
        </app-empty-state>
      </div>
      <div class="mb-4 max-w-xs">
        <app-segmented [options]="segmentedOptions" [selected]="'tree'" />
      </div>
      <div class="mb-4 h-64 max-w-xl border border-line">
        <app-graph-canvas [data]="{ mode: 'trace', root: mockTrace, maxDepth: 3 }" />
      </div>
    </div>
  `,
})
export class StyleguidePage {
  protected readonly colorTokens = COLOR_TOKENS;
  protected readonly seamNames = Object.keys(SEAM_COLORS);
  protected readonly mockKinds = MOCK_KINDS;
  protected readonly mockTrace = MOCK_TRACE;
  protected readonly iconSample = ['search', 'layers', 'map', 'zap', 'network', 'refresh', 'copy', 'check', 'x', 'settings'];
  protected readonly segmentedOptions = [
    { label: 'Tree', value: 'tree' },
    { label: 'Graph', value: 'graph' },
  ];
}
