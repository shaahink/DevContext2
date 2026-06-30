LIBRARY  Serilog     (109 public types)

ENTRY API
   implement IDestructuringPolicy   (IDestructuringPolicy.cs)
      Determine how, when destructuring, a supplied value is represented as a complex log event property.
   implement ILogEventEnricher   (ILogEventEnricher.cs)
      Applied during logging to add additional information to log events.
   implement ILogEventSink   (ILogEventSink.cs)
      A destination for log events.
   implement IMessageTemplateParser   (IMessageTemplateParser.cs)
   extend    LoggerExtensions   (LoggerExtensions.cs)
      Extends with additional methods.

ABSTRACTIONS
   ILogEventSink (interface)  — 25 implementors
   ILogEventEnricher (interface)  — 9 implementors
   IDestructuringPolicy (interface)  — 6 implementors
   IMessageTemplateParser (interface)  — 5 implementors
   IScalarConversionPolicy (interface)  — 4 implementors
   ISetLoggingFailureListener (interface)  — 4 implementors
   LogEventPropertyValue (class)  — 4 implementors
   ILogEventPropertyValueFactory (interface)  — 3 implementors
   ILogger (interface)  — 3 implementors
   ILoggingFailureListener (interface)  — 3 implementors

PUBLIC SURFACE
   JetBrains.Annotations
      NoEnumerationAttribute (class)
   Serilog
      ILogger (interface):  BindMessageTemplate, BindProperty, Debug, Error, Fatal, ForContext, Information, IsEnabled, Verbose, Warning, Write
         The core Serilog logging API, used for writing log events.
      Log (class):  BindMessageTemplate, BindProperty, CloseAndFlush, Debug, Error, Fatal, ForContext, Information, IsEnabled, Verbose, Warning, Write
         An optional static entry point for logging that can be easily referenced by different parts of an application.
      LoggerConfiguration (class):  CreateLogger, LoggerConfiguration
         Configuration object for creating instances.
      LoggerExtensions (class):  ForContext
         Extends with additional methods.
   Serilog.Capturing
      DepthLimiter (class):  CreatePropertyValue, DefaultIfMaximumDepth, DepthLimiter, SetCurrentDepth
      MessageTemplateProcessor (class):  CreateProperty, CreatePropertyValue, MessageTemplateProcessor, Process
      PropertyValueConverter (class):  BuildArrayValue, CreateProperty, CreatePropertyValue, IsValidDictionaryKeyType, PropertyValueConverter, Stringify, TruncateIfNecessary, TryConvertEnumerable, TryConvertStructure, TryConvertValueTuple, TryGetDictionary
      TrimConfiguration (class)
   Serilog.Configuration
      BatchingOptions (class)
         Initialization options for .
      ILoggerSettings (interface):  Configure
         Implemented on types that apply settings to a logger configuration.
      LoggerAuditSinkConfiguration (class):  Logger, Sink
         Controls audit sink configuration.
      LoggerDestructuringConfiguration (class):  AsDictionary, AsScalar, ByTransforming, ByTransformingWhere, ToMaximumCollectionCount, ToMaximumDepth, ToMaximumStringLength, With
         Controls template parameter destructuring configuration.
      LoggerEnrichmentConfiguration (class):  AtLevel, FromLogContext, When, With, WithProperty, Wrap
         Controls enrichment configuration.
      LoggerFilterConfiguration (class):  ByExcluding, ByIncludingOnly, With
         Controls filter configuration.
      LoggerMinimumLevelConfiguration (class):  ControlledBy, Debug, Error, Fatal, Information, Is, Override, Verbose, Warning
         Controls sink configuration.
      LoggerSettingsConfiguration (class):  KeyValuePairs, Settings
         Allows additional setting sources to drive the logger configuration.
      LoggerSinkConfiguration (class):  Conditional, CreateSink, FallbackChain, Fallible, Logger, Sink, Wrap
         Controls sink configuration.
   Serilog.Context
      ContextStackBookmark (class):  ContextStackBookmark, Dispose
      EnricherStack (class):  EnricherStack, GetEnumerator, Push
      LogContext (class):  Clone, GetOrCreateEnricherStack, Push, PushProperty, Reset, Suspend
         Holds ambient properties that can be attached to log events.
      LogContextEnricher (class):  Enrich
   Serilog.Core
      Constants (class)
         Constants used in the core logging pipeline and associated types.
      IBatchedLogEventSink (interface):  EmitBatchAsync, OnEmptyBatchAsync
         A destination that accepts events in batches.
      IDestructuringPolicy (interface):  TryDestructure
         Determine how, when destructuring, a supplied value is represented as a complex log event property.
      ILogEventEnricher (interface):  Enrich
         Applied during logging to add additional information to log events.
      ILogEventFilter (interface):  IsEnabled
         Provides filtering of the log event stream.
      ILogEventPropertyFactory (interface):  CreateProperty
         Creates log event properties from regular .NET objects, applying policies as required.
      ILogEventPropertyValueFactory (interface):  CreatePropertyValue
         Supports the policy-driven construction of s given regular .NET objects.
      ILogEventSink (interface):  Emit
         A destination for log events.
      ILoggingFailureListener (interface):  OnLoggingFailed
         Implementers can be notified of various failure conditions in the Serilog pipeline.
      IMessageTemplateParser (interface):  Parse
      IScalarConversionPolicy (interface):  TryConvertToScalar
         Determine how a simple value is carried through the logging pipeline as an immutable .
      ISetLoggingFailureListener (interface):  SetFailureListener
         Implemented by sinks that can report failures through an .
      LevelOverride (struct):  LevelOverride
      LevelOverrideMap (class):  GetEffectiveLevel, LevelOverrideMap
      Logger (class):  BindMessageTemplate, BindProperty, Debug, Dispose, Emit, Error, Fatal, ForContext, Information, IsEnabled, PostLevelCheckEmit, Verbose, Warning, Write
         The core Serilog logging pipeline.
      LoggingLevelSwitch (class):  LoggingLevelSwitch
         Dynamically controls logging level.
      LoggingLevelSwitchChangedEventArgs (class):  LoggingLevelSwitchChangedEventArgs
         Event arguments for event.
      MessageTemplateFormatMethodAttribute (class):  MessageTemplateFormatMethodAttribute
         Indicates that the marked method logs data using a message template and (optional) arguments.
   Serilog.Core.Enrichers
      ConditionalEnricher (class):  ConditionalEnricher, Dispose, Enrich
      EmptyEnricher (class):  Enrich
      FixedPropertyEnricher (class):  Enrich, FixedPropertyEnricher
      PropertyEnricher (class):  Enrich, PropertyEnricher
         Adds a new property enricher to the log event.
      SafeAggregateEnricher (class):  Enrich, SafeAggregateEnricher
   Serilog.Core.Filters
      DelegateFilter (class):  DelegateFilter, IsEnabled
   Serilog.Core.Pipeline
      ByReferenceStringComparer (class):  ByReferenceStringComparer, Equals, GetHashCode
      MessageTemplateCache (class):  MessageTemplateCache, Parse
      SilentLogger (class):  BindMessageTemplate, BindProperty, Debug, Error, Fatal, ForContext, Information, IsEnabled, Verbose, Warning, Write
   Serilog.Core.Sinks
      AggregateSink (class):  AggregateSink, Emit
      ConditionalSink (class):  ConditionalSink, Dispose, Emit
      DelegatingLoggingFailureListener (class):  OnLoggingFailed
      DisposingAggregateSink (class):  Dispose, DisposingAggregateSink, Emit, ReportDisposingException
      FailureListenerSink (class):  Emit, FailureListenerSink
      FilteringSink (class):  Emit, FilteringSink
      OptionalInterfaceForwardingSink (class):  Dispose, Emit, OptionalInterfaceForwardingSink, SetFailureListener, SupportsAll, SupportsAny
      RestrictedSink (class):  Emit, RestrictedSink
      SafeAggregateSink (class):  Emit, SafeAggregateSink
      SecondaryLoggerSink (class):  Dispose, Emit, SecondaryLoggerSink
         Forwards log events to another logging pipeline.
   Serilog.Core.Sinks.Batching
      BatchingSink (class):  BatchingSink, Dispose, DrainOnFailure, Emit, LoopAsync, SetFailureListener, SignalShutdown, TryWaitToReadAsync
         Buffers log events into batches for background flushing.
      FailureAwareBatchScheduler (class):  FailureAwareBatchScheduler, MarkFailure, MarkSuccess
         Manages reconnection period and transient fault response for .
   Serilog.Data
      LogEventPropertyValueRewriter (class)
         A base class for visitors that rewrite the value with modifications.
      LogEventPropertyValueVisitor (class)
         An abstract base class for visitors that walk data in the format.
   Serilog.Debugging
      LoggingFailedException (class):  LoggingFailedException
         May be thrown by log event sinks when a failure occurs.
      SelfLog (class):  Disable, Enable, WriteLine
         A simple source of information generated by Serilog itself, for example when exceptions are thrown and caught interna...
      SelfLogFailureListener (class):  OnLoggingFailed
      SelfMetrics (class)
      TagNames (class)
   Serilog.Events
      DictionaryValue (class):  DictionaryValue, Render
         A value represented as a mapping from keys to values.
      EventProperty (struct):  Deconstruct, Equals, EventProperty, GetHashCode
         A property associated with a .
      LevelAlias (class)
         Descriptive aliases for .
      LogEvent (class):  AddOrUpdateProperty, AddPropertyIfAbsent, LogEvent, RemovePropertyIfPresent, RenderMessage, UnstableAssembleFromParts
         A log event.
      LogEventProperty (class):  IsValidName, LogEventProperty
         A property associated with a .
      LogEventPropertyValue (class):  Render, ToString
         The value associated with a .
      MessageTemplate (class):  GetElementsOfTypeToArray, MessageTemplate, Render, ToString
         Represents a message template passed to a log method.
      ScalarValue (class):  Equals, GetHashCode, Render, ScalarValue
         A property value corresponding to a simple, scalar type.
      SequenceValue (class):  Render, SequenceValue
         A value represented as an ordered sequence of values.
      StructureValue (class):  Render, StructureValue
         A value represented as a collection of name-value properties.
   Serilog.Filters
      Matching (class):  FromSource, WithProperty
         Predicates applied to log events that can be used
   Serilog.Formatting
      ITextFormatter (interface):  Format
         Formats log events in a textual representation.
   Serilog.Formatting.Display
      LevelOutputFormat (class):  GetLevelMoniker
         Implements the {Level} element.
      MessageTemplateTextFormatter (class):  Format, MessageTemplateTextFormatter
         A that supports the Serilog message template format.
      OutputProperties (class)
         Describes the properties available in standard message template-based output format strings.
      PropertiesOutputFormat (class):  Render, TemplateContainsPropertyName
   Serilog.Formatting.Json
      JsonFormatter (class):  Format, JsonFormatter, WriteRenderingsValues
         Formats log events in a simple JSON structure.
      JsonValueFormatter (class):  Format, FormatBooleanValue, FormatDateTimeOffsetValue, FormatDateTimeValue, FormatDoubleValue, FormatExactNumericValue, FormatFloatValue, FormatLiteralObjectValue, FormatNullValue, FormatStringValue, FormatTimeSpanValue, JsonValueFormatter, WriteQuotedJsonString
         Converts Serilog's structured property value format into JSON.
   Serilog.Parsing
      Alignment (struct):  Alignment
         A structure representing the alignment settings to apply when rendering a property.
      MessageTemplateParser (class):  IsValidInFormat, MessageTemplateParser, Parse, ParsePropertyToken, ParseTextToken, Tokenize, TryContinuePropertyName, TryGetDestructuringHint, TrySplitTagContent
         Parses message template strings into sequences of text or property tokens.
      MessageTemplateToken (class):  Render
         An element parsed from a message template string.
      PropertyToken (class):  Equals, GetHashCode, PropertyToken, Render, ToString, TryGetPositionalValue
         A message template token representing a log event property.
      TextToken (class):  Equals, GetHashCode, Render, TextToken, ToString
         A message template token representing literal text.
   Serilog.Policies
      ByteArrayScalarConversionPolicy (class):  TryConvertToScalar
      DelegateDestructuringPolicy (class):  TryDestructure
      EnumScalarConversionPolicy (class):  TryConvertToScalar
      PrimitiveScalarConversionPolicy (class):  TryConvertToScalar
      ProjectedDestructuringPolicy (class):  ProjectedDestructuringPolicy, TryDestructure
      ReflectionTypesScalarDestructuringPolicy (class):  TryDestructure
      SimpleScalarConversionPolicy (class):  SimpleScalarConversionPolicy, TryConvertToScalar
   Serilog.Rendering
      Casing (class):  Format
      MessageTemplateRenderer (class):  Render, RenderPropertyToken, RenderTextToken, RenderValue
      Padding (class):  Apply
      ReusableStringWriter (class):  GetOrCreate, ReusableStringWriter
         Class that provides reusable StringWriters to reduce memory allocations
   Serilog.Settings.KeyValuePairs
      CallableConfigurationMethodFinder (class)
      KeyValuePairSettings (class):  ApplyDirectives, Configure, ConvertOrLookupByName, KeyValuePairSettings, LookUpSwitchByName, ParseNamedLevelSwitchDeclarationDirectives
      SettingValueConversions (class):  ConvertToType
      SurrogateConfigurationMethods (class)
         Contains "fake extension" methods for the Serilog configuration API.
   System
      SystemTimeProvider (class)
      TimeProvider (class):  GetElapsedTime, GetLocalNow, GetTimestamp, GetUtcNow
         A super-simple, cut-down subset of `System.TimeProvider` which we use internally to avoid a package dependency on pla...
   global
      Guard (class):  AgainstNull

CONSUMER PATHS
   contract  →  implement IDestructuringPolicy
   contract  →  implement ILogEventEnricher
   contract  →  implement ILogEventSink
   contract  →  implement IMessageTemplateParser
   configure  →  LoggerExtensions.*

PACKAGES
   Other:  PolySharp 1.15.0, System.Diagnostics.DiagnosticSource 8.0.1, System.Threading.Channels 8.0.0, System.ValueTuple 4.5.0

→ drill in:  --focus "<TypeName>"   (e.g. --focus IDestructuringPolicy)
