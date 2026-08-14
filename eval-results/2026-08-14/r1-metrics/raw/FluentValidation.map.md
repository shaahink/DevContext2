LIBRARY  FluentValidation     (96 public types)

ENTRY API
   register  ServiceCollectionExtensions.AddValidatorsFromAssemblies   
(ServiceCollectionExtensions.cs)
      Adds all validators in specified assemblies
   register  ServiceCollectionExtensions.AddValidatorsFromAssembly   
(ServiceCollectionExtensions.cs)
      Adds all validators in specified assembly
   register  ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining   
(ServiceCollectionExtensions.cs)
      Adds all validators in the assembly of the specified type
   derive    AbstractValidator   (AbstractValidator.cs)
      Base class for object validators.
   implement IPropertyValidator   (IPropertyValidator.cs)
      A custom property validator.
   derive    InlineValidator   (InlineValidator.cs)
      Validator implementation that allows rules to be defined without 
inheriting from AbstractValidator.
   derive    PropertyValidator   (PropertyValidator.cs)
   extend    DefaultValidatorExtensions   (DefaultValidatorExtensions.cs)
      Extension methods that provide the default set of validators.
   extend    DefaultValidatorOptions   (DefaultValidatorOptions.cs)
      Default options that can be used to configure a validator.
   extend    ValidationTestExtension   (ValidatorTestExtensions.cs)

ABSTRACTIONS
   AbstractValidator (class)  - 56 implementors
   PropertyValidator (class)  - 20 implementors
   IPropertyValidator (interface)  - 12 implementors
   InlineValidator (class)  - 8 implementors
   IValidationRule (interface)  - 6 implementors
   IComparisonValidator (interface)  - 5 implementors
   AbstractComparisonValidator (class)  - 4 implementors
   ILengthValidator (interface)  - 4 implementors
   IRuleBuilder (interface)  - 4 implementors
   IAsyncPropertyValidator (interface)  - 3 implementors

PUBLIC SURFACE
   FluentValidation
      AbstractValidator (class):  CanValidateInstancesOfType, CreateDescriptor, 
GetEnumerator, Include, RuleFor, RuleForEach, RuleSet, Unless, UnlessAsync, 
Validate, ValidateAsync, When, WhenAsync
         Base class for object validators.
      AssemblyScanResult (class):  AssemblyScanResult
         Result of performing a scan.
      AssemblyScanner (class):  AssemblyScanner, FindValidatorsInAssemblies, 
FindValidatorsInAssembly, FindValidatorsInAssemblyContaining, ForEach, 
GetEnumerator
         Class that can be used to find all the validators from a collection of 
types.
      AsyncValidatorInvokedSynchronouslyException (class)
         This exception is thrown when an asynchronous validator is executed 
synchronously.
      DefaultValidatorExtensions (class):  ChildRules, CreditCard, Custom, 
CustomAsync, EmailAddress, Empty, Equal, ExclusiveBetween, ForEach, GreaterThan,
GreaterThanOrEqualTo, InclusiveBetween, IsEnumName, IsInEnum, Length
         Extension methods that provide the default set of validators.
      DefaultValidatorOptions (class):  Cascade, Configurable, Configure, 
OverrideIndexer, OverridePropertyName, Unless, UnlessAsync, When, WhenAsync, 
Where, WhereAsync, WithErrorCode, WithMessage, WithName, WithSeverity
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
      . and 20 more (the structured surface lists them all)
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
         Base class for all comparison validators
      AspNetCoreCompatibleEmailValidator (class):  IsValid
      AsyncPredicateValidator (class):  AsyncPredicateValidator, IsValidAsync
         Asynchronous custom validator
      AsyncPropertyValidator (class):  GetDefaultMessageTemplate, IsValidAsync
      ChildValidatorAdaptor (class):  ChildValidatorAdaptor, GetValidator, 
IsValid, IsValidAsync
      CreditCardValidator (class):  IsValid
         Ensures that the property value is a valid credit card number.
      EmailValidator (class):  IsValid
      EmptyValidator (class):  IsValid
      EnumValidator (class):  IsValid
      EqualValidator (class):  EqualValidator, IsValid
      ExactLengthValidator (class):  ExactLengthValidator
      ExclusiveBetweenValidator (class):  ExclusiveBetweenValidator
         Performs range validation where the property value must be between the 
two specified values (exclusive).
      . and 43 more (the structured surface lists them all)
   INTERNAL  (16 types in *.Internal - available on request)

CONSUMER PATHS
   wire into DI    ServiceCollectionExtensions.AddValidatorsFromAssemblies(...)
   wire into DI    ServiceCollectionExtensions.AddValidatorsFromAssembly(...)
   wire into DI    
ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining(...)
   extend    derive AbstractValidator
   contract    implement IPropertyValidator
   extend    derive InlineValidator

PACKAGES
   Other:  Microsoft.Extensions.DependencyInjection.Abstractions 2.1.0, 
Zomp.SyncMethodGenerator 1.6.17

 drill in:  trace a focused type   (e.g. trace ServiceCollectionExtensions)

