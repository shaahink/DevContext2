# REPORT
**FluentValidation**

Style: Unknown
_2 projects  ·  net10.0, net8.0 + fluentvalidation_

## Stats

| Metric | Value |
|--------|-------|
| Files | 218 |
| Projects | 4 |
| Nodes | 605 |
| Edges | 508 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 61% |
| Analyzed in | 9.5s |

## Top Flows

_No entries found._

## Insights

_3 info_

### _INFO_: Public surface: 44 interfaces, 227 classes (273 total public types)
*(Shape)*

- 44 interfaces
- 227 classes

### _INFO_: Most depended-upon: FluentValidation (2 dependents) · FluentValidation.DependencyInjectionExtensions (1 dependents)
*(Topology)*

- FluentValidation (2 dependents)
- FluentValidation.DependencyInjectionExtensions (1 dependents)

### _INFO_: Wiring hubs: InlineValidator (187) · TestValidator (47) · IRuleBuilderInitial (46) · IRuleBuilder (28) · IValidationRule (18)
*(Wiring)*

- InlineValidator (187)
- TestValidator (47)
- IRuleBuilderInitial (46)
- IRuleBuilder (28)
- IValidationRule (18)

LIBRARY  FluentValidation     (92 public types)

ENTRY API
   register  ServiceCollectionExtensions.AddValidatorsFromAssemblies   (ServiceCollectionExtensions.cs)
      Adds all validators in specified assemblies
   register  ServiceCollectionExtensions.AddValidatorsFromAssembly   (ServiceCollectionExtensions.cs)
      Adds all validators in specified assembly
   register  ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining   (ServiceCollectionExtensions.cs)
      Adds all validators in the assembly of the specified type
   derive    AbstractValidator   (AbstractValidator.cs)
      Base class for object validators.
   implement IPropertyValidator   (IPropertyValidator.cs)
   implement IValidationRule   (IValidationRule.cs)
   derive    PropertyValidator   (PropertyValidator.cs)
   extend    DefaultValidatorExtensions   (DefaultValidatorExtensions.cs)
      Extension methods that provide the default set of validators.
   extend    DefaultValidatorOptions   (DefaultValidatorOptions.cs)
      Default options that can be used to configure a validator.
   extend    ValidationTestExtension   (ValidatorTestExtensions.cs)

ABSTRACTIONS
   AbstractValidator (class)  — 52 implementors
   PropertyValidator (class)  — 20 implementors
   IPropertyValidator (interface)  — 12 implementors
   IValidationRule (interface)  — 6 implementors
   InlineValidator (class)  — 6 implementors
   IComparisonValidator (interface)  — 5 implementors
   AbstractComparisonValidator (class)  — 4 implementors
   ILengthValidator (interface)  — 4 implementors
   IRuleBuilder (interface)  — 4 implementors
   IAsyncPropertyValidator (interface)  — 3 implementors

PUBLIC SURFACE
   FluentValidation
      AbstractValidator (class):  CanValidateInstancesOfType, CreateDescriptor, GetEnumerator, Include, RuleFor, RuleForEach, RuleSet, Unless, UnlessAsync, Validate, ValidateAsync, When, WhenAsync
         Base class for object validators.
      AssemblyScanResult (class):  AssemblyScanResult
         Result of performing a scan.
      AssemblyScanner (class):  AssemblyScanner, FindValidatorsInAssemblies, FindValidatorsInAssembly, FindValidatorsInAssemblyContaining, ForEach, GetEnumerator
         Class that can be used to find all the validators from a collection of types.
      AsyncValidatorInvokedSynchronouslyException (class)
         This exception is thrown when an asynchronous validator is executed synchronously.
      DefaultValidatorExtensions (class):  ChildRules, CreditCard, Custom, CustomAsync, EmailAddress, Empty, Equal, ExclusiveBetween, ForEach, GreaterThan, GreaterThanOrEqualTo, InclusiveBetween, IsEnumName, IsInEnum, Length
         Extension methods that provide the default set of validators.
      DefaultValidatorOptions (class):  Cascade, Configurable, Configure, OverrideIndexer, OverridePropertyName, Unless, UnlessAsync, When, WhenAsync, Where, WhereAsync, WithErrorCode, WithMessage, WithName, WithSeverity
         Default options that can be used to configure a validator.
      ICollectionRule (interface)
         Represents a rule defined against a collection with RuleForEach.
      IConditionBuilder (interface):  Otherwise
         Fluent interface for conditions (When/Unless/WhenAsync/UnlessAsync)
      IRuleBuilder (interface):  SetAsyncValidator, SetValidator
         Rule builder
      IRuleBuilderInitial (interface)
         Rule builder that starts the chain
      IRuleBuilderInitialCollection (interface)
         Rule builder that starts the chain for a child collection
      IRuleBuilderOptions (interface):  DependentRules
         Rule builder
      … and 17 more (use --format json for the full surface)
   FluentValidation.Resources
      ILanguageManager (interface):  GetString
         Allows the default error message translations to be managed.
      LanguageManager (class):  AddTranslation, Clear, GetString
         Allows the default error message translations to be managed.
   FluentValidation.Results
      ValidationFailure (class):  ToString, ValidationFailure
         Defines a validation failure
      ValidationResult (class):  ToDictionary, ToString, ValidationResult
         The result of running a validator
   FluentValidation.TestHelper
      ITestValidationContinuation (interface)
      ITestValidationWith (interface)
      TestValidationResult (class):  ShouldHaveValidationErrorFor, ShouldHaveValidationErrors, ShouldNotHaveAnyValidationErrors, ShouldNotHaveValidationErrorFor, TestValidationResult
      ValidationTestException (class):  ValidationTestException
      ValidationTestExtension (class):  Only, ShouldHaveChildValidator, TestValidate, TestValidateAsync, When, WhenAll, WithCustomState, WithErrorCode, WithErrorMessage, WithMessageArgument, WithSeverity, WithoutCustomState, WithoutErrorCode, WithoutErrorMessage, WithoutSeverity
   FluentValidation.Validators
      AbstractComparisonValidator (class):  GetComparisonValue, IsValid
         Base class for all comparison validators
      AspNetCoreCompatibleEmailValidator (class):  IsValid
      AsyncPredicateValidator (class):  AsyncPredicateValidator, IsValidAsync
         Asynchronous custom validator
      AsyncPropertyValidator (class):  GetDefaultMessageTemplate, IsValidAsync
      ChildValidatorAdaptor (class):  ChildValidatorAdaptor, GetValidator, IsValid, IsValidAsync
      CreditCardValidator (class):  IsValid
         Ensures that the property value is a valid credit card number.
      EmailValidator (class):  IsValid
      EmptyValidator (class):  IsValid
      EnumValidator (class):  IsValid
      EqualValidator (class):  EqualValidator, IsValid
      ExactLengthValidator (class):  ExactLengthValidator
      ExclusiveBetweenValidator (class):  ExclusiveBetweenValidator
         Performs range validation where the property value must be between the two specified values (exclusive).
      … and 42 more (use --format json for the full surface)
   INTERNAL  (15 types in *.Internal — available on request)

CONSUMER PATHS
   wire into DI  →  ServiceCollectionExtensions.AddValidatorsFromAssemblies(...)
   wire into DI  →  ServiceCollectionExtensions.AddValidatorsFromAssembly(...)
   wire into DI  →  ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining(...)
   extend  →  derive AbstractValidator
   contract  →  implement IPropertyValidator
   contract  →  implement IValidationRule

PACKAGES
   Other:  Microsoft.Extensions.DependencyInjection.Abstractions 2.1.0, Microsoft.NETFramework.ReferenceAssemblies 1.0.3, System.Threading.Tasks.Extensions 4.5.4, Zomp.SyncMethodGenerator 1.3.8-beta

→ drill in:  --focus "<TypeName>"   (e.g. --focus ServiceCollectionExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 174ms |
| GenericExtraction | 675ms |
| SignalSealing | 0ms |
| SpecificExtraction | 571ms |
| Compression | 72ms |
| **Total** | **9485ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 669ms | 371 | 4 |
| ProgramCsFlowExtractor | 609ms | 0 | 4 |
| DiRegistrationExtractor | 605ms | 0 | 4 |
| SourceBodyExtractor | 384ms | 0 | 0 |
| CallGraphExtractor | 265ms | 0 | 0 |
| BodyFactsExtractor | 199ms | 0 | 0 |
| InMemoryEventBusExtractor | 172ms | 0 | 2 |
| SolutionDiscovery | 79ms | 0 | 0 |
| IndirectWiringDetector | 61ms | 0 | 2 |
| ProjectStructure | 45ms | 0 | 0 |
| FileTreeExtractor | 43ms | 0 | 0 |
| DependencyExtractor | 31ms | 0 | 0 |
| LayerClassifier | 29ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 476 | 170 |
| Resolves | 32 | 28 |

_218 files · 4 projects_
