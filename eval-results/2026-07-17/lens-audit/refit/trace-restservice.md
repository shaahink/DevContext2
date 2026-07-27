TRACE  RestService
       src/Refit/RestService.cs

▸ ENTRY  RestService
       /// <summary>Creates Refit interface implementations.</summary>
       public static class RestService
       /// <summary>Caches the resolved generated implementation type per interface.</summary>
   ├─ call RestService.ForGenerated  (src/Refit/RestService.cs)
   │      [SuppressMessage(
   │      "Design",
   │      "SST2307:Generic method type parameters should be inferable from the parameters",
   │  ├─ call RestService.settingsFactory  (src/Refit/RestService.cs:91) [approx]
   │  ├─ call RestService.untypedSettingsFactory  (src/Refit/RestService.cs:96) [approx]
   │  ├─ call RestService.untypedFactory  (src/Refit/RestService.cs:101) [approx]
   │  ├─ call RestService.factory  (src/Refit/RestService.cs:106) [approx]
   │  ├─ call RestService.CreateMissingGeneratedFactoryException  (src/Refit/RestService.cs:109) [verified]
   │  │      private static InvalidOperationException CreateMissingGeneratedFactoryException(Type refitInterfaceType)
   │  │      var message =
   │  │      refitInterfaceType.Name
   │  └─ call RestService.CreateHttpClient  (src/Refit/RestService.cs:137) [verified]
   │         public static HttpClient CreateHttpClient(string hostUrl, RefitSettings? settings)
   │         #if NET8_0_OR_GREATER
   │         ArgumentException.ThrowIfNullOrWhiteSpace(hostUrl);
   │     └─ call RefitSettings.HttpMessageHandlerFactory  (src/Refit/RestService.cs:409) [verified]
   │            public Func<HttpMessageHandler>? HttpMessageHandlerFactory { get; set; }
   ├─ call RestService.For  (src/Refit/RestService.cs)
   │      [RequiresUnreferencedCode("Creating a generated client through the reflection path requires runtime type lookup and constructor metadata.")]
   │      public static T For<
   │      [DynamicallyAccessedMembers(
   │  ├─ call RestService.settingsFactory  (src/Refit/RestService.cs:220) [approx]
   │  ├─ call RestService.untypedSettingsFactory  (src/Refit/RestService.cs:225) [approx]
   │  ├─ call RequestBuilder.ForType  (src/Refit/RestService.cs:228) [verified]
   │  │      [SuppressMessage(
   │  │      "Design",
   │  │      "SST2307:Generic method type parameters should be inferable from the parameters",
   │  │  ├─ call RequestBuilderFactory.Create  (src/Refit/RequestBuilder.cs:38) [verified]
   │  │  │      [SuppressMessage(
   │  │  │      "Design",
   │  │  │      "SST2307:Generic method type parameters should be inferable from the parameters",
   │  │  └─ call ReflectionRequestBuilderResolver.GetFactory  (src/Refit/RequestBuilder.cs:38) [verified]
   │  │         [RequiresUnreferencedCode("The reflection request builder requires runtime type lookup and request metadata.")]
   │  │         internal static IRequestBuilderFactory GetFactory() => _factory ??= CreateFactory();
   │  │     └─ call ReflectionRequestBuilderResolver.CreateFactory  (src/Refit/ReflectionRequestBuilderResolver.cs:26) [verified]
   │  │            [RequiresUnreferencedCode("The reflection request builder requires runtime type lookup and request metadata.")]
   │  │            [ExcludeFromCodeCoverage] // The not-installed throw is unreachable in-process: Refit.Reflection is always present when this resolver runs.
   │  │            private static IRequestBuilderFactory CreateFactory() =>
   │  ├─ call RestService.CreateHttpClient  (src/Refit/RestService.cs:266) [verified]
   │  │      public static HttpClient CreateHttpClient(string hostUrl, RefitSettings? settings)
   │  │      #if NET8_0_OR_GREATER
   │  │      ArgumentException.ThrowIfNullOrWhiteSpace(hostUrl);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  └─ call RestService.factory  (src/Refit/RestService.cs:304) [approx]
   ├─ call RestService.CreateHttpClient  (src/Refit/RestService.cs)
   │  (stopped at depth 1; 1 branch omitted)
   ├─ call RestService.GetGeneratedType  (src/Refit/RestService.cs)
   │      [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
   │      [RequiresUnreferencedCode("Resolving a generated client type by name requires runtime type lookup.")]
   │      private static Type GetGeneratedType(
   │  └─ call RestService.CreateMissingGeneratedFactoryException  (src/Refit/RestService.cs:442) [verified]
   │         private static InvalidOperationException CreateMissingGeneratedFactoryException(Type refitInterfaceType)
   │         var message =
   │         refitInterfaceType.Name
   ├─ call RestService.RegisterGeneratedFactory  (src/Refit/RestService.cs)
   │      [EditorBrowsable(EditorBrowsableState.Never)]
   │      public static void RegisterGeneratedFactory(
   │      Type refitInterfaceType,
   │  └─ call RestService.factory  (src/Refit/RestService.cs:48) [approx]
   └─ call RestService.RegisterGeneratedSettingsFactory  (src/Refit/RestService.cs)
          [EditorBrowsable(EditorBrowsableState.Never)]
          public static void RegisterGeneratedSettingsFactory<T>(Func<HttpClient, RefitSettings, T> factory)
          ArgumentExceptionHelper.ThrowIfNull(factory);
      └─ call RestService.factory  (src/Refit/RestService.cs:60) [approx]
