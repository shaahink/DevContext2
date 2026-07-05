# REPORT
**FluentValidation**

Style: Unknown
_2 projects  ·  net10.0, net8.0 + fluentvalidation_

## Stats

| Metric | Value |
|--------|-------|
| Files | 218 |
| Projects | 4 |
| Nodes | 195 |
| Edges | 32 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 12% |
| Analyzed in | 2.3s |

## Top Flows

_No entries found._

## Insights

_2 info · 3 notable_

### **NOTABLE**: ViewModel-View: 1 VMs + 0 Views (0 call edges)
*(Wiring)*

- 1 ViewModels

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- IMaximumLengthValidator
- WelshLanguage
- ChineseTraditionalLanguage
- EventDisposable
- GreaterThanValidator

### **NOTABLE**: Extension seats: AddValidatorsFromAssembly (3 impls)
*(Wiring)*

- AddValidatorsFromAssembly (3 impls)

### _INFO_: Public surface: 44 interfaces, 227 classes (273 total public types)
*(Shape)*

- 44 interfaces
- 227 classes

### _INFO_: Most depended-upon: FluentValidation (2 dependents) · FluentValidation.DependencyInjectionExtensions (1 dependents)
*(Topology)*

- FluentValidation (2 dependents)
- FluentValidation.DependencyInjectionExtensions (1 dependents)

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
      IRuleBuilderOptionsConditions (interface):  DependentRules
         Rule builder (for validators that only support conditions, but no other options)
      IValidationContext (interface)
      IValidationRule (interface):  AddAsyncValidator, AddValidator, ApplyAsyncCondition, ApplyCondition, ApplySharedAsyncCondition, ApplySharedCondition, GetDisplayName, GetPropertyValue, SetDisplayName, TryGetPropertyValue
      IValidator (interface):  CanValidateInstancesOfType, CreateDescriptor, Validate, ValidateAsync
         Defines a validator for a particular type.
      IValidatorDescriptor (interface):  GetMembersWithValidators, GetName, GetRulesForMember, GetValidatorsForMember
         Provides metadata about a validator.
      IValidatorFactory (interface):  GetValidator
         Gets validators for a particular type.
      InlineValidator (class):  Add
         Validator implementation that allows rules to be defined without inheriting from AbstractValidator.
      RulesetMetadata (class):  RulesetMetadata
         Information about rulesets
      ServiceCollectionExtensions (class):  AddValidatorsFromAssemblies, AddValidatorsFromAssembly, AddValidatorsFromAssemblyContaining
      ServiceProviderValidatorFactory (class):  CreateInstance, ServiceProviderValidatorFactory
         Validator factory implementation that uses the asp.net service provider to construct validators.
      ValidationContext (class):  AddFailure, CloneForChildValidator, CreateWithOptions, GetFromNonGenericContext, ValidationContext
         Validation context
      ValidationException (class):  GetObjectData, ValidationException
         An exception that represents failed validation
      ValidatorConfiguration (class):  DefaultDisplayNameResolver, DefaultErrorCodeResolver, DefaultPropertyNameResolver
         Configuration options for validators.
      ValidatorDescriptor (class):  GetMembersWithValidators, GetName, GetRulesByRuleset, GetRulesForMember, GetValidatorsForMember, ValidatorDescriptor
         Used for providing metadata about a validator.
      ValidatorFactoryBase (class):  CreateInstance, GetValidator
         Factory for creating validators
      ValidatorOptions (class)
         Validator runtime options
      ValidatorSelectorOptions (class)
         ValidatorSelector options
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
      GreaterThanOrEqualValidator (class):  GreaterThanOrEqualValidator, IsValid
      GreaterThanValidator (class):  GreaterThanValidator, IsValid
      IAsyncPropertyValidator (interface):  IsValidAsync
      IBetweenValidator (interface)
      IChildValidatorAdaptor (interface)
         Indicates that this validator wraps another validator.
      IComparisonValidator (interface)
         Defines a comparison validator
      ICreditCardValidator (interface)
      IEmailValidator (interface)
      IEnumValidator (interface)
      IEqualValidator (interface)
      IExactLengthValidator (interface)
      IGreaterThanOrEqualValidator (interface)
      IInclusiveBetweenValidator (interface)
      ILengthValidator (interface)
      ILessThanOrEqualValidator (interface)
      IMaximumLengthValidator (interface)
      IMinimumLengthValidator (interface)
      INotEmptyValidator (interface)
      INotNullValidator (interface)
      INullValidator (interface)
      IPredicateValidator (interface)
      IPropertyValidator (interface):  GetDefaultMessageTemplate, IsValid
      IRegularExpressionValidator (interface)
      InclusiveBetweenValidator (class):  InclusiveBetweenValidator
         Performs range validation where the property value must be between the two specified values (inclusive).
      LengthValidator (class):  IsValid, LengthValidator
      LessThanOrEqualValidator (class):  IsValid, LessThanOrEqualValidator
      LessThanValidator (class):  IsValid, LessThanValidator
      MaximumLengthValidator (class):  MaximumLengthValidator
      MinimumLengthValidator (class):  MinimumLengthValidator
      NoopPropertyValidator (class):  IsValid
      NotEmptyValidator (class):  IsValid
      NotEqualValidator (class):  IsValid, NotEqualValidator
      NotNullValidator (class):  IsValid
      NullValidator (class):  IsValid
      PolymorphicValidator (class):  Add, GetValidator, PolymorphicValidator
         Performs runtime checking of the value being validated, and passes validation off to a subclass validator.
      PrecisionScaleValidator (class):  IsValid, PrecisionScaleValidator
         Allows a decimal to be validated for scale and precision.
      PredicateValidator (class):  IsValid, PredicateValidator
      PropertyValidator (class):  GetDefaultMessageTemplate, IsValid
      RangeValidator (class):  IsValid, RangeValidator
         Base class for range validation.
      RangeValidatorFactory (class):  CreateExclusiveBetween, CreateInclusiveBetween
      RegularExpressionValidator (class):  IsValid, RegularExpressionValidator
      StringEnumValidator (class):  IsValid, StringEnumValidator
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
| DiscoveryAndCacheWarmup | 199ms |
| GenericExtraction | 1267ms |
| SignalSealing | 0ms |
| SpecificExtraction | 301ms |
| Compression | 51ms |
| **Total** | **2333ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1263ms | 371 | 4 |
| DiRegistrationExtractor | 1261ms | 0 | 4 |
| SourceBodyExtractor | 193ms | 0 | 0 |
| ProgramCsFlowExtractor | 168ms | 0 | 0 |
| ProjectStructure | 144ms | 0 | 0 |
| CallGraphExtractor | 119ms | 0 | 0 |
| InMemoryEventBusExtractor | 104ms | 0 | 2 |
| SolutionDiscovery | 35ms | 0 | 0 |
| IndirectWiringDetector | 27ms | 0 | 2 |
| DependencyExtractor | 18ms | 0 | 0 |
| LayerClassifier | 18ms | 0 | 0 |
| FileTreeExtractor | 16ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |
| AwsLambdaExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Resolves | 32 | 28 |

_218 files · 0 projects_
