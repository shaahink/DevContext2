Overview map (no focus).
Analyzing project...

LIBRARY  dotnet     (136 public types)

ENTRY API
   implement IRecipient   (IRecipient{TMessage}.cs)
      An interface for a recipient that declares a registration for a specific 
message type.
   implement IRelayCommand   (IRelayCommand.cs)
      An interface expanding with the ability to raise the event externally.
   derive    ObservableRecipient   (ObservableRecipient.cs)
      A base class for observable objects that also acts as recipients for 
messages.
   derive    ObservableValidator   (ObservableValidator.cs)
      A base class for objects implementing the interface.
   extend    ArrayExtensions   (ArrayExtensions.1D.cs)
      Helpers for working with the type.
   extend    ArrayPoolBufferWriterExtensions   
(ArrayPoolBufferWriterExtensions.cs)
      Helpers for working with the type.
   extend    ArrayPoolExtensions   (ArrayPoolExtensions.cs)
      Helpers for working with the type.
   extend    BoolExtensions   (BoolExtensions.cs)
      Helpers for working with the type.
   extend    BoxExtensions   (Box{T}.cs)
      Helpers for working with the type.
   extend    EventHandlerExtensions   (EventHandlerExtensions.cs)
      Extensions to for Deferred Events.
   extend    HashCodeExtensions   (HashCodeExtensions.cs)
      Helpers for working with the type.
   extend    IAsyncRelayCommandExtensions   (IAsyncRelayCommandExtensions.cs)
      Extensions for the type.

ABSTRACTIONS
   ObservableValidator (class)  — 20 implementors
   IRecipient (interface)  — 14 implementors
   IRelayCommand (interface)  — 5 implementors
   ObservableRecipient (class)  — 5 implementors
   ObservableRecipient (class)  — 5 implementors
   IReadOnlyObservableGroup (interface)  — 4 implementors
   IAsyncRelayCommand (interface)  — 3 implementors
   IAction (interface)  — 2 implementors
   IAction2D (interface)  — 2 implementors
   IBuffer (interface)  — 2 implementors

PUBLIC SURFACE
   CommunityToolkit.Common
      ArrayExtensions (class):  GetColumn, ToArrayString
         Helpers for working with arrays.
      Converters (class):  ToFileSizeString
         Set of helpers to convert between data types and notations.
      StringExtensions (class):  DecodeHtml, FixHtml, IsCharacterString, 
IsDecimal, IsEmail, IsNumeric, IsPhoneNumber, Truncate
         Helpers for working with strings and string representations.
      TaskExtensions (class):  GetResultOrDefault
         Helpers for working with tasks.
   CommunityToolkit.Common.Collections
      IIncrementalSource (interface):  GetPagedItemsAsync
         This interface represents a data source whose items can be loaded 
incrementally.
   CommunityToolkit.Common.Deferred
      DeferredCancelEventArgs (class)
         which can also be canceled.
      DeferredEventArgs (class):  GetCurrentDeferralAndReset, GetDeferral
         which can retrieve a in order to process data asynchronously before an 
completes and returns to the calling control.
      EventDeferral (class):  Complete, Dispose, WaitForCompletion
         Deferral handle provided by a .
      EventHandlerExtensions (class):  InvokeAsync
         Extensions to for Deferred Events.
   CommunityToolkit.Common.Extensions
      ISettingsStorageHelperExtensions (class):  Delete, GetValueOrDefault, Read
         Helpers methods for working with implementations.
   CommunityToolkit.Diagnostics
      Guard (class):  CanRead, CanSeek, CanWrite, HasSizeEqualTo, 
HasSizeGreaterThan, HasSizeGreaterThanOrEqualTo, HasSizeLessThan, 
HasSizeLessThanOrEqualTo, HasSizeNotEqualTo, HasStatusEqualTo, 
HasStatusNotEqualTo, IsAssignableToType, IsAtStartPosition, IsBetween, 
IsBetweenOrEqualTo
      ThrowHelper (class):  ThrowArgumentException, 
ThrowArgumentExceptionForBitwiseEqualTo, ThrowArgumentExceptionForCanRead, 
ThrowArgumentExceptionForCanSeek, ThrowArgumentExceptionForCanWrite, 
ThrowArgumentExceptionForHasSizeEqualTo, 
ThrowArgumentExceptionForHasSizeGreaterThan, 
ThrowArgumentExceptionForHasSizeGreaterThanOrEqualTo, 
ThrowArgumentExceptionForHasSizeLessThan, 
ThrowArgumentExceptionForHasSizeLessThanOrEqualTo, 
ThrowArgumentExceptionForHasSizeNotEqualTo, 
ThrowArgumentExceptionForHasStatusEqualTo, 
ThrowArgumentExceptionForHasStatusNotEqualTo, 
ThrowArgumentExceptionForIsAssignableToType, 
ThrowArgumentExceptionForIsAtStartPosition
         Helper methods to efficiently throw exceptions.
      TypeExtensions (class):  ToTypeString
         Helpers for working with types.
      ValueTypeExtensions (class):  ToHexString
         Helpers for working with value types.
   CommunityToolkit.HighPerformance
      ArrayExtensions (class):  AsMemory2D, AsSpan2D, Count, 
DangerousGetReference, DangerousGetReferenceAt, Enumerate, GetColumn, 
GetDjb2HashCode, GetRow, IsCovariant, Tokenize
         Helpers for working with the type.
      ArrayPoolBufferWriterExtensions (class):  AsStream
         Helpers for working with the type.
      ArrayPoolExtensions (class):  EnsureCapacity, Resize
         Helpers for working with the type.
      BoolExtensions (class):  ToBitwiseMask32, ToBitwiseMask64, ToByte
         Helpers for working with the type.
      Box (class):  DangerousGetFrom, Equals, GetFrom, GetHashCode, ToString, 
TryGetFrom
         A that represents a boxed value on the managed heap.
      BoxExtensions (class):  GetReference
         Helpers for working with the type.
      Enumerator (struct):  MoveNext
         Provides an enumerator for the elements of a instance.
      HashCodeExtensions (class):  Add
         Helpers for working with the type.
      IBufferWriterExtensions (class):  AsStream, Write
         Helpers for working with the type.
      IMemoryOwnerExtensions (class):  AsStream
         Helpers for working with the type.
      Memory2D (struct):  CopyTo, DangerousCreate, Equals, GetHashCode, 
Memory2D, Pin, Slice, ToArray, ToString, TryCopyTo, TryGetMemory
         represents a 2D region of arbitrary memory.
      MemoryExtensions (class):  AsBytes, AsStream, Cast
         Helpers for working with the type.
      ReadOnlyMemory2D (struct):  CopyTo, DangerousCreate, Equals, GetHashCode, 
Pin, ReadOnlyMemory2D, Slice, ToArray, ToString, TryCopyTo, TryGetMemory
         A readonly version of .
      ReadOnlyMemoryExtensions (class):  AsBytes, AsStream, Cast
         Helpers for working with the type.
      ReadOnlySequenceExtensions (class):  AsStream
         Helpers for working with the type.
      ReadOnlySpan2D (struct):  CopyTo, DangerousGetReference, 
DangerousGetReferenceAt, Equals, GetColumn, GetEnumerator, GetHashCode, 
GetPinnableReference, GetRow, ReadOnlySpan2D, Slice, ToArray, ToString, 
TryCopyTo, TryGetSpan
         A readonly version of .
      ReadOnlySpanExtensions (class):  AsBytes, Cast, CopyTo, Count, 
DangerousGetLookupReferenceAt, DangerousGetReference, DangerousGetReferenceAt, 
Enumerate, GetDjb2HashCode, IndexOf, Tokenize, TryCopyTo
         Helpers for working with the type.
      Span2D (struct):  Clear, CopyTo, DangerousGetReference, 
DangerousGetReferenceAt, Equals, Fill, GetColumn, GetEnumerator, GetHashCode, 
GetPinnableReference, GetRow, Slice, Span2D, ToArray, ToString
         represents a 2D region of arbitrary memory.
      SpanExtensions (class):  AsBytes, Cast, CopyTo, Count, 
DangerousGetReference, DangerousGetReferenceAt, Enumerate, GetDjb2HashCode, 
IndexOf, Tokenize, TryCopyTo
         Helpers for working with the type.
      SpinLockExtensions (class):  Enter
         Helpers for working with the type.
      StreamExtensions (class):  Read, ReadAsync, Write, WriteAsync
         Helpers for working with the type.
      StringExtensions (class):  Count, DangerousGetReference, 
DangerousGetReferenceAt, Enumerate, GetDjb2HashCode, Tokenize
         Helpers for working with the type.
      UnsafeLock (struct):  Dispose
         A that is used to enter and hold a through a block or statement.
   CommunityToolkit.HighPerformance.Buffers
      ArrayPoolBufferWriter (class):  Advance, ArrayPoolBufferWriter, Clear, 
DangerousGetArray, Dispose, GetMemory, GetSpan, ToString
         Represents a heap-based, array-backed output sink into which data can 
be written.
      IBuffer (interface):  Clear
         An interface that expands with the ability to also inspect the written 
data, and to reset the underlying buffer to wr...
      MemoryBufferWriter (class):  Advance, Clear, GetMemory, GetSpan, 
MemoryBufferWriter, ToString
         Represents an output sink into which data can be written, backed by a 
instance.
      MemoryOwner (class):  Allocate, DangerousGetArray, DangerousGetReference, 
Dispose, Slice, ToString
         An implementation with an embedded length and a fast accessor.
      SpanOwner (struct):  Allocate, DangerousGetArray, DangerousGetReference, 
Dispose, ToString
         A stack-only type with the ability to rent a buffer of a specified 
length and getting a from it.
      StringPool (class):  Add, GetOrAdd, Reset, StringPool, TryGet
         A configurable pool for instances.
   CommunityToolkit.HighPerformance.Enumerables
      Enumerator (struct):  MoveNext
         A custom enumerator type to traverse items within a instance.
      Item (struct):  Item
         An item from a source instance.
      ReadOnlyRefEnumerable (struct):  CopyTo, GetEnumerator, ToArray, TryCopyTo
         A that iterates readonly items from arbitrary memory locations.
      ReadOnlySpanEnumerable (struct):  GetEnumerator, MoveNext, 
ReadOnlySpanEnumerable
         A that enumerates the items in a given instance.
      ReadOnlySpanTokenizer (struct):  GetEnumerator, MoveNext, 
ReadOnlySpanTokenizer
         A that tokenizes a given instance.
      RefEnumerable (struct):  Clear, CopyTo, Fill, GetEnumerator, ToArray, 
TryCopyFrom, TryCopyTo
         A that iterates items from arbitrary memory locations.
      SpanEnumerable (struct):  GetEnumerator, MoveNext, SpanEnumerable
         A that enumerates the items in a given instance.
      SpanTokenizer (struct):  GetEnumerator, MoveNext, SpanTokenizer
         A that tokenizes a given instance.
   CommunityToolkit.HighPerformance.Helpers
      BitHelper (class):  ExtractRange, HasByteEqualTo, HasFlag, HasLookupFlag, 
HasZeroByte, SetFlag, SetRange
         Helpers to perform bit operations on numeric types.
      HashCode (struct):  Combine
         Combines the hash code of sequences of values into a single hash code.
      IAction (interface):  Invoke
         A contract for actions being executed with an input index.
      IAction2D (interface):  Invoke
         A contract for actions being executed with two input indices.
      IInAction (interface):  Invoke
         A contract for actions being executed on items of a specific type, with
readonly access.
      IRefAction (interface):  Invoke
         A contract for actions being executed on items of a specific type, with
side effect.
      ParallelHelper (class):  For, For2D, ForEach
         Helpers to work with parallel code in a highly optimized manner.
   CommunityToolkit.Mvvm
      For (class):  ThrowIfNull
         A specialized version for generic values.
   CommunityToolkit.Mvvm.CodeFixers
      AsyncVoidReturningRelayCommandMethodCodeFixer (class):  GetFixAllProvider,
RegisterCodeFixesAsync
         A code fixer that automatically updates the return type of methods 
using [RelayCommand] to return a instead.
      ClassUsingAttributeInsteadOfInheritanceCodeFixer (class):  
GetFixAllProvider, RegisterCodeFixesAsync
         A code fixer that automatically updates types using [ObservableObject] 
or [INotifyPropertyChanged] that have no base ...
      FieldReferenceForObservablePropertyFieldCodeFixer (class):  
GetFixAllProvider, RegisterCodeFixesAsync
         A code fixer that automatically updates references to fields with 
[ObservableProperty] to reference the generated pro...
   CommunityToolkit.Mvvm.Collections
      IReadOnlyObservableGroup (interface)
         An interface for a grouped collection of items.
      ObservableGroup (class):  ObservableGroup
         An observable group.
      ObservableGroupedCollection (class):  Contains, GetEnumerator, 
ObservableGroupedCollection
         An observable list of observable groups.
      ObservableGroupedCollectionExtensions (class):  AddGroup, AddItem, 
FirstGroupByKey, FirstGroupByKeyOrDefault, InsertGroup, InsertItem, RemoveGroup,
RemoveItem
         The extensions methods to simplify the usage of .
      ReadOnlyObservableGroup (class):  ReadOnlyObservableGroup
         A read-only observable group.
      ReadOnlyObservableGroupedCollection (class):  Contains, GetEnumerator, 
ReadOnlyObservableGroupedCollection
         A read-only list of groups.
   CommunityToolkit.Mvvm.ComponentModel
      INotifyPropertyChangedAttribute (class)
         An attribute that indicates that a given type should implement the 
interface and have minimal built-in functionality ...
      NotifyCanExecuteChangedForAttribute (class):  
NotifyCanExecuteChangedForAttribute
         An attribute that can be used to support properties in generated 
properties.
      NotifyDataErrorInfoAttribute (class)
         An attribute that can be used to support in generated properties, when 
applied to partial properties contained in a t...
      NotifyPropertyChangedForAttribute (class):  
NotifyPropertyChangedForAttribute
         An attribute that can be used to support in generated properties.
      NotifyPropertyChangedRecipientsAttribute (class)
         An attribute that can be used to support in generated properties, when 
applied to fields and properties contained in ...
      ObservablePropertyAttribute (class)
         An attribute that indicates that a given partial property should be 
implemented by the source generator.
      ObservableRecipient (class)
         A base class for observable objects that also acts as recipients for 
messages.
      ObservableRecipientAttribute (class)
         An attribute that indicates that a given type should have all the 
members from generated into it.
      ObservableValidator (class):  GetErrors
         A base class for objects implementing the interface.
   CommunityToolkit.Mvvm.ComponentModel.__Internals
      Awaiter (struct):  Awaiter, GetResult, OnCompleted, UnsafeOnCompleted
         An awaiter object for .
      TaskAwaitableWithoutEndValidation (struct):  GetAwaiter, 
TaskAwaitableWithoutEndValidation
         A custom task awaitable object that skips end validation.
      __ObservableValidatorHelper (class):  ValidateProperty
         An internal helper to support the source generator APIs related to .
      __TaskExtensions (class):  GetAwaitableWithoutEndValidation
         An internal helper used to support and generated code from its 
template.
   CommunityToolkit.Mvvm.DependencyInjection
      Ioc (class):  ConfigureServices, GetRequiredService, GetService
         A type that facilitates the use of the type.
   CommunityToolkit.Mvvm.Input
      AsyncRelayCommand (class):  AsyncRelayCommand, CanExecute, Cancel, 
Execute, ExecuteAsync, NotifyCanExecuteChanged
         A command that mirrors the functionality of , with the addition of 
accepting a returning a as the execute action, and...
      IAsyncRelayCommand (interface):  Cancel, ExecuteAsync
         An interface expanding to support asynchronous operations.
      IAsyncRelayCommandExtensions (class):  CreateCancelCommand
         Extensions for the type.
      IRelayCommand (interface):  CanExecute, Execute, NotifyCanExecuteChanged
         An interface expanding with the ability to raise the event externally.
      RelayCommand (class):  CanExecute, Execute, NotifyCanExecuteChanged, 
RelayCommand
         A command whose sole purpose is to relay its functionality to other 
objects by invoking delegates.
      RelayCommandAttribute (class)
         An attribute that can be used to automatically generate properties from
declared methods.
   CommunityToolkit.Mvvm.Messaging
      IMessenger (interface):  Cleanup, IsRegistered, Register, Reset, Send, 
Unregister, UnregisterAll
         An interface for a type providing the ability to exchange messages 
between different objects.
      IMessengerExtensions (class):  CreateObservable, IsRegistered, Register, 
RegisterAll, Send, Unregister
         Extensions for the type.
      IRecipient (interface):  Receive
         An interface for a recipient that declares a registration for a 
specific message type.
      StrongReferenceMessenger (class):  Cleanup, IsRegistered, Register, Reset,
Send, Unregister, UnregisterAll
         A class providing a reference implementation for the interface.
      WeakReferenceMessenger (class):  Cleanup, IsRegistered, Register, Reset, 
Send, Unregister, UnregisterAll, WeakReferenceMessenger
         A class providing a reference implementation for the interface.
   CommunityToolkit.Mvvm.Messaging.Messages
      AsyncCollectionRequestMessage (class):  GetAsyncEnumerator, 
GetResponsesAsync, Reply
         A for request messages that can receive multiple replies, which can 
either be used directly or through derived classes.
      AsyncRequestMessage (class):  GetAwaiter, Reply
         A for async request messages, which can either be used directly or 
through derived classes.
      CollectionRequestMessage (class):  GetEnumerator, Reply
         A for request messages that can receive multiple replies, which can 
either be used directly or through derived classes.
      PropertyChangedMessage (class):  PropertyChangedMessage
         A message used to broadcast property changes in observable objects.
      RequestMessage (class):  Reply
         A for request messages, which can either be used directly or through 
derived classes.
      ValueChangedMessage (class):  ValueChangedMessage
         A base message that signals whenever a specific value has changed.
   CommunityToolkit.Mvvm.SourceGenerators
      AsyncVoidReturningRelayCommandMethodAnalyzer (class):  Initialize
         A diagnostic analyzer that generates a warning when using 
[RelayCommand] over an method.
      AutoPropertyWithFieldTargetedObservablePropertyAttributeAnalyzer (class):
Initialize
         A diagnostic analyzer that generates an error when an auto-property is 
using [field: ObservableProperty].
      ClassUsingAttributeInsteadOfInheritanceAnalyzer (class):  Initialize
         A diagnostic analyzer that generates a warning when a class is using a 
code generation attribute when it could inheri...
      FieldReferenceForObservablePropertyFieldAnalyzer (class):  Initialize
         A diagnostic analyzer that generates a warning when accessing a field 
instead of a generated observable property.
      FieldWithOrphanedDependentObservablePropertyAttributesAnalyzer (class):  
Initialize
         A diagnostic analyzer that generates an error whenever a field has an 
orphaned attribute that depends on [ObservableP...
      IMessengerRegisterAllGenerator (class):  Initialize
         A source generator for message registration without relying on compiled
LINQ expressions.
      INotifyPropertyChangedGenerator (class):  INotifyPropertyChangedGenerator
         A source generator for the INotifyPropertyChangedAttribute type.
      InvalidClassLevelNotifyDataErrorInfoAttributeAnalyzer (class):  Initialize
         A diagnostic analyzer that generates an error when a class level 
[NotifyDataErrorInfo] use is detected.
      InvalidClassLevelNotifyPropertyChangedRecipientsAttributeAnalyzer (class):
Initialize
         A diagnostic analyzer that generates an error when a class level 
[NotifyPropertyChangedRecipients] use is detected.
      InvalidGeneratedPropertyObservablePropertyAttributeAnalyzer (class):  
Initialize
         A diagnostic analyzer that generates an error when a field or property 
with [ObservableProperty] is not valid (specia...
      InvalidPointerTypeObservablePropertyAttributeAnalyzer (class):  Initialize
         A diagnostic analyzer that generates an error whenever 
[ObservableProperty] is used with pointer types.
      InvalidPropertyLevelObservablePropertyAttributeAnalyzer (class):  
Initialize
         A diagnostic analyzer that generates an error whenever 
[ObservableProperty] is used on an invalid property declaration.
      InvalidTargetObservablePropertyAttributeAnalyzer (class):  Initialize
         A diagnostic analyzer that generates an error when a field or property 
with [ObservableProperty] is not a valid target.
      ObservablePropertyAttributeWithSupportedTargetDiagnosticSuppressor 
(class):  ReportSuppressions
         A diagnostic suppressor to suppress CS0657 warnings for fields with 
[ObservableProperty] using a [property:] attribut...
      ObservablePropertyGenerator (class):  Initialize
         A source generator for the ObservablePropertyAttribute type.
      ObservableRecipientGenerator (class):  ObservableRecipientGenerator
         A source generator for the ObservableRecipientAttribute type.
      ObservableValidatorValidateAllPropertiesGenerator (class):  Initialize
         A source generator for property validation without relying on compiled 
LINQ expressions.
      PropertyNameCollisionObservablePropertyAttributeAnalyzer (class):  
Initialize
         A diagnostic analyzer that generates an error when a generated property
from [ObservableProperty] would collide with ...
      RelayCommandAttributeWithFieldOrPropertyTargetDiagnosticSuppressor 
(class):  ReportSuppressions
         A diagnostic suppressor to suppress CS0657 warnings for methods with 
[RelayCommand] using a [field:] or [property:] a...
      RelayCommandGenerator (class):  Initialize
         A source generator for generating command properties from annotated 
methods.
      TransitiveMembersGenerator (class):  Initialize
         A source generator for a given attribute type.
      UnsupportedCSharpLanguageVersionAnalyzer (class):  Initialize
         A diagnostic analyzer that generates an error whenever a 
source-generator attribute is used with not high enough C# v...
      UnsupportedRoslynVersionForPartialPropertyAnalyzer (class):  Initialize
         A diagnostic analyzer that generates an error whenever 
[ObservableProperty] is used on a property, if the Roslyn vers...
      WinRTClassUsingNotifyPropertyChangedAttributesAnalyzer (class):  
Initialize
         A diagnostic analyzer that generates a warning when [ObservableObject] 
and [INotifyPropertyChanged] are used on a cla...
   CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models
      Array (record):  GetSyntax
         A type representing an array.
      Boolean (record):  GetSyntax
         A type representing a value.
      Enum (record):  GetSyntax
         A type representing an enum value.
      INotifyPropertyChangedInfo (record)
         A model with gathered info on a given INotifyPropertyChangedAttribute 
instance.
      Null (record):  GetSyntax
         A type representing a value.
      ObservableRecipientInfo (record)
         A model with gathered info on a given ObservableRecipientAttribute 
instance.
      Of (record):  GetSyntax
         A type representing a generic primitive value.
      Primitive (record)
         A type representing a primitive value.
      String (record):  GetSyntax
         A type representing a value.
      Type (record):  GetSyntax
         A type representing a type.
   System.Collections.Generic
      Enumerator (struct):  GetKey, GetValue, MoveNext
         Enumerator for .
   global
      INotifyPropertyChanged (class)
         A base class for objects implementing .
      ObservableRecipient (class)
         A base class for observable objects that also acts as recipients for 
messages.
   INTERNAL  (1 type in *.Internal — available on request)

CONSUMER PATHS
   contract  →  implement IRecipient
   contract  →  implement IRelayCommand
   extend  →  derive ObservableRecipient
   extend  →  derive ObservableValidator
   configure  →  ArrayExtensions.*
   configure  →  ArrayPoolBufferWriterExtensions.*

PACKAGES
   Other:  Microsoft.Bcl.AsyncInterfaces 10.0.1, Microsoft.Bcl.HashCode 6.0.0, 
Microsoft.Windows.CsWinRT 2.2.0, System.ComponentModel.Annotations 5.0.0, 
System.Memory 4.6.3, System.Runtime.CompilerServices.Unsafe 6.1.2, 
System.Threading.Tasks.Extensions 4.6.3

→ drill in:  --focus "<TypeName>"   (e.g. --focus IRecipient)

analyzed 367 files · 238 nodes · 4 edges · 0 entries · ~6148 tokens · 5.1s 
stage2 ×2.1 stage3 ×1.5
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     dotnet.slnx      │
│   Time   │        5322ms        │
│  Tokens  │ ~6148 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.136 │
╰──────────┴──────────────────────╯
