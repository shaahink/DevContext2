LIBRARY  MediatR     (41 public types)

ENTRY API
   register  MediatRServiceCollectionExtensions.AddMediatR   
(MediatRServiceCollectionExtensions.cs)
      Registers handlers and mediator types from the specified assemblies
   implement INotificationHandler   (INotificationHandler.cs)
      Defines a handler for a notification
   implement IPipelineBehavior   (IPipelineBehavior.cs)
      Pipeline behavior to surround the inner handler.
   implement IRequest   (IRequest.cs)
      Marker interface to represent a request with a void response
   implement IRequestHandler   (IRequestHandler.cs)
      Defines a handler for a request with a void response.

ABSTRACTIONS
   IRequestHandler (interface)  - 50 implementors
   IRequest (interface)  - 47 implementors
   IPipelineBehavior (interface)  - 31 implementors
   INotificationHandler (interface)  - 22 implementors
   IStreamPipelineBehavior (interface)  - 15 implementors
   INotification (interface)  - 12 implementors
   IRequestExceptionHandler (interface)  - 12 implementors
   IRequestExceptionAction (interface)  - 9 implementors
   IStreamRequest (interface)  - 9 implementors
   IStreamRequestHandler (interface)  - 9 implementors

PUBLIC SURFACE
   MediatR
      IBaseRequest (interface)
         Allows for generic type constraints of objects implementing IRequest or
IRequest{TResponse}
      IMediator (interface)
         Defines a mediator to encapsulate request/response and publishing 
interaction patterns
      INotification (interface)
         Marker interface to represent a notification
      INotificationHandler (interface):  Handle
         Defines a handler for a notification
      INotificationPublisher (interface):  Publish
      IPipelineBehavior (interface):  Handle
         Pipeline behavior to surround the inner handler.
      IPublisher (interface):  Publish
         Publish a notification or event through the mediator pipeline to be 
handled by multiple handlers.
      IRequest (interface)
         Marker interface to represent a request with a void response
      IRequest (interface)
         Marker interface to represent a request with a response
      IRequestHandler (interface):  Handle
         Defines a handler for a request with a void response.
      IRequestHandler (interface):  Handle
         Defines a handler for a request
      ISender (interface):  CreateStream, Send
         Send a request through the mediator pipeline to be handled by a single 
handler.
      . and 7 more (the structured surface lists them all)
   MediatR.Entities
      OpenBehavior (class):  OpenBehavior
         Represents a registration entity for pipeline behaviors with a 
specified service lifetime.
   MediatR.NotificationPublishers
      ForeachAwaitPublisher (class):  Publish
         Awaits each notification handler in a single foreach loop: foreach (var
handler in handlers) { await handler(notifica...
      TaskWhenAllPublisher (class):  Publish
         Uses Task.WhenAll with the list of Handler tasks: var tasks = handlers 
.Select(handler => handler.Handle(notification...
   MediatR.Pipeline
      IRequestExceptionAction (interface):  Execute
         Defines an exception action for a request
      IRequestExceptionHandler (interface):  Handle
         Defines an exception handler for a request and response
      IRequestPostProcessor (interface):  Process
         Defines a request post-processor for a request
      IRequestPreProcessor (interface):  Process
         Defined a request pre-processor for a handler
      RequestExceptionActionProcessorBehavior (class):  Handle, 
RequestExceptionActionProcessorBehavior
         Behavior for executing all instances after an exception is thrown by 
the following pipeline steps
      RequestExceptionHandlerState (class):  SetHandled
         Represents the result of handling an exception thrown by a request 
handler
      RequestExceptionProcessorBehavior (class):  Handle, 
RequestExceptionProcessorBehavior
         Behavior for executing all instances after an exception is thrown by 
the following pipeline steps
      RequestPostProcessorBehavior (class):  Handle, 
RequestPostProcessorBehavior
         Behavior for executing all instances after handling the request
      RequestPreProcessorBehavior (class):  Handle, RequestPreProcessorBehavior
         Behavior for executing all instances before handling a request
   MediatR.Registration
      ServiceRegistrar (class):  AddMediatRClasses, 
AddMediatRClassesWithTimeout, AddRequiredServices, GenerateCombinations, 
SetGenericRequestHandlerRegistrationLimitations
   MediatR.Wrappers
      NotificationHandlerWrapper (class):  Handle
      NotificationHandlerWrapperImpl (class):  Handle
      RequestHandlerBase (class):  Handle
      RequestHandlerWrapper (class):  Handle
      RequestHandlerWrapper (class):  Handle
      RequestHandlerWrapperImpl (class):  Handle
      RequestHandlerWrapperImpl (class):  Handle
   Microsoft.Extensions.DependencyInjection
      MediatRServiceCollectionExtensions (class):  AddMediatR
         Extensions to scan for MediatR handlers and registers them.
      MediatRServiceConfiguration (class):  AddBehavior, AddOpenBehavior, 
AddOpenBehaviors, AddOpenRequestPostProcessor, AddOpenRequestPreProcessor, 
AddOpenStreamBehavior, AddRequestPostProcessor, AddRequestPreProcessor, 
AddStreamBehavior, RegisterServicesFromAssemblies, RegisterServicesFromAssembly,
RegisterServicesFromAssemblyContaining

CONSUMER PATHS
   wire into DI    MediatRServiceCollectionExtensions.AddMediatR(...)
   contract    implement INotificationHandler
   contract    implement IPipelineBehavior
   contract    implement IRequest
   contract    implement IRequestHandler

ENTRY POINTS
   Domain (1)
      NotificationHandler  (src/MediatR/INotificationHandler.cs:25)

PACKAGES
   Mediator/CQRS:  MediatR.Contracts [2.0.1, 3.0.0)
   Other:  IsExternalInit 1.0.3, Microsoft.Bcl.AsyncInterfaces [10.0.0, ), 
Microsoft.Extensions.DependencyInjection.Abstractions [10.0.0, ), 
Microsoft.Extensions.Logging.Abstractions [10.0.0, ), 
Microsoft.IdentityModel.JsonWebTokens [8.14.0, ), Microsoft.SourceLink.GitHub 
8.0.0, MinVer 6.0.0

 drill in:  trace a focused type   (e.g. trace 
MediatRServiceCollectionExtensions)

