# FluentValidation — Library Surface BENCHMARK (hand-built target)

> The **target** DevContext should produce for FluentValidation, hand-built from a source read at pinned
> SHA `943979089b55664ceb8390547ea1eb84ee99252a`. This is the golden we score the CLI against (companion
> to `IDEAL-OUTPUT-TARGET.md §4`). It is the *FluentValidation instance* of the canonical Library
> Benchmark Format — adding a new library = re-author these sections from its source.
>
> **Build-free constraint:** every line below is derivable from syntax + `///` doc trivia only. Members
> inherited from *external* base types are NOT enumerated (shown as `+ inherited from <Base>`), and
> overload-dependent facts may be marked `[approx]`. The benchmark must never demand what a build-free
> engine cannot know.
>
> Shape exercised: **abstract-seat + extension-DI + fluent DSL** (the dominant modern-lib shape #1).

```
LIBRARY  FluentValidation   (validation library · 107 public types / 371 total)
         TFMs: multi-targeted (netstandard2.0 + net8.0)            archetype: Library

ENTRY API  (how you use it — ranked front doors, with /// summary + file:line)
   1. Define rules   AbstractValidator<T>  (abstract — derive this)   src/FluentValidation/AbstractValidator.cs:36
                        "Base class for object validators."
                        RuleFor(x => x.Prop) · RuleForEach(x => x.Items) · RuleSet(name, …) · When/Unless
   2. Rule DSL       DefaultValidatorExtensions  (extends IRuleBuilder<T,TProperty>)   src/FluentValidation/DefaultValidatorExtensions.cs:35
                        "Extension methods that provide the default set of validators."
                        .NotEmpty() · .NotNull() · .EmailAddress() · .Length(min,max) · .GreaterThan(v)
                        .Must(predicate) · .WithMessage(s) · .WithErrorCode(c)   — fluent, chainable
   3. Register (DI)  ServiceCollectionExtensions.AddValidatorsFromAssembly(this IServiceCollection)   src/FluentValidation.DependencyInjectionExtensions/ServiceCollectionExtensions.cs:53
                        "Adds all validators in specified assembly"
                        + AddValidatorsFromAssemblies(...) · AddValidatorsFromAssemblyContaining<T>(...)
   4. Execute        IValidator<T>.Validate(T) / ValidateAsync(T)   src/FluentValidation/IValidator.cs
                        TestHelper: validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Prop)

ABSTRACTIONS / SEATS  (what you implement / derive — implementor counts)
   AbstractValidator<T> : IValidator<T>          derive-seat (the primary consumer extension point)
   IValidator / IValidator<T>                    the validator contract
   IPropertyValidator<T,TProperty>               custom-validator seat  (impls: NotEmptyValidator, EmailValidator, LengthValidator, …)
   IValidatorSelector · ILanguageManager         pluggable strategy seats

PUBLIC SURFACE  (by namespace · signature + /// one-liner · [obsolete] flagged · internals demoted)
   FluentValidation              AbstractValidator<T>, IValidator<T>, DefaultValidatorExtensions,
                                 ServiceCollectionExtensions, ValidationContext, ValidationException,
                                 InlineValidator<T>, ValidatorOptions, …
   FluentValidation.Results      ValidationResult, ValidationFailure
   FluentValidation.Validators   NotEmptyValidator, EmailValidator, LengthValidator, RegularExpressionValidator, … (property validators)
   FluentValidation.Resources    ILanguageManager, LanguageManager
   FluentValidation.TestHelper   TestValidationResult, ValidationTestExtension   [consumer test support — grouped, not mixed into the core API]
   FluentValidation.Internal     [internal-by-convention — collapsed]  AccessorCache, MessageFormatter, PropertyChain, RuleComponent, … (available on request)

CONSUMER PATHS
   "validate a model"  → derive AbstractValidator<T> → RuleFor(x => x.X).NotEmpty() → validator.Validate(model)
   "wire into DI"      → services.AddValidatorsFromAssembly(typeof(Startup).Assembly)
   "custom rule"       → .Must(predicate)  /  implement IPropertyValidator<T,TProperty>
   "unit-test rules"   → validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.X)

PACKAGES  (runtime deps only — test/benchmark deps excluded)
   DI extensions:  Microsoft.Extensions.DependencyInjection.Abstractions
```

## What makes this the target (vs today's flat dump)

1. **ENTRY API ranked** — the 4 things a consumer touches (derive `AbstractValidator<T>`, the `.NotEmpty()`
   DSL, `AddValidatorsFromAssembly`, `Validate`) lead, each with its `///` summary and `file:line` — instead
   of an alphabetical wall of 107 types where `AbstractValidator` sits between `AssemblyScanner` and
   `AsyncValidatorInvokedSynchronouslyException`.
2. **Real extension methods** — `DefaultValidatorExtensions` (the `.NotEmpty()/.EmailAddress()` rule DSL,
   `this IRuleBuilder<T,TProperty>`) is surfaced as the rule DSL; today's name-prefix heuristic misses it
   entirely (no `Add`/`With`/`Use` prefix) while listing TestHelper `With*` noise.
3. **`///` summaries** — the doc comment *is* the contract for a library; bare member names are not enough.
4. **Abstractions/seats** — `AbstractValidator<T>`/`IValidator<T>`/`IPropertyValidator<>` named as the
   derive/implement points, with implementor counts.
5. **Internal-by-convention demoted, TestHelper grouped, test/benchmark packages excluded** — the surface
   reflects the *supported public API*, not every `public` type in the assembly.

## Acceptance gates (encoded in `eval/expectations/fluentvalidation.json`)
`archetype=Library` · `ENTRY API` present · `ABSTRACTIONS` present · `DefaultValidatorExtensions` surfaced as
extension DSL · `AddValidatorsFromAssembly` front door · no test/benchmark packages · no `[approx]` on the
declared surface · deterministic (byte-identical re-run). Soft (`aspirational`) until the engine delta lands,
then flipped `expected`.
