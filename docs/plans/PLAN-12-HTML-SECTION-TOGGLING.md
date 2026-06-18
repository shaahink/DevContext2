# Handover: Desktop UI HTML Section Filtering

## Task
Make HTML output in the desktop app re-render when section toggles change, and make nav
links work reliably. The LLM view currently appears "live" (token counts update) but neither
view actually filters content on section toggle. Also, HTML is built via raw `StringBuilder`
concatenation — evaluate better templating/binding approaches.

## Project context
- **Repo**: DevContext — a .NET codebase analysis tool that produces context for humans/LLMs
- **Desktop**: WPF + BlazorWebView (WebView2). MVVM with CommunityToolkit.Mvvm.
- **Key directories**:
  - `src/DevContext.Core/Rendering/` — HTML/Markdown/JSON renderers
  - `src/DevContext.Desktop/ViewModels/` — MainViewModel, OutputViewModel, SectionSelectionModel
  - `src/DevContext.Desktop/Components/` — Blazor components (OutputPanel.razor, ConfigPanel.razor)
  - `src/DevContext.Desktop/wwwroot/` — index.html, app.css
  - `src/DevContext.Desktop/Services/` — AnalysisService

## How the current code works

### HTML generation
`HtmlContextRenderer.cs` (468 lines) builds the entire HTML document in a single `StringBuilder`
via string concatenation. Each section emits raw HTML tags as string literals:

```csharp
sb.AppendLine($"<section class='dc-section' id='dc-{SectionNames.Endpoints}'>");
sb.AppendLine("<div class='dc-table-wrap'><table class='dc-table'>");
// ...
```

### Rendering flow
1. `MainViewModel.RerenderAsync()` calls `AnalysisService.RenderAsync()` which calls the
   pipeline TWICE: once for markdown (LLM view), once for HTML (human view)
2. HTML render returns monolithic string stored in `_output.HumanViewHtml`
3. Blazor renders it via `@((MarkupString)VM.HumanViewHtml)` in `OutputPanel.razor:157`
4. When sections are toggled in the drawer, `OnSectionChanged` fires → which calls
   `RebuildLlmViewText()` (a no-op that reassigns `RawContent` to `LlmViewText`)
5. **No re-render or content filtering happens** — only token math updates

### Section drawer
- Checkboxes in `OutputPanel.razor:142-148` bind to `SectionViewModel.IsIncluded`
- `SectionViewModel` objects are created in `SectionSelectionModel.BuildSectionDataFromStat()`
  line 149, which receives `renderResult.Sections` (currently always empty — bug #3 below)
- When a checkbox toggles, `PropertyChanged` fires → `RecalcTokenTotal()` + `OnSectionChanged`

### Human vs LLM view
- Human: `<div class="output-rendered">@((MarkupString)VM.HumanViewHtml)</div>` — raw HTML
- LLM: `<pre class="output-text"><code>@VM.LlmViewText</code></pre>` — escaped plain text
- Both views use the same `RawContent` (full markdown); sections don't filter either

## Root cause of the bugs

**`MarkupString` in Blazor replaces entire innerHTML**: When `HumanViewHtml` changes even
slightly, Blazor wipes the `<div class="output-rendered">` DOM and rebuilds it from scratch.
This means:
- Nav link targets (`<a href="#dc-Endpoints">`) point to elements that just got destroyed
- Scroll position resets to top
- `<details open>` state collapses
- Any browser-native state is lost

## Bugs identified

| # | Bug | File:Line |
|---|-----|-----------|
| 1 | Section drawer toggles don't filter HTML or text content | `OutputPanel.razor:145` → `OnSectionChanged` only does token math |
| 2 | Nav links stop working after toggle/re-render — DOM destroyed | Blazor `MarkupString` replacement |
| 3 | `RenderedContext.Sections` always empty — never populated by any renderer | `HtmlContextRenderer.cs:71` |
| 4 | AntiPatterns & EventFlow bypass `ShouldRender()` section filtering | `HtmlContextRenderer.cs:52-53` |
| 5 | `SectionTokenRecord` list collected during render but never used | `HtmlContextRenderer.cs:14` |
| 6 | Duplicate `_output.HasOutput = true` assignment | `MainViewModel.cs:427` |
| 7 | Section drawer VM state (IsIncluded) lost on re-render — new VMs each time | `SectionSelectionModel.cs:149` |
| 8 | `RebuildLlmViewText()` is a no-op — just reassigns same value | `MainViewModel.cs:119-121` |
| 9 | HTML sections render empty wrappers even when query returns 0 items | Various render methods |
| 10 | Map/Trace narrative path ignores section filtering entirely | `DiscoveryPipeline.cs:330-363` |

## Recommended approach

**JS-based DOM section toggling** — don't rebuild the HTML string. Instead:

1. **Render once** — full HTML with all sections in the DOM

2. **Toggle visibility via JS** — when sections change, call JS to set `display:none` on
   section elements by their `id` attribute. Section IDs already exist:
   ```html
   <section class="dc-section" id="dc-Endpoints">...</section>
   <section class="dc-section" id="dc-Architecture overview">...</section>
   ```

3. **JS function in `wwwroot/index.html`**:
   ```javascript
   window.devContext = {
       syncSections: function (enabledIds) {
           document.querySelectorAll('.dc-section[id]').forEach(function (el) {
               var id = el.id.replace(/^dc-/, '');
               el.style.display = enabledIds.indexOf(id) >= 0 ? '' : 'none';
           });
           // Also hide/show nav links to match
           var nav = document.querySelector('.dc-nav');
           if (nav) {
               nav.querySelectorAll('a').forEach(function (link) {
                   var href = (link.getAttribute('href') || '').replace(/^#dc-/, '').replace(/%20/g, ' ');
                   link.style.display = enabledIds.indexOf(href) >= 0 ? '' : 'none';
               });
           }
       }
   };
   ```

4. **Blazor calls JS on toggle** — in `OutputPanel.razor`, checkbox `@onchange` handlers
   call `JS.InvokeVoidAsync("devContext.syncSections", enabledIds)`.

5. **Also call on `OnAfterRenderAsync`** — to sync after full re-renders (e.g. token budget
   change rebuilds the HTML).

### Why this approach
- **DOM stays intact** — scroll position, `<details>` state, nav link targets all preserved
- **Instant feedback** — no pipeline cost, no string assembly, just a CSS display toggle
- **Battle-tested** — same pattern as MDN docs, Vue docs, every documentation site
- **Works with existing section `id` attributes** — no HTML changes needed
- **Nav links always work** — targets exist in DOM, just hidden/shown

### Also needed

**Fragment architecture** (optional complement): Have `HtmlContextRenderer` capture each
section's HTML as a named fragment in a dictionary. Used only for the **initial** HTML
assembly (`AssembleFullHtml`), not for interactive toggling. This solves bugs #3, #4, #5, #9.

**Template library** (future): Consider Scriban or Razor templates to replace raw
`StringBuilder` HTML concatenation. Orthogonal to the toggle fix.

## Files involved in any fix

| File | Role |
|------|------|
| `wwwroot/index.html` | Add JS `syncSections` function |
| `Components/OutputPanel.razor` | Inject IJSRuntime, call syncSections on checkbox toggle + OnAfterRenderAsync |
| `ViewModels/OutputViewModel.cs` | Store `EnabledSectionIds` (ISet<string>), `HumanViewHtml` set-once |
| `ViewModels/MainViewModel.cs` | `OnSectionChanged` updates EnabledSectionIds, raises PropertyChanged |
| `ViewModels/SectionSelectionModel.cs` | `BuildSectionDataFromStat` needs section data (fix bug #3 first) |
| `Core/Rendering/HtmlContextRenderer.cs` | Populate Sections, fix ShouldRender gaps, produce fragments |
| `Core/Pipeline/DiscoveryPipeline.cs` | Pass section filter to narrative path (Map/Trace) — bug #10 |
| `Core/Contracts/IContextRenderer.cs` | Add SectionFragments field to RenderedContext |

## Build & test
- `dotnet build` — solution builds (net10.0-windows)
- `dotnet test` — 258 Core tests + 65 Desktop tests (2 pre-existing skips)
- Analyzer: MA0016 (prefer collection abstractions in public API)
- Analyzer: MA0051 (methods max 80 lines)
