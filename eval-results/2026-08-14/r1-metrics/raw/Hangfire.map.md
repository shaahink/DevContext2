LIBRARY  Hangfire     (279 public types)

ENTRY API
   register  HangfireApplicationBuilderExtensions.RegisterHangfireServer   
(HangfireApplicationBuilderExtensions.cs)
   register  HangfireApplicationBuilderExtensions.UseHangfireDashboard   
(HangfireApplicationBuilderExtensions.cs)
   register  HangfireApplicationBuilderExtensions.UseHangfireServer   
(HangfireApplicationBuilderExtensions.cs)
   register  HangfireServiceCollectionExtensions.AddHangfire   
(HangfireServiceCollectionExtensions.cs)
   register  HangfireServiceCollectionExtensions.AddHangfireServer   
(HangfireServiceCollectionExtensions.cs)
   implement IBackgroundProcess   (IBackgroundProcess.cs)
      Provides methods for defining processes that will be executed in a 
background thread by .
   implement ILog   (LibLog.cs)
      Simple interface that represent a logger.
   derive    JobFilterAttribute   (JobFilterAttribute.cs)
      Represents the base class for job filter attributes.
   derive    RazorPage   (RazorPage.cs)
   extend    AppBuilderExtensions   (AppBuilderExtensions.cs)
      Provides extension methods for the IAppBuilder interface defined in the 
Owin NuGet package to simplify the integratio...
   extend    AspNetCoreDashboardContextExtensions   
(AspNetCoreDashboardContextExtensions.cs)
   extend    BackgroundJobClientExtensions   (BackgroundJobClientExtensions.cs)
      Provides extension methods for the interface to simplify the creation of 
fire-and-forget jobs, delayed jobs, continua...

ABSTRACTIONS
   RazorPage (class)  - 23 implementors
   JobFilterAttribute (class)  - 14 implementors
   ILog (interface)  - 10 implementors
   IBackgroundProcess (interface)  - 9 implementors
   ILogProvider (interface)  - 9 implementors
   IState (interface)  - 7 implementors
   IDashboardDispatcher (interface)  - 6 implementors
   IElectStateFilter (interface)  - 6 implementors
   IStateHandler (interface)  - 5 implementors
   IServerFilter (interface)  - 4 implementors

PUBLIC SURFACE
   Hangfire
      AppBuilderExtensions (class):  UseHangfireDashboard, UseHangfireServer
         Provides extension methods for the IAppBuilder interface defined in the
Owin NuGet package to simplify the integratio...
      AutomaticRetryAttribute (class):  AutomaticRetryAttribute, OnStateApplied,
OnStateElection, OnStateUnapplied
         Represents a job filter that performs automatic retries for background 
jobs whose processing was failed due to an exc...
      BackgroundJob (class):  BackgroundJob, ContinueJobWith, ContinueWith, 
Delete, Enqueue, Requeue, Reschedule, Schedule
         Provides static methods for creating fire-and-forget, delayed jobs and 
continuations as well as re-queue and delete e...
      BackgroundJobClient (class):  BackgroundJobClient, ChangeState, Create
         Provides methods for creating background jobs and changing their 
states.
      BackgroundJobClientException (class):  BackgroundJobClientException
         The exception that is thrown when an instance of the class that 
implements the interface is unable to perform an oper...
      BackgroundJobClientExtensions (class):  ChangeState, ContinueJobWith, 
ContinueWith, Create, Delete, Enqueue, Requeue, Reschedule, Schedule
         Provides extension methods for the interface to simplify the creation 
of fire-and-forget jobs, delayed jobs, continua...
      BackgroundJobServer (class):  BackgroundJobServer, Dispose, SendStop, 
Start, Stop, WaitForShutdown, WaitForShutdownAsync
      BackgroundJobServerHostedService (class):  
BackgroundJobServerHostedService, Dispose, StartAsync, StopAsync
      BackgroundJobServerOptions (class):  BackgroundJobServerOptions
      BackgroundProcessingServerHostedService (class):  
BackgroundProcessingServerHostedService, Dispose, StartAsync, StopAsync
      BootstrapperConfigurationExtensions (class):  UseServer
      CaptureCultureAttribute (class):  CaptureCultureAttribute, OnCreated, 
OnCreating, OnPerformed, OnPerforming
      . and 53 more (the structured surface lists them all)
   Hangfire.Annotations
      BaseTypeRequiredAttribute (class):  BaseTypeRequiredAttribute
         When applied to a target attribute, specifies a requirement for any 
type marked with the target attribute to implemen...
      CanBeNullAttribute (class)
         Indicates that the value of the marked element could be null sometimes,
so the check for null is necessary before its...
      CannotApplyEqualityOperatorAttribute (class)
         Indicates that the value of the marked type (or its derivatives) cannot
be compared using '==' or '!=' operators and ...
      ContractAnnotationAttribute (class):  ContractAnnotationAttribute
         Describes dependency between method input and output
      HtmlAttributeValueAttribute (class):  HtmlAttributeValueAttribute
      HtmlElementAttributesAttribute (class):  HtmlElementAttributesAttribute
      InstantHandleAttribute (class)
         Tells code analysis engine if the parameter is completely handled when 
the invoked method is on stack.
      InvokerParameterNameAttribute (class)
         Indicates that the function argument should be string literal and match
one of the parameters of the caller function.
      LocalizationRequiredAttribute (class):  LocalizationRequiredAttribute
         Indicates that marked element should be localized or not
      MeansImplicitUseAttribute (class):  MeansImplicitUseAttribute
         Should be used on attributes and causes ReSharper to not mark symbols 
marked with such attributes as unused (as well ...
      NamespaceDoc (class)
         The namespace contains attributes that enable additional code 
inspections in design time with JetBrains ReSharper.
      NotNullAttribute (class)
         Indicates that the value of the marked element could never be null
      . and 5 more (the structured surface lists them all)
   Hangfire.AspNetCore
      AspNetCoreJobActivator (class):  AspNetCoreJobActivator, BeginScope
      AspNetCoreLogProvider (class):  AspNetCoreLogProvider, GetLogger
   Hangfire.Client
      BackgroundJobFactory (class):  BackgroundJobFactory, Create
      ClientExceptionContext (class):  ClientExceptionContext
         Provides the context for the method of the interface.
      CreateContext (class):  CreateContext
         Provides information about the context in which the job is created.
      CreateJobFailedException (class):  CreateJobFailedException
         The exception that is thrown when a class instance could not create a 
job due to another exception was thrown.
      CreatedContext (class):  CreatedContext, SetJobParameter
         Provides the context for the method of the interface.
      CreatingContext (class):  CreatingContext, GetJobParameter, 
SetJobParameter
         Provides the context for the method of the interface.
      IBackgroundJobFactory (interface):  Create
         This interface acts as extensibility point for the process of job 
creation.
      IClientExceptionFilter (interface):  OnClientException
         Defines methods that are required for the client exception filter.
      IClientFilter (interface):  OnCreated, OnCreating
         Defines methods that are required for a client filter.
      NamespaceDoc (class)
         The namespace contains types that allow you to customize the background
job creation pipeline using the , or define y...
   Hangfire.Common
      CancellationEvent (class):  CancellationEvent, Dispose
      CancellationTokenExtentions (class):  GetCancellationEvent, Wait, 
WaitOrThrow
      Enumerator (struct):  MoveNext
      FilterCollection (struct):  GetEnumerator
      IJobFilter (interface)
         Defines members that specify the order of filters and whether multiple 
filters are allowed.
      IJobFilterProvider (interface):  GetFilters
         Provides an interface for finding filters.
      Job (class):  FromExpression, Job, Perform, ToString
         Represents an action that can be marshalled to another process to be 
performed.
      JobFilter (class):  JobFilter
         Represents a metadata class that contains a reference to the 
implementation of one or more of the filter interfaces, ...
      JobFilterAttribute (class)
         Represents the base class for job filter attributes.
      JobFilterAttributeFilterProvider (class):  GetFilters, 
JobFilterAttributeFilterProvider
         Defines a filter provider for filter attributes.
      JobFilterCollection (class):  Add, Clear, Contains, GetEnumerator, 
GetFilters, Remove
         Represents a class that contains the job filters.
      JobFilterProviderCollection (class):  GetFilters, 
JobFilterProviderCollection
         Represents the collection of filter providers for the application.
      . and 9 more (the structured surface lists them all)
   Hangfire.Dashboard
      AspNetCoreDashboardContext (class):  AspNetCoreDashboardContext, 
GetBackgroundJobClient, GetRecurringJobManager
      AspNetCoreDashboardContextExtensions (class):  GetHttpContext
      AspNetCoreDashboardMiddleware (class):  AspNetCoreDashboardMiddleware, 
Invoke
      DashboardContext (class):  GetBackgroundJobClient, GetRecurringJobManager
         Provides the context for the Dashboard UI.
      DashboardMetric (class):  DashboardMetric
      DashboardMetrics (class):  AddMetric, DashboardMetrics, GetMetrics
      DashboardOwinExtensions (class):  MapHangfireDashboard
      DashboardRequest (class):  GetFormValuesAsync, GetQuery
         Provides the request details for the Dashboard UI.
      DashboardResponse (class):  SetExpire, WriteAsync
         Provides the response details for the Dashboard UI.
      DashboardRoutes (class):  AddJavaScript, AddStylesheet, 
AddStylesheetDarkMode, DashboardRoutes
         Provides the routing mechanisms for the Dashboard UI.
      HtmlHelper (class):  BlockMetric, Breadcrumbs, FormatProperties, 
HtmlEncode, HtmlHelper, InlineMetric, JobId, JobIdLink, JobName, JobNameLink, 
JobsSidebar, LocalTime, MomentTitle, Paginator, PerPageSelector
      IAuthorizationFilter (interface):  Authorize
      . and 23 more (the structured surface lists them all)
   Hangfire.Dashboard.Owin
      IOwinDashboardAntiforgery (interface):  GetToken, ValidateRequest
   Hangfire.Dashboard.Pages
      BlockMetric (class):  BlockMetric, Execute
      Breadcrumbs (class):  Breadcrumbs, Execute
      EnqueuedJobsPage (class):  EnqueuedJobsPage, Execute
      FetchedJobsPage (class):  Execute, FetchedJobsPage
      HomePage (class):  Execute
      InlineMetric (class):  Execute, InlineMetric
      JobDetailsPage (class):  Execute, JobDetailsPage
      LayoutPage (class):  Execute, LayoutPage
      NamespaceDoc (class)
         The namespace contains the class, layout for all the Dashboard UI 
pages.
      Paginator (class):  Execute, Paginator
      PerPageSelector (class):  Execute, PerPageSelector
      SidebarMenu (class):  Execute, SidebarMenu
   Hangfire.Dashboard.Resources
      Strings (class)
         A strongly-typed resource class, for looking up localized strings, etc.
   Hangfire.Logging
      ILog (interface):  Log
         Simple interface that represent a logger.
      ILogProvider (interface):  GetLogger
         Represents a way to get a
      LogExtensions (class):  Debug, DebugException, DebugFormat, Error, 
ErrorException, ErrorFormat, Fatal, FatalException, FatalFormat, Info, 
InfoException, InfoFormat, IsDebugEnabled, IsErrorEnabled, IsFatalEnabled
      LogProvider (class):  For, GetCurrentClassLogger, GetLogger, 
SetCurrentLogProvider
         Provides a mechanism to create instances of objects.
      NamespaceDoc (class)
         The namespace contains types that allow you to integrate Hangfire's 
logging with your projects as well as use it to l...
      NamespaceGroupDoc (class)
         The Hangfire.Logging namespaces contain types that allow you to 
integrate Hangfire's logging with your projects as we...
   Hangfire.Logging.LogProviders
      ColouredConsoleLogProvider (class):  ColouredConsoleLogProvider, GetLogger
      ElmahLogProvider (class):  ElmahLogProvider, GetLogger, IsLoggerAvailable
      EntLibLogProvider (class):  EntLibLogProvider, GetLogger, 
IsLoggerAvailable
      Log4NetLogProvider (class):  GetLogger, IsLoggerAvailable, 
Log4NetLogProvider
      LoupeLogProvider (class):  GetLogger, IsLoggerAvailable, LoupeLogProvider
      NLogLogProvider (class):  GetLogger, IsLoggerAvailable, NLogLogProvider
      NamespaceDoc (class)
         The namespace contains types for supporting most popular logging 
frameworks to simplify the logging integration with ...
      SerilogLogProvider (class):  GetLogger, IsLoggerAvailable, 
SerilogLogProvider
   Hangfire.Processing
      BackgroundTaskScheduler (class):  BackgroundTaskScheduler, Dispose
         Represents a custom implementation of the that uses its own threads to 
execute -based work items and their continuati...
      IBackgroundDispatcher (interface):  Wait, WaitAsync
      IBackgroundExecution (interface):  Run, RunAsync
   Hangfire.Server
      BackgroundJobPerformer (class):  BackgroundJobPerformer, Perform
      BackgroundProcessContext (class):  BackgroundProcessContext, Wait
      BackgroundProcessExtensions (class):  UseBackgroundPool, UseThreadPool
      BackgroundProcessingServer (class):  BackgroundProcessingServer, Dispose, 
SendStop, WaitForShutdown, WaitForShutdownAsync
         Responsible for running the given collection background processes.
      BackgroundProcessingServerOptions (class):  
BackgroundProcessingServerOptions
      BackgroundServerContext (class):  BackgroundServerContext
      DelayedJobScheduler (class):  DelayedJobScheduler, Execute, ToString
         Represents a background process responsible for enqueueing delayed 
jobs.
      IBackgroundJobPerformer (interface):  Perform
      IBackgroundProcess (interface):  Execute
         Provides methods for defining processes that will be executed in a 
background thread by .
      IBackgroundProcessAsync (interface):  ExecuteAsync
      IBackgroundProcessDispatcherBuilder (interface):  Create
      IBackgroundProcessingServer (interface):  SendStop, WaitForShutdown, 
WaitForShutdownAsync
      . and 16 more (the structured surface lists them all)
   Hangfire.SqlServer
      EnqueuedAndFetchedCountDto (class)
      IPersistentJobQueue (interface):  Dequeue, Enqueue
      IPersistentJobQueueMonitoringApi (interface):  GetEnqueuedAndFetchedCount,
GetEnqueuedJobIds, GetFetchedJobIds, GetQueues
      IPersistentJobQueueProvider (interface):  GetJobQueue, 
GetJobQueueMonitoringApi
      PersistentJobQueueProviderCollection (class):  Add, GetEnumerator, 
GetProvider, PersistentJobQueueProviderCollection
      SqlServerBootstrapperConfigurationExtensions (class):  UseSqlServerStorage
      SqlServerDistributedLock (class):  Dispose, SqlServerDistributedLock
      SqlServerDistributedLockException (class):  
SqlServerDistributedLockException
      SqlServerObjectsInstaller (class):  GetInstallScript, Install
      SqlServerStorage (class):  GetComponents, GetConnection, GetMonitoringApi,
GetServerRequiredProcesses, GetStorageWideProcesses, HasFeature, 
SqlServerStorage, ToString, WriteOptionsToLog
      SqlServerStorageOptions (class):  SqlServerStorageOptions
   Hangfire.SqlServer.Msmq
      MsmqSqlServerStorageExtensions (class):  UseMsmqQueues
   Hangfire.States
      ApplyStateContext (class):  ApplyStateContext, GetJobParameter
      AwaitingState (class):  AwaitingState, SerializeData
         Defines the intermediate state of a background job when it is waiting 
for a parent background job to be finished befo...
      BackgroundJobStateChanger (class):  BackgroundJobStateChanger, ChangeState
      DeletedState (class):  DeletedState, SerializeData
         Defines the final state of a background job when nobody is interested 
whether it was performed or not.
      ElectStateContext (class):  ElectStateContext, GetJobParameter, 
SetJobParameter
      EnqueuedState (class):  EnqueuedState, SerializeData
         Defines the intermediate state of a background job when it is placed on
a message queue to be processed by the backgr...
      Enumerator (struct):  Enumerator, MoveNext
      FailedState (class):  FailedState, SerializeData
         Defines the intermediate state of a background job when its processing 
was interrupted by an exception and it is a de...
      IApplyStateFilter (interface):  OnStateApplied, OnStateUnapplied
         Provides methods that are required for a state changed filter.
      IBackgroundJobStateChanger (interface):  ChangeState
      IElectStateFilter (interface):  OnStateElection
         Defines methods that are required for a state changing filter.
      IState (interface):  SerializeData
         Provides the essential members for describing a background job state.
      . and 10 more (the structured surface lists them all)
   Hangfire.Storage
      BackgroundServerGoneException (class):  BackgroundServerGoneException
      Connection (class)
      DistributedLockTimeoutException (class):  DistributedLockTimeoutException
      IFetchedJob (interface):  RemoveFromQueue, Requeue
      IMonitoringApi (interface):  DeletedJobs, DeletedListCount, EnqueuedCount,
EnqueuedJobs, FailedByDatesCount, FailedCount, FailedJobs, FetchedCount, 
FetchedJobs, GetStatistics, HourlyFailedJobs, HourlySucceededJobs, JobDetails, 
ProcessingCount, ProcessingJobs
      IStorageConnection (interface):  AcquireDistributedLock, AnnounceServer, 
CreateExpiredJob, CreateWriteTransaction, FetchNextJob, GetAllEntriesFromHash, 
GetAllItemsFromSet, GetFirstByLowestScoreFromSet, GetJobData, GetJobParameter, 
GetStateData, Heartbeat, RemoveServer, RemoveTimedOutServers, SetJobParameter
      IWriteOnlyTransaction (interface):  AddJobState, AddToQueue, AddToSet, 
Commit, DecrementCounter, ExpireJob, IncrementCounter, InsertToList, PersistJob,
RemoveFromList, RemoveFromSet, RemoveHash, SetJobState, SetRangeInHash, TrimList
      InvocationData (class):  Deserialize, DeserializeJob, DeserializePayload, 
InvocationData, Serialize, SerializeJob, SerializePayload, SetTypeResolver, 
SetTypeSerializer
      JobData (class):  EnsureLoaded
      JobStorageConnection (class):  AcquireDistributedLock, AnnounceServer, 
CreateExpiredJob, CreateWriteTransaction, Dispose, FetchNextJob, 
GetAllEntriesFromHash, GetAllItemsFromList, GetAllItemsFromSet, GetCounter, 
GetFirstByLowestScoreFromSet, GetHashCount, GetHashTtl, GetJobData, 
GetJobParameter
      JobStorageFeatures (class):  GetNotSupportedException
      JobStorageMonitor (class):  AwaitingCount, AwaitingJobs, 
DeletedByDatesCount, DeletedJobs, DeletedListCount, EnqueuedCount, EnqueuedJobs,
FailedByDatesCount, FailedCount, FailedJobs, FetchedCount, FetchedJobs, 
GetStatistics, HourlyDeletedJobs, HourlyFailedJobs
      . and 8 more (the structured surface lists them all)
   Hangfire.Storage.Monitoring
      AwaitingJobDto (class):  AwaitingJobDto
      DeletedJobDto (class):  DeletedJobDto
      EnqueuedJobDto (class):  EnqueuedJobDto
      FailedJobDto (class):  FailedJobDto
      FetchedJobDto (class)
      JobDetailsDto (class)
      JobList (class):  JobList
      NamespaceDoc (class)
         The provides data transfer objects for the interface.
      ProcessingJobDto (class):  ProcessingJobDto
      QueueWithTopEnqueuedJobsDto (class)
      ScheduledJobDto (class):  ScheduledJobDto
      ServerDto (class)
      . and 3 more (the structured surface lists them all)
   MoreLinq
      MoreEnumerable (class):  Pairwise

CONSUMER PATHS
   wire into DI    
HangfireApplicationBuilderExtensions.RegisterHangfireServer(...)
   wire into DI    
HangfireApplicationBuilderExtensions.UseHangfireDashboard(...)
   wire into DI    HangfireApplicationBuilderExtensions.UseHangfireServer(...)
   wire into DI    HangfireServiceCollectionExtensions.AddHangfire(...)
   wire into DI    HangfireServiceCollectionExtensions.AddHangfireServer(...)
   contract    implement IBackgroundProcess

ENTRY POINTS
   Background (2)
      BackgroundJobServerHostedService  
(src/Hangfire.NetCore/BackgroundJobServerHostedService.cs:29)
      BackgroundProcessingServerHostedService  
(src/Hangfire.NetCore/BackgroundProcessingServerHostedService.cs:28)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Antiforgery 2.0.0, 
Microsoft.AspNetCore.Http.Abstractions 2.0.0
   ORM/Data:  Dapper 2.1.28
   Utilities:  Newtonsoft.Json 11.0.1
   Other:  CronExpressionDescriptor 1.21.0, Cronos 0.11.1, LibLog 1.5.0, 
Microsoft.CSharp 4.4.0, Microsoft.Extensions.DependencyInjection.Abstractions 
3.0.0, Microsoft.Extensions.Hosting.Abstractions 3.0.0, 
Microsoft.Extensions.Logging.Abstractions 3.0.0, 
Microsoft.NETFramework.ReferenceAssemblies 1.0.3 . (15 total)

 drill in:  trace a focused type   (e.g. trace 
HangfireApplicationBuilderExtensions)

