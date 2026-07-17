LIBRARY  Refit     (88 public types)

ENTRY API
   annotate  [AliasAs]   (AliasAsAttribute.cs)
      Override the key that will be sent in the query string.
   annotate  [AttachmentName]   (AttachmentNameAttribute.cs)
      Names an attachment in a multipart request.
   annotate  [Authorize]   (AuthorizeAttribute.cs)
      Add the Authorize header to the request with the value of the associated parameter.
   annotate  [Body]   (BodyAttribute.cs)
      Set a parameter to be sent as the HTTP request's body.
   annotate  [Delete]   (DeleteAttribute.cs)
      Send the request with HTTP method 'DELETE'.
   annotate  [Encoded]   (EncodedAttribute.cs)
      Marks a parameter value as already URL-encoded so Refit passes it through verbatim instead of escaping it — the equiv...
   annotate  [FormObject]   (FormObjectAttribute.cs)
      Marks a complex-object parameter of a [Multipart] method so its public properties are written as individual multipart...
   annotate  [Get]   (GetAttribute.cs)
      Send the request with HTTP method 'GET'.
   annotate  [Head]   (HeadAttribute.cs)
      Send the request with HTTP method 'HEAD'.
   annotate  [Header]   (HeaderAttribute.cs)
      Add a header to the request.
   annotate  [HeaderCollection]   (HeaderCollectionAttribute.cs)
      Allows you to provide a Dictionary of headers to be added to the request.
   annotate  [Headers]   (HeadersAttribute.cs)
      Add multiple headers to the request.

ABSTRACTIONS
   IHttpContentSerializer (interface)  — 11 implementors
   IReturnTypeAdapter (interface)  — 11 implementors
   HttpMethodAttribute (class)  — 8 implementors
   IRequestBuilder (interface)  — 8 implementors
   DefaultUrlParameterFormatter (class)  — 6 implementors
   IUrlParameterFormatter (interface)  — 6 implementors
   IUrlParameterKeyFormatter (interface)  — 5 implementors
   IFormUrlEncodedParameterFormatter (interface)  — 4 implementors
   IApiResponse (interface)  — 3 implementors
   ISynchronousContentDeserializer (interface)  — 3 implementors

GENERATORS
   generator   InterfaceStubGeneratorV2
      An incremental source generator that produces Refit interface stub implementations.
   analyzer    RefitInterfaceAnalyzer
      Analyzes Refit interface contracts independently of the source generation path.
   code-fixer  RefitInterfaceCodeFixProvider
      Provides safe code fixes for Refit interface analyzer diagnostics.

PUBLIC SURFACE
   Refit
      AliasAsAttribute (class)
         Override the key that will be sent in the query string.
      ApiException (class):  Create, GetContentAs, GetContentAsAsync, TryGetContentAs
         Represents an error that occurred after a response was received from the server.
      ApiExceptionBase (class)
         Represents an error that occurred while sending an API request.
      ApiRequestException (class):  ApiRequestException
         Represents an error that occurred while sending an API request before a response could be received from the server.
      ApiResponseExtensions (class)
         Convenience helpers for working with and .
      AttachmentNameAttribute (class)
         Names an attachment in a multipart request.
      AuthorizeAttribute (class)
         Add the Authorize header to the request with the value of the associated parameter.
      BodyAttribute (class):  BodyAttribute
         Set a parameter to be sent as the HTTP request's body.
      ByteArrayPart (class)
         Allows the use of a array in a multipart form body.
      CamelCaseUrlParameterKeyFormatter (class):  Format
         Provides an implementation of that formats URL parameter keys in camelCase.
      DefaultApiExceptionFactory (class):  CreateAsync
         Default Api exception factory.
      DefaultFormUrlEncodedParameterFormatter (class):  Format
         Default form Url-encoded parameter formatter.
      … and 66 more (use --format json for the full surface)
   Refit.Generator
      Enumerator (record):  MoveNext
         A struct enumerator that iterates the backing array without allocation.
      UniqueNameBuilder (class):  New, Reserve
         Builds unique identifier names within a nested scope hierarchy, ensuring generated members do not collide with names ...
      WellKnownTypes (class):  Get, TryGet
         Resolves and caches well-known named type symbols from a compilation.
   Refit.Testing
      NetworkBehavior (class):  CreateErrorResponse, CreateFailure, NetworkBehavior, NextDelay, NextIsError, NextIsFailure
         Deterministic network-condition simulation for , modelled on Retrofit's NetworkBehavior.
      Reply (class):  Content, From, Json, Status, Text, With
         Factory for the a route returns.
      Route (class):  Any, Delete, Fallback, For, Get, Head, Patch, Post, Put
         Factory for the common shapes, one per HTTP method.
      RouteMatcher (class)
         Matches an incoming request by HTTP method and a path that mirrors the [Get("/users/{id}")] attributes on a Refit int...
      StubApiResponse (class):  Dispose, HasRequestError, HasResponseError
         A hand-written for unit-testing code that consumes a Refit interface returning or , without going through HTTP.
      StubHttp (class):  Add, CreateClient, CreateGeneratedClient, GetEnumerator, LastRequestBodyAsync, RequestBodyAsync, StubHttp, ToSettings, VerifyAllCalled, VerifyAllCalledAsync
         A declarative test for Refit clients, written as a route table: each entry pairs a (built with ) with a (built with ).
      StubResponse (class)
         The response a returns for a matched .

CONSUMER PATHS
   annotate  →  [AliasAs] on a partial class/member
   annotate  →  [AttachmentName] on a partial class/member
   annotate  →  [Authorize] on a partial class/member
   annotate  →  [Body] on a partial class/member
   annotate  →  [Delete] on a partial class/member
   annotate  →  [Encoded] on a partial class/member

PACKAGES
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  Microsoft.CodeAnalysis.Analyzers, Microsoft.CodeAnalysis.CSharp, Microsoft.CodeAnalysis.CSharp.Workspaces, Microsoft.Extensions.Http, Microsoft.NETFramework.ReferenceAssemblies, ReactiveUI.Primitives, System.Net.Http.Json, System.Net.ServerSentEvents … (9 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus AliasAsAttribute)
