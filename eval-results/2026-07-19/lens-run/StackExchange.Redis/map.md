LIBRARY  StackExchange.Redis     (141 public types)

ENTRY API
   annotate  [AsciiHash]   (AsciiHash.cs)
      This type is intended to provide fast hashing functions for small ASCII strings, for example well-known RESP literals...
   derive    Condition   (Condition.cs)
      Describes a precondition used in a redis transaction.
   derive    DefaultOptionsProvider   (DefaultOptionsProvider.cs)
      A defaults providers for .
   implement IReconnectRetryPolicy   (IReconnectRetryPolicy.cs)
      Describes retry policy functionality that can be provided to the multiplexer to be used for connection reconnects.
   implement IRedis   (IRedis.cs)
      Common operations available to all redis connections.
   extend    DatabaseExtensions   (DatabaseExtension.cs)
      Provides the extension method to .
   extend    ExtensionMethods   (ExtensionMethods.cs)
      Utility methods.

ABSTRACTIONS
   Condition (class)  — 7 implementors
   DefaultOptionsProvider (class)  — 6 implementors
   IReconnectRetryPolicy (interface)  — 4 implementors
   IRedis (interface)  — 4 implementors
   Predicate (class)  — 4 implementors
   Role (class)  — 4 implementors
   Tunnel (class)  — 4 implementors
   IBatch (interface)  — 3 implementors
   IDatabaseAsync (interface)  — 3 implementors
   RedisResult (class)  — 3 implementors

GENERATORS
   generator   AsciiHashGenerator

PUBLIC SURFACE
   RESPite
      AsciiHash (struct):  AsciiHash, Equals, EqualsCI, EqualsCS, GetHashCode, Hash, HashCS, HashUC, IsCI, IsCS, SequenceEqualsCI, SequenceEqualsCS, ToLower, ToString, ToUpper
      AsciiHashAttribute (class)
         This type is intended to provide fast hashing functions for small ASCII strings, for example well-known RESP literals...
      RespException (class)
         Represents a RESP error message.
   RESPite.Buffers
      CycleBuffer (struct):  Commit, Create, DiscardCommitted, GetAllCommitted, GetCommittedLength, GetUncommittedMemory, GetUncommittedSpan, Release, TryGetCommitted, TryGetFirstCommittedMemory, TryGetFirstCommittedSpan, Write
         Manages the state for a based IO buffer.
      CycleBufferPool (class):  Rent
      ICycleBufferCallback (interface):  PageComplete
   RESPite.Messages
      AggregateEnumerator (struct):  AggregateEnumerator, DemandNext, FillAll, GetEnumerator, MoveNext, MoveNextRaw, MovePast, ReadOne
         Reads the sub-elements associated with an aggregate value.
      RespAttributeReader (class):  Read, ReadKeyValuePair
         Allows attribute data to be parsed conveniently.
      RespFrameScanner (class):  TryRead, ValidateRequest
         Scans RESP frames.
      RespReader (struct):  AggregateChildren, AggregateIsEmpty, AggregateLength, AggregateLengthIs, Clone, CopyTo, DemandAggregate, DemandEnd, DemandNotNull, DemandScalar, FillAll, Is, MoveNext, MoveNextAggregate, MoveNextScalar
      RespScanState (struct):  Equals, GetHashCode, ToString, TryRead
         Holds state used for RESP frame parsing, i.e.
      ScalarEnumerator (struct):  GetEnumerator, MoveNext, MovePast, ScalarEnumerator
         Allows enumeration of chunks in a scalar value; this includes simple values that span multiple segments, and streamin...
   StackExchange.Redis
      ArrayGrepRequest (class):  AddPredicate
         Describes an array grep operation.
      ArrayInfo (struct):  ArrayInfo, ToDictionary
         Contains metadata information about an array returned by the ARINFO command.
      BacklogPolicy (class)
         The backlog policy to use for commands.
      ChannelMessage (struct):  Equals, GetHashCode, ToString, TryParseKeyNotification
         Represents a message that is broadcast via publish/subscribe.
      ChannelMessageQueue (class):  GetAsyncEnumerator, OnMessage, ReadAsync, ToString, TryGetCount, TryRead, Unsubscribe, UnsubscribeAsync
         Represents a message queue of ordered pub/sub notifications.
      ClientInfo (class):  ToString
         Represents the state of an individual client connection to redis.
      ClientKillFilter (class):  ClientKillFilter, WithClientType, WithEndpoint, WithId, WithMaxAgeInSeconds, WithServerEndpoint, WithSkipMe, WithUsername
         Filter determining which Redis clients to kill.
      ClusterConfiguration (class):  GetBySlot
         Describes the state of the cluster as reported by a single node.
      ClusterNode (class):  CompareTo, Equals, GetHashCode, ToString
         Represents the configuration of a single node in a cluster configuration.
      CommandMap (class):  Create, ToString
         Represents the commands mapped on a particular configuration.
      CommandTrace (class):  GetHelpUrl
         Represents the information known about long-running commands.
      Condition (class):  HashEqual, HashExists, HashLengthEqual, HashLengthGreaterThan, HashLengthLessThan, HashNotEqual, HashNotExists, KeyExists, KeyNotExists, ListIndexEqual, ListIndexExists, ListIndexNotEqual, ListIndexNotExists, ListLengthEqual, ListLengthGreaterThan
         Describes a precondition used in a redis transaction.
      … and 103 more (use --format json for the full surface)
   StackExchange.Redis.Build
      BasicArray (struct):  Equals, GetEnumerator, GetHashCode
      Builder (struct):  Add, Build
   StackExchange.Redis.Configuration
      AzureManagedRedisOptionsProvider (class):  AfterConnectAsync, GetDefaultSsl, IsMatch
         Options provider for Azure Managed Redis environments.
      AzureOptionsProvider (class):  AfterConnectAsync, GetDefaultSsl, IsMatch
         Options provider for Azure environments.
      DefaultOptionsProvider (class):  AddProvider, AfterConnectAsync, GetDefaultSsl, GetProvider, GetSslHostFromEndpoints, IsMatch
         A defaults providers for .
      LoggingTunnel (class):  BeforeAuthenticateAsync, BeforeSocketConnectAsync, DefaultFormatCommand, DefaultFormatResponse, GetSocketConnectEndpointAsync, LogToDirectory, ReplayAsync, ValidateAsync
         Captures redis traffic; intended for debug use.
      Tunnel (class):  BeforeAuthenticateAsync, BeforeSocketConnectAsync, GetSocketConnectEndpointAsync, HttpProxy
         Allows interception of the transport used to communicate with Redis.
   StackExchange.Redis.KeyspaceIsolation
      DatabaseExtensions (class):  WithKeyPrefix
         Provides the extension method to .
   StackExchange.Redis.Maintenance
      AzureMaintenanceEvent (class)
         Azure node maintenance event.
      ServerMaintenanceEvent (class):  ToString
         Base class for all server maintenance events.
   StackExchange.Redis.Profiling
      Enumerator (struct):  Dispose, MoveNext, Reset
         Implements IEnumerator for ProfiledCommandEnumerable.
      IProfiledCommand (interface)
         A profiled command against a redis instance.
      ProfiledCommandEnumerable (struct):  Count, GetEnumerator, ToArray, ToList
         A collection of IProfiledCommands.
      ProfilingSession (class):  FinishProfiling, ProfilingSession
         Lightweight profiling session that can be optionally registered (via ConnectionMultiplexer.RegisterProfiler) to track...

CONSUMER PATHS
   annotate  →  [AsciiHash] on a partial class/member
   extend  →  derive Condition
   extend  →  derive DefaultOptionsProvider
   contract  →  implement IReconnectRetryPolicy
   contract  →  implement IRedis
   configure  →  DatabaseExtensions.*

PACKAGES
   Other:  Microsoft.Bcl.AsyncInterfaces 10.0.5, Microsoft.CodeAnalysis.CSharp 5.3.0, Microsoft.Extensions.Logging.Abstractions 10.0.5, System.Buffers 4.6.1, System.IO.Compression 4.3.0, System.IO.Hashing 10.0.5, System.IO.Pipelines 10.0.5, System.Memory 4.6.3 … (10 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus Condition)
