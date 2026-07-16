# REPORT
**Spectre.Console**

Style: Unknown
_7 projects  ·  net10.0;net9.0;net8.0, net10.0;net9.0;net8.0;netstandard2.0_

## Stats

| Metric | Value |
|--------|-------|
| Files | 466 |
| Projects | 9 |
| Nodes | 671 |
| Edges | 322 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 75% |
| Analyzed in | 14.0s |

## Top Flows

_No entries found._

## Insights

_2 info · 1 notable_

### **NOTABLE**: Most depended-upon: Spectre.Console (3 dependents) · Spectre.Console.Ansi (2 dependents) · Spectre.Console.SourceGenerator (2 dependents)
*(Topology)*

- Spectre.Console (3 dependents)
- Spectre.Console.Ansi (2 dependents)
- Spectre.Console.SourceGenerator (2 dependents)

### _INFO_: Public surface: 30 interfaces, 402 classes (441 total public types)
*(Shape)*

- 30 interfaces
- 402 classes

### _INFO_: Wiring hubs: TestConsole (61) · IAnsiConsole (37) · TextPrompt (24) · Progress (22) · String (18)
*(Wiring)*

- TestConsole (61)
- IAnsiConsole (37)
- TextPrompt (24)
- Progress (22)
- String (18)

LIBRARY  Spectre.Console     (245 public types)

ENTRY API
   build     AnsiCodeBuilder   (AnsiWriter.cs)
   derive    BoxBorder   (BoxBorder.cs)
      Represents a border.
   derive    Renderable   (Renderable.cs)
      Base class for a renderable object implementing .
   derive    Spinner   (Spinner.cs)
      Represents a spinner used in a .
   derive    TableBorder   (TableBorder.cs)
      Represents a border.
   extend    AlignExtensions   (Align.cs)
      Contains extension methods for .
   extend    AlignableExtensions   (IAlignable.cs)
      Contains extension methods for .
   extend    AnsiConsoleExtensions   (AnsiConsoleExtensions.Ansi.cs)
      Contains extension methods for .
   extend    BarChartExtensions   (BarChart.cs)
      Contains extension methods for .
   extend    BoxExtensions   (BoxBorder.cs)
      Contains extension methods for .
   extend    BreakdownChartExtensions   (BreakdownChart.cs)
      Contains extension methods for .
   extend    CalendarExtensions   (Calendar.cs)
      Contains extension methods for .

ABSTRACTIONS
   Spinner (class)  — 182 implementors
   Renderable (class)  — 24 implementors
   BoxBorder (class)  — 23 implementors
   TableBorder (class)  — 19 implementors
   IProfileEnricher (interface)  — 14 implementors
   ProgressColumn (class)  — 8 implementors
   IExpandable (interface)  — 7 implementors
   IHasJustification (interface)  — 7 implementors
   JsonSyntax (class)  — 7 implementors
   IHasCulture (interface)  — 5 implementors

GENERATORS
   generator   ColorGenerator
      Source generator that produces Color, ColorPalette, and ColorTable from colors.json.
   generator   EmojiGenerator
      Source generator that produces Emoji from emoji.json.
   generator   SpinnerGenerator
      Source generator that produces Spinner from spinners_default.json and spinners_sindresorhus.json.

PUBLIC SURFACE
   Backport.System.Threading
      Scope (struct):  Dispose
         A disposable structure that is returned by , which when disposed, exits the lock.
   Spectre.Console
      Align (class):  Align, Center, Left, Right
         Represents a renderable used to align content.
      AlignExtensions (class):  BottomAligned, Height, MiddleAligned, TopAligned, VerticalAlignment, Width
         Contains extension methods for .
      AlignableExtensions (class):  Alignment, Centered, LeftAligned, RightAligned
         Contains extension methods for .
      AnsiCapabilities (class):  Create
         Represents ANSI capabilities.
      AnsiCodeBuilder (class):  Build
      AnsiConsole (class):  AlternateScreen, Ask, AskAsync, Clear, Confirm, ConfirmAsync, Create, ExportCustom, ExportHtml, ExportText, Live, Markup, MarkupInterpolated, MarkupLine, MarkupLineInterpolated
         A console capable of writing ANSI escape sequences.
      AnsiConsoleExtensions (class):  AlternateScreen, Ask, AskAsync, Clear, Confirm, ConfirmAsync, CreateRecorder, Live, Markup, MarkupInterpolated, MarkupLine, MarkupLineInterpolated, Progress, Prompt, PromptAsync
         Contains extension methods for .
      AnsiConsoleOutput (class):  AnsiConsoleOutput, SetEncoding
         Represents console output.
      AnsiConsoleSettings (class):  AnsiConsoleSettings
         Settings used when building a .
      AnsiMarkup (class):  AnsiMarkup, Escape, Highlight, Parse, Remove, Write, WriteLine
         Utility used for working with markup text.
      AnsiMarkupSegment (class):  AnsiMarkupSegment, ToString
         Represents a markup segment.
      AnsiWriter (class):  AnsiWriter, Background, BeginLink, ClearScrollback, CursorBackward, CursorBackwardTabulation, CursorDown, CursorForward, CursorHome, CursorHorizontalAbsolute, CursorHorizontalTabulation, CursorLeft, CursorNextLine, CursorPosition, CursorPreviousLine
         Represents an ANSI writer, capable of outputting ANSI/VT escape sequences.
      … and 154 more (use --format json for the full surface)
   Spectre.Console.Json
      IJsonParser (interface):  Parse
         Represents a JSON parser.
      JsonText (class):  JsonText
         A renderable piece of JSON text.
      JsonTextExtensions (class):  BooleanColor, BooleanStyle, BracesColor, BracesStyle, BracketColor, BracketStyle, ColonColor, ColonStyle, CommaColor, CommaStyle, Indentation, MemberColor, MemberStyle, NullColor, NullStyle
         Contains extension methods for .
   Spectre.Console.Json.Syntax
      JsonArray (class):  JsonArray
         Represents an array in the JSON abstract syntax tree.
      JsonBoolean (class):  JsonBoolean
         Represents a boolean literal in the JSON abstract syntax tree.
      JsonMember (class):  JsonMember
         Represents a member in the JSON abstract syntax tree.
      JsonNull (class):  JsonNull
         Represents a null literal in the JSON abstract syntax tree.
      JsonObject (class):  JsonObject
         Represents an object in the JSON abstract syntax tree.
      JsonString (class):  JsonString
         Represents a string literal in the JSON abstract syntax tree.
      JsonSyntax (class)
         Represents a syntax node in the JSON abstract syntax tree.
   Spectre.Console.Rendering
      Ascii2TableBorder (class):  GetPart
         Represents another old school ASCII border.
      AsciiBoxBorder (class):  GetPart
         Represents an old school ASCII border.
      AsciiDoubleHeadTableBorder (class):  GetPart
         Represents an old school ASCII border with a double header border.
      AsciiTableBorder (class):  GetPart
         Represents an old school ASCII border.
      AsciiTreeGuide (class):  GetPart
         An ASCII tree guide.
      BeveledBoxBorder (class):  GetPart
         Represents a beveled border using thin block edges and diagonal corners.
      BoldLineTreeGuide (class):  GetPart
         A tree guide made up of bold lines.
      DashedBoxBorder (class):  GetPart
         Represents a dashed border with square corners.
      DashedWideBoxBorder (class):  GetPart
         Represents a wide-dashed border with square corners.
      DottedBoxBorder (class):  GetPart
         Represents a dotted border with square corners.
      DoubleBoxBorder (class):  GetPart
         Represents a double border.
      DoubleEdgeTableBorder (class):  GetPart
         Represents a border with a double edge.
      … and 49 more (use --format json for the full surface)
   Spectre.Console.Testing
      ShouldlyExtensions (class):  And
         Provides extensions for testing using the Shouldly-style fluent assertions.
      StringExtensions (class):  NormalizeLineEndings, TrimLines
         Contains extensions for .
      StyleExtensions (class):  SetColor
         Contains extensions for .
      TestCapabilities (class):  CreateRenderContext
         Represents fake capabilities useful in tests.
      TestConsole (class):  Clear, Dispose, TestConsole, Write, WriteAnsi
         A testable console.
      TestConsoleExtensions (class):  Colors, EmitAnsiSequences, Height, Interactive, Size, SupportsAnsi, SupportsUnicode, Width
         Contains extensions for .
      TestConsoleInput (class):  IsKeyAvailable, PushCharacter, PushKey, PushText, PushTextWithEnter, ReadKey, ReadKeyAsync, TestConsoleInput
         Represents a testable console input mechanism.

CONSUMER PATHS
   build  →  new AnsiCodeBuilder()…Build()
   extend  →  derive BoxBorder
   extend  →  derive Renderable
   extend  →  derive Spinner
   extend  →  derive TableBorder
   configure  →  AlignExtensions.*

PACKAGES
   Other:  IsExternalInit 1.0.3, Microsoft.Bcl.TimeProvider 10.0.8, Microsoft.CodeAnalysis.Analyzers 5.3.0, Microsoft.CodeAnalysis.CSharp 5.3.0, Polyfill 10.5.1, SixLabors.ImageSharp 3.1.12, System.Memory 4.6.3, System.Text.Json 10.0.8 … (9 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus AnsiCodeBuilder)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 269ms |
| GenericExtraction | 2509ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1145ms |
| Compression | 105ms |
| **Total** | **13982ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 2506ms | 750 | 0 |
| DiRegistrationExtractor | 2503ms | 0 | 0 |
| ProgramCsFlowExtractor | 2420ms | 0 | 0 |
| SourceBodyExtractor | 832ms | 0 | 0 |
| BodyFactsExtractor | 344ms | 0 | 0 |
| InMemoryEventBusExtractor | 305ms | 0 | 1 |
| CallGraphExtractor | 223ms | 0 | 0 |
| ProjectStructure | 171ms | 0 | 0 |
| IndirectWiringDetector | 110ms | 0 | 1 |
| FileTreeExtractor | 52ms | 0 | 0 |
| SolutionDiscovery | 41ms | 0 | 0 |
| DependencyExtractor | 19ms | 0 | 0 |
| LayerClassifier | 18ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 312 | 71 |
| Resolves | 10 | 10 |

_466 files · 9 projects_
