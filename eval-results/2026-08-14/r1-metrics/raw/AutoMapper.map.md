LIBRARY  AutoMapper     (146 public types)

ENTRY API
   register  ServiceCollectionExtensions.AddAutoMapper   
(ServiceCollectionExtensions.cs)
   implement IMemberValueResolver   (IMemberConfigurationExpression.cs)
      Extension point to provide custom resolution for a destination value
   implement ITypeConverter   (IMappingExpressionBase.cs)
      Converts source type to destination type instead of normal member mapping
   implement IValueResolver   (TypeMapPlanBuilder.cs)
   derive    Profile   (Profile.cs)
      Provides a named configuration for maps.
   extend    ExpressionBuilder   (ExpressionBuilder.cs)
   extend    Extensions   (Extensions.cs)
      Queryable extensions for AutoMapper
   extend    FeatureExtensions   (Features.cs)
   extend    ValueTransformerConfigurationExtensions   (MemberMap.cs)

ABSTRACTIONS
   ITypeConverter (interface)  - 38 implementors
   Profile (class)  - 36 implementors
   IValueResolver (interface)  - 31 implementors
   IMemberValueResolver (interface)  - 29 implementors
   IValueConverter (interface)  - 29 implementors
   IMemberConfigurationProvider (interface)  - 8 implementors
   IProjectionMapper (interface)  - 6 implementors
   IMappingAction (interface)  - 5 implementors
   INamingConvention (interface)  - 5 implementors
   ICondition (interface)  - 3 implementors

PUBLIC SURFACE
   AutoMapper
      AutoMapAttribute (class):  ApplyConfiguration
         Auto map to this destination type from the specified source type.
      AutoMapperConfigurationException (class):  
AutoMapperConfigurationException
      AutoMapperMappingException (class):  AutoMapperMappingException
         Wraps mapping exceptions.
      ConstructorMap (class):  AddParameter, ApplyMap, ParametersCanResolve, 
Reset
      ConstructorParameterMap (class):  ApplyMap, ConstructorParameterMap, 
DefaultValue, ToString
      ContextCacheKey (record):  Equals, GetHashCode
      DuplicateTypeMapConfigurationException (class):  GetErrors
      ExactMatchNamingConvention (class):  Split
      ICondition (interface):  Evaluate
         Condition to determine if a destination member should be mapped.
      IConfigurationProvider (interface):  AssertConfigurationIsValid, 
BuildExecutionPlan, CompileMappings, CreateMapper
      IDestinationFactory (interface):  Construct
         Custom destination factory for instantiating destination objects with 
dependency injection support.
      IMapper (interface):  Map, ProjectTo
      . and 45 more (the structured surface lists them all)
   AutoMapper.Configuration
      ConditionParameters (record)
      ConfigurationValidator (class):  AssertConfigurationExpressionIsValid, 
AssertConfigurationIsValid
      CtorParamConfigurationExpression (class):  Configure, ExplicitExpansion, 
MapFrom
      ICtorParamConfigurationExpression (interface):  ExplicitExpansion, MapFrom
      ICtorParamConfigurationExpression (interface):  MapFrom
      ICtorParameterConfiguration (interface):  Configure
      IMemberConfigurationProvider (interface):  ApplyConfiguration
      IPathConfigurationExpression (interface):  Condition, Ignore, MapFrom
         Member configuration options
      IPropertyMapConfiguration (interface):  Configure, 
GetDestinationExpression, Reverse
      ISourceMemberConfiguration (interface):  Configure
      ISourceMemberConfigurationExpression (interface):  DoNotValidate
         Source member configuration options
      MappingExpression (class):  ForAllMembers, ForMember, IncludeMembers, 
MappingExpression, ReverseMap
      . and 9 more (the structured surface lists them all)
   AutoMapper.Configuration.Annotations
      IgnoreAttribute (class):  ApplyConfiguration
         Ignore this member for configuration validation and skip during 
mapping.
      MapAtRuntimeAttribute (class):  ApplyConfiguration
         Do not precompute the execution plan for this member, just map it at 
runtime.
      MappingOrderAttribute (class):  ApplyConfiguration
         Supply a custom mapping order instead of what the .NET runtime returns
      NullSubstituteAttribute (class):  ApplyConfiguration
         Substitute a custom value when the source member resolves as null
      SourceMemberAttribute (class):  ApplyConfiguration
         Specify the source member to map from.
      UseExistingValueAttribute (class):  ApplyConfiguration
         Use the destination value instead of mapping from the source value or 
creating a new instance
      ValueConverterAttribute (class):  ApplyConfiguration
         Specify a value converter type to convert from the matching source 
member to the destination member Use with to speci...
      ValueResolverAttribute (class):  ApplyConfiguration
         Map destination member using a custom value resolver.
   AutoMapper.Configuration.Conventions
      ConventionsNameSplitMember (class):  IsMatch
      DefaultNameSplitMember (class):  IsMatch
      ISourceToDestinationNameMapper (interface):  GetSourceMember, Merge
      MemberConfiguration (class):  GetSourceMember, IsMatch, Merge, Seal
      MemberNameReplacer (record)
      NameSplitMember (class):  IsMatch
      PrePostfixName (class):  GetSourceMember, Merge
      ReplaceName (class):  GetSourceMember, Merge
   AutoMapper.Execution
      ClassTypeConverter (class):  CloseGenerics, GetExpression
      ClassValueResolver (class):  ClassValueResolver, GetExpression, 
GetSourceMember
      ConvertParameterReplaceVisitor (class):  Replace
      ExpressionBuilder (class):  ApplyTransformers, Call, Chain, CheckContext, 
ContextMap, ConvertReplaceParameters, Default, ForEach, GetChain, GetMember, 
GetMemberExpressions, GetMembersChain, IfNullElse, IsMemberPath, Lambda
      ExpressionResolver (class):  GetExpression, GetSourceMember
      ExpressionTypeConverter (class)
      FuncResolver (class):  GetExpression, GetSourceMember
      IValueResolver (interface):  CloseGenerics, GetExpression, GetSourceMember
      LambdaTypeConverter (class):  GetExpression
      LambdaValueResolver (class)
      Member (record)
      MemberPathResolver (class):  CloseGenerics, GetExpression, GetSourceMember
      . and 13 more (the structured surface lists them all)
   AutoMapper.Features
      FeatureExtensions (class):  ReverseTo, SetFeature
      Features (class):  Get, GetEnumerator, Set
      IGlobalFeature (interface):  Configure
      IMappingFeature (interface):  Configure, Reverse
      IRuntimeFeature (interface):  Seal
   AutoMapper.QueryableExtensions
      Extensions (class):  ProjectTo, Select, ToCore
         Queryable extensions for AutoMapper
      MemberVisitor (class):  GetMemberPath
   AutoMapper.QueryableExtensions.Impl
      AssignableProjectionMapper (class):  IsMatch, Project
      ConstantVisitor (class)
      EnumProjectionMapper (class):  IsMatch, Project
      EnumerableProjectionMapper (class):  IsMatch, Project
      FirstPassLetPropertyMaps (class):  GetSubQueryExpression, 
GetSubQueryMarker, New
      GePropertiesVisitor (class):  Retrieve
      IProjectionBuilder (interface):  CreateProjection, GetProjection
      IProjectionMapper (interface):  IsMatch, Project
      LetPropertyMaps (class):  GetCurrentPath, GetSubQueryExpression, 
GetSubQueryMarker, IncrementDepth, New, Pop, Push
      MemberProjection (record)
      NullableSourceProjectionMapper (class):  IsMatch, Project
      ParameterVisitor (class):  SetParameters
      . and 7 more (the structured surface lists them all)
   Microsoft.Extensions.DependencyInjection
      ServiceCollectionExtensions (class):  AddAutoMapper
         Extensions to scan for AutoMapper classes and register the 
configuration, mapping, and extensions with the service co...
   INTERNAL  (35 types in *.Internal - available on request)

CONSUMER PATHS
   wire into DI    ServiceCollectionExtensions.AddAutoMapper(...)
   contract    implement IMemberValueResolver
   contract    implement ITypeConverter
   contract    implement IValueResolver
   extend    derive Profile
   configure    ExpressionBuilder.*

PACKAGES
   Other:  Microsoft.Bcl.HashCode [6.0.0, ), 
Microsoft.Extensions.Logging.Abstractions [10.0.0, ), 
Microsoft.Extensions.Options [10.0.0, ), Microsoft.IdentityModel.JsonWebTokens 
[8.14.0, ), MinVer 6.0.0, PolySharp 1.15.0, System.Reflection.Emit [4.7.0, )

 drill in:  trace a focused type   (e.g. trace ServiceCollectionExtensions)

