MAP  bitwarden-server     (33 projects)

STACK  net10.0 · Minimal APIs · Controllers · EF Core

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration of 10 runnable services

       per service:
         AppHost: Aspire AppHost [Aspire]
         Admin: MVC
         Api: Web API
         Billing: MVC
         Events: MVC
         EventsProcessor: Web API
         Icons: MVC
         Identity: Identity provider [IdentityServer]
         Notifications: SignalR host [SignalR]
         MsSqlMigratorUtility: CLI [CLI]
         SeederApi: MVC
         SeederUtility: CLI [CLI]
         Server: Web API
         Setup: Unknown
         Scim: MVC
         Sso: Identity provider [IdentityServer]

TOPOLOGY (depends-on)
   Core ── Data
   SharedWeb ── Core, Infrastructure.Dapper, Infrastructure.EntityFramework
   Infrastructure.EntityFramework ── Core
   Migrator ── Core
   Commercial.Core ── Core
   Seeder ── Core, Infrastructure.EntityFramework, RustSdk, SharedWeb
   Api ── Commercial.Core, Commercial.Infrastructure.EntityFramework, Core, HttpExtensions, Pam, SharedWeb
   Commercial.Infrastructure.EntityFramework ── Core, Infrastructure.EntityFramework
   HttpExtensions
   Identity ── Core, SharedWeb
   Admin ── Commercial.Core, Commercial.Infrastructure.EntityFramework, Core, Migrator, MySqlMigrations, PostgresMigrations, SharedWeb, SqliteMigrations
   Billing ── Commercial.Core, Core, SharedWeb
   Data
   Events ── Core, SharedWeb
   EventsProcessor ── Core, SharedWeb
   Icons ── Core, SharedWeb
   Infrastructure.Dapper ── Core
   MySqlMigrations ── Core, Infrastructure.EntityFramework
   Notifications ── Core, SharedWeb
   Pam ── Core, HttpExtensions
   PostgresMigrations ── Core, Infrastructure.EntityFramework
   RustSdk
   Scim ── Core, SharedWeb
   SqliteMigrations ── Core, Infrastructure.EntityFramework
   Sso ── Core, SharedWeb
   AppHost ── Admin, Api, Billing, Events, EventsProcessor, Icons, Identity, Notifications, Scim, Sso
   IntegrationTestCommon ── Identity, Migrator, Seeder
   MsSqlMigratorUtility ── Migrator
   SeederApi ── Core, Seeder, SharedWeb
   SeederUtility ── Seeder
   Server
   Setup ── Core, Migrator
   SqlServerEFScaffold ── Api, Core, Infrastructure.EntityFramework

CROSS-SERVICE
  bus (3)
    [bus] Core → Admin  (C:\code\DevContext2\eval-repos\bitwarden-server\src\Core\Platform\Push\Engines\AzureQueuePushEngine.cs:38 raises AzureStorageQueue queue)
    [bus] Core → EventsProcessor  (C:\code\DevContext2\eval-repos\bitwarden-server\src\Core\Platform\Push\Engines\AzureQueuePushEngine.cs:38 raises AzureStorageQueue queue)
    [bus] Core → Notifications  (C:\code\DevContext2\eval-repos\bitwarden-server\src\Core\Platform\Push\Engines\AzureQueuePushEngine.cs:38 raises AzureStorageQueue queue)

EVENT WIRING  (4 integration events, 1 cross-service)
  AzureStorageQueue queue: Core → Admin, EventsProcessor, Notifications
  AzureServiceBus queue: Core · Core
  RabbitMQ queue: Core · Core
  ServiceBusMessage: Core · (no consumer)

ENTRY POINTS
   HTTP (662)
      DELETE /accounts  → UserService.DeleteAsync  (src/Api/Auth/Controllers/AccountsController.cs:561)
      DELETE /accounts/sso/{organizationId}  → OrganizationService.DeleteSsoUserAsync  (src/Api/Auth/Controllers/AccountsController.cs:637)
      DELETE /ciphers  → CipherService.DeleteManyAsync  (src/Api/Vault/Controllers/CiphersController.cs:966)
      DELETE /ciphers/{id}  → CipherService.DeleteAsync  (src/Api/Vault/Controllers/CiphersController.cs:925)
      DELETE /ciphers/{id}/admin  → CipherService.DeleteAsync  (src/Api/Vault/Controllers/CiphersController.cs:945)
      DELETE /ciphers/{id}/attachment/{attachmentId}  → CipherService.DeleteAttachmentAsync  (src/Api/Vault/Controllers/CiphersController.cs:1557)
      DELETE /ciphers/{id}/attachment/{attachmentId}/admin  → CipherService.DeleteAttachmentAsync  (src/Api/Vault/Controllers/CiphersController.cs:1578)
      DELETE /ciphers/admin  → CipherService.DeleteManyAsync  (src/Api/Vault/Controllers/CiphersController.cs:986)
      DELETE /devices/{id}  → DeviceService.DeactivateAsync  (src/Api/Controllers/DevicesController.cs:264)
      DELETE /emergency-access/{id}  → EmergencyAccessService.DeleteAsync  (src/Api/Auth/Controllers/EmergencyAccessController.cs:101)
      DELETE /folders/{id}  → CipherService.DeleteFolderAsync  (src/Api/Vault/Controllers/FoldersController.cs:86)
      DELETE /folders/all  → CipherService.DeleteFolderAsync  (src/Api/Vault/Controllers/FoldersController.cs:106)
      DELETE /organization/sponsorship/{organizationId}/{sponsoredFriendlyName}/revoke  → CloudRevokeSponsorshipCommand.RevokeSponsorshipAsync  (src/Api/Billing/Controllers/OrganizationSponsorshipsController.cs:257)
      DELETE /organization/sponsorship/{sponsoringOrganizationId}  → CloudRevokeSponsorshipCommand.RevokeSponsorshipAsync  (src/Api/Billing/Controllers/OrganizationSponsorshipsController.cs:230)
      DELETE /organization/sponsorship/self-hosted/{organizationId}/{sponsoredFriendlyName}/revoke  → CloudRevokeSponsorshipCommand.RevokeSponsorshipAsync  (src/Api/Controllers/SelfHosted/SelfHostedOrganizationSponsorshipsController.cs:92)
      DELETE /organization/sponsorship/self-hosted/{sponsoringOrgId}  → CloudRevokeSponsorshipCommand.RevokeSponsorshipAsync  (src/Api/Controllers/SelfHosted/SelfHostedOrganizationSponsorshipsController.cs:69)
      DELETE /organization/sponsorship/sponsored/{sponsoredOrgId}  → CurrentContext.OrganizationOwner  (src/Api/Billing/Controllers/OrganizationSponsorshipsController.cs:272)
      DELETE /organizations/{id}  → ProviderBillingService.ScaleSeats  (src/Api/AdminConsole/Controllers/OrganizationsController.cs:284)
      DELETE /organizations/{id}/two-factor/duo  → CurrentContext.ManagePolicies  (src/Api/Auth/Controllers/TwoFactorController.cs:283)
      DELETE /organizations/{organizationId:guid}/integrations/{integrationId:guid}  → DeleteOrganizationIntegrationCommand.DeleteAsync  (src/Api/Dirt/Controllers/OrganizationIntegrationController.cs:81)
      … and 642 more (http entries — use --focus for a drill-in)
   Bus (5)
      AzureQueueMailHostedService  → AzureQueueMailHostedService  (src/Admin/HostedServices/AzureQueueMailHostedService.cs:102)
      AzureServiceBusEventListenerService  → AzureServiceBusEventListenerService  (src/Core/Dirt/Services/Implementations/AzureServiceBusEventListenerService.cs:29)
      AzureServiceBusIntegrationListenerService  → AzureServiceBusIntegrationListenerService  (src/Core/Dirt/Services/Implementations/AzureServiceBusIntegrationListenerService.cs:38)
      RabbitMqEventListenerService  → RabbitMqEventListenerService  (src/Core/Dirt/Services/Implementations/RabbitMqEventListenerService.cs:40)
      RabbitMqIntegrationListenerService  → RabbitMqIntegrationListenerService  (src/Core/Dirt/Services/Implementations/RabbitMqIntegrationListenerService.cs:63)
   Background (7)
      ApplicationCacheHostedService  → ApplicationCacheHostedService  (src/Api/Startup.cs:230)
      AzureQueueHostedService  → AzureQueueHostedService  (src/EventsProcessor/Startup.cs:37)
      AzureQueueMailHostedService  → AzureQueueMailHostedService  (src/Admin/Startup.cs:130)
      DatabaseMigrationHostedService  → DatabaseMigrationHostedService  (src/Admin/Startup.cs:124)
      HeartbeatHostedService  → HeartbeatHostedService  (src/Notifications/Startup.cs:68)
      IpRateLimitSeedStartupService  → IpRateLimitSeedStartupService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:658)
      JobsHostedService  → JobsHostedService  (src/Admin/Startup.cs:121)
   SignalR (2)
      AnonymousNotificationsHub (1 methods: OnConnectedAsync)  → Context.GetHttpContext  (src/Notifications/AnonymousNotificationsHub.cs:9)
      NotificationsHub (5 methods: OnConnectedAsync, OnDisconnectedAsync, GetInstallationGroup)  → ConnectionCounter  (src/Notifications/NotificationsHub.cs:12)

PACKAGES
   Web/API:  AspNetCore.HealthChecks.SqlServer [8.0.2], AspNetCore.HealthChecks.Uris [8.0.1], AspNetCoreRateLimit [5.0.0], AspNetCoreRateLimit.Redis [2.0.0], Azure.Extensions.AspNetCore.DataProtection.Blobs [1.3.4], Fido2.AspNet [3.0.1], Microsoft.AspNetCore.Authentication.JwtBearer [10.0.8], Microsoft.AspNetCore.DataProtection [10.0.8] … (15 total)
   ORM/Data:  Aspire.Hosting.SqlServer [13.3.4], Dapper [2.1.66], dbup-sqlserver [7.2.0], linq2db.EntityFrameworkCore [8.1.0], Microsoft.Azure.Cosmos [3.52.0], Microsoft.EntityFrameworkCore.Design [8.0.8], Microsoft.EntityFrameworkCore.Relational [8.0.8], Microsoft.EntityFrameworkCore.Sqlite [8.0.8] … (13 total)
   Messaging:  Azure.Messaging.EventGrid [5.0.0], Azure.Messaging.ServiceBus [7.20.1], RabbitMQ.Client [7.1.2]
   Logging:  Serilog.Extensions.Logging.File [3.0.0]
   Testing:  AutoFixture.AutoNSubstitute, AutoFixture.Xunit2, Bogus [35.6.5], coverlet.collector [10.0.0], MartinCostello.Logging.XUnit [0.7.0], Neovolve.Logging.Xunit [6.3.0], NSubstitute, Testcontainers [4.11.0] … (11 total)
   Cloud:  Aspire.Hosting.Azure.Storage [13.3.4], Azure.Data.Tables [12.11.0], Azure.Storage.Blobs [12.26.0], Azure.Storage.Blobs.Batch [12.23.0], Azure.Storage.Queues [12.24.0], Microsoft.Azure.NotificationHubs [4.2.0]
   Utilities:  AutoMapper [14.0.0], Newtonsoft.Json [13.0.3]
   Other:  AngleSharp [1.4.0], Aspire.Hosting.AppHost [13.3.4], Aspire.Hosting.JavaScript [13.3.4], AWSSDK.SimpleEmail [4.0.2.5], AWSSDK.SQS [4.0.2.5], BenchmarkDotNet [0.15.3], BitPay.Light [1.0.1907], Braintree [5.36.0] … (48 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "PUT /public/members/{id}")
