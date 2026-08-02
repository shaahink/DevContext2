LIBRARY  System.CommandLine     (37 public types)

ENTRY API
   derive    Directive   (Directive.cs)
      Provides cross-cutting functionality that can apply across command-line apps.
   derive    Symbol   (Symbol.cs)
      Defines a named symbol that resides in a hierarchy with parent and child symbols.
   derive    SymbolResult   (SymbolResult.cs)
      A result produced during parsing for a specific symbol.
   derive    SynchronousCommandLineAction   (SynchronousCommandLineAction.cs)
      Defines a synchronous behavior associated with a command line symbol.
   extend    ArgumentValidation   (ArgumentValidation.cs)
      Provides extension methods for .
   extend    CompletionSourceExtensions   (CompletionSourceExtensions.cs)
      Provides extension methods for working with completion sources.
   extend    OptionValidation   (OptionValidation.cs)
      Provides extension methods for .

ABSTRACTIONS
   SynchronousCommandLineAction (class)  — 11 implementors
   Directive (class)  — 4 implementors
   Symbol (class)  — 4 implementors
   SymbolResult (class)  — 4 implementors
   AsynchronousCommandLineAction (class)  — 3 implementors
   Option (class)  — 3 implementors
   Argument (class)  — 2 implementors
   CommandLineAction (class)  — 2 implementors
   Command (class)  — 1 implementor
   CompletionContext (class)  — 1 implementor

PUBLIC SURFACE
   System.CommandLine
      Argument (class):  GetCompletions, GetDefaultValue, ToString
         A symbol defining a value that can be passed on the command line to a command or option.
      Argument (class):  Argument
      ArgumentArity (struct):  ArgumentArity, Equals, GetHashCode
         Defines the arity of an option or argument.
      ArgumentValidation (class):  AcceptExistingOnly, AcceptLegalFileNamesOnly, AcceptLegalFilePathsOnly, AcceptOnlyFromAmong
         Provides extension methods for .
      Command (class):  Add, Command, GetCompletions, GetEnumerator, Parse, SetAction
         Represents a specific action that the application performs.
      CompletionSourceExtensions (class):  Add
         Provides extension methods for working with completion sources.
      DiagramDirective (class):  DiagramDirective
         Enables the use of the [diagram] directive, which when specified on the command line will short circuit normal comman...
      Directive (class):  Directive, GetCompletions
         Provides cross-cutting functionality that can apply across command-line apps.
      EnvironmentVariablesDirective (class):  EnvironmentVariablesDirective
         Enables the use of the [env:key=value] directive, allowing environment variables to be set from the command line duri...
      InvocationConfiguration (class)
      Option (class):  GetCompletions, GetDefaultValue
         A symbol defining a named parameter and a value for that parameter.
      Option (class):  AcceptLegalFileNamesOnly, AcceptLegalFilePathsOnly, AcceptOnlyFromAmong, Option
      … and 6 more (the structured surface lists them all)
   System.CommandLine.Completions
      CompletionContext (class)
         Supports command line completion operations.
      CompletionItem (class):  CompletionItem, Equals, GetHashCode, ToString
         Provides details about a command line completion item.
      SuggestDirective (class):  SuggestDirective
         Enables the use of the [suggest] directive, which, when specified in command-line input, short circuits normal comman...
      TextCompletionContext (class):  AtCursorPosition
         Provides details for calculating completions in the context of complete, unsplit command line text.
   System.CommandLine.Help
      Default (class):  AdditionalArgumentsSection, CommandArgumentsSection, CommandUsageSection, GetArgumentDefaultValue, GetArgumentDescription, GetArgumentUsageLabel, GetCommandUsageLabel, GetLayout, GetOptionUsageLabel, OptionsSection, ShouldShowDefaultValue, SubcommandsSection, SynopsisSection
         Provides default formatting for help output.
      HelpAction (class):  Invoke
         Provides command-line help.
      HelpOption (class):  HelpOption
         A standard option that indicates that command line help should be displayed.
   System.CommandLine.Invocation
      AsynchronousCommandLineAction (class):  InvokeAsync
      CommandLineAction (class)
         Defines a behavior associated with a command line symbol.
      ParseErrorAction (class):  Invoke
         Provides command-line output with error details in the case of a parsing error.
      SynchronousCommandLineAction (class):  Invoke
         Defines a synchronous behavior associated with a command line symbol.
   System.CommandLine.Parsing
      ArgumentResult (class):  AddError, GetValueOrDefault, OnlyTake, ToString
         Represents a result produced when parsing an .
      CommandLineParser (class):  Parse, SplitCommandLine
         Parses command line input.
      CommandResult (class):  ToString
         Represents a result produced when parsing a .
      DirectiveResult (class)
         Represents a result produced when parsing a .
      OptionResult (class):  GetValueOrDefault, ToString
         Represents a result produced when parsing an .
      ParseError (class):  ToString
         Describes an error that occurs while parsing command line input.
      SymbolResult (class):  AddError, GetRequiredValue, GetResult, GetValue
         A result produced during parsing for a specific symbol.
      Token (class):  Equals, GetHashCode, ToString, Token
         A unit of significant text on the command line.

CONSUMER PATHS
   extend  →  derive Directive
   extend  →  derive Symbol
   extend  →  derive SymbolResult
   extend  →  derive SynchronousCommandLineAction
   configure  →  ArgumentValidation.*
   configure  →  CompletionSourceExtensions.*

ENTRY POINTS
   CLI (2)
      dotnet-suggest (Main)  (src/System.CommandLine.Suggest/Program.cs:14)
      RootCommand —settings object  (src/System.CommandLine/RootCommand.cs:21)

PACKAGES
   Other:  Drop.App, Microsoft.DotNet.IBCMerge, Microsoft.ManifestTool.CrossPlatform, Microsoft.VisualStudioEng.MicroBuild.Core, Microsoft.VisualStudioEng.MicroBuild.Plugins.SwixBuild, System.Memory 4.5.5

→ drill in:  trace a focused type   (e.g. trace Directive)
