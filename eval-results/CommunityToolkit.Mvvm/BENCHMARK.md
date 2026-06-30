# CommunityToolkit.Mvvm — Library Surface BENCHMARK (hand-built target)

> Target DevContext output for the MVVM Toolkit, hand-built from a source read at pinned SHA
> `b135626dd54d33b8f05f2ff31591592c004aa848` (the `CommunityToolkit/dotnet` monorepo). The third library
> shape: **source generators + analyzers + marker attributes** (the framework/SDK-authoring target).
> Build-free constraint applies (syntax + `///` only).
>
> Note: the repo is a monorepo — its surface legitimately also shows `CommunityToolkit.Common` /
> `.Diagnostics` / `.HighPerformance`. This benchmark focuses on the MVVM + source-gen story.

```
LIBRARY  CommunityToolkit.Mvvm   (MVVM source-generator toolkit)   archetype: Library

ENTRY API                        (annotate-first: for a source-gen lib, the ATTRIBUTES are the API)
   annotate  [ObservableProperty]       "…a given partial property should be implemented by the source generator."
   annotate  [RelayCommand]             "…automatically generate commands from annotated methods."
   annotate  [INotifyPropertyChanged] / [ObservableRecipient]   "…implement the interface via generated members."
   annotate  [NotifyPropertyChangedFor] / [NotifyCanExecuteChangedFor] / [NotifyDataErrorInfo] / [NotifyPropertyChangedRecipients]
   derive    ObservableObject / ObservableValidator / ObservableRecipient    runtime base classes
   implement IRelayCommand / IRecipient<TMessage> / IMessenger

GENERATORS                       (the Roslyn tooling behind the attributes)
   generator   ObservablePropertyGenerator · RelayCommandGenerator · INotifyPropertyChangedGenerator ·
               ObservableRecipientGenerator · TransitiveMembersGenerator · IMessengerRegisterAllGenerator ·
               ObservableValidatorValidateAllPropertiesGenerator
   analyzer    ~15 diagnostics (InvalidTarget…/PropertyNameCollision…/ClassUsingAttributeInsteadOfInheritance…) + suppressors
   code-fixer  AsyncVoidReturningRelayCommandMethodCodeFixer · ClassUsingAttributeInsteadOfInheritanceCodeFixer ·
               FieldReferenceForObservablePropertyFieldCodeFixer

ABSTRACTIONS                     ObservableValidator · IRecipient · IRelayCommand · ObservableRecipient · IMessenger · …
PUBLIC SURFACE                   CommunityToolkit.Mvvm.ComponentModel / .Input / .Messaging / .DependencyInjection(Ioc) /
                                 .Collections / … (+ Common/Diagnostics/HighPerformance — monorepo) · generator .Models demoted
CONSUMER PATHS
   annotate  → [ObservableProperty] on a partial class/member
   annotate  → [RelayCommand] on a method
   extend    → derive ObservableObject
PACKAGES                         runtime only (Microsoft.Bcl.*, System.*)
```

## What makes this the target

1. **A source-gen library's API is its attributes.** `[ObservableProperty]`/`[RelayCommand]`/… lead the
   ENTRY API as `annotate` front doors (not buried alphabetically in PUBLIC SURFACE).
2. **The generators are surfaced, not hidden.** A dedicated `GENERATORS` section lists the source
   generators (`IIncrementalGenerator`), analyzers (`DiagnosticAnalyzer`/suppressors), and code fixers
   (`CodeFixProvider`) the package ships — the tooling identity of the library.
3. **Tooling internals demoted.** Generator data-model types (`*.SourceGenerators.*.Models`) and the
   `*.CodeFixers` namespaces are kept out of the runtime surface.
4. **Runtime seats still surfaced** — `ObservableObject`/`ObservableValidator`/`IMessenger` in ABSTRACTIONS
   + ENTRY API derive/implement.

## Gates (`eval/expectations/communitytoolkit.json`)
`archetype=Library` · `ENTRY API`/`ABSTRACTIONS`/`GENERATORS`/`PUBLIC SURFACE`/`CONSUMER PATHS` present ·
`annotate` marker tier with `[ObservableProperty]` + `[RelayCommand]` · `ObservablePropertyGenerator` +
`RelayCommandGenerator` surfaced. All `expected` (produced after WP6).
