# REPORT
**Spectre.Console**

Style: Unknown
_7 projects  ·  net10.0;net9.0;net8.0, net10.0;net9.0;net8.0;netstandard2.0_

## Stats

| Metric | Value |
|--------|-------|
| Files | 466 |
| Projects | 9 |
| Nodes | 1074 |
| Edges | 835 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 76% |
| Analyzed in | 17.2s |

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

### _INFO_: Wiring hubs: TestConsole (227) · Table (86) · AnsiFixture (54) · IAnsiConsole (37) · TextPrompt (30)
*(Wiring)*

- TestConsole (227)
- Table (86)
- AnsiFixture (54)
- IAnsiConsole (37)
- TextPrompt (30)

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
      AnsiWriterSettings (class)
         Represents settings for .
      BarChart (class):  BarChart
         A renderable (horizontal) bar chart.
      BarChartExtensions (class):  AddItem, AddItems, CenterLabel, HideValues, Label, LeftAlignLabel, RightAlignLabel, ShowValues, UseValueFormatter, Width, WithMaxValue
         Contains extension methods for .
      BarChartItem (class):  BarChartItem
         An item that's shown in a bar chart.
      BoxBorder (class):  GetPart
         Represents a border.
      BoxExtensions (class):  GetSafeBorder
         Contains extension methods for .
      BreakdownChart (class):  BreakdownChart
         A renderable breakdown chart.
      BreakdownChartExtensions (class):  AddItem, AddItems, Compact, FullSize, HideTagValues, HideTags, ShowPercentage, ShowTagValues, ShowTags, UseValueFormatter, Width, WithValueColor
         Contains extension methods for .
      BreakdownChartItem (class):  BreakdownChartItem
         An item that's shown in a breakdown chart.
      Calendar (class):  Calendar
         A renderable calendar.
      CalendarEvent (class):  CalendarEvent
         Represents a calendar event.
      CalendarExtensions (class):  AddCalendarEvent, HeaderStyle, HideHeader, HighlightStyle, ShowHeader
         Contains extension methods for .
      Canvas (class):  Canvas, SetPixel
         Represents a renderable canvas.
      CanvasImage (class):  CanvasImage
         Represents a renderable image.
      CanvasImageExtensions (class):  BicubicResampler, BilinearResampler, MaxWidth, Mutate, NearestNeighborResampler, NoMaxWidth, PixelWidth
         Contains extension methods for .
      Capabilities (class):  Create
         Represents terminal capabilities.
      CharExtensions (class)
         Contains extension methods for .
      CircularTreeException (class)
         Indicates that the tree being rendered includes a cycle, and cannot be rendered.
      Color (struct):  Blend, Color, Equals, ExactOrClosest, FromConsoleColor, FromHex, FromInt32, FromName, GetHashCode, ToConsoleColor, ToHex, ToMarkup, ToString, TryFromHex
         Represents a color.
      ColumnExtensions (class):  NoWrap, Width
         Contains extension methods for .
      Columns (class):  Columns
         Renders things in columns.
      ConfirmationPrompt (class):  ConfirmationPrompt, Show, ShowAsync
         A prompt that is answered with a yes or no.
      ConfirmationPromptExtensions (class):  ChoicesStyle, DefaultValueStyle, HideChoices, HideDefaultValue, InvalidChoiceMessage, No, RequireEnter, ShowChoices, ShowDefaultValue, Yes
         Contains extension methods for .
      ControlCode (class):  ControlCode, Create
         A control code.
      CursorExtensions (class):  Hide, MoveDown, MoveLeft, MoveRight, MoveUp, Show
         Contains extension methods for .
      DownloadedColumn (class):  Render
         A column showing download progress.
      ElapsedTimeColumn (class):  GetColumnWidth, Render
         A column showing the elapsed time of a task.
      Emoji (class):  Emoji, Remap, Replace
         Utility for working with emojis.
      ExceptionExtensions (class):  GetRenderable
         Contains extension methods for .
      ExceptionInfoResolver (class):  GetFileLineNumber, GetFileName, GetMethodName, GetParameterName
         Used to resolve information from an .
      ExceptionSettings (class):  ExceptionSettings
         Exception settings.
      ExceptionStyle (class)
         Represent an exception style.
      ExpandableExtensions (class):  Collapse, Expand
         Contains extension methods for .
      FigletFont (class):  FigletFont, Load, Parse
         Represents a Figlet font.
      FigletText (class):  FigletText
         Represents text rendered with a Figlet font.
      FigletTextExtensions (class):  Color, Fitted, FullSize, LayoutMode, Smushed
         Contains extension methods for .
      Grid (class):  AddColumn, AddRow, Grid
         A renderable grid.
      GridColumn (class)
         Represents a grid column.
      GridExtensions (class):  AddColumns, AddEmptyRow, AddRow, Width
         Contains extension methods for .
      GridRow (class):  GetEnumerator, GridRow
         Represents a grid row.
      HasBorderExtensions (class):  BorderColor, BorderStyle, NoSafeBorder, SafeBorder
         Contains extension methods for .
      HasBoxBorderExtensions (class):  AsciiBorder, BeveledBorder, Border, DashedBorder, DashedWideBorder, DottedBorder, DoubleBorder, DoubleHorizontalBorder, DoubleVerticalBorder, HeavyBorder, HeavyDashedBorder, HeavyDashedWideBorder, HeavyDottedBorder, HeavyHorizontalBorder, HeavyVerticalBorder
         Contains extension methods for .
      HasCultureExtensions (class):  Culture
         Contains extension methods for .
      HasJustificationExtensions (class):  Centered, Justify, LeftJustified, RightJustified
         Contains extension methods for .
      HasTableBorderExtensions (class):  Ascii2Border, AsciiBorder, AsciiDoubleHeadBorder, Border, DoubleBorder, DoubleEdgeBorder, HeavyBorder, HeavyEdgeBorder, HeavyHeadBorder, HorizontalBorder, MarkdownBorder, MinimalBorder, MinimalDoubleHeadBorder, MinimalHeavyHeadBorder, MinimalistBorder
         Contains extension methods for .
      HasTreeNodeExtensions (class):  AddNode, AddNodes
         Contains extension methods for .
      IAlignable (interface)
         Represents something that is alignable.
      IAnsiConsole (interface):  Clear, Write, WriteAnsi
         Represents a console.
      IAnsiConsoleCursor (interface):  Move, SetPosition, Show
         Represents the console's cursor.
      IAnsiConsoleInput (interface):  IsKeyAvailable, ReadKey, ReadKeyAsync
         Represents the console's input mechanism.
      IAnsiConsoleOutput (interface):  SetEncoding
         Represents console output.
      IBarChartItem (interface)
         Represents a bar chart item.
      IBreakdownChartItem (interface)
         Represents a breakdown chart item.
      IColumn (interface)
         Represents a column.
      IExclusivityMode (interface):  Run, RunAsync
         Represents an exclusivity mode.
      IExpandable (interface)
         Represents something that is expandable.
      IHasBorder (interface)
         Represents something that has a border.
      IHasBoxBorder (interface)
         Represents something that has a box border.
      IHasCulture (interface)
         Represents something that has a culture.
      IHasJustification (interface)
         Represents something that has justification.
      IHasTableBorder (interface)
         Represents something that has a border.
      IHasTreeNodes (interface)
         Represents something that has tree nodes.
      IHasVisibility (interface)
         Represents something that can be hidden.
      IMultiSelectionItem (interface):  Select
         Represent a multi selection prompt item.
      IOverflowable (interface)
         Represents something that can overflow.
      IPaddable (interface)
         Represents something that is paddable.
      IProfileEnricher (interface):  Enabled, Enrich
         Represents something that can enrich a profile.
      IPrompt (interface):  Show, ShowAsync
         Represents a prompt.
      IReadOnlyAnsiCapabilities (interface)
         Represents read-only ANSI capabilities.
      IReadOnlyCapabilities (interface)
         Represents (read-only) terminal capabilities.
      ISelectionItem (interface):  AddChild
         Represent a selection item.
      IndexedMarkupSegment (class):  Parse
      Known (class)
         Contains well-known emojis.
      Layout (class):  GetLayout, Layout, SplitColumns, SplitRows, Update
         Represents a renderable to divide a fixed height into rows or columns.
      LayoutExtensions (class):  MinimumSize, Ratio, Size
         Contains extension methods for .
      Link (class):  Equals, GetHashCode
         Represents a link.
      LiveDisplay (class):  LiveDisplay, Start, StartAsync
         Represents a live display.
      LiveDisplayContext (class):  Refresh, UpdateTarget
         Represents a context that can be used to interact with a .
      LiveDisplayExtensions (class):  AutoClear, Cropping, Overflow
         Contains extension methods for .
      Markup (class):  Escape, FromInterpolated, Markup, Remove
         A renderable piece of markup text.
      MarkupToken (class):  MarkupToken
      MarkupTokenizer (class):  Dispose, MarkupTokenizer, MoveNext
      MultiSelectionPrompt (class):  AddChoice, CalculateInitialIndex, CalculatePageSize, GetParent, GetParents, HandleInput, MultiSelectionPrompt, Render, Show, ShowAsync
         Represents a multi selection list prompt.
      MultiSelectionPromptExtensions (class):  AddCancelResult, AddChoiceGroup, AddChoices, DefaultValue, HighlightStyle, InstructionsText, Mode, MoreChoicesText, NotRequired, PageSize, Required, Select, Title, UseConverter, WrapAround
         Contains extension methods for .
      OverflowableExtensions (class):  Crop, Ellipsis, Fold, Overflow
         Contains extension methods for .
      PaddableExtensions (class):  PadBottom, PadLeft, PadRight, PadTop, Padding
         Contains extension methods for .
      Padder (class):  Padder
         Represents padding around a object.
      Padding (struct):  Equals, GetHashCode, GetHeight, GetWidth, Padding
         Represents padding.
      PaddingExtensions (class):  GetBottomSafe, GetLeftSafe, GetRightSafe, GetTopSafe
         Contains extension methods for .
      Panel (class):  Panel
         A renderable panel.
      PanelExtensions (class):  Header, HeaderAlignment
         Contains extension methods for .
      PanelHeader (class):  PanelHeader, SetAlignment, SetStyle
         Represents a panel header.
      Paragraph (class):  Append, Paragraph
         A paragraph of text where different parts of the paragraph can have individual styling.
      PercentageColumn (class):  GetColumnWidth, Render
         A column showing task progress in percentage.
      PercentageColumnExtensions (class):  CompletedStyle, Style
         Contains extension methods for .
      Profile (class):  Profile, Supports
         Represents a console profile.
      ProfileEnrichment (class)
         Contains settings for profile enrichment.
      Progress (class):  Progress, Start, StartAsync
         Represents a task list.
      ProgressBarColumn (class):  Render
         A column showing task progress as a progress bar.
      ProgressBarColumnExtensions (class):  CompletedStyle, FinishedStyle, RemainingStyle
         Contains extension methods for .
      ProgressColumn (class):  GetColumnWidth, Render
         Represents a progress column.
      ProgressContext (class):  AddTask, AddTaskAfter, AddTaskAt, AddTaskBefore, Refresh, RemoveTask
         Represents a context that can be used to interact with a .
      ProgressExtensions (class):  AutoClear, AutoRefresh, Columns, ExcludeVerticalPadding, HideCompleted, IncludeVerticalPadding, UseRenderHook
         Contains extension methods for .
      ProgressTask (class):  Increment, ProgressTask, Report, StartTask, StopTask
         Represents a progress task.
      ProgressTaskExtensions (class):  Description, HideWhenCompleted, IsIndeterminate, MaxValue, Tag, Value
         Contains extension methods for .
      ProgressTaskSettings (class)
         Represents settings for a progress task.
      ProgressTaskState (class):  Get, ProgressTaskState, Update
         Represents progress task state.
      Recorder (class):  Clear, Dispose, Export, Recorder, Write, WriteAnsi
         A console recorder used to record output from a console.
      RecorderExtensions (class):  ExportHtml, ExportText
         Contains extension methods for .
      Region (struct):  Region
         Represents a region.
      RemainingTimeColumn (class):  GetColumnWidth, Render
         A column showing the remaining time of a task.
      RemainingTimeColumnExtensions (class):  Style
         Contains extension methods for .
      Rows (class):  Rows
         Renders things in rows.
      Rule (class):  Rule
         A renderable horizontal rule.
      RuleExtensions (class):  RuleStyle, RuleTitle
         Contains extension methods for .
      SelectionPrompt (class):  AddChoice, CalculateInitialIndex, CalculatePageSize, HandleInput, Render, SelectionPrompt, Show, ShowAsync
         Represents a single list prompt.
      SelectionPromptExtensions (class):  AddCancelResult, AddChoiceGroup, AddChoices, DefaultValue, DisableSearch, EnableSearch, HighlightStyle, Mode, MoreChoicesText, PageSize, SearchPlaceholderText, Title, UseConverter, WrapAround
         Contains extension methods for .
      Size (struct):  Size
         Represents a size.
      Spinner (class)
         Represents a spinner used in a .
      SpinnerColumn (class):  GetColumnWidth, Render, SpinnerColumn
         A column showing a spinner.
      SpinnerColumnExtensions (class):  CompletedStyle, CompletedText, Style
         Contains extension methods for .
      SpinnerExtensions (class):  Spinner
         Provides extension methods for running tasks with a spinner animation.
      Status (class):  Start, StartAsync, Status
         Represents a status display.
      StatusContext (class):  Refresh
         Represents a context that can be used to interact with a .
      StatusContextExtensions (class):  Spinner, SpinnerStyle, Status
         Contains extension methods for .
      StatusExtensions (class):  AutoRefresh, Spinner, SpinnerStyle
         Contains extension methods for .
      StringExtensions (class):  EscapeMarkup, GetCellWidth, Mask, RemoveMarkup
         Contains extension methods for .
      Style (record):  Combine, GetHashCode, Parse, Style, ToMarkup, TryParse
         Represents color and text decoration.
      StyleExtensions (class):  Background, Decoration, Foreground
         Contains extension methods for .
      Table (class):  AddColumn, Table
         A renderable table.
      TableBorder (class):  GetColumnRow, GetPart
         Represents a border.
      TableBorderExtensions (class):  GetSafeBorder
         Contains extension methods for .
      TableCell (class):  Measure, Render, Span, TableCell
         Represents a table cell that can span multiple columns.
      TableColumn (class):  TableColumn
         Represents a table column.
      TableColumnExtensions (class):  Footer, Header
         Contains extension methods for .
      TableExtensions (class):  AddColumn, AddColumns, AddEmptyRow, AddRow, Caption, HideFooters, HideHeaders, HideRowSeparators, InsertRow, RemoveRow, ShowFooters, ShowHeaders, ShowRowSeparators, Title, UpdateCell
         Contains extension methods for .
      TableRow (class):  GetEnumerator, TableRow
         Represents a table row.
      TableRowCollection (class):  Add, Clear, GetEnumerator, Insert, RemoveAt, Update
         Represents a collection holding table rows.
      TableTitle (class):  SetStyle, TableTitle
         Represents a table title such as a heading or footnote.
      TaskDescriptionColumn (class):  Render
         A column showing the task description.
      Text (class):  Text
         A renderable piece of text.
      TextPath (class):  Measure, Render, TextPath
         Representation of a file system path.
      TextPathExtensions (class):  LeafColor, LeafStyle, RootColor, RootStyle, SeparatorColor, SeparatorStyle, StemColor, StemStyle
         Contains extension methods for .
      TextPrompt (class):  Show, ShowAsync, TextPrompt
         Represents a prompt.
      TextPromptExtensions (class):  AddChoice, AddChoices, AllowEmpty, ChoicesStyle, ClearOnFinish, DefaultValue, DefaultValueStyle, EditableDefaultValue, HideChoices, HideDefaultValue, InvalidChoiceMessage, PromptStyle, Secret, ShowChoices, ShowDefaultValue
         Contains extension methods for .
      TransferSpeedColumn (class):  Render
         A column showing transfer speed.
      Tree (class):  Tree
         Representation of non-circular tree data.
      TreeExtensions (class):  Guide, Style
         Contains extension methods for .
      TreeGuide (class):  GetPart
         Represents tree guide lines.
      TreeGuideExtensions (class):  GetSafeTreeGuide
         Contains extension methods for .
      TreeNode (class):  TreeNode
         Represents a tree node.
      TreeNodeExtensions (class):  Collapse, Expand
         Contains extension methods for .
      ValidationResult (class):  Error, Success
         Represents a validation result.
      VisibilityExtensions (class):  Invisible, Visible
         Contains extension methods for .
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
      DoubleHorizontalBoxBorder (class):  GetPart
         Represents a border with double horizontal edges and single vertical edges.
      DoubleLineTreeGuide (class):  GetPart
         A tree guide made up of double lines.
      DoubleTableBorder (class):  GetPart
         Represents a double border.
      DoubleVerticalBoxBorder (class):  GetPart
         Represents a border with double vertical edges and single horizontal edges.
      HeavyBoxBorder (class):  GetPart
         Represents a heavy border.
      HeavyDashedBoxBorder (class):  GetPart
         Represents a heavy dashed border.
      HeavyDashedWideBoxBorder (class):  GetPart
         Represents a heavy wide-dashed border.
      HeavyDottedBoxBorder (class):  GetPart
         Represents a heavy dotted border.
      HeavyEdgeTableBorder (class):  GetPart
         Represents a border with a heavy edge.
      HeavyHeadTableBorder (class):  GetPart
         Represents a border with a heavy header.
      HeavyHorizontalBoxBorder (class):  GetPart
         Represents a border with heavy horizontal edges and light vertical edges.
      HeavyTableBorder (class):  GetPart
         Represents a heavy border.
      HeavyVerticalBoxBorder (class):  GetPart
         Represents a border with heavy vertical edges and light horizontal edges.
      HorizontalTableBorder (class):  GetPart
         Represents a horizontal border.
      IAnsiConsoleEncoder (interface):  Encode
         Represents a console encoder that can encode recorded segments into a string.
      IHasDirtyState (interface)
         Represents something that can be dirty.
      IRenderHook (interface):  Process
         Represents a render hook.
      IRenderable (interface):  Measure, Render
         Represents something that can be rendered to the console.
      JustInTimeRenderable (class)
         Represents something renderable that's reconstructed when its state change in any way.
      LineTreeGuide (class):  GetPart
         A tree guide made up of lines.
      MarkdownTableBorder (class):  GetColumnRow, GetPart
         Represents a Markdown border.
      McGuganHorizontalBoxBorder (class):  GetPart
         Represents the horizontal variant of the McGugan border, which draws thin block edges inside the content's bounds.
      McGuganVerticalBoxBorder (class):  GetPart
         Represents the vertical variant of the McGugan border, which draws thin block edges inside the content's bounds.
      Measurement (struct):  Equals, GetHashCode, Measurement
         Represents a measurement.
      MinimalDoubleHeadTableBorder (class):  GetPart
         Represents a minimal border with a double header border.
      MinimalHeavyHeadTableBorder (class):  GetPart
         Represents a minimal border with a heavy header.
      MinimalTableBorder (class):  GetPart
         Represents a minimal border.
      MinimalistTableBorder (class):  GetColumnRow, GetPart
         Represents a minimalist border.
      NearBoxBorder (class):  GetPart
         Represents a "near" border that hugs the content using thin block elements, drawn on the inside edges with empty corn...
      NoBoxBorder (class):  GetPart
         Represents an invisible border.
      NoTableBorder (class):  GetPart
         Represents an invisible border.
      RenderHookScope (class):  Dispose, RenderHookScope
         Represents a render hook scope.
      RenderOptions (record):  Create
         Represents render options.
      RenderPipeline (class):  Attach, Detach, Process, RenderPipeline
         Represents the render pipeline.
      Renderable (class):  Measure, Render
         Base class for a renderable object implementing .
      RenderableExtensions (class):  GetSegments
         Contains extension methods for .
      RoundedBoxBorder (class):  GetPart
         Represents a rounded border.
      RoundedDashedBoxBorder (class):  GetPart
         Represents a dashed border with rounded corners.
      RoundedDashedWideBoxBorder (class):  GetPart
         Represents a wide-dashed border with rounded corners.
      RoundedDottedBoxBorder (class):  GetPart
         Represents a dotted border with rounded corners.
      RoundedTableBorder (class):  GetPart
         Represents a rounded border.
      Segment (class):  CellCount, Clone, Control, Padding, Segment, Split, SplitLines, SplitOverflow, StripLineEndings, Truncate
         Represents a renderable segment.
      SegmentLine (class):  CellCount, Prepend, SegmentLine
         Represents a collection of segments.
      SegmentLineEnumerator (class):  GetEnumerator, SegmentLineEnumerator
         An enumerator for collections.
      SegmentLineIterator (class):  Dispose, MoveNext, Reset, SegmentLineIterator
         An iterator for collections.
      SimpleHeavyTableBorder (class):  GetPart
         Represents a simple border with heavy lines.
      SimpleTableBorder (class):  GetPart
         Represents a simple border.
      SquareBoxBorder (class):  GetPart
         Represents a square border.
      SquareTableBorder (class):  GetPart
         Represents a square border.
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
| DiscoveryAndCacheWarmup | 403ms |
| GenericExtraction | 2847ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1283ms |
| Compression | 171ms |
| **Total** | **17165ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 2842ms | 750 | 0 |
| DiRegistrationExtractor | 2837ms | 0 | 0 |
| SourceBodyExtractor | 921ms | 0 | 0 |
| InMemoryEventBusExtractor | 352ms | 0 | 1 |
| CallGraphExtractor | 351ms | 0 | 0 |
| BodyFactsExtractor | 344ms | 0 | 0 |
| ProjectStructure | 239ms | 0 | 0 |
| ProgramCsFlowExtractor | 216ms | 0 | 0 |
| IndirectWiringDetector | 143ms | 0 | 1 |
| FileTreeExtractor | 111ms | 0 | 0 |
| SolutionDiscovery | 48ms | 0 | 0 |
| LayerClassifier | 26ms | 0 | 0 |
| DependencyExtractor | 24ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 825 | 187 |
| Resolves | 10 | 10 |

_466 files · 9 projects_
