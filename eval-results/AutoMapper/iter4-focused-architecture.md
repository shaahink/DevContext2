## DevContext -- Architecture Overview on project

**Architecture**: Not detected
**Signals**: none
**Projects**: 6 -- AutoMapper, AutoMapper.DI.Tests, Benchmark, AutoMapper.IntegrationTests, TestApp, AutoMapper.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 145 in output

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

## Related types grouped by layer

- **Unknown**: CollectionMapper, SubQueryPath, AutoMapperConfigurationException, Extensions, DifferentProfiles, ToStringDictionaryMapper, ConvertMapper, IMappingOperationOptions, Dto, IMemberValueResolver, ProxyGenerator, IProfileExpression, FirstPassLetPropertyMaps, StackingTypeMapAndRootAndProfileAndMemberConfig, ConvertParameterReplaceVisitor, ConventionsNameSplitMember, ExpressionResolver, ParameterVisitor, StringToEnumMapper, ConstructorParameters, CustomConverter, TypeMapPlanBuilder, ToDynamicMapper, QueryExpressions, ReplaceName, BarResolver, PrimitiveHelper, IProfileConfiguration, TestProblem, GenericWrappedDictionary, NullableDestinationMapper, LetPropertyMaps, DuplicateValuesIssue, AddingConfigurationForNonMatchingDestinationMemberBug, ISourceMemberConfigurationExpression, Foo, GenericMethod, FuncResolver, InterfaceMultipleInheritanceBug1016, ParentIdToChildDtoListConverter, DuplicateTypeMapConfigurationException, Example, IDest, ExpressionTypeConverter, ProxyBase, PrimitiveExample, ParseStringMapper, PathMap, TypeDetails, Features, BasicTransforming, ValueResolverConfig, IInternalRuntimeMapper, ISourceMemberConfiguration, Resolver, IMapperConfigurationExpression, TestEnumerable, MappingOrderAttribute, IMapperBase, MemberConfiguration, SourceObject, MapperConfigurationExpression, TypePair, DependencyResolver, ReflectionHelper, ConstructorMapper, Mapper, StackingRootAndProfileAndMemberConfig, Chris_bennages_nullable_datetime_issue, ReplaceMemberAccessesVisitor, MemberMap, IRuntimeFeature, DataDictionary, FooService, MemberMapDetails, People, Desitination, KeyValueMapper, ClassTypeConverter, HiPerfTimer, InternalApi, MappingExpression, ICondition, Example, ComplexTypeMapper, Config, LockingConcurrentDictionary, IPathConfigurationExpression, EnumerableProjectionMapper, ExactMatchNamingConvention, IValueConverter, StackingRootConfigAndProfileTransform, AutoMapAttribute, LazyValue, MappingExpressionBase, ReplaceVisitor, Dto11, ProjectionBuilder, MemberConfigurationExpression, ConstructorMap, IMapper, AssignableProjectionMapper, IPreCondition, ISourceToDestinationNameMapper, IGlobalFeature, ToStringMapper, NullSubstituteAttribute, IPropertyMapConfiguration, IgnoreAttribute, MapFrom, IMapFromElementDerived1, FromStringDictionaryMapper, IValueResolver, FeatureExtensions, StackingRootConfigAndProfileTransform, SourceMemberAttribute, IProjectionMemberConfiguration, IProjectionBuilder, ConditionParameters, TypeConverter, MappingOperationOptions, MyValueResolver, ICtorParamConfigurationExpression, Destination, ITestDomainItem, TypeMapConfiguration, INamingConvention, A, IMapFromElement, IGlobalConfiguration, Profile, LambdaTypeConverter, LambdaValueResolver, ParameterReplaceVisitor, IProfileExpressionInternal, ICtorParameterConfiguration, ISomeService, Source, IValueResolver, IMappingExpression, IProjectionMapper, IGlobalConfigurationExpression, IMemberConfigurationProvider, IConfigurationProvider, IMapFrom

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Initialize |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.DeviceDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.DerivedDevice |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Device |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.IDerivedDevice |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.IDevice |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Dynamic.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.TestProfile |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.TestProfile |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.SubSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.TestProfile |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: NonGenericReverseMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Parent |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Child |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Level1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CreateCustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerDtoBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CreateCustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerDtoBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CreateCustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerDtoBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ParentOfSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TitleResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DescriptionResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.CollectionController |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Branch |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Organization |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.CollectionDTOController |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BranchDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.OrganizationDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ConditionBug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ConditionBug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Container |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummyDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummySource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummyDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummySource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.MaxDepth.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class3DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class2DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class1DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelSubDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelSubDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ElementDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ElementSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.LinkImpl |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.ILink |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.DestinationDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.SourceDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.NullBehavior.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationExpressionFeatureBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestinationItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestinationItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.DestinationItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArraysAndLists.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformerTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FormElementDTO2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FormControlBaseDTO2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TextBoxControl2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TextFieldControl2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FieldControl2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FormElement2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestinationBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FooDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SpecificDomain |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseDomain |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummyDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummySource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Inner |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummyDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DummySource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.MaxDepth.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.ClientContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.SourceInner |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.SourceDeepInner |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class3DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class2DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Class1DTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Configuration.MemberConfigurationExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Configuration.MappingExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination6 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination5 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination4 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source6 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source5 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source4 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination6 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination5 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination4 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source6 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source5 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source4 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TypeMapFeatureBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TypeMapFeatureB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.TypeMapFeatureA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExpressionFeatureBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExpressionFeatureBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExpressionFeatureB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExpressionFeatureA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.DestinationBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestinationBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.DestinationBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.IDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.ISource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InterfaceMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ChildDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ChildSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Child |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.SourceClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.DestinationClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.SourceClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.DestinationClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.DestinationClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.SourceClass |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Tests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomValueResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomValueResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Resolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.DestinationType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.SourceType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.ClassB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.ClassA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.TestObjectMapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: NestedContainers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: NestedContainers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ForAllMembers.ConditionalValueResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ForAllMembers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ForAllMembers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FooDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomTypeConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ContextResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestInner2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestInner1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.SourceFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationBar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.DestinationFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.MyType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.GeoCoordinate |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.GeolocationDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.PersonDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.PersonDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.PersonDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ChildModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ParentModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ChildDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ParentDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ChildModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ParentModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ChildDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.ParentDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Constructors.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MyCollection |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MyCollection |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomEnumerator |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomList |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomEnumerator |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomList |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomEnumerator |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomList |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomEnumerator |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomList |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomEnumerator |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomList |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MyJObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MyJObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Inner |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.IDest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.MyValueConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AttributeBasedMaps.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ClientContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SubModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BarModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BarModelBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BarBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.PersonModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BarBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.DestinationDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.SourceDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.B |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.A |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.DestinationBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Boo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Boo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Boo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SpecificDomain |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseDomain |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.EntityTwo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.EntityOne |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoThree |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoTwo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoOne |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseTypeDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Inner |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ClientModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.AddressModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Client |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityP |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityN |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityM |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityL |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityK |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityJ |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityI |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityH |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityG |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityF |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityE |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityD |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityC |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeEntityA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoP |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoN |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoM |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoL |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoK |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoJ |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoI |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoH |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoG |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoF |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoE |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoD |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoC |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SomeDtoA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Entity |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationLevel2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationLevel1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DestinationLevel0 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceLevel2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceLevel1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SourceLevel0 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Converter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Converter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.OtherSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExceptions.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExceptions.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: General.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestinationValuePair |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FooObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FooDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FooDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Dictionaries.FooDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Dictionaries.DestinationValue |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Dictionaries.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Dictionaries.SourceValue |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Dictionaries.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.DestinationItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.C |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.B |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.A |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConfigurationValidation.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ConditionalMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ChildDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ParentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ContactViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SupplierViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Contact |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Supplier |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ContactViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SupplierViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ArticleViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Contact |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Supplier |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Article |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ChildModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BidirectionalRelationships.ParentModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.AfterMapAction |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.BeforeMapAction |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.BeforeAfterMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Level2 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Level1 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsWrapperA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceWrapperA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetailsWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherDestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DestinationDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSourceDetails |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.OtherInnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.CategoryDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourseDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Category |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContent |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourse |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.ClientContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.CategoryDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourseDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Category |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContent |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourse |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.ClientContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IMemberConfigurationExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IProjectionExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IMappingExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.UserGroupDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CategoryDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.UserDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.UserGroupModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CategoryModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.UserModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MyProfile |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Resolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Resolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Resolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Resolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValuesResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.KeyResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerItemCodes |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.ActorDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Actor |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MovieDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Movie |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.NamingConventions.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.NamingConventions.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.NamingConventions.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.JObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IMappingOperationOptions |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.InnerDestination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.InnerSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.ValueTypes.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.DestBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.SourceBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ValueTransformers.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OtherSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.FourDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.EightDigitIntToStringConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.CustomConverter |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomMapping.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CategoryDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomResolver |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Address |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.CustomerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.User |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MapFromTest.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MapFromTest.InnerModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MapFromTest.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MapFromTest.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.MapFromTest.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DtoB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DtoA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.B |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.A |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.UserDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.AddressDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Users |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Addresses |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceWrapper |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceB |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceA |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.DestinationValue |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceValue |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.SourceItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Projection.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.INodeModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.OrderDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.CustomerDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.CustomerStubDTO |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseDest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseBaseDest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseBaseSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseDest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseBaseDest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.BaseBaseSource |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ContainerDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Container |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SpecificItemDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ItemDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.SpecificItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ItemBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DtoBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.DomainBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Derivation |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.AbstractChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Concrete |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.FromDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.From |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Derivation |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.AbstractChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Concrete |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.FromDerived |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.From |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Derivation |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.AbstractChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.Concrete |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingInheritance.From |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ItemDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Container |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.SomeOne |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.SomeBody |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.SomeBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ReadOnlyCollections.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ReadOnlyCollections.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ReadOnlyCollections.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: ReadOnlyCollections.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.InheritedFoo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Foo |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Product |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Article |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ChildType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ChildType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.ChildType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.BaseType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Person |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.ValidityDayType |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.CalendarDay |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Calendar |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.MotorcycleModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.CarModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.VehicleModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Motorcycle |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Car |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Vehicle |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.MotorcycleModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.VehicleModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Motorcycle |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Car |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Vehicle |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Inheritance.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContentDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.CategoryDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourseDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.Category |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContent |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourse |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.ClientContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.Parent |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.ApplicationDBContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.Configuration.ICtorParamConfigurationExpression |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Profiles.Dto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Profiles.Model |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ChildProfile |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.IModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.DtoObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelSubObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MemberResolution.ModelObject |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Dest |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.MappingExpressionFeatureBase |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.OrderDto |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.CustomerHolder |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Order |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Destination |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.UnitTests.Bug.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerItemCodes |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Item |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Context |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.CustomerViewModel |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Customer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TargetChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TargetChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Target |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.SourceChild |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.Source |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.TestContext |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.IntegrationTests.DatabaseInitializer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: AutoMapper.TypeMapConfigErrors |
| Info | SolutionDiscovery | No .sln file found |

### Pruning notes

- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ISource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DomainBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.FromDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionDTOController'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ISourceBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SourceA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ConvertUsingBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericMapWithUntypedMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.InnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ConstantVisitor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_collection_null_behavior'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ForAllMapsTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.ConstructorValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Source2'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.DateTimeToNullableDateTime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoM'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MappingInheritanceBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.SomeBody'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics_With_Struct'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MultipleMappingsOfSameTypeFails'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Order'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldMapMethodExtensionMethods'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.SourceItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.CheckingStepModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DerivedClassWithDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DifferentItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_readonly_property_as_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_generic_ienumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ExplicitValues'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ServiceProviderTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DuckDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_destination_type_requires_a_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.KeyResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersSelectFirstOrDefaultWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeBaseOpenGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_a_constant_value'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_source_member_name_via_attributes_using_nameof_operator'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.QueryableInterfaceImpl'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_struct_from_dynamic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.QueryableValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ParentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_a_concrete_type_to_an_interface_type_and_reverse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IncludeBaseInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.EnumToEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MyClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ProjectionOrderTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NullableToStringTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorLetClause'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreOverrideShouldBeInheritedIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersReverseMapGenericsOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.SomeOne'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MultiThreadingIssues'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Three2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.MappingSpecificBeforeMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationFeatureBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ResourcePointDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_adding_a_simple_custom_mapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_contextual_values_in_resolve_func'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_type_of_converter_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InnerSourceWrapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Order'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArticleViewModel'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_map_with_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_type_and_string_based_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersMembersFirstOrDefaultWithNullSubstitute'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ClientContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.When_configuring_before_and_after_methods_multiple_times'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.MyProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_mapping_to_a_nullable_flags_enum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_a_derived_interface_to_an_derived_concrete_type_with_readonly_interface_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseTypeDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ConstructorExplicitExpansion'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FromDateToNullableDateTime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.When_conditionally_applying_a_resolver_per_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.SourceBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceWrapperA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IChildModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OtherSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.DestInner1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExpressionFeatureWithReverseTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InformationDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Children'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Target'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityG'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_overriding_global_ignore'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_constructor_partially_matches_and_ctor_param_configured'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowChart'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SpecificDomain'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.AddressDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ISomeDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooBaseBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableResolveUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityH'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooValueConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestWithIEnumerableInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SomeSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AmbigousMethod'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationDetailsA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.StubNamingConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoC'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_configuring_a_global_constructor_function_for_resolvers'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.class1DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_a_list_of_object_to_readonly_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceWithIEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_configuring_ctor_param_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseNotMatching'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FullName'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowNode'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelObjectWithConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullArrayBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OverrideIgnore'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseBaseSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EntityOne'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ConcreteSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Child'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_ICollection_types_but_implementations_are_different'
- PatternRelevancePruner: pruned test type 'CircularReferences.When_mapping_circular_references'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ConstructorMappingValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Student'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IgnoreAttributeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_instance_for_string_based_non_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reusing_the_execution_plan_inner_map'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ITarget'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CannotMapICollectionToAggregateSumDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullFlattening'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomEnumerator'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_existing_collection_typed_as_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionMapperMapsIEnumerableToISetIncorrectly'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ChildSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_list_to_ICollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MapOverloadsWithDynamic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Thing'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MaxDepthTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ConcreteUserEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenerateSimilarType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.PersonDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.KeyValueModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoF'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UsersB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.MatchingNonMemberExpressionWithSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapToIncludeMembersGenericsOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Include'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SubSetting'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ConcreteDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.iclass2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.When_constructing_type_maps_with_matching_property_names'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.InheritanceWithoutIncludeShouldWork'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomerDtoBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_contextual_values_wrong_overload'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TargetItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.SubBaseEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DateProvider'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.MappingSpecificAfterMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_with_no_source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_struct_collection'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.OtherDestinationDetails'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Vehicle'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersTransformers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Nullable_enum_default_value_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.BazDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.MappingMultipleConstructorArguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_from_dynamic_null_to_int'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FormControlBaseDTO2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Product'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.MaxDepthWithCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapWithPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.SourceClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseUserEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.BarDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityC'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MasterWithNoExistingCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreShouldBeInheritedWithOpenGenerics'
- PatternRelevancePruner: pruned test type 'AssignableLists.AutoMapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullablePropertiesBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableToInvalid'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CSrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EditModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableShortWithCustomMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.MyTableModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldIgnoreImplicitStaticConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationExpressionFeatureB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Order'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CombinedNames'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Branch'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.iclass1'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.IQueryableInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomCollectionTester'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_null_array_to_list'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableDateTimeOffsetConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DatabaseCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_building_custom_configuration_mapping_to_itself'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NestedInnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.SubDtoChildObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ChildSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Circular'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.RootModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ChildDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DataDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldMapMethodInstanceMethods'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.OtherDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_a_primitive_array_with_custom_object_mapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FirstOrDefaultCounter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ChildDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Bicycle'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ConstructorExplicitExpansionOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GeneralItemDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.RecursiveOpenGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomProjectionStringToString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.LazyCollectionMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Res'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonGenericConstructorTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ProjectionAdvanced'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithPreConditions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_destination_object_is_specified_with_child_objects'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Dest2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_mapping_order'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Car'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_unknown_collection_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class3DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Level2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductCategory'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.ParentSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_translator_using_generics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ModelSubDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.MutableService'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AsWithGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_a_primitive_array'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ChildDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Source3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ADTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_readonly_property_UseDestinationValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectCollectionEnumerableTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReadonlyCollectionProperties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapWithStaticField'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoL'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionsNullability'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AsWithMissingMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EnumCaseSensitivityBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityF'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.BaseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultNoPolymorhism'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.When_value_types_are_the_source_of_map_cycles'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IgnoreBaseMatching'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Destination1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_member_typed_as_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.When_value_types_are_the_source_of_map_cycles_with_PreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectCollectionListTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserPropertiesContainer'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.CustDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityP'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EnumMatchingOnValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MetaData'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_source_has_cyclical_references_with_ignored_ForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_member_names_match_with_underscores'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_the_target_has_an_enummemberattribute_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.UserModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceDetailsWrapperA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapWithoutPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_dispose_struct'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_specifying_a_type_converter_implementing_multiple_type_converter_interfaces'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.ForAllPropertyMaps_ConvertUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FactoryDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceDetailsA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_classes_with_implicit_conversion_operators_on_the_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_member_mapping_with_a_cast'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.StructMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.SourceItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.MyProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.DtoModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ByrefConstructorParameter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_to_existing_object_through_interfaces'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FromGarage'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ChildProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_construct_mapping_a_struct'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reverse_mapping_and_ignoring_via_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableIntToNullableDecimal'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Booking'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.When_using_a_source_member_name_replacer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.ParentDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWrapperFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Parent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_value_with_mismatched_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ProjectToAbstractTypeWithInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OtherDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.Id'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_to_a_type_with_explicitly_implemented_interface_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Child'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathGenericsSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Dest3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_a_constructor_with_single_optional_arguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Fu'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithNullExpressionShouldFail'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceWrapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ChildDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GenericTypeConverterWithTwoArguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_collection_null_behavior_in_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_source_has_cyclical_references'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Outlay'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MapFromReverseResolveUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BoolToIntConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DtoType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Addresses'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_ignoring_all_properties_with_an_inaccessible_setter_and_explicitly_implemented_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExceptions.When_encountering_a_member_mapping_problem_during_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorLetClauseWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ApplicationDBContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source5'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_using_a_custom_member_mappings'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_to_a_generic_dictionary_with_mapped_value_pairs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityK'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.DestinationClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ParentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GuidTryExpression'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IDbContextFixture'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.UseDestinationValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CollectionMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.CustomerViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseBaseDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_readonly_property_as_IEnumerable_and_existing_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationRules'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_a_bidirectional_relationship_that_includes_arrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersCascadedNullCheck'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForCtorParam_MapFrom_ProjectTo'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultWithSubqueryMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Item'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeAs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.SourcePreCondition'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_dto_with_get_methods'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ConstructorDefaultValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Order'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.DisableNamingConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableBytesAndEnums'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_instance_for_string_based_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForCtorParam_MapFrom_String'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.UsersInRolePoco'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ParentVM'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoE'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.IModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductSubcategoryDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersWithMapFromExpression'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_existing_observable_collection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_translator_with_mismatched_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Address'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DestModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationValuePair'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapProjection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ExtendedProductDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.SubSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Type1Point3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserGroupModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.DestinationItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.EnumToArray'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NestedConstructorsWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Planning'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations.ServiceLifetimeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BranchDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ScoreModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomProjectionChildClasses'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DestinationFactoryDependencyTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DestinationTypePolymorphismTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.NestedExplicitExpandWithFields'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_destination_object_is_specified_and_you_are_converting_an_enum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelObject2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InitializeNRE'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SubModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SpecificDescriptionDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_validating_only_against_source_members_and_source_does_not_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_matches_but_the_destination_is_passed'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersTransformersPerMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ParameterizedQueries'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapFromNamingConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorIncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.DefaultSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.DecimalAndNullableDecimal'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OrderDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Dynamic_constructor_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DataModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.EnumToUnderlyingTypeProjectionMapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityN'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersFirstOrDefaultReverseMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TimesheetModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_with_a_dto_subtype'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IntToNullableIntConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_matching_member_names_but_mismatched_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectIReadOnlyCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UnderscoreNamingConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestinationA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullCheckCollectionsFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecordOtherObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reusing_the_execution_plan'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreOverrideShouldBeInherited'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ChildrenStructModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TypeMapFeatureBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SeparateConfiguration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullConstructorParameterName'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.StepInput'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ToGarage'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseDefaultFlattening'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.StepGroup'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MyProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_a_constructor_with_a_matching_argument'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionBaseClassGetConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_enumerable_to_array'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.MapToBaseClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_adding_a_custom_mapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BillOfMaterials'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithNullSubstitute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ResultDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.When_using_a_class_to_do_before_after_mappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.AutoMapperBugTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductSubcategory'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectEnumTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NestedOtherInnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_with_optional_parameters_and_constructor_mapping_is_disabled'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Category'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.PolymorphismTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonGenericQueryableTests'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_nullable_type_to_a_nullable_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DescriptionResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowNodeModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Three'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ContactViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorsWithCollectionsWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Pager'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MapToAttribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowSwitch'
- PatternRelevancePruner: pruned test type 'SetterOnlyBug.MappingTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReportMissingIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Level1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.SubChildModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseMapChildProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_null_with_context_mapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Detail'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Profile1'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.TestContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseUserDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Destination2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Article'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MaxExecutionPlanDepthDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetWithISet'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MapAtRuntimeWithCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooMemberValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ExpandCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceLevel2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Nested'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.OrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_a_model_with_null_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SubclassMappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorsWithCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoObjectWithConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.DtoStatusValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ModelSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.Derived'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Level1A'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.User'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooDtoBaseBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConvertMapperThreading'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class1DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship'
- PatternRelevancePruner: pruned test type 'Indexers.When_mapping_to_a_destination_with_an_indexer_property'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_configuring_a_map_to_ignore_all_properties_with_an_inaccessible_setter'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_source_member_name_via_attributes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.RootLevel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.TrainingCourseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.Well'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_converting_to_string'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reusing_the_execution_plan_existing_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InheritForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooScreenModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CComponentDefinitionModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationFeatureA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ForAllMaps'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.Child'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_non_generic_ResolveUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefaultWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableExtensionMethodHelpers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.RootLevelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceLevel1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.TConcrete'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Person'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_using_expressions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Address'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationValueInitializedByCtorBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.NestedDtos'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_list_types_with_mismatched_element_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldUseConstructorDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Door'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SubSubModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.MemberResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Type1Point2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableIntToNullableEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.PersonSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_multiple_source_members_match_postfix'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Man'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_an_existing_HashSet_typed_as_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_an_array'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_constructor_function_for_custom_converters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters_on_types_with_incompatible_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_destination_object_is_specified'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDtoWithNullableStatus'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_from_null_interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_a_constructor_with_multiple_optional_arguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.FooConfiguration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_type_and_string_based_non_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reseting_a_mapping_from_a_property_to_a_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BaseClassWithDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ContainsADest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.RecursiveQuery'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithIncludeDifferentOrder'
- PatternRelevancePruner: pruned test type 'NestedAndArraysTests.LinqTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.NodeDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelPager'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.IntToIntMapper'
- PatternRelevancePruner: pruned test type 'Profiles.When_configuring_a_profile_through_a_profile_subclass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InformationBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.C'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination5'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.CreateProjectionEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_is_partial_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AssertionExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source6'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ElementSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectionOverrides'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reverse_mapping_and_ignoring'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.InnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithConditions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_using_a_type_converter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.PersonModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_an_array_as_ICollection_with_MapAtRuntime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowDecision'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NonExistingProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Baz'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.When_mapping_with_a_bidirectional_relationship_that_includes_arrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BarDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SpecificDestinationItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowStep'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_matches_with_destination_prefix_and_postfix'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithoutSettersForSubObjects'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ITarget'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomList'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MapAtRuntime'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.TrainingCourseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TypeMapFeatureB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.StandardDomain'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FromCar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsAndNonGenericsWithIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_a_private_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithAfterMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MailOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.TrainingCourse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.ValueTypeDestinationPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseMatchingDifferentType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GoodProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TypeWithStringProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_specifying_mapping_with_the_BCL_type_converter_class'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_extension_method_returns_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.SubEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_collection_with_instantiation_managed_by_the_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_object_to_string_with_use_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_in_a_specfic_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ActivityBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.RotatorAdRunViewModel'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_dto_with_a_private_parameterless_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.JsonNetDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CComponentDefinitionDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.NonMemberExpressionWithSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TransformingInheritanceForMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.InnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_with_missing_property'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_member_value_resolver_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullDestinationType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.PersonModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CannotConvertEnumToNullableWhenPassedNull'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExpressionFeatureB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoG'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ExpandMembersPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserProperties'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ChildModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Id'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AllowNullWithMapAtRuntime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.InterfaceInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ConstructorParameterNamedType'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DependencyCondition'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BDTO2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SpecificItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MappingInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IEnumerableAggregateProjections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ManEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_to_a_top_level_camelCased_destination_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.InnerModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.OtherInnerSourceDetails'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.SourceModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_readonly_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.When_mapping_from_private_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Client'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_configuring_a_resolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TypeExtensionsTests'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_reverse_map_with_sourcemember_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.LocalizedString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithIncludeBaseOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Target'
- PatternRelevancePruner: pruned test type 'NestedContainers.When_specifying_a_custom_contextual_constructor_for_type_converters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ForAllMembersAndResolveUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MinusOneResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.ConditionalValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsWithInclude'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.EnumDescriptor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DtoObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ITypeA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MasterDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.AfterMapAction'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics_With_UntypedMapFromStructs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.User'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IRes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_using_a_custom_member_mapping_and_source_is_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DtoA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GenericCreateMapsWithCircularReference'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DummyDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.SourceBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDtoString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_a_primitive_array_with_custom_mapping_function'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.InMemoryMapObjectPropertyFromSubQuery'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceTree'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_constructor_does_not_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.SourceDeepInner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.PrimitiveExtensionsTester'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithValueTypeValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Profile2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UnsupportedCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersCycle'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersWithNullSubstituteWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_matches_but_is_overriden_by_ConstructUsing'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersNullCheck'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SubEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OtherDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Self_referencing_existing_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_multiple_source_members_match'
- PatternRelevancePruner: pruned test type 'NestedExpressionTests.NestedExpressionMapFromTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ExpressionBuilderExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OtherChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.FooViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.ParentDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ServiceLifetimeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_getter_only_ienumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.StringPadder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.SubBarr'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Two'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeAllDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics_With_UntypedMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SubA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_is_partial_match_with_value_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DependencyResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UserViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_a_dto_with_a_set_only_property_and_a_get_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CurrencyDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_configuring_a_member_to_skip_based_on_the_property_value_with_custom_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_one_child_relationship'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IgnoreAllTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestB'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ScoreRecord'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_classes_with_explicit_conversion_operator_on_the_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableUntypedMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullNullableIntToNullableDecimal'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_the_source_has_an_enummemberattribute_value'
- PatternRelevancePruner: pruned test type 'ByteArrayBug.When_mapping_byte_arrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationLevel0'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExceptions.Sub'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.EnumMapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationItemBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.InstructionStepModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Inner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.PropertyNamedType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceLevel0'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ScopeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Derivation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IDestModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.Book'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ConstructorMapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UsingEngineInsideMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CyclesWithInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.Cust'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.DefaultDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DerivedLocalizedString'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_map_and_reverse_map_with_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_resolve_throws'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_a_derived_interface_to_an_derived_concrete_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Target'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ShouldInheritBeforeAndAfterMapOnlyOnce'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped_with_resolvers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.SomeBase'
- PatternRelevancePruner: pruned test type 'General.When_mapping_to_a_dto_string_property_and_the_dto_type_is_not_a_string'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_custom_validation_for_convertusing_with_mappingfunction'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SubB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.NullableConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.CircularAs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericMapsPriority'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ClientContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.QueryableInterfaceInheritanceIssue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ValueViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyTestResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TagDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarModelBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.B'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination6'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.EntityDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.MapFromExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReportMissingInclude'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Dest2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomerHolder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_array_to_IEnumerable_with_MapAtRuntime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MoreSpecificDomain'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ChildModelBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_custom_validation_for_convertusing_with_mappingexpression'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_readonly_collection_without_setter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationDetails'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestWithNoConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.StringLower'
- PatternRelevancePruner: pruned test type 'Profiles.When_disabling_constructor_mapping_with_profiles'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OtherDest'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_non_nullable_type_to_a_nullable_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ConditionDependencyTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ComputerModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TestContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SelectiveConfigurationValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Profile1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ParameterizedQueriesTests_with_dictionary_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceValidationWithIgnore'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Step'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CFieldDefinitionDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_collection_null_behavior_in_profile_with_MapAtRuntime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.NameValueCollectionMapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_constructor_argument_fails'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ContainerClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ResolversWithSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_an_interface_type_to_a_concrete_type_and_reverse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ChildModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.A'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InheritedFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_custom_collection_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ChildIdToParentDtoConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Role'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_nested_context_items'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_a_mapping_order_with_attributes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_configuring_nullable_ctor_param_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ExplicitExpansionWithInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FourDigitIntToStringConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_to_existing_destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.TrainingContent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DtoChildObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DummySource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_generic_ilist'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Actor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CDataTypeDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_one_child_relationship'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ConditionSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_implementing_multiple_IValueResolver_interfaces'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.StringToEnumConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.GrandChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.CustomMapFromExpressionTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Baz'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.A'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IEnumerableMemberProjections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Converter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_array_with_AllowNullDestinationValues_false'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.User'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.CategoryDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ClassA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IntToBoolConverter'
- PatternRelevancePruner: pruned test type 'NestedContainers.When_specifying_a_custom_contextual_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.D'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_disabling_method_maping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_one_type_to_another'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsWithAs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ResolveWithGenericMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NestedModel'
- PatternRelevancePruner: pruned test type 'SourceValueConditionPropertyBug.ConditionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.TInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.TypeHelperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.When_configuring_before_and_after_methods'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.TrainingContent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_skipping_validation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TypeMapIncludeBaseTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bar'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullSubstituteWithEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NonPublicEnumeratorCurrent'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_not_specifying_as_proxy_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.DestinationClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CurrencyModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullSubstituteInnerClass'
- PatternRelevancePruner: pruned test type 'ContextValuesIncorrect.When_conditionally_skipping_null_destination_values'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ReverseMapWithIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_to_StringDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_validating_reverse_mapping_classes_with_missing_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_configuring_all_members_and_some_do_not_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_translator'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EntityTwo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NestedConstructors'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MappingToAReadOnlyCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_collections_with_structs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_an_extension_method_is_for_a_base_class'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_through_constructor_and_destination_has_setter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.TestProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldIgnoreOpenGenericMethods'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DerivedModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.MultipleRegistrationTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlowChartModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SecondClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ProjectEnumerableOfIntToHashSet'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Source1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.UsingMappingEngineToResolveConstructorArguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ParentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_mismatched_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ClassB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_member_type_mapped_mappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_ignoring_a_dto_property_during_configuration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_configuring__non_generic_ctor_param_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.NameCondition'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_member_resolver_and_custom_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DifferentItem2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EmptyNullSubstituteBug'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_generic_reverse_map_with_sourcemember_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_getter_only_list_with_existing_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InterfaceSelfMappingBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.BarDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Initialize'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooMappingAction'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OneTimeEnumerator'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ExtraProduct'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Bar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CannotProjectStringToNullableEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NonGenericDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MySpecificDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.From'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Computer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorToStringWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DestinationConcrete'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_all_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MySpecificClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Response'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IProperties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IgnoreMapAttribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Person'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExpressionFeatureBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.IId'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_getter_only_existing_ienumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DuplicateExtensionMethods'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionController'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_value_converter_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigCompilation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Supplier'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomValidations'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_ignoring_members_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_with_DoNotAllowNull'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.AssertConfigurationIsValidNullables'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldUseConstructorPublic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_to_a_self_referential_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BadDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IDestinationBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.DestInner2'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ConcreteTypeA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_object_to_string'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DestinationBaseBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.Bar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersReverseMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.EnumMappingFixture'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ResolveGenericTypeMapThreadingIssues'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.RecursiveMappingWithStruct'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.StepInputModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Device'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OverrideIgnoreMapFromString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_null_behavior_in_a_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations.ISingletonService'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ImmutableCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CategoryModel'
- PatternRelevancePruner: pruned test type 'PrimitiveArrays.PrimitiveArraysExpressionTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_custom_validation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_value_specified_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_collection_with_AllowNullCollections_true'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Dst'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithInclude'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ExplicitExpansion'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.SourceClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FactoryProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IDevice'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Parent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.When_mapping_to_concrete_readonly_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OrganizationDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_validating_only_against_source_members_and_source_matches'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExceptions.Dest'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_as_proxy_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DependencyFactory'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Licensing.LicenseValidatorTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_to_dynamic_from_getter_only_property'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AutoMapperTester'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.RecordConstructorValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersMembersFirstOrDefaultWithNullSubstituteWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OnlineOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.PolymorphismTptTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValuesResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ShouldInheritBeforeAndAfterMapOnlyOnceIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TransformingNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CorrectCtorIsPickedOnDestinationType'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_mapping_from_a_null_object_with_a_nullable_enum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.A'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.InterfaceWithObjectProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Source2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IOtherDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MaxExecutionPlanDepth'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CollectionWhere'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.EnumToUnderlyingType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExceptions.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Boo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.class2DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Category'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_collections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OrderEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Command'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SequenceContainsNoElementsTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.MyContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_to_dynamic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithIncludeBaseOverrideConvention'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BaseEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_different_types_with_ResolveUsing'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_null_model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination4'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AbstractChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ConventionMappedCollectionShouldMapBaseTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.StudentViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MasterWithCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DuplicateValuesIssue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ComplexProductDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.Author'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DtoSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.CustomConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_constructor_does_not_match_ForCtorParam'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourseDetailDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_mismatched_member_names_and_mismatched_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Inner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DifferentDescriptionDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Preserve_references_with_constructor_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.iclass1DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CreateMapExpressionWithIgnoredPropertyBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.ParentModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyEnumerator'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.VendorModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.iclass2DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeBaseIndirectBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Resolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersReverseMapOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BoolToNullableIntConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_derived_classes_in_arrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_context_state'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersSelectFirstOrDefault'
- PatternRelevancePruner: pruned test type 'ParentChildResolversBug.ParentChildResolverTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MemberListSourceAndForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestinationValue'
- PatternRelevancePruner: pruned test type 'ReadOnlyCollections.ReadOnlyCollectionMapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SealGenerics'
- PatternRelevancePruner: pruned test type 'EnumConditionsBug.EnumMapperTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecordObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ShouldInheritBeforeAndAfterMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.LinkImpl'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ProjectConstructorParameters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ExplicitMapperCreation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ItemBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ParentModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ConditionDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DetailDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.GenericsTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_an_extension_method_is_for_a_base_interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithValueTypesAndFields'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.When_mapping_to_interface_readonly_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithGenericsInvalidStrings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UserModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.When_mapping_with_lowercase_naming_conventions_two_ways_in_profiles'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AnimalDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapToIncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Utils'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludedBaseMappingShouldInheritBaseMappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.AttributeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ConvertMapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MethodsWithReverse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Neda'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.matrixDigiInStruct1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_extension_method_returns_value_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_source_has_cyclical_references_with_ForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GrandGrandChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityE'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.CustomTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.EFCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.GoodSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Container2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GenericDescriptionDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithValueConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ICalendar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FormElement2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OrderItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_string_based_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_a_source_child_object_is_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestB'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.ArtDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_using_a_custom_resolver_and_the_source_value_is_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithIncludeBaseOverrideMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_construct_mapping_a_struct_with_string'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.PreserveReferencesSameDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ParentPrivate'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.CarModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Target'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersWithMapFromExpressionWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ContainsASrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ChildType'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_from_a_list_of_object_to_generic_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectEnumerableToArrayTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonGenericProjectionOverrides'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.WithoutPreserveReferencesSameDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullSubstituteType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_contextual_values'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_setter_only_peroperty_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_ilist'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_throwing_NRE_from_MapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecordOtherSubObjectWithExtraParam'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ObjectPropertyAndNestedTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_translator_using_projection'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_mapping_from_a_null_object_with_a_nullable_enum_as_string'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_mapping_order_for_base_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.When_using_a_source_member_name_replacer_with_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations.TestSingletonService'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_to_a_generic_dictionary_that_does_not_use_keyvaluepairs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Nullable_conversion_operator'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class2DTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ContextResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersConstructorMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReadOnlyFieldMappingBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CDataTypeModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IMyInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_generic_icollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.RecursiveDynamicMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Dario'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.When_destination_type_is_a_value_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NullSubstitutesWithMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithIgnoreShouldNotSetValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_non_generic_icollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.CustomerStubDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ChildrenStructDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.GeolocationDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OtherDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomerItemCodes'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersExplicitExpansionWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Parent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Nullable_enum_default_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomerViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IPropertyA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NestedMappingProjectionsExplicitExpanding'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MissingMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_custom_validation_for_convertusing_with_typeconvertertype'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersReverseMapGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ParameterizedQueriesTests_with_anonymous_object_and_factory'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_classes_with_explicit_conversion_operator_on_the_source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.TypeTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithMapFromExpression'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationChildDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Calendar'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.Order'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.DefaultEnumValueToString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Constructor_mapping_without_preserve_references'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BoolModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_string_based_non_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Context_try_get_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MailOrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.INodeModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MultipleInterfaceInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityJ'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.DigiIn1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_custom_resolving_mismatched_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Organization'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ItemToMapDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IncludedMappingShouldInheritBaseMappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UserDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.SpecificEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.UserPoco'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.PersonDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MapFromClosureBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_a_custom_resolver_for_a_child_model_property_instead_of_the_model'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.BaseDbObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultWithSubqueryMapFromWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_instance_for_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ItemDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoSubObjectWithConstructorAndWrongType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_destination_members_contain_prefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BasicFlattening'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductTypeDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_renaming_class_constructor_parameter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Src'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProdTypeB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Dest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GoodSrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_multiple_matching_keys'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.TrainingCourse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ProxyOfSubA'
- PatternRelevancePruner: pruned test type 'ReadOnlyCollections.When_mapping_to_concrete_readonly_collection'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_to_construct_using_service_locator_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Level1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ParentIdToChildDtoListConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_for_non_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Category'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_translator_and_passing_in_the_destination_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SourceType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyJObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FactorySource'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_to_a_generic_dictionary_interface_with_mapped_value_pairs'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_source_has_cyclical_references_with_dynamic_map'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.TestProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_untyped_null_to_IEnumerable_and_AllowNullCollections_is_true'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.CustomerDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldUseConstructorPrivate'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_non_disposable_struct'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.TModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityD'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Address'
- PatternRelevancePruner: pruned test type 'InheritedMaps.When_projecting_and_inheriting_maps'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.AgeCondition'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AssignableCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityM'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InternetOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithCascadedIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceInner'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ICollectionAggregateProjections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReadonlyCollectionPropertiesOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectionAndMappingCombined'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_with_DoNotAllowNull_and_inheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics_With_Base_Generic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IncludeFromDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.BadDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Level2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MyDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.Sem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Chu'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_member_mapping_to_a_nested_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GenericItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.When_conditionally_applying_a_resolver_globally'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_with_context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.ChildModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Base'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.Address'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.SemDto'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_max_depth_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_with_AllowNull_and_inheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DerivedComplexTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.MyTable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.EntitySource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecordOtherSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_an_existing_array_typed_as_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_a_concrete_type_to_an_interface_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.OverrideInclude'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableIntToBoolConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InvalidReverseMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InitializeNRE2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_using_custom_member_mappings_without_generics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FieldControl2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_inheritance_with_value_resoluvers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.JObjectField'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.SubSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorMapFromWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.SourceDerived'
- PatternRelevancePruner: pruned test type 'Profiles.When_segregating_configuration_through_a_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceValidationWithInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SupplierViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CustomCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.IDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ValueConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DeviceDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReportMissingIncludeCreateMissingMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InternetOrderModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.DestinationBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.Model'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.ClientContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DisablingPolymorphismTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.StringToItemConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.B'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ImplementedClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.SourceFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.StructConstructorMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FormElementDTO2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersMultipleConstructorMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithoutSettersShouldBehaveAsForMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsProfileValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DifferentDescriptionDto2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.PascalCaseAcronymInPropertyName'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TextFieldControl2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.SecondLevel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.MissingConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_extension_method_returns_object_SourceExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetInner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_generic_interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.JObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ITargetBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Concrete'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericMemberValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.SecondLevelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.StringToNullableEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_list_with_existing_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ReverseMapWithInclude'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityI'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExceptions.When_encountering_a_path_mapping_problem_during_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.OverrideDestinationMappingsTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.StopgapNBehaveExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RolePoco'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelSubObjectWithConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.DateTimeTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationExpressionFeatureA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ConcreteSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_struct_from_StringDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.FooModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_adding_an_object_based_custom_mapper'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_to_preserve_references_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NonValidatingSpecBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.CDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullSubstituteWithStrings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ADto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.A'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Container'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ILink'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_value_resolver_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.BeforeMapAction'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.MapToInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueConverters'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.MembersToExpandExpressions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.Base'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DestinationBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeBaseWithGenericUsage'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonNullableToNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BadSrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_dto_with_fields_and_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_null_is_passed_to_an_extension_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.CustomerDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_contain_postfixes_with_lowercase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_a_destination_containing_two_dtos_mapped_from_the_same_source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableEnums'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_constructor_partially_matches_and_constructor_validation_skipped'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OrderModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceWithList'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AddProfiles'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Type1Point1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Entity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ChildModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TrainingContentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.When_mapping_from_and_source_member_both_can_work'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UsersA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_to_abstract_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GrandChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.EnumToNullableEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecordSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.EightDigitIntToStringConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.DestinationItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_destination_type_has_private_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_an_extension_methods_contraints_fail'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceToDestinationNameMapperAttributesMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_dto_with_only_fields'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BookingDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeOnlySelectedMembersWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.NestedExplicitExpand'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoThree'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_throwing_NRE_from_MapFrom_value_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Motorcycle'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DeepCloningBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeEntityL'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DerivedDevice'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DescriptionBaseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.GoodDest'
- PatternRelevancePruner: pruned test type 'NonGenericReverseMapping.When_reverse_mapping_and_ignoring'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.CustomerViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Level2A'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.MappingToInterfacesWithPolymorphism'
- PatternRelevancePruner: pruned test type 'ReadOnlyCollections.When_mapping_to_interface_readonly_collection'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDtoWithOwnStatus'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ProxyTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IGeneric'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OneSourceWithMultipleDestinationsAndPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDtoInt'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ProjectAndAllowNullCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.DestinationDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForCtorParamValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Settings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForAllMembers.PostPutWellViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.AppDomainResolutionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.BicycleModel'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_List_of_model_objects'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TypeMapFeatureA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IntEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersFirstOrDefaultWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_configuring_a_member_to_skip_based_on_the_property_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IDateProvider'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_derived_classes_as_property_of_top_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationFeatureB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeBaseShouldNotCreateMaps'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_extension_contains_LINQ_methods'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.MyType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SettersInBaseClasses'
- PatternRelevancePruner: pruned test type 'General.When_mapping_dto_with_a_missing_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceItem'
- PatternRelevancePruner: pruned test type 'NonGenericReverseMapping.When_reverse_mapping_classes_with_simple_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ClientContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DestinationTypePolymorphismTestNonGeneric'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreShouldBeInheritedIfConventionCannotMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ParameterizedQueriesTests_with_filter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_the_destination_has_a_matching_constructor_with_optional_extra_parameters'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.EnumValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_with_AllowNull'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooContainerModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BillOfMaterialsDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TransformingInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_configuring_all_non_source_value_null_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullableBoolToLabel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.ParentModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Inner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_an_interface_with_getter_only_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ChildDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ModelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Connection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_collection_with_AllowNullCollections_false'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TitleResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SpecificItemDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Tag'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoP'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableLong'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InlineWithoutPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OtherInnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReverseMapAndReplaceMemberName'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_an_existing_list_with_existing_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FuEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersExplicitExpansion'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Movie'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericMapForMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_configuring_ctor_param_members_without_source_property_1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsWithIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_same_map_is_used_again'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Source1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IncludeFromBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.AssemblyResolutionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CreateProxyThreading'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.TestObjectMapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.CategoryDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source3'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.UnderlyingTypeToEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ASrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DropCreateDatabaseAlways'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ComplexType'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.IValidityDayType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelDto3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_to_types_in_a_non_generic_manner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseDomain'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.CustomerViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.DestinationType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Class'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_constructor_partially_matches'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_derived_classes_from_intefaces_to_abstract'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_a_dto_with_names_matching_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.Target'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.TestContext'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_nongeneric_type_inherited_from_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GoodDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.OtherSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.VehicleModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.One'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.EntityBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreShouldBeInheritedRegardlessOfMapOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CascadedIncludeMembersForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.SubBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_with_whitespace'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ExplicitlyExpandCollectionsAndChildReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_a_null_model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_is_match_with_default_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_custom_validation_for_convertusing_with_typeconverter_instance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReadOnlyCollectionMappingBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Holder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsProfileValidationNonGenericMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_configuring_ctor_param_members_without_source_property_2'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMultipleExpressionsWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mapping_from_null_interface_and_AllowNullDestinationValues_is_false'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceDetails'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.CustomMapFromTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DestinationDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ParentTrainingCourseDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseDefaultFlatteningWithIgnoreMember'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Appointment'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_an_interface_to_an_abstract_type'
- PatternRelevancePruner: pruned test type 'Dictionaries.When_mapping_from_a_source_with_a_null_dictionary_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.TrainingContentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Outer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_Existing_IEnumerable'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_duplicating_map_configuration_with_code_and_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.class1'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.TypeResolutionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DestinationBase'
- PatternRelevancePruner: pruned test type 'General.When_mapping_tuples'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.PriceModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MultidimensionalArrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ListSourceMapperBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenerics_With_MemberConfiguration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ExistingArrays'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_different_types_with_explicit_value'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CannotConvertEnumToNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Type1'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ThingDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reseting_a_mapping_to_use_a_resolver_to_a_different_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CreateCustomerDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationADetails'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DeepInheritanceIssue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_from_dynamic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ClientModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OverrideDifferentMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceChildDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceProperty'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullChildItemTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CategoryDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DerivedDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Source4'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Converter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.One'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RecognizeDestinationPostfixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ForAllMapsWithConstructors'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SubDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NewCustomer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersNested'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ChuEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Data'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.PropertyOnMappingShouldResolveMostSpecificType'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooService'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ExpandCollectionsOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NullSubstitutes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.FooScreenModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GenericTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GrandChildPrivate'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ProjectEnumerableOfIntToList'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ConcreteDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_matched_members_but_mismatched_types_that_are_ignored'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Person'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ConditionProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_matching_source_and_destination_members_with_underscored_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.When_using_a_custom_source_naming_convention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters_for_object_mapper_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomProjectionCustomClasses'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoI'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_destination_property_does_not_have_a_getter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationFeatureTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.SubSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ConstructUsingReturnsNull'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_null_behavior_with_null_source_items'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ActorDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DtoB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.DestinationFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ContactModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MaxExecutionPlanDepthWithPreserveReferences'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeBaseShouldValidateTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SignedResponse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CollectionHolder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.People'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BaseMapWithIncludesAndUnincludedMappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SourceB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.ModelSubSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreOverrideShouldBeOverriden'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonGenericProjectAndMapEnumTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Script'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CascadedIncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MapAtRuntimeWithExtensionMethodAndNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_configured_in_a_root_profile_contain_postfixes_and_prefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Input'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ChildPrivate'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_null_list_to_array'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TestViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Item'
- PatternRelevancePruner: pruned test type 'ChildClassTests.UnitTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullTypeMapFlattening'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DerivedModelType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FirstClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooProfile'
- PatternRelevancePruner: pruned test type 'AssemblyScanning.When_scanning_by_assembly'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConstructorTransforming'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithConditions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_dto_with_only_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullSubstitute'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.FooTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.EntityDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.AddressModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestinationB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_custom_resolver_requests_property_to_be_ignored'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ModelSubObject'
- PatternRelevancePruner: pruned test type 'Internationalization.When_mapping_a_source_with_non_english_property_names'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForwardProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelDto2'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DatabaseFixture'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Person'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_with_contextual_values_shortcut'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Service'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MovieDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProductCategoryDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.IBaseQueryableInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MapToAttributeTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorToString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetOuter'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersSelectFirstOrDefaultWithSubqueryMapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.PersonTarget'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DependencyTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FooBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapToIncludeMembersGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InternalProperties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.SourceType'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.NullableIntToLong'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.DictionaryConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_an_optional_GUID_constructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ToCar'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_mismatches_in_multiple_children'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableEnumToNullableValueType'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ICalendarDay'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.DtoObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.RecursiveCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OnlineOrderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.ISomeService'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CustomConverters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IntToNullableDecimal'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.ClassBasedConditionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.class2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.DestItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.NodeModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_dispose_exception'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithMapFromFunc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceBar'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceDetailsWrapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Bar'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.SubDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.AbstractProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.When_using_a_class_to_do_before_after_mappings_with_resolutioncontext'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersWithNullSubstitute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_mapping_for_derived_class_is_duplicated'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.AddressModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_an_interface_type_to_an_interface_type_and_reverse'
- PatternRelevancePruner: pruned test type 'ReadOnlyCollections.When_mapping_to_interface_readonly_list'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_recognizing_explicit_member_aliases'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Product'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ItemModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ParentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomerDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.FooInputModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_redirecting_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullCheckDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapFromSourceMemberName'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericMapsAsNonGeneric'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_concrete_non_generic_ienumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DontUseDestinationValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ProjectToInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.MyClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AbstractProductDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_a_member_name_replacer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_passing_a_not_empty_collection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ISomeInterface'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SourceChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_array_types_with_mismatched_element_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.PersonDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NonGenericMemberTransformer'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_base_interface_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_a_dto_member_with_custom_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ConvertUsingWithNullables'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Parameter'
- PatternRelevancePruner: pruned test type 'SourceValueExceptionConditionPropertyBug.ConditionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.Interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DataLayer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.GenericsAndInterfaces'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectionMappers'
- PatternRelevancePruner: pruned test type 'RecognizeIxesBug.IxesTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.NavigationPropertySO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Rotator_Ad_Run'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AutoMapperSpecBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CustomMapFrom.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ContainerDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.AbstractStepModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Source2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_from_dynamic_with_missing_property'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Input'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceWrapperA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FlatteningWithSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoOne'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.DetailsValueResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_dispose'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.DestinationWithNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultWithMapFromExpressionWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.DbEntityA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Ext'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.NullableEnumToString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.NestedSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.When_mapping_to_a_destination_containing_two_dtos_mapped_from_the_same_source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ConcreteUserDto'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_nullable_type_to_non_nullable_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.RecordConstructorValidationForCtorParam'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.ChargeCollection'
- PatternRelevancePruner: pruned test type 'NonGenericReverseMapping.When_reverse_mapping_and_ignoring_via_method'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableIntEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.MotorcycleModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_null_to_int'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_from_dynamic_to_dynamic'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CaseSensitivityBug'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ParameterizedQueriesTests_with_anonymous_object'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseForPath'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OneSourceWithMultipleDestinationsWithoutPR'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Self_referencing_existing_destination_without_PreserveReferences'
- PatternRelevancePruner: pruned test type 'AssemblyScanning.When_scanning_by_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CascadedIncludeMembers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SubDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_contain_prefixes_with_lowercase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_a_static_method_has_first_parameter_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.AbstractStep'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.StringCAPS'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ProjectUsingTheQueriedEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapIgnoreAttributeTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.PreserveReferencesWithInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExpressionFeatureA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultMixedPolymorhism'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationWrongType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.PersonModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.SourceDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TestEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Nullable_enum_default_value_not_null'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DomainModelBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.IAbstractDest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IDerivedDevice'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TimesheetViewModel'
- PatternRelevancePruner: pruned test type 'AssemblyScanning.When_scanning_by_name'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationTree'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Grandchild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_specifying_a_non_generic_type_converter_for_a_non_generic_configuration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.IncludeBaseWithNonGenericUsage'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OpenGenericsValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DerivedDataModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProductModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoH'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AsShouldWorkOnlyWithDerivedTypesWithGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.DuckProxyClassFoo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorTestsWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetISet'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.When_mapping_with_lowercase_naming_conventions_two_ways'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationLevel2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.AllowNullCollectionsAssignableArray'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_value_converter_instance_for_non_matching_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.InformationClass'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.StringTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_mapping_from_a_null_object_with_an_enum'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.CascadedIncludeMembersWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.AddressDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.EnumDescriptorTypeConverter'
- PatternRelevancePruner: pruned test type 'General.When_mapping_a_dto_with_mismatched_property_types'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.NullCheckCollections'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReverseMapAndReplaceMemberNameWithProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ProjectionWithExplicitExpansion'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OtherInnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ReverseMapAs'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.QueryableDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FirstName'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.When_mapping_from_StringDictionary_to_StringDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.NullableLongToLong'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ModelSubDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations.Bar'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_map_with__multiple_attribute_sources'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.C'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GrandGrandChildPrivate'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_AllowNullDestinationValues_is_false'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_null_substitute_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.Art'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.When_mapping_a_flags_enum'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_mismatched_custom_member_mapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoK'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.ChildDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.When_using_a_custom_destination_naming_convention'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.RemovePrefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.B'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.OtherInnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Item'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationB'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderDtoWithOwnNullableStatus'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.StepGroupModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SourceToDestinationMapperAttribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.InvalidStringToEnum'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_adding_a_condition_for_all_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations.Foo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Tests.MapperTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelObjectNotMatching'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableDateTimeMapFromArray'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.GeneralItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.NonGenericProjectEnumTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.InnerSourceWrapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IntegrationTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableDateTime'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Article'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationExpressionFeatureBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.TrainingCourse'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_disposable_at_runtime_class'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.When_mapping_from_dynamic_to_nullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BSrc'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomResolver2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_abstract_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.CustomerDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ModelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.TestContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Contact'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Charge'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingExpressionFeatureWithoutReverseTest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceWrapperB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.ISourceModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ApplyIncludeBaseRecursively'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoN'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Restaurant'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ProjectToAbstractType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.When_mapping_from_chained_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.StepModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldIgnoreExplicitStaticConstructor'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersSelectFirstOrDefaultWithSubqueryMapFromWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.UsersInRole'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationStructMapping'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ValidityDayType'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_overriding_null_behavior_in_sub_profile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.OrderDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Foo2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Map'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ExpiredItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ModelDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.DestinationItem'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.MemberNamedTypeWrong'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_destination_property_does_not_have_a_setter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.TargetB'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DeepNestingStackOverflow'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.CannotProjectIEnumerableToAggregateDestinations'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapToIncludeMembersOverride'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.SimpleProductDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyList'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ParentOfSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reverse_mapping_classes_with_simple_properties'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullToExistingDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.SourceValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullMappingOrderComesFirst'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_from_a_list_of_object_to_IReadOnly_dictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IncludeInheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ProjectWithFields'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ConstructorIncludeMembersWithIheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.FooContainerModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Initializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.EnumeratorBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullToString'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ItemToMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.InnerSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.ViewModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IntToNullableConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_matching_void_method_member'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.TargetChild'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.GenericValueResolverTypeMismatch'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NullableTypeConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.NullToExistingValue'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.ExpandCollectionsWithStrings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.ValueCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.TypeConverterMapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithMemberResolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.Class1'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_using_existing_value_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.GeoCoordinate'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Override'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Animal'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.OtherSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestinationWithList'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.BadProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_extension_method_returns_value_type_SourceExtensions'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoTwo'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BuildExecutionPlan'
- PatternRelevancePruner: pruned test type 'General.When_mapping_an_array_of_model_objects'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DestA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reverse_mapping_open_generics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MoreExplanatoryExceptionTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ChildDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_using_service_location'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.OrderDTO'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationLevel1'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.ChildEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ADest'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Enumerator_dispose_exception_struct'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Context'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Parent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoD'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ToStringTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.When_configuring_a_reverse_map_to_ignore_all_source_properties_with_an_inaccessible_setter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_to_a_custom_list_with_the_same_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DestinationDerived'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_constructor_matches_with_prefix_and_postfix'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.DtoSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_contain_postfixes_and_prefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathWithPrivateSetters'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BeforeAfterMapping.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_using_a_type_converter_class'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.Duck'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.MyProfile'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_a_custom_constructor_and_member_resolver'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.DependencyValueConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Unmapped'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Product'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.When_source_struct_config_has_custom_mappings'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ArraysAndLists.When_mapping_a_collection_with_null_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.ByteArrayColumns'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.Dynamic.DynamicDictionary'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.JContainer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.ModelObject3'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_a_constructor_with_extra_parameters_doesnt_match'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Child'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SourceB'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_value_converter_with_different_member_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.B'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IgnoreAllPropertiesWithAnInaccessibleSetterTests'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.SourceInner'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.Source'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_specifying_a_resolver_for_a_nullable_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_a_constructor_parameter_from_nested_members'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_using_contextual_service_location'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.UseDestinationValueNullable'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.CalendarDay'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MasterWithList'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReverseMapConventions'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.BuiltInTypes.Item'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.MaxDepth.Customer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UserBDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.UserADto'
- PatternRelevancePruner: pruned test type 'AutoMapper.Tests.OrderWithNullableStatus'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ScriptModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConstructorValidationGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_multiple_constructors_and_constructor_mapping_is_disabled'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ValueTypes.When_destination_type_is_a_nullable_value_type'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.DestinationA'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.FooDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_to_classes_with_implicit_conversion_operators_on_the_source'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.Model'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CollectionHolderDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_mapping_derived_classes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.When_mapping_a_concrete_type_to_an_interface_type_that_derives_from_INotifyPropertyChanged'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.EntityBaseModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_contain_prefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.When_mapping_to_an_assignable_object_with_nullable_off'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.When_testing_a_dto_with_fully_mapped_and_custom_matchers'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Two'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.ChildSource'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ReadonlyPropertiesGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.ElementDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.InstructionStep'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.LastName'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreOverrideShouldBeOverridenIncludeBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ForPathGenerics'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.DomainModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ISome'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CustomMapping.When_specifying_a_type_converter_for_a_non_generic_configuration'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.OpenGenericInheritanceOrder'
- PatternRelevancePruner: pruned test type 'AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Profile2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConditionalMapping.Person'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_mapping_collections_with_inheritance'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.TrainingContentDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.UserDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.RemoveNameSplitMapper'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.CFieldDefinitionModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BidirectionalRelationships.Parent'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.Users'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DualConverter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.UserGroupDto'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Inheritance.CheckingStep'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.InnerDestination'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.IncludeMembersFirstOrDefaultWithMapFromExpression'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.BarBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IPager'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ModelObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.FooBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_specifying_member_and_member_resolver_using_string_property_names'
- PatternRelevancePruner: pruned test type 'AttributeBasedMaps.When_specifying_to_include_all_derived_via_attribute'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.LocalDbContext'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooDtoBase'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.TextBoxControl2'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ShouldUseConstructorInternal'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_using_IMemberResolver_derived_interface'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.ExplicitExpansion.DatabaseInitializer'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.AsShouldWorkOnlyWithDerivedTypes'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.Dto'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Mappers.IChargeCollection'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.ReportMissingIncludeBaseCreateMissingMap'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.SomeDtoJ'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_the_destination_object_has_child_objects'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.FooInputModel'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ProdTypeA'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.DtoSubObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Converter'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MemberResolution.When_source_members_configured_in_a_root_profile_contain_prefixes'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Constructors.When_mapping_to_an_object_with_a_constructor_with_string_optional_arguments'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.ConfigurationValidation.Query'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.IncludeMembersWithGenericsSourceValidation'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.Result'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.When_reverse_mapping_open_generics_with_MapFrom'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MyProfile'
- PatternRelevancePruner: pruned test type 'General.When_mapping_dto_with_an_array_property'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.FillMultidimensionalArray'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.MappingInheritance.BaseEntity'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.NamingConventions.Destination'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.DtoObject'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.InterfaceMapping.IPropertyB'
- PatternRelevancePruner: pruned test type 'AutoMapper.IntegrationTests.SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefault'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.NullBehavior.When_mappping_null_array_to_IEnumerable'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Bug.IgnoreShouldBeInherited'
- PatternRelevancePruner: pruned test type 'AutoMapper.UnitTests.Projection.MapFromTest.Model'
- PatternRelevancePruner: library mode active — boosting public API, penalizing test types
- TokenBudgetEnforcer: kept 404 types (158 pruned for budget 19500)

---
*Generated in 18.7ms | 2713 types (145 active, 2568 pruned) | Compression: TrivialMemberCompressor(−20%) · BoilerplateCompressor(−1%) · StructuralDeduplicator(−27%) | Schema v2.0*
