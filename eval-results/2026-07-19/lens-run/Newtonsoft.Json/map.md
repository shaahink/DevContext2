LIBRARY  Newtonsoft.Json     (97 public types)

ENTRY API
   derive    DefaultContractResolver   (DefaultContractResolver.cs)
      Used by to resolve a for a given .
   derive    JsonConverter   (JsonConverter.cs)
      Converts an object to and from JSON.
   derive    JsonReader   (JsonReader.cs)
      Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
   derive    JsonWriter   (JsonWriter.cs)
      Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
   extend    Extensions   (Extensions.cs)
      Contains the LINQ to JSON extension methods.

ABSTRACTIONS
   JsonConverter (class)  — 55 implementors
   DefaultContractResolver (class)  — 22 implementors
   JsonReader (class)  — 9 implementors
   JsonWriter (class)  — 7 implementors
   CustomCreationConverter (class)  — 6 implementors
   IJsonLineInfo (interface)  — 5 implementors
   ISerializationBinder (interface)  — 5 implementors
   ITraceWriter (interface)  — 5 implementors
   JContainer (class)  — 4 implementors
   JsonException (class)  — 4 implementors

PUBLIC SURFACE
   Newtonsoft.Json
      DefaultJsonNameTable (class):  Add, DefaultJsonNameTable, Get
         The default JSON name table implementation.
      IArrayPool (interface):  Rent, Return
         Provides an interface for using pooled arrays.
      IJsonLineInfo (interface):  HasLineInfo
         Provides an interface to enable a class to return line and position information.
      JsonArrayAttribute (class):  JsonArrayAttribute
         Instructs the how to serialize the collection.
      JsonConstructorAttribute (class)
         Instructs the to use the specified constructor when deserializing that object.
      JsonContainerAttribute (class)
         Instructs the how to serialize the object.
      JsonConvert (class):  DeserializeAnonymousType, DeserializeObject, PopulateObject, SerializeObject, ToString
         Provides methods for converting between .NET types and JSON types.
      JsonConverter (class):  CanConvert, ReadJson, WriteJson
         Converts an object to and from JSON.
      JsonConverterAttribute (class):  JsonConverterAttribute
         Instructs the to use the specified when serializing the member or class.
      JsonConverterCollection (class)
         Represents a collection of .
      JsonDictionaryAttribute (class):  JsonDictionaryAttribute
         Instructs the how to serialize the collection.
      JsonException (class):  JsonException
         The exception thrown when an error occurs during JSON serialization or deserialization.
      … and 16 more (use --format json for the full surface)
   Newtonsoft.Json.Bson
      BsonObjectId (class):  BsonObjectId
         Represents a BSON Oid (object id).
      BsonReader (class):  BsonReader, Close, Read
         Represents a reader that provides fast, non-cached, forward-only access to serialized BSON data.
      BsonWriter (class):  BsonWriter, Close, Flush, WriteComment, WriteNull, WriteObjectId, WritePropertyName, WriteRaw, WriteRawValue, WriteRegex, WriteStartArray, WriteStartConstructor, WriteStartObject, WriteUndefined, WriteValue
         Represents a writer that provides a fast, non-cached, forward-only way of generating BSON data.
   Newtonsoft.Json.Converters
      BsonObjectIdConverter (class):  CanConvert, ReadJson, WriteJson
         Converts a to and from JSON and BSON.
      CustomCreationConverter (class):  CanConvert, Create, ReadJson, WriteJson
         Creates a custom object.
      DateTimeConverterBase (class):  CanConvert
         Provides a base class for converting a to and from JSON.
      IsoDateTimeConverter (class):  ReadJson, WriteJson
         Converts a to and from the ISO 8601 date format (e.g.
      JavaScriptDateTimeConverter (class):  ReadJson, WriteJson
         Converts a to and from a JavaScript Date constructor (e.g.
      KeyValuePairConverter (class):  CanConvert, ReadJson, WriteJson
         Converts a to and from JSON.
      RegexConverter (class):  CanConvert, ReadJson, WriteJson
         Converts a to and from JSON and BSON.
      StringEnumConverter (class):  CanConvert, ReadJson, StringEnumConverter, WriteJson
         Converts an to and from its name string value.
      UnixDateTimeConverter (class):  ReadJson, UnixDateTimeConverter, WriteJson
         Converts a to and from Unix epoch time
      VersionConverter (class):  CanConvert, ReadJson, WriteJson
         Converts a to and from a string (e.g.
   Newtonsoft.Json.Linq
      Extensions (class):  Ancestors, AncestorsAndSelf, AsJEnumerable, Children, Descendants, DescendantsAndSelf, Properties, Value, Values
         Contains the LINQ to JSON extension methods.
      IJEnumerable (interface)
         Represents a collection of objects.
      JArray (class):  Add, Clear, Contains, CopyTo, FromObject, GetEnumerator, IndexOf, Insert, JArray, Load, Parse, Remove, RemoveAt, WriteTo
         Represents a JSON array.
      JConstructor (class):  JConstructor, Load, WriteTo
         Represents a JSON constructor.
      JContainer (class):  Add, AddFirst, Children, Clear, Contains, CopyTo, CreateWriter, Descendants, DescendantsAndSelf, IndexOf, Insert, Merge, Remove, RemoveAll, RemoveAt
         Represents a token that can contain other tokens.
      JEnumerable (struct):  Equals, GetEnumerator, GetHashCode, JEnumerable
         Represents a collection of objects.
      JObject (class):  Add, Clear, Contains, ContainsKey, CopyTo, FromObject, GetEnumerator, GetValue, JObject, Load, Parse, Properties, Property, PropertyValues, Remove
         Represents a JSON object.
      JProperty (class):  JProperty, Load, WriteTo
         Represents a JSON property.
      JRaw (class):  Create, JRaw
         Represents a raw JSON string.
      JToken (class):  AddAfterSelf, AddAnnotation, AddBeforeSelf, AfterSelf, Ancestors, AncestorsAndSelf, Annotation, Annotations, BeforeSelf, Children, CreateReader, DeepClone, DeepEquals, FromObject, GetEnumerator
         Represents an abstract JSON token.
      JTokenEqualityComparer (class):  Equals, GetHashCode
         Compares tokens to determine whether they are equal.
      JTokenReader (class):  HasLineInfo, JTokenReader, Read
         Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
      … and 6 more (use --format json for the full surface)
   Newtonsoft.Json.Schema
      Extensions (class):  IsValid, Validate
         Contains the JSON schema extension methods.
      JsonSchema (class):  JsonSchema, Parse, Read, ToString, WriteTo
         An in-memory representation of a JSON Schema.
      JsonSchemaException (class):  JsonSchemaException
         Returns detailed information about the schema exception.
      JsonSchemaGenerator (class):  Generate
         Generates a from a specified .
      JsonSchemaResolver (class):  GetSchema, JsonSchemaResolver
         Resolves from an id.
      ValidationEventArgs (class)
         Returns detailed information related to the .
   Newtonsoft.Json.Serialization
      CamelCaseNamingStrategy (class):  CamelCaseNamingStrategy
         A camel case naming strategy.
      CamelCasePropertyNamesContractResolver (class):  CamelCasePropertyNamesContractResolver, ResolveContract
         Resolves member mappings for a type, camel casing property names.
      DefaultContractResolver (class):  DefaultContractResolver, GetResolvedPropertyName, ResolveContract
         Used by to resolve a for a given .
      DefaultNamingStrategy (class)
         The default naming strategy.
      DefaultSerializationBinder (class):  BindToName, BindToType, DefaultSerializationBinder
         The default serialization binder used when resolving and loading classes from type names.
      ErrorContext (class)
         Provides information surrounding an error.
      ErrorEventArgs (class):  ErrorEventArgs
         Provides data for the Error event.
      ExpressionValueProvider (class):  ExpressionValueProvider, GetValue, SetValue
         Get and set values for a using dynamic methods.
      IAttributeProvider (interface):  GetAttributes
         Provides methods to get attributes.
      IContractResolver (interface):  ResolveContract
         Used by to resolve a for a given .
      IReferenceResolver (interface):  AddReference, GetReference, IsReferenced, ResolveReference
         Used to resolve references when serializing and deserializing JSON by the .
      ISerializationBinder (interface):  BindToName, BindToType
         Allows users to control class loading and mandate what class to load.
      … and 19 more (use --format json for the full surface)
   Newtonsoft.Json.Utilities
      CastConverters (class)

CONSUMER PATHS
   extend  →  derive DefaultContractResolver
   extend  →  derive JsonConverter
   extend  →  derive JsonReader
   extend  →  derive JsonWriter
   configure  →  Extensions.*

PACKAGES
   Other:  Microsoft.CodeAnalysis.NetAnalyzers, Microsoft.CSharp, Microsoft.SourceLink.GitHub, System.ComponentModel.TypeConverter, System.Runtime.Serialization.Formatters, System.Runtime.Serialization.Primitives, System.Xml.XmlDocument

→ drill in:  --focus "<TypeName>"   (e.g. --focus DefaultContractResolver)
