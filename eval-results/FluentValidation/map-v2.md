Overview map (no focus).
Analyzing project...

LIBRARY  FluentValidation     (107 public types)

STYLE  Unknown

PUBLIC SURFACE
   FluentValidation
      AbstractValidator (class):  CanValidateInstancesOfType, CreateDescriptor, 
GetEnumerator, Include, RuleFor, RuleForEach, RuleSet, Unless, UnlessAsync, 
Validate, ValidateAsync, When, WhenAsync
      AssemblyScanResult (class):  AssemblyScanResult
      AssemblyScanner (class):  AssemblyScanner, FindValidatorsInAssemblies, 
FindValidatorsInAssembly, FindValidatorsInAssemblyContaining, ForEach, 
GetEnumerator
      AsyncValidatorInvokedSynchronouslyException (class)
      DefaultValidatorExtensions (class):  ChildRules, CreditCard, Custom, 
CustomAsync, EmailAddress, Empty, Equal, ExclusiveBetween, ForEach, GreaterThan,
GreaterThanOrEqualTo, InclusiveBetween, IsEnumName, IsInEnum, Length
      DefaultValidatorOptions (class):  Cascade, Configurable, Configure, 
OverrideIndexer, OverridePropertyName, Unless, UnlessAsync, When, WhenAsync, 
Where, WhereAsync, WithErrorCode, WithMessage, WithName, WithSeverity
      ICollectionRule (interface)
      IConditionBuilder (interface):  Otherwise
      IRuleBuilder (interface):  SetAsyncValidator, SetValidator
      IRuleBuilderInitial (interface)
      IRuleBuilderInitialCollection (interface)
      IRuleBuilderOptions (interface):  DependentRules
      IRuleBuilderOptionsConditions (interface):  DependentRules
      IValidationContext (interface)
      IValidationRule (interface):  AddAsyncValidator, AddValidator, 
ApplyAsyncCondition, ApplyCondition, ApplySharedAsyncCondition, 
ApplySharedCondition, GetDisplayName, GetPropertyValue, SetDisplayName, 
TryGetPropertyValue
      IValidator (interface):  CanValidateInstancesOfType, CreateDescriptor, 
Validate, ValidateAsync
      IValidatorDescriptor (interface):  GetMembersWithValidators, GetName, 
GetRulesForMember, GetValidatorsForMember
      IValidatorFactory (interface):  GetValidator
      InlineValidator (class):  Add
      RulesetMetadata (class):  RulesetMetadata
      ServiceCollectionExtensions (class):  AddValidatorsFromAssemblies, 
AddValidatorsFromAssembly, AddValidatorsFromAssemblyContaining
      ServiceProviderValidatorFactory (class):  CreateInstance, 
ServiceProviderValidatorFactory
      ValidationContext (class):  AddFailure, CloneForChildValidator, 
CreateWithOptions, GetFromNonGenericContext, ValidationContext
      ValidationException (class):  GetObjectData, ValidationException
      ValidatorConfiguration (class):  DefaultDisplayNameResolver, 
DefaultErrorCodeResolver, DefaultPropertyNameResolver
      ValidatorDescriptor (class):  GetMembersWithValidators, GetName, 
GetRulesByRuleset, GetRulesForMember, GetValidatorsForMember, 
ValidatorDescriptor
      ValidatorFactoryBase (class):  CreateInstance, GetValidator
      ValidatorOptions (class)
      ValidatorSelectorOptions (class)
   FluentValidation.Internal
      AccessorCache (class):  Clear, GetCachedAccessor
      DefaultValidatorSelector (class):  CanExecute
      EventDisposable (class):  Dispose, EventDisposable
      Extensions (class):  GetMember
      IIncludeRule (interface)
      IMessageBuilderContext (interface):  GetDefaultMessage
      IRuleComponent (interface):  ApplyAsyncCondition, ApplyCondition, 
GetUnformattedErrorMessage, SetErrorMessage
      IValidatorSelector (interface):  CanExecute
      MemberNameValidatorSelector (class):  CanExecute, 
MemberNameValidatorSelector, MemberNamesFromExpressions
      MessageBuilderContext (class):  GetDefaultMessage, MessageBuilderContext
      MessageFormatter (class):  AppendArgument, AppendPropertyName, 
AppendPropertyValue, BuildMessage
      PropertyChain (class):  Add, AddIndexer, BuildPropertyName, 
BuildPropertyPath, FromExpression, GetParentChain, IsChildChainOf, 
PropertyChain, ToString
      RuleComponent (class):  ApplyAsyncCondition, ApplyCondition, 
GetErrorMessage, GetUnformattedErrorMessage, SetErrorMessage
      RulesetValidatorSelector (class):  CanExecute, RulesetValidatorSelector
      ValidationStrategy (class):  IncludeAllRuleSets, IncludeProperties, 
IncludeRuleSets, IncludeRulesNotInRuleSet, ThrowOnFailures, UseCustomSelector
   FluentValidation.Resources
      ILanguageManager (interface):  GetString
      LanguageManager (class):  AddTranslation, Clear, GetString
   FluentValidation.Results
      ValidationFailure (class):  ToString, ValidationFailure
      ValidationResult (class):  ToDictionary, ToString, ValidationResult
   FluentValidation.TestHelper
      ITestValidationContinuation (interface)
      ITestValidationWith (interface)
      TestValidationResult (class):  ShouldHaveValidationErrorFor, 
ShouldHaveValidationErrors, ShouldNotHaveAnyValidationErrors, 
ShouldNotHaveValidationErrorFor, TestValidationResult
      ValidationTestException (class):  ValidationTestException
      ValidationTestExtension (class):  Only, ShouldHaveChildValidator, 
TestValidate, TestValidateAsync, When, WhenAll, WithCustomState, WithErrorCode, 
WithErrorMessage, WithMessageArgument, WithSeverity, WithoutCustomState, 
WithoutErrorCode, WithoutErrorMessage, WithoutSeverity
   FluentValidation.Validators
      AbstractComparisonValidator (class):  GetComparisonValue, IsValid
      AspNetCoreCompatibleEmailValidator (class):  IsValid
      AsyncPredicateValidator (class):  AsyncPredicateValidator, IsValidAsync
      AsyncPropertyValidator (class):  GetDefaultMessageTemplate, IsValidAsync
      ChildValidatorAdaptor (class):  ChildValidatorAdaptor, GetValidator, 
IsValid, IsValidAsync
      CreditCardValidator (class):  IsValid
      EmailValidator (class):  IsValid
      EmptyValidator (class):  IsValid
      EnumValidator (class):  IsValid
      EqualValidator (class):  EqualValidator, IsValid
      ExactLengthValidator (class):  ExactLengthValidator
      ExclusiveBetweenValidator (class):  ExclusiveBetweenValidator
      GreaterThanOrEqualValidator (class):  GreaterThanOrEqualValidator, IsValid
      GreaterThanValidator (class):  GreaterThanValidator, IsValid
      IAsyncPropertyValidator (interface):  IsValidAsync
      IBetweenValidator (interface)
      IChildValidatorAdaptor (interface)
      IComparisonValidator (interface)
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
      PrecisionScaleValidator (class):  IsValid, PrecisionScaleValidator
      PredicateValidator (class):  IsValid, PredicateValidator
      PropertyValidator (class):  GetDefaultMessageTemplate, IsValid
      RangeValidator (class):  IsValid, RangeValidator
      RangeValidatorFactory (class):  CreateExclusiveBetween, 
CreateInclusiveBetween
      RegularExpressionValidator (class):  IsValid, RegularExpressionValidator
      StringEnumValidator (class):  IsValid, StringEnumValidator

EXTENSION POINTS
   DefaultValidatorOptions.Configure
   DefaultValidatorOptions.WithErrorCode
   DefaultValidatorOptions.WithMessage
   DefaultValidatorOptions.WithName
   DefaultValidatorOptions.WithSeverity
   DefaultValidatorOptions.WithState
   ServiceCollectionExtensions.AddValidatorsFromAssemblies
   ServiceCollectionExtensions.AddValidatorsFromAssembly
   ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining
   ValidationTestExtension.WithCustomState
   ValidationTestExtension.WithErrorCode
   ValidationTestExtension.WithErrorMessage
   ValidationTestExtension.WithMessageArgument
   ValidationTestExtension.WithSeverity
   ValidationTestExtension.WithoutCustomState
   ValidationTestExtension.WithoutErrorCode
   ValidationTestExtension.WithoutErrorMessage
   ValidationTestExtension.WithoutSeverity

PACKAGES
   Testing:  Bogus 30.0.2, xunit 2.2.0, xunit.runner.visualstudio 2.2.0
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  BenchmarkDotNet 0.12.1, Microsoft.Extensions.DependencyInjection 
8.0.0, Microsoft.Extensions.DependencyInjection.Abstractions 2.1.0, 
Microsoft.NET.Test.Sdk 16.0.1, Microsoft.NETFramework.ReferenceAssemblies 1.0.3,
Microsoft.TestPlatform.TestHost 16.0.1, System.ComponentModel.Annotations 4.4.1,
System.Threading.Tasks.Extensions 4.5.4 … (9 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus Mapper)

analyzed 218 files · 195 nodes · 32 edges · 0 entries · ~2385 tokens · 2.5s 
stage2 ×2.2 stage3 ×1.6
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │ FluentValidation.sln │
│   Time   │        2577ms        │
│  Tokens  │ ~2385 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.132 │
╰──────────┴──────────────────────╯
