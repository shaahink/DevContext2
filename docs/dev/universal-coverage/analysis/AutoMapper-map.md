LIBRARY  AutoMapper     (138 public types)

ENTRY API
   register  ServiceCollectionExtensions.AddAutoMapper   (ServiceCollectionExtensions.cs)
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
   ITypeConverter (interface)  — 36 implementors
   IValueResolver (interface)  — 31 implementors
   Profile (class)  — 31 implementors
   IMemberValueResolver (interface)  — 24 implementors
   IMemberConfigurationProvider (interface)  — 8 implementors
   IValueConverter (interface)  — 8 implementors
   IProjectionMapper (interface)  — 6 implementors
   INamingConvention (interface)  — 5 implementors
   ICondition (interface)  — 3 implementors
   IMappingAction (interface)  — 3 implementors

PUBLIC SURFACE
   AutoMapper
      AutoMapAttribute (class):  ApplyConfiguration
         Auto map to this destination type from the specified source type.
      AutoMapperConfigurationException (class):  AutoMapperConfigurationException
      AutoMapperMappingException (class):  AutoMapperMappingException
         Wraps mapping exceptions.
      ConstructorMap (class):  AddParameter, ApplyMap, ParametersCanResolve, Reset
      ConstructorParameterMap (class):  ApplyMap, ConstructorParameterMap, DefaultValue, ToString
      ContextCacheKey (record):  Equals, GetHashCode
      DuplicateTypeMapConfigurationException (class):  GetErrors
      ExactMatchNamingConvention (class):  Split
      ICondition (interface):  Evaluate
         Condition to determine if a destination member should be mapped.
      IConfigurationProvider (interface):  AssertConfigurationIsValid, BuildExecutionPlan, CompileMappings, CreateMapper
      IDestinationFactory (interface):  Construct
         Custom destination factory for instantiating destination objects with dependency injection support.
      IMapper (interface):  Map, ProjectTo
      IMapperBase (interface):  Map
      IMapperConfigurationExpression (interface):  AddMaps, AddProfile, AddProfiles, ConstructServicesUsing, CreateProfile
      IMappingAction (interface):  Process
         Custom mapping action
      IMappingExpression (interface):  As, ForAllMembers, ForMember, ForPath, ForSourceMember, Include, IncludeBase, IncludeMembers, ReverseMap
         Mapping configuration options for non-generic maps
      IMappingExpressionBase (interface):  AfterMap, As, AsProxy, BeforeMap, ConstructUsing, ConstructUsingServiceLocator, ConvertUsing, DisableCtorValidation, ForSourceMember, IgnoreAllPropertiesWithAnInaccessibleSetter, IgnoreAllSourcePropertiesWithAnInaccessibleSetter, Include, IncludeAllDerived, IncludeBase, PreserveReferences
         Common mapping configuration options between generic and non-generic mapping configuration
      IMappingOperationOptions (interface):  AfterMap, BeforeMap, ConstructServicesUsing
         Options for a single map operation
      IMemberConfigurationExpression (interface):  Condition, ConvertUsing, DoNotUseDestinationValue, MapAtRuntime, MapFrom, PreCondition, SetMappingOrder, UseDestinationValue
         Member configuration options
      IMemberValueResolver (interface):  Resolve
         Extension point to provide custom resolution for a destination value
      INamingConvention (interface):  Split
         Defines a naming convention strategy
      IPreCondition (interface):  Evaluate
         Pre-condition evaluated before source member resolution.
      IProfileConfiguration (interface)
      IProfileExpression (interface):  AddGlobalIgnore, ClearPrefixes, CreateMap, CreateProjection, DisableConstructorMapping, IncludeSourceExtensionMethods, RecognizeDestinationPostfixes, RecognizeDestinationPrefixes, RecognizePostfixes, RecognizePrefixes, ReplaceMemberName
         Configuration for profile-specific maps
      IProjectionExpression (interface):  AddTransform, ForMember, IncludeMembers
      IProjectionExpressionBase (interface):  ConstructUsing, ConvertUsing, ForCtorParam, MaxDepth, ValidateMemberList
         Common mapping configuration options between generic and non-generic mapping configuration
      IProjectionMemberConfiguration (interface):  AddTransform, AllowNull, DoNotAllowNull, ExplicitExpansion, Ignore, MapFrom, NullSubstitute
         Member configuration options
      IRuntimeMapper (interface)
      ITypeConverter (interface):  Convert
         Converts source type to destination type instead of normal member mapping
      IValueConverter (interface):  Convert
         Converts a source member value to a destination member value
      IValueResolver (interface):  Resolve
         Extension point to provide custom resolution for a destination value
      IncludedMember (record):  Chain, Equals, GetHashCode, IncludedMember
      LazyValue (struct)
      LowerUnderscoreNamingConvention (class):  Split
      Mapper (class):  Map, Mapper, ProjectTo
      MapperConfiguration (class):  AssertConfigurationIsValid, Build, BuildExecutionPlan, CompileMappings, ConvertParameterReplaceVisitor, CreateMapper, CreateProjectionBuilder, FindMapper, FindTypeMapFor, GetAllTypeMaps, GetDefault, GetExecutionPlan, GetIncludedTypeMap, GetIncludedTypeMaps, GetMappers
      MapperConfigurationExpression (class):  AddMaps, AddProfile, AddProfiles, ConstructServicesUsing, CreateProfile, Validator
      MappingOperationOptions (class):  AfterMap, BeforeMap, ConstructServicesUsing
      MemberMap (class):  ChainSourceMembers, GetExpression, GetSourceMember, GetSourceMemberName, MapByConvention, MapFrom, SetResolver, ToString, Types
         The base class for member maps (property, constructor and path maps).
      MemberMapDetails (class):  AddValueTransformation, ApplyInheritedPropertyMap
      PascalCaseNamingConvention (class):  Split
      PathMap (class):  PathMap
      Profile (class):  AddGlobalIgnore, ClearPrefixes, CreateMap, CreateProjection, DisableConstructorMapping, ForAllMaps, ForAllPropertyMaps, IncludeSourceExtensionMethods, RecognizeDestinationPostfixes, RecognizeDestinationPrefixes, RecognizePostfixes, RecognizePrefixes, ReplaceMemberName
         Provides a named configuration for maps.
      ProfileMap (class):  AllowsNullCollectionsFor, AllowsNullDestinationValuesFor, Configure, CreateClosedGenericTypeMap, CreateTypeDetails, GetGenericMap, MapDestinationPropertyToSource, ProfileMap, Register
      PropertyMap (class):  AddValueTransformation, ApplyInheritedPropertyMap, PropertyMap
      PropertyMapAction (record)
      ResolutionContext (class):  Map, TryGetItems
         Context information regarding resolution of a destination value
      TypeMap (class):  AddAfterMapAction, AddBeforeMapAction, AddInheritedMap, AddMemberMap, AddValueTransformation, AsProxy, CheckProjection, CheckRecord, CloseGenerics, ConstructUsingObjectConstructor, ConstructUsingServiceLocator, ConstructorParameterMatches, FindOrCreatePathMapFor, FindOrCreatePropertyMapFor, FindOrCreateSourceMemberConfigFor
         Main configuration object holding all mapping configuration for a source and destination type
      TypeMapConfigErrors (record)
      TypeMapDetails (class):  AddAfterMapAction, AddBeforeMapAction, AddInheritedMap, AddMemberMap, AddValueTransformation, ApplyInheritedMapActions, FindOrCreatePathMapFor, FindOrCreateSourceMemberConfigFor, IncludeBaseTypes, IncludeDerivedTypes, Seal
      ValueTransformerConfiguration (record):  IsMatch
      ValueTransformerConfigurationExtensions (class):  Add
   AutoMapper.Configuration
      ConditionParameters (record)
      ConfigurationValidator (class):  AssertConfigurationExpressionIsValid, AssertConfigurationIsValid
      CtorParamConfigurationExpression (class):  Configure, ExplicitExpansion, MapFrom
      ICtorParamConfigurationExpression (interface):  ExplicitExpansion, MapFrom
      ICtorParameterConfiguration (interface):  Configure
      IMemberConfigurationProvider (interface):  ApplyConfiguration
      IPathConfigurationExpression (interface):  Condition, Ignore, MapFrom
         Member configuration options
      IPropertyMapConfiguration (interface):  Configure, GetDestinationExpression, Reverse
      ISourceMemberConfiguration (interface):  Configure
      ISourceMemberConfigurationExpression (interface):  DoNotValidate
         Source member configuration options
      MappingExpression (class):  AddTransform, As, ConstructUsing, ForAllMembers, ForCtorParam, ForMember, ForPath, ForSourceMember, Include, IncludeBase, IncludeMembers, MappingExpression, MaxDepth, ReverseMap, ValidateMemberList
      MappingExpressionBase (class):  AfterMap, As, AsProxy, BeforeMap, ConstructUsing, ConstructUsingServiceLocator, ConvertUsing, DisableCtorValidation, ForCtorParam, ForSourceMember, IgnoreAllPropertiesWithAnInaccessibleSetter, IgnoreAllSourcePropertiesWithAnInaccessibleSetter, Include, IncludeAllDerived, IncludeBase
      MemberConfigurationExpression (class):  AddTransform, AllowNull, Condition, Configure, ConvertUsing, DoNotAllowNull, DoNotUseDestinationValue, ExplicitExpansion, GetDestinationExpression, Ignore, MapAtRuntime, MapFrom, NullSubstitute, PreCondition, Reverse
      PathConfigurationExpression (class):  Condition, Configure, GetDestinationExpression, Ignore, MapFrom, MapFromUntyped, Reverse
      SourceMappingExpression (class):  Configure, DoNotValidate
      SourceMemberConfig (class)
         Contains member configuration relating to source members
      TypeMapConfiguration (class):  Configure, GetDestinationMemberConfiguration
      ValidationContext (record)
   AutoMapper.Configuration.Annotations
      IgnoreAttribute (class):  ApplyConfiguration
         Ignore this member for configuration validation and skip during mapping.
      MapAtRuntimeAttribute (class):  ApplyConfiguration
         Do not precompute the execution plan for this member, just map it at runtime.
      MappingOrderAttribute (class):  ApplyConfiguration
         Supply a custom mapping order instead of what the .NET runtime returns
      NullSubstituteAttribute (class):  ApplyConfiguration
         Substitute a custom value when the source member resolves as null
      SourceMemberAttribute (class):  ApplyConfiguration
         Specify the source member to map from.
      UseExistingValueAttribute (class):  ApplyConfiguration
         Use the destination value instead of mapping from the source value or creating a new instance
      ValueConverterAttribute (class):  ApplyConfiguration
         Specify a value converter type to convert from the matching source member to the destination member Use with to speci...
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
      ClassValueResolver (class):  ClassValueResolver, GetExpression, GetSourceMember
      ConvertParameterReplaceVisitor (class):  Replace
      ExpressionBuilder (class):  ApplyTransformers, Call, Chain, CheckContext, ContextMap, ConvertReplaceParameters, Default, ForEach, GetChain, GetMember, GetMemberExpressions, GetMembersChain, IfNullElse, IsMemberPath, Lambda
      ExpressionResolver (class):  GetExpression, GetSourceMember
      ExpressionTypeConverter (class)
      FuncResolver (class):  GetExpression, GetSourceMember
      IValueResolver (interface):  CloseGenerics, GetExpression, GetSourceMember
      LambdaTypeConverter (class):  GetExpression
      LambdaValueResolver (class)
      Member (record)
      MemberPathResolver (class):  CloseGenerics, GetExpression, GetSourceMember
      ParameterReplaceVisitor (class)
      PropertyDescription (record):  PropertyDescription
      PropertyEmitter (class):  PropertyEmitter
      ProxyBase (class):  ProxyBase
      ProxyGenerator (class):  GetProxyType, GetSimilarType
      ReplaceVisitor (class):  Visit
      ReplaceVisitorBase (class):  Replace
      TypeConverter (class):  CloseGenerics, GetExpression
      TypeDescription (record):  Equals, GetHashCode
      TypeMapPlanBuilder (struct):  CreateMapperLambda, DestinationMemberValue, GetParameters, IncludeMembers, MapMember, Precondition, ReplaceParameters, SetVariables
      ValueConverter (class):  GetExpression, GetSourceMember, ValueConverter
      ValueResolverConfig (class)
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
      FirstPassLetPropertyMaps (class):  GetSubQueryExpression, GetSubQueryMarker, New
      GePropertiesVisitor (class):  Retrieve
      IProjectionBuilder (interface):  CreateProjection, GetProjection
      IProjectionMapper (interface):  IsMatch, Project
      LetPropertyMaps (class):  GetCurrentPath, GetSubQueryExpression, GetSubQueryMarker, IncrementDepth, New, Pop, Push
      MemberProjection (record)
      NullableSourceProjectionMapper (class):  IsMatch, Project
      ParameterVisitor (class):  SetParameters
      ProjectionBuilder (class):  CannotMap, CreateProjection, GetProjection, PolymorphicMaps, ProjectionBuilder
      ProjectionRequest (record):  Equals, GetHashCode, InnerRequest, ShouldExpand
      PropertyVisitor (class)
      QueryExpressions (record):  Chain, QueryExpressions
      ReplaceMemberAccessesVisitor (class)
      StringProjectionMapper (class):  IsMatch, Project
      SubQueryPath (record):  GetPropertyDescription, GetSourceExpression, SubQueryPath
   Microsoft.Extensions.DependencyInjection
      ServiceCollectionExtensions (class):  AddAutoMapper
         Extensions to scan for AutoMapper classes and register the configuration, mapping, and extensions with the service co...
   System.Runtime.CompilerServices
      IsExternalInit (class)
   INTERNAL  (33 types in *.Internal — available on request)

CONSUMER PATHS
   wire into DI  →  ServiceCollectionExtensions.AddAutoMapper(...)
   contract  →  implement IMemberValueResolver
   contract  →  implement ITypeConverter
   contract  →  implement IValueResolver
   extend  →  derive Profile
   configure  →  ExpressionBuilder.*

PACKAGES
   Other:  Microsoft.Bcl.HashCode [6.0.0, ), Microsoft.Extensions.Logging.Abstractions [10.0.0, ), Microsoft.Extensions.Options [10.0.0, ), Microsoft.IdentityModel.JsonWebTokens [8.14.0, ), MinVer 6.0.0, PolySharp 1.15.0, System.Reflection.Emit [4.7.0, )

→ drill in:  --focus "<TypeName>"   (e.g. --focus ServiceCollectionExtensions)
