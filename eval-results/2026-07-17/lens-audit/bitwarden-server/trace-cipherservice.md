TRACE  CipherService
       src/Core/Vault/Services/Implementations/CipherService.cs

▸ ENTRY  CipherService
       public class CipherService : ICipherService
       public const long MAX_FILE_SIZE = Constants.FileSize501mb;
       public const string MAX_FILE_SIZE_READABLE = "500 MB";
   (23 more branches omitted beyond fan-out)
   ├─ call ShareAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:569)
   │      public async Task ShareAsync(Cipher originalCipher, Cipher cipher, Guid organizationId,
   │      IEnumerable<Guid> collectionIds, Guid sharingUserId, DateTime? lastKnownRevisionDate)
   │      var attachments = cipher.GetAttachments() ?? new Dictionary<string, CipherAttachment.MetaData>();
   │  (14 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:572) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:572) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:602) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  ├─ di CipherRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:76) [×2 impls]
   │  │  │      public class CipherRepository : Repository<Core.Vault.Entities.Cipher, Cipher, Guid>, ICipherRepository
   │  │  │      public CipherRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
   │  │  │      : base(serviceScopeFactory, mapper, (DatabaseContext context) => context.Ciphers)
   │  │  └─ di CipherRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:38) [×2 impls]
   │  │         public class CipherRepository : Repository<Cipher, Guid>, ICipherRepository
   │  │         public CipherRepository(GlobalSettings globalSettings)
   │  │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:608) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  └─ di EventService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:164) [×2 impls]
   │  │         public class EventService : IEventService
   │  │         private readonly IEventWriteService _eventWriteService;
   │  │         private readonly IOrganizationUserRepository _organizationUserRepository;
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:615) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  ├─ di NoopAttachmentStorageService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:401) [×3 impls]
   │  │  │      public class NoopAttachmentStorageService : IAttachmentStorageService
   │  │  │      public FileUploadType FileUploadType => FileUploadType.Direct;
   │  │  │      public Task CleanupAsync(Guid cipherId)
   │  │  ├─ di LocalAttachmentStorageService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:352) [×3 impls]
   │  │  │      public class LocalAttachmentStorageService : IAttachmentStorageService
   │  │  │      private readonly string _baseDirPath;
   │  │  │      private readonly string _baseTempDirPath;
   │  │  └─ di AzureAttachmentStorageService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:348) [×3 impls]
   │  │         public class AzureAttachmentStorageService : IAttachmentStorageService
   │  │         public FileUploadType FileUploadType => FileUploadType.Azure;
   │  │         public const string EventGridEnabledContainerName = "attachments-v2";
   │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:638) [approx]
   │  │      public interface ICollectionCipherRepository
   │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │  ├─ di CollectionCipherRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:77) [×2 impls]
   │  │  │      public class CollectionCipherRepository : BaseEntityFrameworkRepository, ICollectionCipherRepository
   │  │  │      public CollectionCipherRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
   │  │  │      : base(serviceScopeFactory, mapper)
   │  │  └─ di CollectionCipherRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:39) [×2 impls]
   │  │         public class CollectionCipherRepository : BaseRepository, ICollectionCipherRepository
   │  │         public CollectionCipherRepository(GlobalSettings globalSettings)
   │  │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
   │  ├─ call IUserRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:653) [approx]
   │  │      public interface IUserRepository : IRepository<User, Guid>
   │  │      Task<User?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<User?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  ├─ di EFTestUserTrackingUserRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:28) [×2 impls]
   │  │  │      /// <summary>
   │  │  │      /// EntityFramework decorator around the <see cref="Bit.Infrastructure.EntityFramework.Repositories.UserRepository"/> that tracks
   │  │  │      /// created Users for seeding.
   │  │  ├─ di DapperTestUserTrackingUserRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:17) [×2 impls]
   │  │  │      /// <summary>
   │  │  │      /// Dapper decorator around the <see cref="Bit.Infrastructure.Dapper.Repositories.UserRepository"/> that tracks
   │  │  │      /// created Users for seeding.
   │  │  ├─ di UserRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:103) [×2 impls]
   │  │  │      public class UserRepository : Repository<Core.Entities.User, User, Guid>, IUserRepository
   │  │  │      public UserRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
   │  │  │      : base(serviceScopeFactory, mapper, (DatabaseContext context) => context.Users)
   │  │  └─ di UserRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:66) [×2 impls]
   │  │         public class UserRepository : Repository<User, Guid>, IUserRepository
   │  │         private readonly IDataProtector _dataProtector;
   │  │         public UserRepository(
   │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:654) [approx]
   │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  ├─ di EFTestOrganizationTrackingOrganizationRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:27) [×2 impls]
   │  │  │      /// <summary>
   │  │  │      /// EntityFramework decorator around the <see cref="Bit.Infrastructure.EntityFramework.Repositories.OrganizationRepository"/> that tracks
   │  │  │      /// created Organizations for seeding.
   │  │  ├─ di DapperTestOrganizationTrackingOrganizationRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:16) [×2 impls]
   │  │  │      /// <summary>
   │  │  │      /// Dapper decorator around the <see cref="Bit.Infrastructure.Dapper.Repositories.OrganizationRepository"/> that tracks
   │  │  │      /// created Organizations for seeding.
   │  │  ├─ di OrganizationRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:90) [×2 impls]
   │  │  │      public class OrganizationRepository : Repository<Core.AdminConsole.Entities.Organization, Organization, Guid>, IOrganizationRepository
   │  │  │      protected readonly ILogger<OrganizationRepository> _logger;
   │  │  │      public OrganizationRepository(
   │  │  └─ di OrganizationRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:53) [×2 impls]
   │  │         public class OrganizationRepository : Repository<Organization, Guid>, IOrganizationRepository
   │  │         protected readonly ILogger<OrganizationRepository> _logger;
   │  │         public OrganizationRepository(
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:668) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  └─ di CipherSyncPushService [approx]
   │  │         public class CipherSyncPushService : ICipherSyncPushService
   │  │         private readonly IPushNotificationService _pushNotificationService;
   │  │         private readonly ICollectionCipherRepository _collectionCipherRepository;
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:668) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  └─ call PushCipherAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:34) [verified]
   │  │         private async Task PushCipherAsync(Cipher cipher, PushType pushType, IEnumerable<Guid>? collectionIds)
   │  │         if (cipher.OrganizationId.HasValue)
   │  │         if (!_featureService.IsEnabled(FeatureFlagKeys.OrgCipherPushFanout))
   │  │     ├─ data Organization  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:51) [approx]
   │  │     │      /// <summary>
   │  │     │      /// An organization is an entity that allows users to share vault items and
   │  │     │      /// manage billing, access control, and other enterprise features depending on the plan.
   │  │     ├─ data User  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:84) [approx]
   │  │     │      public class User : ITableObject<Guid>, IStorableSubscriber, IRevisable, ITwoFactorProvidersUser
   │  │     │      private Dictionary<TwoFactorProviderType, TwoFactorProvider>? _twoFactorProviders;
   │  │     │      public Guid Id { get; set; }
   │  │     ├─ call IFeatureService  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:43) [approx]
   │  │     │      /// <summary>
   │  │     │      /// Should not be used, use <see cref="Bitwarden.Server.Sdk.Features.IFeatureService"/> instead.
   │  │     │      /// </summary>
   │  │     │  └─ di DelegatingFeatureService [approx]
   │  │     │         public class DelegatingFeatureService : IFeatureService
   │  │     │         private readonly Bitwarden.Server.Sdk.Features.IFeatureService _featureService;
   │  │     │         public DelegatingFeatureService(
   │  │     ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:48) [verified]
   │  │     │      /// <summary>
   │  │     │      /// Used to Push notifications to end-user devices.
   │  │     │      /// </summary>
   │  │     │  └─ di MultiServicePushNotificationService [approx]
   │  │     │         public class MultiServicePushNotificationService : IPushNotificationService
   │  │     │         private readonly IPushEngine[] _services;
   │  │     │         public Guid InstallationId { get; }
   │  │     ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:68) [approx]
   │  │     │      public interface ICollectionCipherRepository
   │  │     │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │     │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │     │  (stopped at depth 4; 2 branches omitted)
   │  │     ├─ call ILogger  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:71) [verified]
   │  │     ├─ call MultiServicePushNotificationService.PushAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:106) [verified]
   │  │     │      public Task PushAsync<T>(PushNotification<T> pushNotification) where T : class
   │  │     │      return PushToServices((s) => s.PushAsync(pushNotification));
   │  │     │  └─ call MultiServicePushNotificationService.PushToServices  (src/Core/Platform/Push/Engines/MultiServicePushNotificationService.cs:69) [verified]
   │  │     │         private Task PushToServices(Func<IPushEngine, Task> pushFunc)
   │  │     │         if (!_services.Any())
   │  │     │         Logger.LogWarning("No services found to push notification");
   │  │     │     └─ call MultiServicePushNotificationService.pushFunc  (src/Core/Platform/Push/Engines/MultiServicePushNotificationService.cs:54) [approx]
   │  │     ├─ call CollectionCipherRepository.GetUserIdsByCollectionIdsAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:79) [verified]
   │  │     │      public async Task<ICollection<Guid>> GetUserIdsByCollectionIdsAsync(IEnumerable<Guid> collectionIds)
   │  │     │      using (var connection = new SqlConnection(ConnectionString))
   │  │     │      var results = await connection.QueryAsync<Guid>(
   │  │     │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Repositories/CollectionCipherRepository.cs:93) [verified]
   │  │     ├─ call CollectionCipherRepository.GetCollectionIdsByCipherIdAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:68) [verified]
   │  │     │      public async Task<ICollection<Guid>> GetCollectionIdsByCipherIdAsync(Guid cipherId)
   │  │     │      using (var connection = new SqlConnection(ConnectionString))
   │  │     │      var results = await connection.QueryAsync<Guid>(
   │  │     └─ call DelegatingFeatureService.IsEnabled  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:43) [verified]
   │  │            public bool IsEnabled(string key, bool defaultValue = false)
   │  │            return _featureService.IsEnabled(key, defaultValue);
   │  ├─ call NoopAttachmentStorageService.CleanupAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:663) [verified]
   │  │      public Task CleanupAsync(Guid cipherId)
   │  │      return Task.FromResult(0);
   │  └─ call NoopAttachmentStorageService.RollbackShareAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:659) [verified]
   │         public Task RollbackShareAttachmentAsync(Guid cipherId, Guid organizationId, CipherAttachment.MetaData attachmentData, string originalContainer)
   │         return Task.FromResult(0);
   ├─ call SaveDetailsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:124)
   │      public async Task SaveDetailsAsync(CipherDetails cipher, Guid savingUserId, DateTime? lastKnownRevisionDate,
   │      IEnumerable<Guid>? collectionIds = null, bool skipPermissionCheck = false)
   │      if (!skipPermissionCheck && !(await UserCanEditAsync(cipher, savingUserId)))
   │  (6 more branches omitted beyond fan-out)
   │  ├─ call ICollectionRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:137) [approx]
   │  │      public interface ICollectionRepository : IRepository<Collection, Guid>
   │  │      Task<int> GetCountByOrganizationIdAsync(Guid organizationId);
   │  │      /// <summary>
   │  │  ├─ di CollectionRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:78) [×2 impls]
   │  │  │      public class CollectionRepository : Repository<Core.Entities.Collection, Collection, Guid>, ICollectionRepository
   │  │  │      public CollectionRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
   │  │  │      : base(serviceScopeFactory, mapper, (DatabaseContext context) => context.Collections)
   │  │  └─ di CollectionRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:40) [×2 impls]
   │  │         public class CollectionRepository : Repository<Collection, Guid>, ICollectionRepository
   │  │         public CollectionRepository(GlobalSettings globalSettings)
   │  │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:142) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call IPolicyRequirementQuery  (src/Core/Vault/Services/Implementations/CipherService.cs:147) [approx]
   │  │      public interface IPolicyRequirementQuery
   │  │      /// <summary>
   │  │      /// Get a policy requirement for a specific user.
   │  │  └─ di PolicyRequirementQuery  (src/Core/AdminConsole/OrganizationFeatures/Policies/PolicyServiceCollectionExtensions.cs:16)
   │  │         public class PolicyRequirementQuery(
   │  │         IPolicyRepository policyRepository,
   │  │         IEnumerable<IPolicyRequirementFactory<IPolicyRequirement>> factories)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:155) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:159) [approx]
   │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:167) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:178) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:175) [verified]
   │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │      if (e != null)
   │  │  ├─ call IEventWriteService  (src/Core/Dirt/Services/Implementations/EventService.cs:123) [approx]
   │  │  │      public interface IEventWriteService
   │  │  │      Task CreateAsync(IEvent e);
   │  │  │      Task CreateManyAsync(IEnumerable<IEvent> e);
   │  │  ├─ call CreateAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:123) [verified]
   │  │  │      public async Task CreateAsync(IEvent e)
   │  │  │      var body = JsonSerializer.Serialize(e);
   │  │  │      await _eventIntegrationPublisher.PublishEventAsync(body: body, organizationId: e.OrganizationId?.ToString());
   │  │  │  ├─ call IEventIntegrationPublisher  (src/Core/Dirt/Services/Implementations/EventIntegrationEventWriteService.cs:19) [approx]
   │  │  │  │      public interface IEventIntegrationPublisher : IAsyncDisposable
   │  │  │  │      Task PublishAsync(IIntegrationMessage message);
   │  │  │  │      Task PublishEventAsync(string body, string? organizationId);
   │  │  │  └─ call IAzureServiceBusService.PublishEventAsync  (src/Core/Dirt/Services/Implementations/EventIntegrationEventWriteService.cs:19) [verified]
   │  │  └─ call BuildCipherEventMessageAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:120) [verified]
   │  │         private async Task<EventMessage> BuildCipherEventMessageAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │         // Only logging organization cipher events for now.
   │  │         if (!cipher.OrganizationId.HasValue || (!_currentContext?.UserId.HasValue ?? true))
   │  │     ├─ call IOrganizationAbilityCacheService  (src/Core/Dirt/Services/Implementations/EventService.cs:151) [approx]
   │  │     │      public interface IOrganizationAbilityCacheService
   │  │     │      Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default);
   │  │     │      Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default);
   │  │     │  └─ di ExtendedOrganizationAbilityCacheService [approx]
   │  │     │         public class ExtendedOrganizationAbilityCacheService(
   │  │     │         [FromKeyedServices(CacheName)] IFusionCache cache,
   │  │     │         IOrganizationRepository organizationRepository)
   │  │     ├─ call GetProviderIdAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:165) [verified]
   │  │     │      private async Task<Guid?> GetProviderIdAsync(Guid? orgId)
   │  │     │      if (_currentContext == null || !orgId.HasValue)
   │  │     │      return null;
   │  │     │  ├─ call ICurrentContext  (src/Core/Dirt/Services/Implementations/EventService.cs:815) [approx]
   │  │     │  │      /// <summary>
   │  │     │  │      /// Provides information about the current HTTP request and the currently authenticated user (if any).
   │  │     │  │      /// This is often (but not exclusively) parsed from the JWT in the current request.
   │  │     │  │  └─ di CurrentContext  (bitwarden_license/src/Sso/Startup.cs:46)
   │  │     │  └─ call CurrentContext.ProviderIdForOrg  (src/Core/Dirt/Services/Implementations/EventService.cs:815) [verified]
   │  │     │         public async Task<Guid?> ProviderIdForOrg(Guid orgId)
   │  │     │         if (Organizations?.Any(org => org.Id == orgId) ?? false)
   │  │     │         return null;
   │  │     ├─ call EventService.CanUseEvents  (src/Core/Dirt/Services/Implementations/EventService.cs:152) [verified]
   │  │     │      private bool CanUseEvents(IDictionary<Guid, OrganizationAbility> orgAbilities, Guid orgId)
   │  │     │      return orgAbilities != null && orgAbilities.TryGetValue(orgId, out var orgAbility) &&
   │  │     │      orgAbility.Enabled && orgAbility.UseEvents;
   │  │     └─ call GetOrganizationAbilityAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:151) [verified]
   │  │            public async Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default)
   │  │            return await cache.GetOrSetAsync<OrganizationAbility?>(
   │  │            orgId.ToString(),
   │  │        ├─ call IOrganizationRepository  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:24) [approx]
   │  │        │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │        │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │        │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │        │  (stopped at depth 5; 4 branches omitted)
   │  │        └─ call EFTestOrganizationTrackingOrganizationRepository.GetAbilityAsync  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:24) [verified]
   │  ├─ call CipherRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:174) [verified]
   │  │      public async Task ReplaceAsync(CipherDetails obj)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:208) [verified]
   │  ├─ call ValidateChangeInCollectionsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:173) [verified]
   │  │      private async Task ValidateChangeInCollectionsAsync(Cipher updatedCipher, IEnumerable<Guid>? newCollectionIds, Guid userId)
   │  │      if (updatedCipher.Id == Guid.Empty || !updatedCipher.OrganizationId.HasValue)
   │  │      return;
   │  │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:1107) [approx]
   │  │  │      public interface ICollectionCipherRepository
   │  │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │  │  (stopped at depth 3; 2 branches omitted)
   │  │  ├─ call ICollectionRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:1115) [approx]
   │  │  │      public interface ICollectionRepository : IRepository<Collection, Guid>
   │  │  │      Task<int> GetCountByOrganizationIdAsync(Guid organizationId);
   │  │  │      /// <summary>
   │  │  │  (stopped at depth 3; 2 branches omitted)
   │  │  ├─ call GetManyByManyIdsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1126) [verified]
   │  │  │      public async Task<ICollection<Collection>> GetManyByManyIdsAsync(IEnumerable<Guid> collectionIds)
   │  │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │  │      var results = await connection.QueryAsync<Collection>(
   │  │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/AdminConsole/Repositories/CollectionRepository.cs:67) [verified]
   │  │  └─ call GetManyByUserIdCipherIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1107) [verified]
   │  │         public async Task<ICollection<CollectionCipher>> GetManyByUserIdCipherIdAsync(Guid userId, Guid cipherId)
   │  │         using (var connection = new SqlConnection(ConnectionString))
   │  │         var results = await connection.QueryAsync<CollectionCipher>(
   │  ├─ call CipherService.ValidateCipherLastKnownRevisionDate  (src/Core/Vault/Services/Implementations/CipherService.cs:171) [verified]
   │  │      private void ValidateCipherLastKnownRevisionDate(Cipher cipher, DateTime? lastKnownRevisionDate)
   │  │      if (cipher.Id == default || !lastKnownRevisionDate.HasValue)
   │  │      return;
   │  └─ call CipherSyncPushService.PushSyncCipherCreateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:167) [verified]
   │         public Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │         => PushCipherAsync(cipher, PushType.SyncCipherCreate, collectionIds);
   │     └─ call PushCipherAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:31) [verified]
   │            private async Task PushCipherAsync(Cipher cipher, PushType pushType, IEnumerable<Guid>? collectionIds)
   │            if (cipher.OrganizationId.HasValue)
   │            if (!_featureService.IsEnabled(FeatureFlagKeys.OrgCipherPushFanout))
   │        (stopped at depth 3; 10 branches omitted)
   ├─ call CreateAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:238)
   │      public async Task CreateAttachmentAsync(Cipher cipher, Stream stream, string fileName, string key,
   │      long requestLength, Guid savingUserId, bool orgAdmin = false, DateTime? lastKnownRevisionDate = null)
   │      ValidateCipherLastKnownRevisionDate(cipher, lastKnownRevisionDate);
   │  (5 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:252) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:267) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:282) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:289) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:289) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:286) [verified]
   │  │      public async Task ReplaceAsync(CipherDetails obj)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:282) [verified]
   │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │      if (e != null)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call NoopAttachmentStorageService.DeleteAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:278) [verified]
   │  │      public Task DeleteAttachmentAsync(Guid cipherId, CipherAttachment.MetaData attachmentData)
   │  │      return Task.FromResult(0);
   │  ├─ call ValidateCipherAttachmentFile  (src/Core/Vault/Services/Implementations/CipherService.cs:270) [verified]
   │  │      public async Task<bool> ValidateCipherAttachmentFile(Cipher cipher, CipherAttachment.MetaData attachmentData)
   │  │      var (valid, realSize) = await _attachmentStorageService.ValidateFileAsync(cipher, attachmentData, _fileSizeLeeway);
   │  │      if (!valid || realSize > MAX_FILE_SIZE)
   │  │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:372) [approx]
   │  │  │      public interface IAttachmentStorageService
   │  │  │      FileUploadType FileUploadType { get; }
   │  │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  │  (stopped at depth 3; 3 branches omitted)
   │  │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:397) [approx]
   │  │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  │  (stopped at depth 3; 2 branches omitted)
   │  │  ├─ call CipherRepository.UpdateAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:397) [verified]
   │  │  │      public async Task UpdateAttachmentAsync(CipherAttachment attachment)
   │  │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │  │      var results = await connection.ExecuteAsync(
   │  │  ├─ call DeleteAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:377) [verified]
   │  │  │      public async Task<DeleteAttachmentResponseData> DeleteAttachmentAsync(Cipher cipher, string attachmentId, Guid deletingUserId,
   │  │  │      bool orgAdmin = false)
   │  │  │      if (!orgAdmin && !(await UserCanEditAsync(cipher, deletingUserId)))
   │  │  │  (4 more branches omitted beyond fan-out)
   │  │  │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:490) [approx]
   │  │  │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │  │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │  │  │      public Guid Id { get; set; }
   │  │  │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:490) [approx]
   │  │  │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │  │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │  │  │      public Guid Id { get; set; }
   │  │  │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:922) [approx]
   │  │  │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │  │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │  │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  │  │  (stopped at depth 4; 2 branches omitted)
   │  │  │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:924) [approx]
   │  │  │  │      public interface IAttachmentStorageService
   │  │  │  │      FileUploadType FileUploadType { get; }
   │  │  │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  │  │  (stopped at depth 4; 3 branches omitted)
   │  │  │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:925) [approx]
   │  │  │  │      public interface IEventService
   │  │  │  │      /// <summary>
   │  │  │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  │  │  (stopped at depth 4; 1 branch omitted)
   │  │  │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:939) [approx]
   │  │  │  │      public interface ICipherSyncPushService
   │  │  │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  │  │  (stopped at depth 4; 1 branch omitted)
   │  │  │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:939) [verified]
   │  │  │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │  │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  │  │  (stopped at depth 4; 1 branch omitted)
   │  │  │  ├─ call CipherRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:935) [verified]
   │  │  │  │      public async Task ReplaceAsync(CipherDetails obj)
   │  │  │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │  │  │      var results = await connection.ExecuteAsync(
   │  │  │  │  (stopped at depth 4; 1 branch omitted)
   │  │  │  ├─ call IGroupRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:931) [verified]
   │  │  │  │      Task ReplaceAsync(Group obj, IEnumerable<CollectionAccessSelection> collections);
   │  │  │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:925) [verified]
   │  │  │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │  │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │  │  │      if (e != null)
   │  │  │  │  (stopped at depth 4; 3 branches omitted)
   │  │  │  ├─ call NoopAttachmentStorageService.DeleteAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:924) [verified]
   │  │  │  │      public Task DeleteAttachmentAsync(Guid cipherId, CipherAttachment.MetaData attachmentData)
   │  │  │  │      return Task.FromResult(0);
   │  │  │  └─ call Cipher.DeleteAttachment  (src/Core/Vault/Services/Implementations/CipherService.cs:923) [verified]
   │  │  │         public void DeleteAttachment(string id)
   │  │  │         var attachments = GetAttachments();
   │  │  │         if (!attachments?.ContainsKey(id) ?? true)
   │  │  │     ├─ call Cipher.SetAttachments  (src/Core/Vault/Entities/Cipher.cs:143) [verified]
   │  │  │     │      public void SetAttachments(Dictionary<string, CipherAttachment.MetaData> data)
   │  │  │     │      if (data == null || data.Count == 0)
   │  │  │     │      _attachmentData = null;
   │  │  │     └─ call Cipher.GetAttachments  (src/Core/Vault/Entities/Cipher.cs:136) [verified]
   │  │  │            public Dictionary<string, CipherAttachment.MetaData> GetAttachments()
   │  │  │            if (string.IsNullOrWhiteSpace(Attachments))
   │  │  │            return null;
   │  │  └─ call NoopAttachmentStorageService.ValidateFileAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:372) [verified]
   │  │         public Task<(bool, long?)> ValidateFileAsync(Cipher cipher, CipherAttachment.MetaData attachmentData, long leeway)
   │  │         return Task.FromResult((false, (long?)null));
   │  └─ call Cipher.AddAttachment  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [verified]
   │         public void AddAttachment(string id, CipherAttachment.MetaData data)
   │         var attachments = GetAttachments();
   │         if (attachments == null)
   │     ├─ call Cipher.SetAttachments  (src/Core/Vault/Entities/Cipher.cs:131) [verified]
   │     │      public void SetAttachments(Dictionary<string, CipherAttachment.MetaData> data)
   │     │      if (data == null || data.Count == 0)
   │     │      _attachmentData = null;
   │     └─ call Cipher.GetAttachments  (src/Core/Vault/Entities/Cipher.cs:124) [verified]
   │            public Dictionary<string, CipherAttachment.MetaData> GetAttachments()
   │            if (string.IsNullOrWhiteSpace(Attachments))
   │            return null;
   ├─ call DeleteAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:482)
   │  (stopped at depth 1; 16 branches omitted)
   ├─ call CreateAttachmentForDelayedUploadAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:199)
   │      public async Task<(string attachmentId, string uploadUrl)> CreateAttachmentForDelayedUploadAsync(Cipher cipher,
   │      string key, string fileName, long fileSize, bool adminRequest, Guid savingUserId, DateTime? lastKnownRevisionDate = null)
   │      ValidateCipherLastKnownRevisionDate(cipher, lastKnownRevisionDate);
   │  (3 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:225) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:215) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:217) [verified]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:225) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:227) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:233) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:233) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:231) [verified]
   │  │      public async Task ReplaceAsync(CipherDetails obj)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:227) [verified]
   │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │      if (e != null)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call Cipher.AddAttachment  (src/Core/Vault/Services/Implementations/CipherService.cs:225) [verified]
   │  │      public void AddAttachment(string id, CipherAttachment.MetaData data)
   │  │      var attachments = GetAttachments();
   │  │      if (attachments == null)
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call CipherRepository.UpdateAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:217) [verified]
   │  │      public async Task UpdateAttachmentAsync(CipherAttachment attachment)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  └─ call NoopAttachmentStorageService.GetAttachmentUploadUrlAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:215) [verified]
   │         public Task<string> GetAttachmentUploadUrlAsync(Cipher cipher, CipherAttachment.MetaData attachmentData)
   │         return Task.FromResult(default(string));
   ├─ call DeleteManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:446)
   │      public async Task DeleteManyAsync(IEnumerable<Guid> cipherIds, Guid deletingUserId, Guid? organizationId = null, bool orgAdmin = false)
   │      var cipherIdsSet = new HashSet<Guid>(cipherIds);
   │      var deletingCiphers = new List<Cipher>();
   │  (2 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:449) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:453) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:468) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:475) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:479) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:479) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:475) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  ├─ call IEventWriteService  (src/Core/Dirt/Services/Implementations/EventService.cs:138) [approx]
   │  │  │      public interface IEventWriteService
   │  │  │      Task CreateAsync(IEvent e);
   │  │  │      Task CreateManyAsync(IEnumerable<IEvent> e);
   │  │  ├─ call CreateManyAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:138) [verified]
   │  │  │      public async Task CreateManyAsync(IEnumerable<IEvent> events)
   │  │  │      var eventList = events as IList<IEvent> ?? events.ToList();
   │  │  │      if (eventList.Count == 0)
   │  │  │  ├─ call IEventIntegrationPublisher  (src/Core/Dirt/Services/Implementations/EventIntegrationEventWriteService.cs:32) [approx]
   │  │  │  │      public interface IEventIntegrationPublisher : IAsyncDisposable
   │  │  │  │      Task PublishAsync(IIntegrationMessage message);
   │  │  │  │      Task PublishEventAsync(string body, string? organizationId);
   │  │  │  ├─ call IAzureServiceBusService.PublishEventAsync  (src/Core/Dirt/Services/Implementations/EventIntegrationEventWriteService.cs:32) [verified]
   │  │  │  └─ call DomainIcons.ToList  (src/Core/Dirt/Services/Implementations/EventIntegrationEventWriteService.cs:24) [verified]
   │  │  └─ call BuildCipherEventMessageAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:132) [verified]
   │  │         private async Task<EventMessage> BuildCipherEventMessageAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │         // Only logging organization cipher events for now.
   │  │         if (!cipher.OrganizationId.HasValue || (!_currentContext?.UserId.HasValue ?? true))
   │  │     (stopped at depth 3; 4 branches omitted)
   │  ├─ call NoopAttachmentStorageService.DeleteAttachmentsForCipherAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:468) [verified]
   │  │      public Task DeleteAttachmentsForCipherAsync(Guid cipherId)
   │  │      return Task.FromResult(0);
   │  ├─ call CipherRepository.DeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:462) [verified]
   │  │      public async Task DeleteAsync(IEnumerable<Guid> ids, Guid userId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:272) [verified]
   │  ├─ call FilterCiphersByDeletePermission  (src/Core/Vault/Services/Implementations/CipherService.cs:460) [verified]
   │  │      private async Task<List<T>> FilterCiphersByDeletePermission<T>(
   │  │      IEnumerable<T> ciphers,
   │  │      HashSet<Guid> cipherIdsSet,
   │  │  ├─ call IUserService  (src/Core/Vault/Services/Implementations/CipherService.cs:1174) [approx]
   │  │  │      public interface IUserService
   │  │  │      Guid? GetProperUserId(ClaimsPrincipal principal);
   │  │  │      Task<User> GetUserByIdAsync(string userId);
   │  │  │  └─ di UserService  (src/Core/Auth/UserFeatures/UserServiceCollectionExtensions.cs:31)
   │  │  │         public class UserService : UserManager<User>, IUserService
   │  │  │         private readonly IUserRepository _userRepository;
   │  │  │         private readonly IOrganizationUserRepository _organizationUserRepository;
   │  │  ├─ call CanDelete  (src/Core/Vault/Services/Implementations/CipherService.cs:1190) [verified]
   │  │  │      public static bool CanDelete(User user, CipherDetails cipherDetails, OrganizationAbility? organizationAbility)
   │  │  │      if (cipherDetails.OrganizationId == null && cipherDetails.UserId == null)
   │  │  │      throw new Exception("Cipher needs to belong to a user or an organization.");
   │  │  ├─ call DomainIcons.SelectMany  (src/Core/Vault/Services/Implementations/CipherService.cs:1183) [approx]
   │  │  ├─ call GetOrganizationAbilitiesAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1181) [verified]
   │  │  │      private async Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync<T>(IEnumerable<IGrouping<Guid?, T>> groupedCiphers) where T : CipherDetails
   │  │  │      var organizationIds = groupedCiphers
   │  │  │      .Select(group => group.Key)
   │  │  │  ├─ call IOrganizationAbilityCacheService  (src/Core/Vault/Services/Implementations/CipherService.cs:1205) [approx]
   │  │  │  │      public interface IOrganizationAbilityCacheService
   │  │  │  │      Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default);
   │  │  │  │      Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default);
   │  │  │  │  (stopped at depth 4; 1 branch omitted)
   │  │  │  ├─ call ExtendedOrganizationAbilityCacheService.GetOrganizationAbilitiesAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1205) [verified]
   │  │  │  │      public async Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default)
   │  │  │  │      var tasks = orgIds.Distinct().Select(async orgId => (orgId, ability: await GetOrganizationAbilityAsync(orgId, cancellationToken)));
   │  │  │  │      var results = await Task.WhenAll(tasks);
   │  │  │  │  ├─ call GetOrganizationAbilityAsync  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:30) [verified]
   │  │  │  │  │      public async Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default)
   │  │  │  │  │      return await cache.GetOrSetAsync<OrganizationAbility?>(
   │  │  │  │  │      orgId.ToString(),
   │  │  │  │  │  (stopped at depth 5; 2 branches omitted)
   │  │  │  │  └─ call DomainIcons.Distinct  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:30) [verified]
   │  │  │  └─ call DomainIcons.Select  (src/Core/Vault/Services/Implementations/CipherService.cs:1199) [verified]
   │  │  ├─ call DomainIcons.Where  (src/Core/Vault/Services/Implementations/CipherService.cs:1176) [verified]
   │  │  └─ call GetUserByIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1174) [verified]
   │  │         public async Task<User> GetUserByIdAsync(string userId)
   │  │         if (_currentContext?.User != null &&
   │  │         string.Equals(_currentContext.User.Id.ToString(), userId, StringComparison.InvariantCultureIgnoreCase))
   │  │     ├─ data User  (src/Core/Services/Implementations/UserService.cs:159) [approx]
   │  │     │      public class User : ITableObject<Guid>, IStorableSubscriber, IRevisable, ITwoFactorProvidersUser
   │  │     │      private Dictionary<TwoFactorProviderType, TwoFactorProvider>? _twoFactorProviders;
   │  │     │      public Guid Id { get; set; }
   │  │     ├─ call ICurrentContext  (src/Core/Services/Implementations/UserService.cs:160) [approx]
   │  │     │      /// <summary>
   │  │     │      /// Provides information about the current HTTP request and the currently authenticated user (if any).
   │  │     │      /// This is often (but not exclusively) parsed from the JWT in the current request.
   │  │     │  (stopped at depth 4; 1 branch omitted)
   │  │     ├─ call IUserRepository  (src/Core/Services/Implementations/UserService.cs:170) [approx]
   │  │     │      public interface IUserRepository : IRepository<User, Guid>
   │  │     │      Task<User?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │     │      Task<User?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │     │  (stopped at depth 4; 4 branches omitted)
   │  │     ├─ call IGroupRepository.GetByIdAsync  (src/Core/Services/Implementations/UserService.cs:181) [verified]
   │  │     └─ call CurrentContext.ToString  (src/Core/Services/Implementations/UserService.cs:160) [approx]
   │  ├─ call CipherRepository.GetManyByUserIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:459) [verified]
   │  │      public async Task<ICollection<CipherDetails>> GetManyByUserIdAsync(Guid userId, bool withOrganizations = true)
   │  │      string sprocName = null;
   │  │      if (withOrganizations)
   │  └─ call CipherRepository.DeleteByIdsOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:455) [verified]
   │         public async Task DeleteByIdsOrganizationIdAsync(IEnumerable<Guid> ids, Guid organizationId)
   │         using (var connection = new SqlConnection(ConnectionString))
   │         var results = await connection.ExecuteAsync(
   │     └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:283) [verified]
   ├─ call SoftDeleteManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:762)
   │      public async Task SoftDeleteManyAsync(IEnumerable<Guid> cipherIds, Guid deletingUserId, Guid? organizationId, bool orgAdmin)
   │      var cipherIdsSet = new HashSet<Guid>(cipherIds);
   │      var deletingCiphers = new List<Cipher>();
   │  (2 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:765) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:769) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call ISecurityTaskRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:781) [approx]
   │  │      public interface ISecurityTaskRepository : IRepository<SecurityTask, Guid>
   │  │      /// <summary>
   │  │      /// Retrieves security tasks for a user based on their organization and cipher access permissions.
   │  │  ├─ di SecurityTaskRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:114) [×2 impls]
   │  │  │      public class SecurityTaskRepository : Repository<Core.Vault.Entities.SecurityTask, SecurityTask, Guid>, ISecurityTaskRepository
   │  │  │      public SecurityTaskRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
   │  │  │      : base(serviceScopeFactory, mapper, (context) => context.SecurityTasks)
   │  │  └─ di SecurityTaskRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:77) [×2 impls]
   │  │         public class SecurityTaskRepository : Repository<SecurityTask, Guid>, ISecurityTaskRepository
   │  │         public SecurityTaskRepository(GlobalSettings globalSettings)
   │  │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:787) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:791) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:791) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:787) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call SecurityTaskRepository.MarkAsCompleteByCipherIds  (src/Core/Vault/Services/Implementations/CipherService.cs:781) [verified]
   │  │      public async Task MarkAsCompleteByCipherIds(IEnumerable<Guid> cipherIds)
   │  │      if (!cipherIds.Any())
   │  │      return;
   │  │  ├─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/SecurityTaskRepository.cs:100) [verified]
   │  │  └─ call DomainIcons.Any  (src/Infrastructure.Dapper/Vault/Repositories/SecurityTaskRepository.cs:92) [verified]
   │  ├─ call CipherRepository.SoftDeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:778) [verified]
   │  │      public async Task SoftDeleteAsync(IEnumerable<Guid> ids, Guid userId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:589) [verified]
   │  ├─ call FilterCiphersByDeletePermission  (src/Core/Vault/Services/Implementations/CipherService.cs:776) [verified]
   │  │      private async Task<List<T>> FilterCiphersByDeletePermission<T>(
   │  │      IEnumerable<T> ciphers,
   │  │      HashSet<Guid> cipherIdsSet,
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call CipherRepository.GetManyByUserIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:775) [verified]
   │  │      public async Task<ICollection<CipherDetails>> GetManyByUserIdAsync(Guid userId, bool withOrganizations = true)
   │  │      string sprocName = null;
   │  │      if (withOrganizations)
   │  └─ call CipherRepository.SoftDeleteByIdsOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:771) [verified]
   │         public async Task SoftDeleteByIdsOrganizationIdAsync(IEnumerable<Guid> ids, Guid organizationId)
   │         using (var connection = new SqlConnection(ConnectionString))
   │         var results = await connection.ExecuteAsync(
   │     └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:294) [verified]
   ├─ call RestoreManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:817)
   │      public async Task<ICollection<CipherOrganizationDetails>> RestoreManyAsync(IEnumerable<Guid> cipherIds, Guid restoringUserId, Guid? organizationId = null, bool orgAdmin = false)
   │      if (cipherIds == null || !cipherIds.Any())
   │      return new List<CipherOrganizationDetails>();
   │  (1 more branch omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:846) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:830) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:850) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:854) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:854) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:850) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call CipherRepository.RestoreAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:839) [verified]
   │  │      public async Task<DateTime> RestoreAsync(IEnumerable<Guid> ids, Guid userId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteScalarAsync<DateTime>(
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:613) [verified]
   │  ├─ call FilterCiphersByDeletePermission  (src/Core/Vault/Services/Implementations/CipherService.cs:837) [verified]
   │  │      private async Task<List<T>> FilterCiphersByDeletePermission<T>(
   │  │      IEnumerable<T> ciphers,
   │  │      HashSet<Guid> cipherIdsSet,
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call CipherRepository.GetManyByUserIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:836) [verified]
   │  │      public async Task<ICollection<CipherDetails>> GetManyByUserIdAsync(Guid userId, bool withOrganizations = true)
   │  │      string sprocName = null;
   │  │      if (withOrganizations)
   │  ├─ call CipherRepository.RestoreByIdsOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:832) [verified]
   │  │      public async Task<DateTime> RestoreByIdsOrganizationIdAsync(IEnumerable<Guid> ids, Guid organizationId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteScalarAsync<DateTime>(
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:626) [verified]
   │  ├─ call DomainIcons.Where  (src/Core/Vault/Services/Implementations/CipherService.cs:831) [approx]
   │  └─ call CipherRepository.GetManyOrganizationDetailsByOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:830) [verified]
   │         public async Task<ICollection<CipherOrganizationDetails>> GetManyOrganizationDetailsByOrganizationIdAsync(
   │         Guid organizationId)
   │         using (var connection = new SqlConnection(ConnectionString))
   ├─ call ShareManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:671)
   │      public async Task<IEnumerable<CipherDetails>> ShareManyAsync(IEnumerable<(CipherDetails cipher, DateTime? lastKnownRevisionDate)> cipherInfos,
   │      Guid organizationId, IEnumerable<Guid> collectionIds, Guid sharingUserId)
   │      var cipherIds = new List<Guid>();
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:690) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:685) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:686) [approx]
   │  │      public interface ICollectionCipherRepository
   │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:693) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:697) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call DomainIcons.Select  (src/Core/Vault/Services/Implementations/CipherService.cs:698) [verified]
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:697) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:693) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call CollectionCipherRepository.UpdateCollectionsForCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:686) [verified]
   │  │      public async Task UpdateCollectionsForCiphersAsync(IEnumerable<Guid> cipherIds, Guid userId,
   │  │      Guid organizationId, IEnumerable<Guid> collectionIds)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │  └─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Repositories/CollectionCipherRepository.cs:134) [verified]
   │  ├─ call CipherRepository.UpdateCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:685) [verified]
   │  │      public async Task UpdateCiphersAsync(Guid userId, IEnumerable<Cipher> ciphers)
   │  │      if (!ciphers.Any())
   │  │      return;
   │  │  ├─ call BulkResourceCreationService.CreateTempCiphersAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:442) [verified]
   │  │  │      public static async Task CreateTempCiphersAsync(SqlConnection connection, SqlTransaction transaction, IEnumerable<Cipher> ciphers, string errorMessage = _defaultErrorMessage)
   │  │  │      using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
   │  │  │      bulkCopy.DestinationTableName = "#TempCipher";
   │  │  │  └─ call BulkResourceCreationService.BuildCiphersTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:62) [verified]
   │  │  │         private static DataTable BuildCiphersTable(SqlBulkCopy bulkCopy, IEnumerable<Cipher> ciphers, string errorMessage)
   │  │  │         var c = ciphers.FirstOrDefault();
   │  │  │         if (c == null)
   │  │  │     └─ call DomainIcons.FirstOrDefault  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:190) [verified]
   │  │  └─ call DomainIcons.Any  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:416) [verified]
   │  ├─ call DomainIcons.Add  (src/Core/Vault/Services/Implementations/CipherService.cs:682) [approx]
   │  └─ call ValidateCipherCanBeShared  (src/Core/Vault/Services/Implementations/CipherService.cs:677) [verified]
   │         private async Task ValidateCipherCanBeShared(
   │         Cipher cipher,
   │         Guid sharingUserId,
   │     ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:1052) [approx]
   │     │      public class Cipher : ITableObject<Guid>, ICloneable
   │     │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │     │      public Guid Id { get; set; }
   │     ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:1052) [approx]
   │     │      public class Cipher : ITableObject<Guid>, ICloneable
   │     │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │     │      public Guid Id { get; set; }
   │     ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:1054) [approx]
   │     │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │     │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │     │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │     │  (stopped at depth 3; 4 branches omitted)
   │     ├─ call CipherService.ValidateCipherLastKnownRevisionDate  (src/Core/Vault/Services/Implementations/CipherService.cs:1075) [verified]
   │     │      private void ValidateCipherLastKnownRevisionDate(Cipher cipher, DateTime? lastKnownRevisionDate)
   │     │      if (cipher.Id == default || !lastKnownRevisionDate.HasValue)
   │     │      return;
   │     ├─ call IgnoreStorageLimitsOnMigrationAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1061) [verified]
   │     │      private async Task<bool> IgnoreStorageLimitsOnMigrationAsync(Guid userId, Organization organization)
   │     │      if (!organization.UsePolicies)
   │     │      return false;
   │     │  ├─ call IPolicyRequirementQuery  (src/Core/Vault/Services/Implementations/CipherService.cs:1088) [approx]
   │     │  │      public interface IPolicyRequirementQuery
   │     │  │      /// <summary>
   │     │  │      /// Get a policy requirement for a specific user.
   │     │  │  (stopped at depth 4; 1 branch omitted)
   │     │  └─ call PolicyRequirementQuery.GetAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1088) [verified]
   │     │         public async Task<T> GetAsync<T>(Guid userId) where T : IPolicyRequirement
   │     │         => (await GetAsync<T>([userId])).Single().Requirement;
   │     │     ├─ call GetPolicyDetails  (src/Core/AdminConsole/OrganizationFeatures/Policies/Implementations/PolicyRequirementQuery.cs:40) [verified]
   │     │     │      private async Task<IEnumerable<OrganizationPolicyDetails>> GetPolicyDetails(IEnumerable<Guid> userIds, PolicyType policyType)
   │     │     │      => await policyRepository.GetPolicyDetailsByUserIdsAndPolicyType(userIds, policyType);
   │     │     │  ├─ call IPolicyRepository  (src/Core/AdminConsole/OrganizationFeatures/Policies/Implementations/PolicyRequirementQuery.cs:50) [approx]
   │     │     │  │      public interface IPolicyRepository : IRepository<Policy, Guid>
   │     │     │  │      /// <summary>
   │     │     │  │      /// Gets all policies of a given type for an organization where the user is in the Confirmed status.
   │     │     │  │  (stopped at depth 6; 2 branches omitted)
   │     │     │  └─ call PolicyRepository.GetPolicyDetailsByUserIdsAndPolicyType  (src/Core/AdminConsole/OrganizationFeatures/Policies/Implementations/PolicyRequirementQuery.cs:50) [verified]
   │     │     │         public async Task<IEnumerable<OrganizationPolicyDetails>> GetPolicyDetailsByUserIdsAndPolicyType(IEnumerable<Guid> userIds, PolicyType type)
   │     │     │         await using var connection = new SqlConnection(ConnectionString);
   │     │     │         var results = await connection.QueryAsync<OrganizationPolicyDetails>(
   │     │     │     (stopped at depth 6; 1 branch omitted)
   │     │     ├─ call DomainIcons.ToList  (src/Core/AdminConsole/OrganizationFeatures/Policies/Implementations/PolicyRequirementQuery.cs:38) [verified]
   │     │     └─ call DomainIcons.OfType  (src/Core/AdminConsole/OrganizationFeatures/Policies/Implementations/PolicyRequirementQuery.cs:32) [verified]
   │     ├─ call IGroupRepository.GetByIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1054) [verified]
   │     └─ call Cipher.GetAttachments  (src/Core/Vault/Services/Implementations/CipherService.cs:1052) [verified]
   │            public Dictionary<string, CipherAttachment.MetaData> GetAttachments()
   │            if (string.IsNullOrWhiteSpace(Attachments))
   │            return null;
   ├─ call CreateAttachmentShareAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:292)
   │      public async Task CreateAttachmentShareAsync(Cipher cipher, Stream stream, string fileName, string key,
   │      long requestLength, string attachmentId, Guid organizationId)
   │      try
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:324) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:312) [approx]
   │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:324) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:348) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:361) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call NoopAttachmentStorageService.CleanupAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:365) [verified]
   │  │      public Task CleanupAsync(Guid cipherId)
   │  │      return Task.FromResult(0);
   │  ├─ call CipherRepository.UpdateAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:361) [verified]
   │  │      public async Task UpdateAttachmentAsync(CipherAttachment attachment)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  ├─ call NoopAttachmentStorageService.UploadShareAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:348) [verified]
   │  │      public Task UploadShareAttachmentAsync(Stream stream, Guid cipherId, Guid organizationId, CipherAttachment.MetaData attachmentData)
   │  │      return Task.FromResult(0);
   │  ├─ call CoreHelpers.CloneObject  (src/Core/Vault/Services/Implementations/CipherService.cs:338) [verified]
   │  │      public static T CloneObject<T>(T obj)
   │  │      return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj))!;
   │  ├─ call Cipher.GetAttachments  (src/Core/Vault/Services/Implementations/CipherService.cs:324) [verified]
   │  │      public Dictionary<string, CipherAttachment.MetaData> GetAttachments()
   │  │      if (string.IsNullOrWhiteSpace(Attachments))
   │  │      return null;
   │  └─ call IGroupRepository.GetByIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:312) [verified]
   ├─ call SaveAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:84)
   │      public async Task SaveAsync(Cipher cipher, Guid savingUserId, DateTime? lastKnownRevisionDate,
   │      IEnumerable<Guid>? collectionIds = null, bool skipPermissionCheck = false, bool limitCollectionScope = true)
   │      if (!skipPermissionCheck && !(await UserCanEditAsync(cipher, savingUserId)))
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:101) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:107) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:110) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:120) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:117) [verified]
   │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │      if (e != null)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IGroupRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:116) [verified]
   │  │      Task ReplaceAsync(Group obj, IEnumerable<CollectionAccessSelection> collections);
   │  ├─ call CipherService.ValidateCipherLastKnownRevisionDate  (src/Core/Vault/Services/Implementations/CipherService.cs:114) [verified]
   │  │      private void ValidateCipherLastKnownRevisionDate(Cipher cipher, DateTime? lastKnownRevisionDate)
   │  │      if (cipher.Id == default || !lastKnownRevisionDate.HasValue)
   │  │      return;
   │  ├─ call CipherSyncPushService.PushSyncCipherCreateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:110) [verified]
   │  │      public Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherCreate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call IGroupRepository.CreateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:105) [verified]
   │  │      Task CreateAsync(Group obj, IEnumerable<CollectionAccessSelection> collections);
   │  ├─ call CreateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:101) [verified]
   │  │      public async Task CreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      cipher.SetNewId();
   │  │      var objWithCollections = JsonSerializer.Deserialize<CipherWithCollections>(
   │  │  ├─ call DomainIcons.First  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:569) [verified]
   │  │  ├─ call BulkResourceCreationService.CreateFoldersAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:564) [verified]
   │  │  │      public static async Task CreateFoldersAsync(SqlConnection connection, SqlTransaction transaction, IEnumerable<Folder> folders, string errorMessage = _defaultErrorMessage)
   │  │  │      using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
   │  │  │      bulkCopy.DestinationTableName = "[dbo].[Folder]";
   │  │  │  └─ call BulkResourceCreationService.BuildFoldersTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:46) [verified]
   │  │  │         private static DataTable BuildFoldersTable(SqlBulkCopy bulkCopy, IEnumerable<Folder> folders, string errorMessage)
   │  │  │         var f = folders.FirstOrDefault();
   │  │  │         if (f == null)
   │  │  │     └─ call DomainIcons.FirstOrDefault  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:264) [verified]
   │  │  ├─ call DomainIcons.Any  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:562) [verified]
   │  │  ├─ call BulkResourceCreationService.CreateCollectionsUsersAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:559) [verified]
   │  │  │      public static async Task CreateCollectionsUsersAsync(SqlConnection connection, SqlTransaction transaction,
   │  │  │      IEnumerable<CollectionUser> collectionUsers, string errorMessage = _defaultErrorMessage)
   │  │  │      // Offload some work from SQL Server by pre-sorting before insert.
   │  │  │  ├─ call BulkResourceCreationService.BuildCollectionsUsersTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:30) [verified]
   │  │  │  │      private static DataTable BuildCollectionsUsersTable(SqlBulkCopy bulkCopy, IEnumerable<CollectionUser> collectionUsers, string errorMessage)
   │  │  │  │      var collectionUser = collectionUsers.FirstOrDefault();
   │  │  │  │      if (collectionUser == null)
   │  │  │  │  └─ call DomainIcons.FirstOrDefault  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:68) [verified]
   │  │  │  └─ call DomainIcons.OrderBySqlGuid  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:17) [verified]
   │  │  ├─ call BulkResourceCreationService.CreateCollectionCiphersAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:554) [verified]
   │  │  │      public static async Task CreateCollectionCiphersAsync(SqlConnection connection, SqlTransaction transaction, IEnumerable<CollectionCipher> collectionCiphers, string errorMessage = _defaultErrorMessage)
   │  │  │      using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
   │  │  │      bulkCopy.DestinationTableName = "[dbo].[CollectionCipher]";
   │  │  │  └─ call BulkResourceCreationService.BuildCollectionCiphersTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:54) [verified]
   │  │  │         private static DataTable BuildCollectionCiphersTable(SqlBulkCopy bulkCopy, IEnumerable<CollectionCipher> collectionCiphers, string errorMessage)
   │  │  │         var cc = collectionCiphers.FirstOrDefault();
   │  │  │         if (cc == null)
   │  │  │     └─ call DomainIcons.FirstOrDefault  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:311) [verified]
   │  │  ├─ call BulkResourceCreationService.CreateCollectionsAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:549) [verified]
   │  │  │      public static async Task CreateCollectionsAsync(SqlConnection connection, SqlTransaction transaction,
   │  │  │      IEnumerable<Collection> collections, string errorMessage = _defaultErrorMessage)
   │  │  │      // Offload some work from SQL Server by pre-sorting before insert.
   │  │  │  ├─ call BulkResourceCreationService.BuildCollectionsTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:128) [verified]
   │  │  │  │      private static DataTable BuildCollectionsTable(SqlBulkCopy bulkCopy, IEnumerable<Collection> collections, string errorMessage)
   │  │  │  │      var collection = collections.FirstOrDefault();
   │  │  │  │      if (collection == null)
   │  │  │  │  └─ call DomainIcons.FirstOrDefault  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:134) [verified]
   │  │  │  └─ call DomainIcons.OrderBySqlGuid  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:119) [verified]
   │  │  ├─ call BulkResourceCreationService.CreateCiphersAsync  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:545) [verified]
   │  │  │      public static async Task CreateCiphersAsync(SqlConnection connection, SqlTransaction transaction, IEnumerable<Cipher> ciphers, string errorMessage = _defaultErrorMessage)
   │  │  │      using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
   │  │  │      bulkCopy.DestinationTableName = "[dbo].[Cipher]";
   │  │  │  └─ call BulkResourceCreationService.BuildCiphersTable  (src/Infrastructure.Dapper/AdminConsole/Helpers/BulkResourceCreationService.cs:38) [verified]
   │  │  │         private static DataTable BuildCiphersTable(SqlBulkCopy bulkCopy, IEnumerable<Cipher> ciphers, string errorMessage)
   │  │  │         var c = ciphers.FirstOrDefault();
   │  │  │         if (c == null)
   │  │  │     (stopped at depth 4; 1 branch omitted)
   │  │  ├─ call DomainIcons.ToGuidIdArrayTVP  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:171) [verified]
   │  │  └─ call Cipher.SetNewId  (src/Infrastructure.Dapper/Vault/Repositories/CipherRepository.cs:168) [verified]
   │  │         public void SetNewId()
   │  │         Id = CoreHelpers.GenerateComb();
   │  │     └─ call CoreHelpers.GenerateComb  (src/Core/Vault/Entities/Cipher.cs:33) [verified]
   │  │            [Obsolete("Use Bit.Core.Utilities.CombGuid.Generate() instead.")]
   │  │            public static Guid GenerateComb()
   │  │            => CombGuid.Generate();
   │  │        └─ call CombGuid.Generate  (src/Core/Utilities/CoreHelpers.cs:53) [verified]
   │  │               public static Guid Generate()
   │  │               => Generate(Guid.NewGuid(), DateTime.UtcNow);
   │  └─ call UserCanEditAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:87) [verified]
   │         private async Task<bool> UserCanEditAsync(Cipher cipher, Guid userId)
   │         if (!cipher.OrganizationId.HasValue && cipher.UserId.HasValue && cipher.UserId.Value == userId)
   │         return true;
   │     ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:879) [approx]
   │     │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │     │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │     │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │     │  (stopped at depth 3; 2 branches omitted)
   │     └─ call CipherRepository.GetCanEditByIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:879) [verified]
   │            public async Task<bool> GetCanEditByIdAsync(Guid userId, Guid cipherId)
   │            using (var connection = new SqlConnection(ConnectionString))
   │            var result = await connection.QueryFirstOrDefaultAsync<bool>(
   └─ call DeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:429)
          public async Task DeleteAsync(CipherDetails cipherDetails, Guid deletingUserId, bool orgAdmin = false)
          if (!orgAdmin && !await UserCanDeleteAsync(cipherDetails, deletingUserId))
          throw new BadRequestException("You do not have permissions to delete this.");
      ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:438) [approx]
      │      public interface ICipherRepository : IRepository<Cipher, Guid>
      │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
      │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
      │  (stopped at depth 2; 2 branches omitted)
      ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:439) [approx]
      │      public interface IAttachmentStorageService
      │      FileUploadType FileUploadType { get; }
      │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
      │  (stopped at depth 2; 3 branches omitted)
      ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:440) [approx]
      │      public interface IEventService
      │      /// <summary>
      │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
      │  (stopped at depth 2; 1 branch omitted)
      ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:443) [approx]
      │      public interface ICipherSyncPushService
      │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
      │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
      │  (stopped at depth 2; 1 branch omitted)
      ├─ call CipherSyncPushService.PushSyncCipherDeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:443) [verified]
      │      public Task PushSyncCipherDeleteAsync(Cipher cipher, IEnumerable<Guid>? collectionIds = null)
      │      => PushCipherAsync(cipher, PushType.SyncLoginDelete, collectionIds);
      │  └─ call PushCipherAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:37) [verified]
      │         private async Task PushCipherAsync(Cipher cipher, PushType pushType, IEnumerable<Guid>? collectionIds)
      │         if (cipher.OrganizationId.HasValue)
      │         if (!_featureService.IsEnabled(FeatureFlagKeys.OrgCipherPushFanout))
      │     (stopped at depth 3; 10 branches omitted)
      ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:440) [verified]
      │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
      │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
      │      if (e != null)
      │  (stopped at depth 2; 3 branches omitted)
      ├─ call NoopAttachmentStorageService.DeleteAttachmentsForCipherAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:439) [verified]
      │      public Task DeleteAttachmentsForCipherAsync(Guid cipherId)
      │      return Task.FromResult(0);
      ├─ call IGroupRepository.DeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:438) [verified]
      ├─ call GetCollectionIdsForPushAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:436) [verified]
      │      private async Task<ICollection<Guid>?> GetCollectionIdsForPushAsync(Cipher cipher)
      │      if (!cipher.OrganizationId.HasValue)
      │      return null;
      │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:1028) [approx]
      │  │      public interface ICollectionCipherRepository
      │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
      │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
      │  │  (stopped at depth 3; 2 branches omitted)
      │  └─ call CollectionCipherRepository.GetCollectionIdsByCipherIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:1028) [verified]
      │         public async Task<ICollection<Guid>> GetCollectionIdsByCipherIdAsync(Guid cipherId)
      │         using (var connection = new SqlConnection(ConnectionString))
      │         var results = await connection.QueryAsync<Guid>(
      └─ call UserCanDeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:431) [verified]
             private async Task<bool> UserCanDeleteAsync(CipherDetails cipher, Guid userId)
             var user = await _userService.GetUserByIdAsync(userId);
             var organizationAbility = cipher.OrganizationId.HasValue ?
         ├─ call IUserService  (src/Core/Vault/Services/Implementations/CipherService.cs:884) [approx]
         │      public interface IUserService
         │      Guid? GetProperUserId(ClaimsPrincipal principal);
         │      Task<User> GetUserByIdAsync(string userId);
         │  (stopped at depth 3; 1 branch omitted)
         ├─ call IOrganizationAbilityCacheService  (src/Core/Vault/Services/Implementations/CipherService.cs:886) [approx]
         │      public interface IOrganizationAbilityCacheService
         │      Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default);
         │      Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default);
         │  (stopped at depth 3; 1 branch omitted)
         ├─ call CanDelete  (src/Core/Vault/Services/Implementations/CipherService.cs:888) [verified]
         │      public static bool CanDelete(User user, CipherDetails cipherDetails, OrganizationAbility? organizationAbility)
         │      if (cipherDetails.OrganizationId == null && cipherDetails.UserId == null)
         │      throw new Exception("Cipher needs to belong to a user or an organization.");
         ├─ call GetOrganizationAbilityAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:886) [verified]
         │      public async Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default)
         │      return await cache.GetOrSetAsync<OrganizationAbility?>(
         │      orgId.ToString(),
         │  (stopped at depth 3; 2 branches omitted)
         └─ call GetUserByIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:884) [verified]
                public async Task<User> GetUserByIdAsync(string userId)
                if (_currentContext?.User != null &&
                string.Equals(_currentContext.User.Id.ToString(), userId, StringComparison.InvariantCultureIgnoreCase))
            (stopped at depth 3; 5 branches omitted)
