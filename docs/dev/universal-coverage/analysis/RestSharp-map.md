LIBRARY  RestSharp     (120 public types)

ENTRY API
   annotate  [DeserializeAs]   (DeserializeAsAttribute.cs)
      Allows control how class and property names and values are deserialized by XmlAttributeDeserializer
   annotate  [GenerateClone]   (GenerateImmutableAttribute.cs)
   annotate  [GenerateImmutable]   (GenerateImmutableAttribute.cs)
   annotate  [SerializeAs]   (SerializeAsAttribute.cs)
      Allows control how class and property names and values are serialized by XmlSerializer Currently not supported with t...
   derive    AuthenticatorBase   (AuthenticatorBase.cs)
   implement IAuthenticator   (IAuthenticator.cs)
   implement IDeserializer   (IDeserializer.cs)
   implement IRestSerializer   (IRestSerializer.cs)
   extend    CollectionExtensions   (CollectionExtensions.cs)
   extend    CookieContainerExtensions   (CookieContainerExtensions.cs)
   extend    DotNetXmlSerializerClientExtensions   (DotNetXmlSerializerClientExtensions.cs)
   extend    Extensions   (Extensions.cs)

ABSTRACTIONS
   AuthenticatorBase (class)  — 5 implementors
   IAuthenticator (interface)  — 5 implementors
   IDeserializer (interface)  — 4 implementors
   IRestSerializer (interface)  — 4 implementors
   ISerializer (interface)  — 4 implementors
   ParametersCollection (class)  — 4 implementors
   IRestClient (interface)  — 2 implementors
   IWithDateFormat (interface)  — 2 implementors
   IWithRootElement (interface)  — 2 implementors
   IXmlDeserializer (interface)  — 2 implementors

GENERATORS
   generator   ImmutableGenerator
   generator   InheritedCloneGenerator

PUBLIC SURFACE
   RestSharp
      AsyncHelpers (class):  RunSync
      BodyExtensions (class)
      BodyParameter (record):  BodyParameter
      BuildUriExtensions (class):  DoBuildUriValidations
      ContentType (class):  ContentType, Equals, FromDataFormat, GetHashCode, Or, OrValue, ToString
      DateFormat (struct)
         Format strings for commonly-used date formats
      DefaultParameters (class):  AddParameter, RemoveParameter, ReplaceParameter
      DeserializationException (class)
      Ensure (class):  NotEmptyString, NotNull
      FileParameter (record):  Create, FileParameter, FromFile
         Container for files to be uploaded with requests
      FileParameterOptions (class)
      GetOrPostParameter (record):  GetOrPostParameter
      HeaderParameter (record):  CheckAndThrowsForInvalidHost, EnsureValidHeaderString, EnsureValidHeaderValue, GetBase64EncodedHeaderValue, GetValue, HeaderParameter, IsInvalidHeaderString, PartSplit
      HttpRequestMessageExtensions (class):  AddHeaders
      HttpResponse (record):  Dispose
      IRestClient (interface):  DownloadStreamAsync, ExecuteAsync
      JsonParameter (record):  JsonParameter
      KnownHeaders (class)
      NamedParameter (record)
      Parameter (record):  CreateParameter, Deconstruct, ToString
         Parameter container for REST requests
      ParametersCollection (class):  Exists, GetEnumerator, GetParameters, TryFind
      ParametersCollectionExtensions (class):  IsPost
      Populator (class):  GetEnumerableOf, GetPopulate, GetPopulateArray, GetPopulateKnown, GetPopulateUnknown, GetSingleEnumeratedTypeOrNull, GetStringValue, GetStringValueKnown, GetStringValueUnknown, Populate, PopulateArray, PopulateArrayKnown, PopulateArrayUnknown, PopulateCsv, PopulateCsvKnown
      PropertyCache (class):  GetParameters
      QueryParameter (record):  QueryParameter
      ReadOnlyRestClientOptions (class):  CopyAdditionalProperties, GetInterceptors
      RedirectOptions (class)
         Options for controlling redirect behavior when RestSharp handles redirects.
      RequestContent (class):  AddBody, AddFiles, AddHeaders, AddPostParameters, BodyShouldBeMultipartForm, BuildContent, CreateMultipartFormDataContent, Dispose, GetBoundary, GetOrSetFormBoundary, ReplaceHeader, Serialize, ToStreamContent
      RequestHeaders (class):  AddAcceptHeader, AddCookieHeaders, AddHeaders, RemoveHeader
      RequestParameters (class):  AddParameter, AddParameters, RemoveParameter, RequestParameters
         Collection of request parameters
      RequestProperty (record):  RequestProperty
      ResponseThrowExtension (class)
      RestClient (class):  AddPendingCookies, BuildRedirectHeaders, CombineInterceptors, ConfigureDefaultParameters, ConfigureOptions, ConfigureSerializers, CreateRedirectMessage, Dispose, DisposeContent, DownloadStreamAsync, ExecuteAsync, ExecuteRequestAsync, GetErrorResponse, GetRedirectMethod, OnAfterHttpRequest
      RestClientExtensions (class)
      RestClientOptions (class):  RestClientOptions
      RestRequest (class):  AddParameter, RemoveParameter, RestRequest
         Container for data used to make requests
      RestRequestExtensions (class):  CheckAndThrowsDuplicateKeys
      RestResponse (class):  GetResponseUri, RestResponse
         Container for data sent back from API including deserialized data
      RestResponseBase (class)
         Base class for common properties shared by RestResponse and RestResponse[[T]]
      RestResponseExtensions (class):  NameIs
      RestXmlRequest (class)
      SimpleClientFactory (class):  GetClient
      UriExtensions (class):  AddQueryString, GetUrlSegmentParamsValues, MergeBaseUrlAndResource
      UrlSegmentParameter (record):  Pattern, UrlSegmentParameter
      UrlSegmentParamsValues (record)
      XmlParameter (record):  XmlParameter
   RestSharp.Authenticators
      AuthenticatorBase (class):  Authenticate
      HttpBasicAuthenticator (class):  GetHeader, HttpBasicAuthenticator
         Allows "basic access authentication" for HTTP requests.
      IAuthenticator (interface):  Authenticate
      JwtAuthenticator (class):  GetToken, SetBearerToken
         JSON WEB TOKEN (JWT) Authenticator class.
      OAuth1Authenticator (class):  Authenticate, ForAccessToken, ForAccessTokenRefresh, ForClientAuthentication, ForProtectedResource, ForRequestToken
      ParametersExtensions (class)
   RestSharp.Authenticators.OAuth
      OAuthTools (class):  ConcatenateRequestElements, ConstructRequestUrl, GetHmacSignature, GetNonce, GetSignature, GetTimestamp, OAuthTools, UrlEncodeRelaxed, UrlEncodeStrict
      OAuthWorkflow (class):  BuildAccessTokenSignature, BuildClientAuthAccessTokenSignature, BuildProtectedResourceSignature, BuildRequestTokenSignature, GenerateAuthParameters, GenerateXAuthParameters
         A class to encapsulate OAuth authentication flow.
      WebPair (class):  GetQueryParameter
      WebPairCollection (class):  Add, AddCollection, AddNotEmpty, AddRange, Clear, Contains, CopyTo, GetEnumerator, IndexOf, Insert, Remove, RemoveAt
   RestSharp.Authenticators.OAuth.Extensions
      OAuthExtensions (class):  HashWith, ToRequestValue
      StringExtensions (class)
      TimeExtensions (class):  ToUnixTime
   RestSharp.Authenticators.OAuth2
      OAuth2AuthorizationRequestHeaderAuthenticator (class):  OAuth2AuthorizationRequestHeaderAuthenticator
         The OAuth 2 authenticator using the authorization request header field.
      OAuth2ClientCredentialsAuthenticator (class)
         OAuth 2.0 Client Credentials authenticator.
      OAuth2EndpointAuthenticatorBase (class):  Authenticate, Dispose, GetOrRefreshTokenAsync
         Base class for OAuth 2.0 authenticators that call a token endpoint.
      OAuth2RefreshTokenAuthenticator (class):  OAuth2RefreshTokenAuthenticator
         OAuth 2.0 Refresh Token authenticator.
      OAuth2Token (record)
         Represents an access token with its expiration time.
      OAuth2TokenAuthenticator (class):  Authenticate, Dispose, GetOrRefreshTokenAsync, OAuth2TokenAuthenticator
         Generic OAuth 2.0 authenticator that delegates token acquisition to a user-provided function.
      OAuth2TokenRequest (class):  OAuth2TokenRequest
         Configuration for OAuth 2.0 token endpoint requests.
      OAuth2TokenResponse (record)
         OAuth 2.0 token endpoint response as defined in RFC 6749 Section 5.1.
      OAuth2UriQueryParameterAuthenticator (class):  OAuth2UriQueryParameterAuthenticator
         The OAuth 2 authenticator using URI query parameter.
   RestSharp.Extensions
      CollectionExtensions (class):  ForEach
      CookieContainerExtensions (class):  AddCookies
      Exclude (class)
      GenerateCloneAttribute (class)
      GenerateImmutableAttribute (class)
      HttpHeadersExtensions (class):  GetHeaderParameters
      HttpResponseExtensions (class):  GetResponseString, MaybeException, ReadResponseStream
      ReflectionExtensions (class):  FindEnumValue, GetAttribute, IsSubclassOfRawGeneric
         Reflection extensions
      StreamExtensions (class):  ReadAsBytes
         Extension method overload!
      StringExtensions (class):  AddDashes1, AddDashes2, AddDashes3, AddSpaces1, AddSpaces2, AddSpaces3, AddUnderscores1, AddUnderscores2, AddUnderscores3, IsUpperCase
      WithExtensions (class)
   RestSharp.Extensions.DependencyInjection
      Constants (class):  GetConfigName
      DefaultRestClientFactory (class):  CreateClient
      IRestClientFactory (interface):  CreateClient
      RestClientConfigOptions (class)
      ServiceCollectionExtensions (class)
   RestSharp.Interceptors
      CompatibilityInterceptor (class):  AfterHttpRequest, BeforeDeserialization, BeforeHttpRequest
         This class allows easier migration of legacy request hooks to interceptors.
      Interceptor (class):  AfterHttpRequest, AfterRequest, BeforeDeserialization, BeforeHttpRequest, BeforeRequest
         Base Interceptor
   RestSharp.Serializers
      DeserializeAsAttribute (class)
         Allows control how class and property names and values are deserialized by XmlAttributeDeserializer
      IDeserializer (interface):  Deserialize
      IRestSerializer (interface):  Serialize
      ISerializer (interface):  Serialize
      IWithDateFormat (interface)
      IWithRootElement (interface)
      RestSerializers (class):  DeserializeContent, GetContentDeserializer, GetSerializer, OnBeforeDeserialization, RestSerializers
      SerializeAsAttribute (class):  TransformName
         Allows control how class and property names and values are serialized by XmlSerializer Currently not supported with t...
      SerializerConfig (class):  UseDefaultSerializers, UseSerializer
      SerializerConfigExtensions (class)
      SerializerRecord (record)
   RestSharp.Serializers.CsvHelper
      CsvHelperSerializer (class):  CsvHelperSerializer, Deserialize, Serialize
      RestClientExtensions (class)
   RestSharp.Serializers.Json
      RestClientExtensions (class)
      SystemTextJsonSerializer (class):  Deserialize, Serialize, SystemTextJsonSerializer
   RestSharp.Serializers.NewtonsoftJson
      JsonNetSerializer (class):  Deserialize, JsonNetSerializer, Serialize
      RestClientExtensions (class)
      WriterBuffer (class):  Dispose, GetJsonTextWriter, GetStringWriter, WriterBuffer
   RestSharp.Serializers.Xml
      DotNetXmlDeserializer (class):  Deserialize
         Wrapper for System.Xml.Serialization.XmlSerializer.
      DotNetXmlSerializer (class):  DotNetXmlSerializer, GetXmlSerializer, Serialize
         Wrapper for System.Xml.Serialization.XmlSerializer.
      DotNetXmlSerializerClientExtensions (class):  UseDotNetXmlSerializer
      EncodingStringWriter (class)
      IXmlDeserializer (interface)
      IXmlSerializer (interface)
      XmlAttributeDeserializer (class)
      XmlDeserializer (class):  Deserialize, HandleListDerivative, IsValidXmlElementName, PopulateListFromElements, RemoveNamespace, TryFindElementsByNameVariations, TryGetFromString
      XmlExtensions (class):  AsNamespaced
         XML Extension Methods
      XmlRestSerializer (class):  Serialize, WithXmlDeserializer, WithXmlSerializer, XmlRestSerializer
      XmlSerializer (class):  GetSerializedValue, IsNumeric, Map, Serialize, SerializeNumber, XmlSerializer
         Default XML Serializer
      XmlSerializerClientExtensions (class):  UseXmlSerializer
   SourceGenerator
      Extensions (class):  GetBaseTypesAndThis
   System
      Index (struct):  Equals, FromEnd, FromStart, GetHashCode, GetOffset, Index, ToString
         Represent a type can be used to index a collection either from the start or the end.
      Range (struct):  EndAt, Equals, GetHashCode, GetOffsetAndLength, Range, StartAt, ToString
         Represent a range has start and end indexes.
      Strings (class):  Split

CONSUMER PATHS
   annotate  →  [DeserializeAs] on a partial class/member
   annotate  →  [GenerateClone] on a partial class/member
   annotate  →  [GenerateImmutable] on a partial class/member
   annotate  →  [SerializeAs] on a partial class/member
   extend  →  derive AuthenticatorBase
   contract  →  implement IAuthenticator

PACKAGES
   Utilities:  Newtonsoft.Json
   Other:  CsvHelper, Microsoft.CodeAnalysis.Analyzers, Microsoft.CodeAnalysis.CSharp, Microsoft.Extensions.Http, Nullable, System.Text.Json

→ drill in:  --focus "<TypeName>"   (e.g. --focus AuthenticatorBase)
