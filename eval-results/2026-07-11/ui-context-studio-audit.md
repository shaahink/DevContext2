# DevContext UI — Context Studio Deep Audit

**Date:** 2026-07-11  
**Branch:** `feat/mcp-drive-audit`  
**Methodology:** Full code audit of all 8 pipeline layers (Angular UI → gRPC → Server → ContextPackBuilder → GraphQuery → Trace → AnalysisSnapshot), combined with MCP `get_context` blind-drive findings.

---

## 1. Pipeline Architecture (verified end-to-end)

```
User clicks "Add to context" (ScopePicker)
  │
  ├─[1] Seeds 9 ContextCard objects (flow, signatures, bodies, di_wiring, config, entities, contracts, tests, identity)
  │
  ├─[2] ContextStudio.loadAllCards() filters out config/tests (client-only stubs)
  │     → Calls api.getContextPack(handle, cardSpecs, { budgetTokens, intent })
  │     → Single gRPC-Web unary call
  │
  ├─[3] DevContextGrpcService.GetContextPack() — server handler
  │     → Creates ContextPackBuilder(session.Query, session.Snapshot)
  │     → builder.BuildMulti(specs, budget, intent)
  │
  ├─[4] ContextPackBuilder.BuildMulti() — engine
  │     a) Collects unique entry focuses (deduped across cards)
  │     b) Resolves each entryId → focus string (e.g. "GET /products")
  │     c) Proportions budget by reach count (complexity proxy)
  │     d) Traces each unique entry: _query.Trace(focus, depth)
  │     e) Builds sections: identity → trace → signatures → bodies → di_wiring → entities
  │     f) Budget-gates each section: if >budget & <60% filled → truncate; else → omit
  │     g) Per-card assembly: picks sections by CardType (flow→[trace], bodies→[bodies], etc.)
  │     h) Assembles final markdown with headers + per-card sections
  │
  ├─[5] ProtoMapper.ToContextPackResponse() — Core → Proto
  │     → Returns Cards[], AssembledMarkdown, TotalTokens, AllocatedTokens, Omitted[]
  │
  ├─[6] ContextStudio receives response
  │     → serverPackMarkdown stored for Copy/Save
  │     → Cards updated with serverTokens, sectionTokens, content (from card.title)
  │     → loading=false
  │
  └─[7] Export/Copy — ContextStudio.onCopy() / onSave()
        → buildContext(format):
           if serverPackMarkdown exists → use it (strip markdown for 'plain' format)
           else → client-side fallback assembly
        → Copy: navigator.clipboard.writeText()
        → Save: Blob + download anchor (always saves as .md)
```

---

## 2. What Works Well

### 2.1 Budget management with server round-trip
The budget slider (1k-16k) flows through to the engine. The `tokensAddSection()` function correctly:
- Truncates sections when over budget but <60% filled
- Omits sections entirely when budget is exhausted
- Reports what was omitted in the `omitted[]` list

This is the right architecture — token counting happens server-side where the actual markdown is assembled.

### 2.2 Intent-driven section ordering
Three intents produce different section orders:
- **trace**: identity → trace → signatures → bodies → di_wiring → entities
- **explain**: identity → di_wiring → entities → signatures → bodies → trace
- **review**: identity → trace → signatures → bodies → di_wiring → entities

Card reordering in the UI follows `INTENT_CARD_ORDER` mapping and uses `effect()` for reactivity.

### 2.3 Server-assembled markdown (Trap A closed)
The `buildContext()` method correctly prefers `serverPackMarkdown` over client-side assembly. This means the Copy/Save output is exactly what the engine produced — consistent with what MCP `get_context` would produce.

### 2.4 Multi-card versus single-card architecture
`GetContextPack` (multi-card) and `GetContext` (single-focus, used by MCP) share the same `ContextPackBuilder`, but:
- Multi-card: traces each unique entry once, then distributes sections by card type
- Single-card: traces one focus and returns all sections

This dedup means an entry that appears in 5 cards is only traced once — correct and efficient.

### 2.5 Preset workflows work
- "I'm changing this endpoint" → seeds flow + bodies + contracts + 2x tests cards (5 cards)
- "Add to context" button → seeds all 9 card types
- "From current trail" → seeds flow cards from TrailStore steps
- Omnibox search → single flow card

---

## 3. Critical Gaps (Functional Defects)

### GAP 1: No verification mechanism — CRITICAL

**What exists:** Nothing. Zero. There is no way to verify that the generated context matches actual source code.

**What's needed:**
- **Per-section provenance**: Each section in the context pack should carry file:line references showing where the content came from. Currently, `BuildTraceSkeleton()` has no line numbers. `BuildCalleeSignatures()` has `Location: file:line` but it's embedded in markdown, not structured.
- **Staleness detection**: The proto has `AnalysisSummary.stale` + `stale_message` fields, but Context Studio never checks them. If the repo changes after analysis, the context pack is silently stale.
- **Source cross-reference**: A "View source" link from any context card section to the original code. Currently: no way to verify that the content in the pack matches the actual file.
- **Confidence score per section**: Sections should carry a confidence indicator (the trace skeleton is deterministic, but salient bodies may be truncated, signatures may miss overloads, etc.)

**Impact on AI agent:** An agent receiving context from the MCP has no way to know if:
- The source file has changed since analysis
- The traced flow matches the actual call graph
- The method bodies in the pack are the latest versions
- Any section was truncated due to budget (the `omitted[]` list is the only signal, and the UI doesn't show it)

### GAP 2: Omitted list never shown to user — MODERATE

**Evidence:** `ContextPackResponse.omitted` is populated by the server (e.g., `"signatures: omitted (1450 tokens, budget exhausted)"`) but:

```typescript
// context-studio.ts:139 — stores serverPackMarkdown but does NOT read pack.omitted
this.serverPackMarkdown = pack.assembledMarkdown || null;

// Card update loop: never touches omitted
this.cards.update((prev) => prev.map((c) => {
    const ci = cardByType.get(c.type);
    // ... no omitted handling ...
}));
```

**Impact:** The user has no way to know what was cut. An agent using MCP `get_context` DOES see omitted entries in the JSON response, but the UI user gets a silent truncation.

### GAP 3: `config` and `tests` card types are dead stubs — MODERATE

```csharp
// ContextPackBuilder.cs:164-168
["config"] = [],   // config is not traced — handled separately
["tests"]  = [],   // tests — handled separately
```

```typescript
// context-studio.ts:124-125
const cardSpecs = newCards
    .filter((c) => c.type !== 'config' && c.type !== 'tests')
```

These card types:
- Get added to the UI as real cards
- Show a loading spinner briefly
- Then go to `loading=false` with no content
- The UI never explains WHY they have no content

**Fix needed:** Either implement server-side config/test assembly in ContextPackBuilder, or remove from UI / mark as "coming soon."

### GAP 4: Focus resolution is brittle — handles routes poorly — HIGH (from MCP drive)

**Evidence from MCP blind-drive:**
```
trace(focus="/products")         → "No entry or node matched '/products'."
trace(focus="/api/Products")     → "No entry or node matched '/api/Products'."
get_context(focus="/products")   → 0 sections, no content
```

**Root cause in `ContextPackBuilder.ResolveFocus()` (line 443-455):**
```csharp
private string? ResolveFocus(string entryId)
{
    foreach (var entry in _snapshot.Entries)
    {
        if (nid == entryId || entry.Title == entryId ||
            (entry.HttpMethod is { } hm && entry.Route is { } rt && $"{hm} {rt}" == entryId))
            return ...;
    }
    return null;
}
```

It matches:
1. Exact nodeId ✅
2. Exact entry title ✅  
3. `"HTTPMETHOD ROUTE"` format (e.g. `"GET /products"`) ✅
4. **NOT**: bare routes like `"/products"` or `"/api/Products"` ❌
5. **NOT**: natural language like `"how does checkout work"` ❌ (this one is expected)

**The MCP `trace` tool has the same issue** — calls `GetTrace` which uses `graph.Find` + `Query.Trace()`. The `Find` path should resolve bare routes.

**Fix:**
1. `ResolveFocus()` should also match `entry.Route` alone (bare route)
2. MCP `trace()` should attempt route-first resolution before falling through to `graph.Find`

### GAP 5: No error UI in composition view — LOW

```typescript
// context-studio.ts:160-162
} catch {
    for (const c of newCards) c.loading = false;  // silent failure!
}
```

When the server RPC fails, all new cards silently stop loading with no error indicator, no toast, no retry button.

### GAP 6: Save always writes .md extension — LOW

```typescript
// context-studio.ts:235
a.download = 'devcontext-context.md';  // hardcoded .md even for plain text format
```

---

## 4. Visual & UX Gaps

### 4.1 The 3-pane layout is solid but dense
- Content preview in CompositionView is `max-h-24` (96px) — for a section with multiple file snippets, this is tiny
- No option to expand a card to full-height for reading
- Source files in `read_source` preview are crammed into the same tiny card view

### 4.2 Provenance chips only show filenames
```typescript
// composition-view.ts:159-161
protected shortProvenance(provenance: string): string {
    const lastSep = Math.max(provenance.lastIndexOf('/'), provenance.lastIndexOf('\\'));
    return lastSep >= 0 ? provenance.slice(lastSep + 1) : provenance;
}
```
Shows `Program.cs` instead of `src/Web/Program.cs:13`. Full path tooltip exists on `[title]` attribute, but no line number is ever shown.

### 4.3 No diff/compare view
There's no way to compare the generated context with the actual source. A "verify" button should:
1. Show original source alongside generated context
2. Highlight what was included vs. truncated
3. Flag staleness

### 4.4 No "copy a single card" functionality
Copy copies the ENTIRE pack. There's no per-card copy button to extract just the flow trace or just the DI wiring section.

### 4.5 Budget panel feedback is limited
- Per-card meters show token counts, but don't show what's INSIDE each card
- Green/red colored bars but no units on the bars (e.g., "234 tok" floating on the bar)
- No "expand all" / "collapse all" for card content previews

### 4.6 No keyboard shortcuts
- No `Ctrl+C` / `Ctrl+S` for copy/save within the context studio (they could override browser defaults in this focused view)
- No `Ctrl+K` to focus the search box in ScopePicker
- No navigation between cards (arrow keys, Tab)

---

## 5. The Verification Gap (DEEP DIVE)

This is the single biggest missing feature. Let me trace what WOULD be needed to verify context:

### 5.1 What verification means for an AI agent user

An agent receives a context pack via MCP `get_context` and needs to answer:
1. Is this content **current**? (stale detection)
2. Is this content **complete**? (what was truncated/cut?)
3. Is this content **accurate**? (does it match the source?)
4. Can I **trust** the trace? (is the call graph correct?)
5. What's my **confidence** in each section?

Currently, NONE of these can be answered.

### 5.2 What would a verification pass require?

```csharp
// Proposed: VerifyContextPack(handle, contextPackId) returns:
public sealed record ContextVerification
{
    public bool AllSectionsVerified { get; init; }
    public ImmutableArray<SectionVerification> Sections { get; init; }
}

public sealed record SectionVerification
{
    public string Section { get; init; }          // e.g. "trace", "bodies"
    public int TotalLines { get; init; }          // lines in the section
    public int SourceLinesReferenced { get; init; } // unique source lines referenced
    public int SourceLinesChanged { get; init; }   // lines that differ from analysis snapshot
    public double AccuracyScore { get; init; }     // 0.0-1.0 how much matches
    public ImmutableArray<string> Warnings { get; init; }
    public bool IsStale { get; init; }             // true if any source file changed since analysis
}
```

### 5.3 What the UI should show for verification

```
┌─ VERIFICATION ───────────────────────────────────────────────┐
│  ┌─────────────────────────────────────────────────────┐     │
│  │  Trace section                        ✓ 94% accurate  │     │
│  │  ▸ 5 steps, all source-verified                      │     │
│  │  ▸ 2 nodes [approx] (syntactic resolution)            │     │
│  │  ▸ src/Web/Program.cs:13 - verified (unchanged)       │     │
│  └─────────────────────────────────────────────────────┘     │
│  ┌─────────────────────────────────────────────────────┐     │
│  │  Bodies section                       ⚠ 78% accurate  │     │
│  │  ▸ 3 of 5 method bodies fully captured                │     │
│  │  ▸ 2 methods truncated (salient-only, 15+ lines cut)  │     │
│  │  ▸ src/Domain/Entities/Product.cs - STALE (changed)   │     │
│  └─────────────────────────────────────────────────────┘     │
│  ┌─────────────────────────────────────────────────────┐     │
│  │  DI Wiring section                    ? Unverified    │     │
│  │  ▸ Based on Resolves edges (confidence: medium)      │     │
│  │  ▸ No source cross-reference available                │     │
│  └─────────────────────────────────────────────────────┘     │
│                                                              │
│  Overall: 86% accurate · 2 stale files · 1 unverified        │
│  [Re-analyze]  [Refresh verification]                        │
└──────────────────────────────────────────────────────────────┘
```

---

## 6. The Copy/Export Audit

### 6.1 What Copy produces (good)
- Full server-assembled markdown (when server responds)
- Plain format strips markdown syntax correctly (headers, italic, HTML comments)
- Toast feedback: "Context copied to clipboard"

### 6.2 What Copy is missing
- **Line numbers in the output**: The current trace skeleton has no `file:line` annotations. An AI agent reading the copied context can't jump to specific lines.
- **Metadata header**: No timestamp, no repo path, no analysis handle, no budget/info, no staleness indicator
- **Format consistency**: The markdown has `<!-- context card: type -->` HTML comments that don't survive copy-paste into most LLM contexts
- **Truncation notice**: When budget cuts a section, the truncated markdown says `... (truncated)` but doesn't say WHAT was cut or from where

### 6.3 What Save produces
Same as Copy, but saved as a file. The filename is always `devcontext-context.md` regardless of format. Good: includes timestamp footer. Bad: no repo name in filename.

### 6.4 What's missing from export entirely
- **JSON export**: For programmatic consumption, the structured card data (with sectionTokens, types, entryIds) is valuable but only available as markdown/plain
- **Export to MCP config**: The user should be able to copy the context as an MCP config snippet
- **Batch export**: Export multiple context packs (for different entries/intents) in one operation

---

## 7. Comparison: UI Context Studio vs MCP `get_context`

| Feature | UI Context Studio | MCP `get_context` |
|---------|------------------|-------------------|
| Multi-card (9 card types) | Yes | No (single focus, all sections) |
| Budget control | Slider 1k-16k | `budgetTokens` param |
| Intent (trace/explain/review) | 3 buttons | `intent` param |
| Per-card body toggle | Eye/eye-off per card | No (all bodies included) |
| Copy/Save | Yes (full pack) | N/A (agent reads response) |
| `omitted` list | **NOT SHOWN** | Included in JSON response ✅ |
| Section token breakdown | Shown per-card ✅ | Included in `sections[]` ✅ |
| Format (markdown/plain) | 2 buttons | Markdown only (in `content` field) |
| Error on bad focus | Silent (cards just don't load) | Structured error + "did you mean?" ✅ |
| Provenance display | File:line chips (filenames only) | `filePath` + `lineNumber` in sections |
| Verification | **NONE** | **NONE** |

---

## 8. Root Cause Analysis: Why These Gaps Exist

### 8.1 The `omitted` list is server-side only
The server's `MultiContextPack.omitted` is populated in `BuildMulti()` correctly. The proto has `repeated string omitted = 7` on `ContextPackResponse`. The TS generated code would have `pack.omitted`. But `context-studio.ts` never reads it. This is a **missing wire** — the code just needs to display what's already computed.

### 8.2 Verification was never in the design
The original design (Meridian M8, Loom L4.4) specified context pack assembly, budget control, and server round-trip. Section §4 of the Loom design doc mentions "Honesty surfaces" with `Coverage`, but verification was never part of the spec. The `omitted[]` list and `Found` flag are honesty surfaces, but they're not exposed end-to-end.

### 8.3 Focus resolution assumes exact format
`ResolveFocus()` matches `"GET /products"` but not `"/products"`. This is a conscious design choice (the entry is `"GET /products"`), but it creates a friction point because:
1. `entrypoints` returns entries with both `httpMethod` and `route` as separate fields
2. `top_flows` returns entries with `route` as a separate field
3. But `trace` and `get_context` expect the combined format

An agent that reads the structured fields will construct broken focus strings.

---

## 9. Recommendations (ordered by impact)

### Immediate (fix existing bugs): ≤2 sessions

| # | Fix | Files Changed | Impact |
|---|-----|---------------|--------|
| R1 | Show `omitted[]` list in BudgetPanel | `budget-panel.ts`, `context-studio.ts` | User knows what was cut |
| R2 | Resolve bare routes in `ResolveFocus()` + MCP `trace` | `ContextPackBuilder.cs`, `DevContextTools.cs` | -40% calls in agent workflows |
| R3 | Stamp `lineNumber` on EntryPoint nodes | Engine node assembly | `read_source` shows correct line |
| R4 | Show error state in CompositionView | `composition-view.ts`, `context-studio.ts` | User knows when things fail |
| R5 | Fix .md extension for plain format | `context-studio.ts:235` | Correct file extension |

### Short-term (add missing features): ≤5 sessions

| # | Feature | Files | Impact |
|---|---------|-------|--------|
| R6 | **Verification panel** — section-by-section accuracy report with staleness detection | New `verification-panel.ts`, `ContextPackBuilder` | Trust — agent can verify content matches source |
| R7 | Per-card copy button | `composition-view.ts` | Copy just one section |
| R8 | JSON export format | `context-studio.ts` | Programmatic consumption |
| R9 | Implement `config` and `tests` card types in ContextPackBuilder | `ContextPackBuilder.cs` | Complete card type support |
| R10 | Section confidence/accuracy metadata | `ContextPackBuilder.cs`, proto | Transparency |

### Medium-term (deepen the verification experience): ≤8 sessions

| # | Feature | Impact |
|---|---------|--------|
| R11 | Source-preview side-by-side with context (diff view) | The "verify" button shows both |
| R12 | Keyboard shortcuts for context studio | Power-user UX |
| R13 | Staleness auto-detection on context studio open | Proactive warning |
| R14 | Export to MCP config snippet | Share context with agents |
| R15 | Batch export multiple context packs | Productivity |
| R16 | Per-node "Add to context" from Inspector/Trail | Quick context from exploring |

---

## 10. Summary

The Context Studio has a **solid core pipeline** — budget allocation, section assembly, server round-trip, multi-card dedup, intent ordering — all working correctly. The architectural decisions (single-trace-per-entry, proportional budget, card type → section mapping) are sound.

**The two biggest gaps are:**

1. **No verification at all.** An agent (or human) generating context has zero ability to confirm that the output matches actual source code. Every other feature — copy, save, export — amplifies unverified content. This is the #1 feature needed for AI agent trust.

2. **Context generation is hard to discover and hard to use correctly.** The focus resolution is brittle (fails on bare routes), card types are incomplete (config/tests are stubs), the `omitted` list is hidden, errors are silent, and the output lacks provenance (no line numbers in trace skeleton).

**The copy/export pipeline is functional but lacks trustworthiness.** It produces markdown that looks right but cannot be verified. An AI agent receiving this context via MCP has the same problem — plus the additional friction of fragile focus resolution.

**Next step recommendation:** Fix R1-R5 (immediate bugs, ≤2 sessions) → Deliver R6 (verification panel, the big feature) → Then R7-R10 (round out the experience).
