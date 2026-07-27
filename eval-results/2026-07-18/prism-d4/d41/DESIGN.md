# D4.1 canvas system — design pass (Fable, in-session) + verification

Proposal §1 staffs D4 on Sonnet 5 with the canvas design pass on Opus-or-better; this D4 session
runs on Fable (Mythos-class, above Opus), so the pass ran in-session before implementation.

## Diagnosis (baseline evidence, gate-baseline/)
Three structural defects produced every symptom class:
1. **fcose (force-directed) for topology** — architecture has ranks, force layouts don't;
   33-project bitwarden rendered as an unlabeled hairball, positions carried no meaning.
2. **Labels floated OUTSIDE nodes** (`text-halign: right`) — the layout engine spaced
   12–30px dots while each dot dragged up to 200px of text it never knew about. Overlap
   was guaranteed by construction, and the label-density/zoom-hiding machinery existed to
   fight a problem the geometry created.
3. **No resize handling** — fit ran once at `layoutstop`; any later container resize left a
   stale viewport = the clipped hero nodes on home (eShop "ntity", podcasts left-edge cut).

## Decisions
- **ONE engine: ELK `layered`, direction RIGHT, all three modes** (topology / trace /
  neighbors). Deterministic given input order; input order pinned by lexicographic sort
  inside the pure layout module, so geometry is reproducible from shuffled callers
  (spec-pinned, not asserted). ELK over dagre: maintained, better edge routing, and its
  compound-node support is exactly what D4.2's per-service expansion + lanes and D4.3's
  layered atlas need — the next two checkpoints build on this engine, not around it.
  cytoscape stays as pure renderer (`preset` positions); `cytoscape-dagre` +
  `cytoscape-fcose` removed, `elkjs` added (dynamic import — loads on first canvas).
- **Nodes are boxes with the label inside**, width = monospace char arithmetic (no DOM
  measurement → deterministic + testable in node). Overlap now impossible by construction;
  the zoom-dependent label-hiding mapper and its frozen-style bug class are DELETED.
  Degree centrality moved from box size (fought label width) to border emphasis.
- **Fit-and-center that never clips**: fit + zoom clamp (compact ≤1.0, full ≤1.25 — the
  baseline refit hero ballooned 4 nodes to comic size), ResizeObserver → rAF-debounced
  re-fit (kills the stale-fit clip class).
- **Neighbors mode now honors true edge direction** (`from`/`to`, same fields node-card's
  Called-by/Calls split uses) — callers seat LEFT of center, callees RIGHT; the old code
  drew every edge center→other, which a layered layout would have rendered as all-right.
- **Stability across pages** falls out of purity: home hero, Atlas diagram, Explore System
  all call the same layout fn on the same data → identical geometry everywhere.

## Verification
- 7 new vitest specs (`graph-layout.spec.ts`): same-input identity, shuffled-input identity,
  pairwise no-box-intersection on a 33-node/76-edge seeded fixture, finite in-bounds
  geometry, dangling-edge drop, empty graph, width clamps. Suite 56/56 green.
- Screenshot gate re-run (gate-d41/): SCREENSHOT-GATE: PASS 16/16, 0 page errors, warm
  cache walls 1.0–5.2s. Diff vs gate-baseline/:
  - podcasts home: clipped/overlapped blob → three readable left-to-right dependency chains.
  - eShop atlas: overlapping hairball w/ half-clipped WebApp → ranked AppHost→APIs→domain/EventBus.
  - bitwarden atlas: unlabeled 33-node hairball, clipped 2 edges → all 33 labeled boxes in
    ranks, nothing clipped (tiny at fit zoom — readability at this scale is D4.2's
    progressive disclosure; the atlas canvas deserves taller-than-hero treatment at D4.3).
  - refit home: ballooned 4-node fragment → hub layout, consumers→Refit→generators, clamped.
- Known-carried: refit still titled DevContext.slnx (F4, D4.5); style chip on Library (F1,
  D4.4); atlas uses the compact 280px hero (D4.3 gives the one-pager its own canvas size).
