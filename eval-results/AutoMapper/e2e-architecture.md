## DevContext -- Architecture Overview on project

**Architecture**: Not detected
**Signals**: automapper
**Projects**: 6 -- AutoMapper, AutoMapper.DI.Tests, Benchmark, AutoMapper.IntegrationTests, TestApp, AutoMapper.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 167 in output

---
## Architecture overview

- AutoMapper
- AutoMapper.DI.Tests
- Benchmark
- AutoMapper.IntegrationTests
- TestApp
- AutoMapper.UnitTests

## Endpoints

No endpoints detected.

## Types by namespace

- **AddingConfigurationForNonMatchingDestinationMember** — 1 types
  Public: AddingConfigurationForNonMatchingDestinationMemberBug
- **AttributeBasedMaps** — 4 types
  Public: CustomConverter, ParentIdToChildDtoListConverter, IDest, MyValueResolver
- **AutoMapper** — 34 types (33 public)
  Public (33): AutoMapperConfigurationException, IMappingOperationOptions, IMemberValueResolver, IProfileExpression, IProfileConfiguration, DuplicateTypeMapConfigurationException, PathMap, IMapperConfigurationExpression, IMapperBase, MapperConfigurationExpression ...
- **AutoMapper.Configuration** — 15 types
  Public (15): ISourceMemberConfigurationExpression, ISourceMemberConfiguration, MappingExpression, IPathConfigurationExpression, MappingExpressionBase, MemberConfigurationExpression, IPropertyMapConfiguration, ConditionParameters, ICtorParamConfigurationExpression, TypeMapConfiguration ...
- **AutoMapper.Configuration.Annotations** — 4 types
  Public: MappingOrderAttribute, NullSubstituteAttribute, IgnoreAttribute, SourceMemberAttribute
- **AutoMapper.Configuration.Conventions** — 5 types
  Public: ConventionsNameSplitMember, ReplaceName, MemberConfiguration, ISourceToDestinationNameMapper, PrePostfixName
- **AutoMapper.Execution** — 16 types
  Public (16): ProxyGenerator, ConvertParameterReplaceVisitor, ExpressionResolver, TypeMapPlanBuilder, FuncResolver, ExpressionTypeConverter, ProxyBase, ValueResolverConfig, ClassTypeConverter, ReplaceVisitor ...
- **AutoMapper.Features** — 5 types
  Public: Features, IRuntimeFeature, IGlobalFeature, FeatureExtensions, IMappingFeature
- **AutoMapper.Internal** — 12 types
  Public (12): ConstructorParameters, PrimitiveHelper, GenericMethod, TypeDetails, TypePair, ReflectionHelper, InternalApi, LockingConcurrentDictionary, TypeExtensions, IGlobalConfiguration ...
- **AutoMapper.Internal.Mappers** — 12 types
  Public (12): CollectionMapper, ToStringDictionaryMapper, ConvertMapper, StringToEnumMapper, ToDynamicMapper, NullableDestinationMapper, ParseStringMapper, ConstructorMapper, KeyValueMapper, ToStringMapper ...
- **AutoMapper.QueryableExtensions** — 1 types
  Public: Extensions
- **AutoMapper.QueryableExtensions.Impl** — 12 types
  Public (12): SubQueryPath, FirstPassLetPropertyMaps, ParameterVisitor, QueryExpressions, LetPropertyMaps, ReplaceMemberAccessesVisitor, EnumerableProjectionMapper, ProjectionBuilder, AssignableProjectionMapper, IProjectionBuilder ...
- **AutoMapperIssue** — 2 types
  Public: TestProblem, IntToEntityConverter
- **Benchmark** — 1 types
  Public: HiPerfTimer
- **Benchmark.Flattening** — 4 types
  Public: Foo, ComplexTypeMapper, Config, Dto11
- **CircularReferences** — 1 types
  Public: A
- **ConditionBug** — 2 types
  Public: PrimitiveExample, Example
- **ConditionPropertyBug** — 1 types
  Public: Example
- **DestinationCtorCalledTwice** — 2 types
  Public: Destination, Bug
- **Dictionaries** — 2 types
  Public: GenericWrappedDictionary, DataDictionary
- **DuplicateValuesBug** — 3 types
  Public: DuplicateValuesIssue, SourceObject, DestObject
- **global** — 3 types
  Public: DependencyResolver, FooService, ISomeService
- **InterfaceMultipleInheritance** — 5 types
  Public: InterfaceMultipleInheritanceBug1016, MapFrom, IMapFromElementDerived1, IMapFromElement, IMapFrom
- **NestedContainers** — 1 types
  Public: BarResolver
- **ParentChildResolversBug** — 2 types
  Public: Resolver, ParentResolver
- **Profiles** — 1 types
  Public: Dto
- **Regression** — 4 types
  Public: TestEnumerable, Chris_bennages_nullable_datetime_issue, People, ITestDomainItem
- **SetterOnlyBug** — 1 types
  Public: Desitination
- **SourceValueExceptionConditionPropertyBug** — 1 types
  Public: Source
- **ValueTransformers** — 5 types
  Public: DifferentProfiles, StackingRootConfigAndProfileTransform, StackingRootAndProfileAndMemberConfig, BasicTransforming, StackingTransformers
- **ValueTransformerTests** — 5 types
  Public: BasicTransforming, StackingRootAndProfileAndMemberConfig, StackingRootConfigAndProfileTransform, StackingTransformers, DifferentProfiles

---
*Generated in 10.4ms | 2713 types (167 active, 2546 pruned) | Compression: TrivialMemberCompressor(−21%) · BoilerplateCompressor(−1%) · StructuralDeduplicator(−27%) | Schema v2.0*
