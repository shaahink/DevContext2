LIBRARY  Dapper     (38 public types)

ENTRY API
   derive    BulkCopy   (BulkCopy.cs)
      Provides provider-agnostic access to bulk-copy services
   implement IDynamicParameters   (SqlMapper.IDynamicParameters.cs)
      Implement this interface to pass an arbitrary db specific set of parameters to Dapper
   implement ITypeHandler   (SqlMapper.ITypeHandler.cs)
      Implement this interface to perform custom type-based parameter handling and value parsing
   derive    Table   (Database.Async.cs)
   extend    DbConnectionExtensions   (DbConnectionExtensions.cs)
      Helper utilities for working with database connections
   extend    DbExceptionExtensions   (DbExceptionExtensions.cs)
      Helper utilities for working with database exceptions
   extend    SqlMapper   (SqlMapper.Async.cs)

ABSTRACTIONS
   IDynamicParameters (interface)  — 2 implementors
   ITypeHandler (interface)  — 2 implementors
   Table (class)  — 2 implementors
   BulkCopy (class)  — 1 implementor
   Database (class)  — 1 implementor
   DynamicParameters (class)  — 1 implementor
   IWrappedDataReader (interface)  — 1 implementor
   TypeHandler (class)  — 1 implementor

PUBLIC SURFACE
   Dapper
      Change (class)
         A holder for listing new values of changes fields and properties.
      CommandDefinition (struct):  CommandDefinition
         Represents the key aspects of a sql operation
      CustomPropertyTypeMap (class):  CustomPropertyTypeMap, FindConstructor, FindExplicitConstructor, GetConstructorParameter, GetMember
         Implements custom property mapping by user provided criteria (usually presence of some custom attribute with column t...
      Database (class):  BeginTransaction, CommitTransaction, Dispose, Execute, ExecuteAsync, Init, Query, QueryAsync, QueryFirstOrDefault, QueryFirstOrDefaultAsync, QueryMultiple, QueryMultipleAsync, RollbackTransaction
      DbString (class):  AddParameter, DbString, ToString
         This class represents a SQL string, it can be used if you need to denote your parameter is a Char vs VarChar vs nVarC...
      DefaultTypeMap (class):  DefaultTypeMap, FindConstructor, FindExplicitConstructor, GetConstructorParameter, GetMember, MatchFirstOrDefault
         Represents default type mapping strategy used by Dapper
      DynamicParameters (class):  Add, AddDynamicParams, AddParameters, DynamicParameters, Get, OnCompleted, Output
      ExplicitConstructorAttribute (class)
         Tell Dapper to use an explicit constructor, passing nulls or 0s for all parameters
      GridReader (class):  Dispose, DisposeAsync, Read, ReadAsync, ReadFirst, ReadFirstAsync, ReadFirstOrDefault, ReadFirstOrDefaultAsync, ReadSingle, ReadSingleAsync, ReadSingleOrDefault, ReadSingleOrDefaultAsync, ReadUnbufferedAsync
      ICustomQueryParameter (interface):  AddParameter
         Implement this interface to pass an arbitrary db specific parameter to Dapper
      IDynamicParameters (interface):  AddParameters
         Implement this interface to pass an arbitrary db specific set of parameters to Dapper
      IMemberMap (interface)
         Implements this interface to provide custom member mapping
      IParameterCallbacks (interface):  OnCompleted
         Extends IDynamicParameters with facilities for executing callbacks after commands have completed
      IParameterLookup (interface)
         Extends IDynamicParameters providing by-name lookup of parameter values
      ITypeHandler (interface):  Parse, SetValue
         Implement this interface to perform custom type-based parameter handling and value parsing
      ITypeMap (interface):  FindConstructor, FindExplicitConstructor, GetConstructorParameter, GetMember
         Implement this interface to change default mapping of reader columns to type members
      IWrappedDataReader (interface)
         Describes a reader that controls the lifetime of both a command and a reader, exposing the downstream command/reader ...
      IgnorePropertyAttribute (class):  IgnorePropertyAttribute
         Specifies whether a property should be ignored for database operations.
      Settings (class):  SetDefaults, Settings
         Permits specifying certain SqlMapper values globally.
      Snapshot (class):  Diff, Snapshot
         A snapshot of an object's state.
      Snapshotter (class):  Start
         Snapshots an object for comparison later.
      SqlBuilder (class):  AddParameters, AddTemplate, GroupBy, Having, InnerJoin, Intersect, Join, LeftJoin, OrWhere, OrderBy, RightJoin, Select, Set, Where
      SqlCompactDatabase (class):  Init
         A SQL Compact specific implementation.
      SqlCompactTable (class):  Insert, SqlCompactTable
         A SQL Compact specific table, which handles the syntax correctly across operations.
      SqlMapper (class):  AddTypeHandler, AddTypeHandlerImpl, AddTypeMap, AsList, AsTableValuedParameter, CreateParamInfoGenerator, Execute, ExecuteAsync, ExecuteReader, ExecuteReaderAsync, ExecuteScalar, ExecuteScalarAsync, FindOrAddParameter, Format, GetCachedSQL
      StringTypeHandler (class):  Parse, SetValue
         Base-class for simple type-handlers that are based around strings
      StructuredHelper (class):  CreateFor, SlowGetHelper
      Table (class):  All, AllAsync, Delete, DeleteAsync, First, FirstAsync, Get, GetAsync, Insert, InsertAsync, Table, Update, UpdateAsync
      Template (class):  Template
      TypeHandler (class):  Parse, SetValue
         Base-class for simple type-handlers
      TypeHandlerCache (class):  Parse, SetValue
         Not intended for direct usage
      UdtTypeHandler (class):  Parse, SetValue, UdtTypeHandler
         A type handler for data-types that are supported by the underlying provider, but which need a well-known UdtTypeName ...
   Dapper.EntityFramework
      DbGeographyHandler (class):  Parse, SetValue
         Type-handler for the DbGeography spatial type.
      DbGeometryHandler (class):  Parse, SetValue
         Type-handler for the DbGeometry spatial type.
      Handlers (class):  Register
         Acts on behalf of all type-handlers in this package
   Dapper.ProviderTools
      BulkCopy (class):  AddColumnMapping, Create, Dispose, TryCreate, WriteToServer, WriteToServerAsync
         Provides provider-agnostic access to bulk-copy services
      DbConnectionExtensions (class):  TryClearAllPools, TryClearPool, TryGetClientConnectionId
         Helper utilities for working with database connections
      DbExceptionExtensions (class):  IsNumber
         Helper utilities for working with database exceptions

CONSUMER PATHS
   extend  →  derive BulkCopy
   contract  →  implement IDynamicParameters
   contract  →  implement ITypeHandler
   extend  →  derive Table
   configure  →  DbConnectionExtensions.*
   configure  →  DbExceptionExtensions.*

PACKAGES
   ORM/Data:  EntityFramework, Microsoft.SqlServer.Types
   Other:  Microsoft.Bcl.AsyncInterfaces, Microsoft.CodeAnalysis.PublicApiAnalyzers, Microsoft.CSharp, System.Reflection.Emit.Lightweight, System.Threading.Tasks.Extensions

→ drill in:  --focus "<TypeName>"   (e.g. --focus BulkCopy)
