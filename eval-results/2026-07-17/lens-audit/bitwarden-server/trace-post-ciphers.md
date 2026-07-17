TRACE  POST /ciphers
       src/Api/Vault/Controllers/CiphersController.cs:162
       Api
▸ ENTRY  POST /ciphers  (src/Api/Vault/Controllers/CiphersController.cs:162)
   └─ call CiphersController.Post  (src/Api/Vault/Controllers/CiphersController.cs:162)
          [HttpPost("")]
          public async Task<CipherResponseModel> Post([FromBody] CipherRequestModel model)
          var user = await _userService.GetUserByPrincipalAsync(User);
      ├─ call IUserService  (src/Api/Vault/Controllers/CiphersController.cs:165) [approx]
      │      public interface IUserService
      │      Guid? GetProperUserId(ClaimsPrincipal principal);
      │      Task<User> GetUserByIdAsync(string userId);
      │  └─ di UserService  (src/Core/Auth/UserFeatures/UserServiceCollectionExtensions.cs:31)
      │         public class UserService : UserManager<User>, IUserService
      │         private readonly IUserRepository _userRepository;
      │         private readonly IOrganizationUserRepository _organizationUserRepository;
      ├─ call CipherRequestModel  (src/Api/Vault/Controllers/CiphersController.cs:177) [approx]
      │      public class CipherRequestModel
      │      /// <summary>
      │      /// The Id of the user that encrypted the cipher. It should always represent a UserId.
      ├─ call ICurrentContext  (src/Api/Vault/Controllers/CiphersController.cs:178) [approx]
      │      /// <summary>
      │      /// Provides information about the current HTTP request and the currently authenticated user (if any).
      │      /// This is often (but not exclusively) parsed from the JWT in the current request.
      │  └─ di CurrentContext  (bitwarden_license/src/Sso/Startup.cs:46)
      ├─ call ICipherService  (src/Api/Vault/Controllers/CiphersController.cs:183) [approx]
      │      public interface ICipherService
      │      Task SaveAsync(Cipher cipher, Guid savingUserId, DateTime? lastKnownRevisionDate, IEnumerable<Guid>? collectionIds = null,
      │      bool skipPermissionCheck = false, bool limitCollectionScope = true);
      │  └─ di CipherService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:157)
      │         public class CipherService : ICipherService
      │         public const long MAX_FILE_SIZE = Constants.FileSize501mb;
      │         public const string MAX_FILE_SIZE_READABLE = "500 MB";
      ├─ call GetOrganizationAbilityAsync  (src/Api/Vault/Controllers/CiphersController.cs:184) [verified]
      │      private async Task<OrganizationAbility?> GetOrganizationAbilityAsync(CipherDetails cipher)
      │      if (cipher.OrganizationId.HasValue)
      │      return await _organizationAbilityCacheService.GetOrganizationAbilityAsync(cipher.OrganizationId.Value);
      │  ├─ call IOrganizationAbilityCacheService  (src/Api/Vault/Controllers/CiphersController.cs:1693) [approx]
      │  │      public interface IOrganizationAbilityCacheService
      │  │      Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default);
      │  │      Task<IDictionary<Guid, OrganizationAbility>> GetOrganizationAbilitiesAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default);
      │  │  └─ di ExtendedOrganizationAbilityCacheService [approx]
      │  │         public class ExtendedOrganizationAbilityCacheService(
      │  │         [FromKeyedServices(CacheName)] IFusionCache cache,
      │  │         IOrganizationRepository organizationRepository)
      │  └─ call GetOrganizationAbilityAsync  (src/Api/Vault/Controllers/CiphersController.cs:1693) [verified]
      │         public async Task<OrganizationAbility?> GetOrganizationAbilityAsync(Guid orgId, CancellationToken cancellationToken = default)
      │         return await cache.GetOrSetAsync<OrganizationAbility?>(
      │         orgId.ToString(),
      │     ├─ call IOrganizationRepository  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:24) [approx]
      │     │      public interface IOrganizationRepository : IRepository<Organization, Guid>
      │     │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
      │     │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
      │     │  ├─ di EFTestOrganizationTrackingOrganizationRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:27) [×2 impls]
      │     │  │      /// <summary>
      │     │  │      /// EntityFramework decorator around the <see cref="Bit.Infrastructure.EntityFramework.Repositories.OrganizationRepository"/> that tracks
      │     │  │      /// created Organizations for seeding.
      │     │  ├─ di DapperTestOrganizationTrackingOrganizationRepository  (src/SharedWeb/Play/PlayServiceCollectionExtensions.cs:16) [×2 impls]
      │     │  │      /// <summary>
      │     │  │      /// Dapper decorator around the <see cref="Bit.Infrastructure.Dapper.Repositories.OrganizationRepository"/> that tracks
      │     │  │      /// created Organizations for seeding.
      │     │  ├─ di OrganizationRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:90) [×2 impls]
      │     │  │      public class OrganizationRepository : Repository<Core.AdminConsole.Entities.Organization, Organization, Guid>, IOrganizationRepository
      │     │  │      protected readonly ILogger<OrganizationRepository> _logger;
      │     │  │      public OrganizationRepository(
      │     │  └─ di OrganizationRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:53) [×2 impls]
      │     │         public class OrganizationRepository : Repository<Organization, Guid>, IOrganizationRepository
      │     │         protected readonly ILogger<OrganizationRepository> _logger;
      │     │         public OrganizationRepository(
      │     └─ call EFTestOrganizationTrackingOrganizationRepository.GetAbilityAsync  (src/Core/AdminConsole/AbilitiesCache/ExtendedOrganizationAbilityCacheService.cs:24) [verified]
      ├─ call SaveDetailsAsync  (src/Api/Vault/Controllers/CiphersController.cs:183) [verified]
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
      │  │  ├─ di CipherRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:76) [×2 impls]
      │  │  │      public class CipherRepository : Repository<Core.Vault.Entities.Cipher, Cipher, Guid>, ICipherRepository
      │  │  │      public CipherRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
      │  │  │      : base(serviceScopeFactory, mapper, (DatabaseContext context) => context.Ciphers)
      │  │  └─ di CipherRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:38) [×2 impls]
      │  │         public class CipherRepository : Repository<Cipher, Guid>, ICipherRepository
      │  │         public CipherRepository(GlobalSettings globalSettings)
      │  │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
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
      │  │  └─ di EventService  (src/SharedWeb/Utilities/ServiceCollectionExtensions.cs:164) [×2 impls]
      │  │         public class EventService : IEventService
      │  │         private readonly IEventWriteService _eventWriteService;
      │  │         private readonly IOrganizationUserRepository _organizationUserRepository;
      │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:159) [approx]
      │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
      │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
      │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
      │  │  (stopped at depth 3; 4 branches omitted)
      │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:167) [approx]
      │  │      public interface ICipherSyncPushService
      │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
      │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
      │  │  └─ di CipherSyncPushService [approx]
      │  │         public class CipherSyncPushService : ICipherSyncPushService
      │  │         private readonly IPushNotificationService _pushNotificationService;
      │  │         private readonly ICollectionCipherRepository _collectionCipherRepository;
      │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:178) [verified]
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
      │  │     │  ├─ di CollectionCipherRepository  (src/Infrastructure.EntityFramework/EntityFrameworkServiceCollectionExtensions.cs:77)
      │  │     │  │      public class CollectionCipherRepository : BaseEntityFrameworkRepository, ICollectionCipherRepository
      │  │     │  │      public CollectionCipherRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
      │  │     │  │      : base(serviceScopeFactory, mapper)
      │  │     │  └─ di CollectionCipherRepository  (src/Infrastructure.Dapper/DapperServiceCollectionExtensions.cs:39)
      │  │     │         public class CollectionCipherRepository : BaseRepository, ICollectionCipherRepository
      │  │     │         public CollectionCipherRepository(GlobalSettings globalSettings)
      │  │     │         : this(globalSettings.SqlServer.ConnectionString, globalSettings.SqlServer.ReadOnlyConnectionString)
      │  │     ├─ call ILogger  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:71) [verified]
      │  │     ├─ call MultiServicePushNotificationService.PushAsync  (src/Core/Vault/Services/Implementations/CipherSyncPushService.cs:106) [verified]
      │  │     │      public Task PushAsync<T>(PushNotification<T> pushNotification) where T : class
      │  │     │      return PushToServices((s) => s.PushAsync(pushNotification));
      │  │     │  └─ call MultiServicePushNotificationService.PushToServices  (src/Core/Platform/Push/Engines/MultiServicePushNotificationService.cs:69) [verified]
      │  │     │         private Task PushToServices(Func<IPushEngine, Task> pushFunc)
      │  │     │         if (!_services.Any())
      │  │     │         Logger.LogWarning("No services found to push notification");
      │  │     │     (stopped at depth 6; 1 branch omitted)
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
      │  │     │  (stopped at depth 5; 1 branch omitted)
      │  │     ├─ call GetProviderIdAsync  (src/Core/Dirt/Services/Implementations/EventService.cs:165) [verified]
      │  │     │      private async Task<Guid?> GetProviderIdAsync(Guid? orgId)
      │  │     │      if (_currentContext == null || !orgId.HasValue)
      │  │     │      return null;
      │  │     │  ├─ call ICurrentContext  (src/Core/Dirt/Services/Implementations/EventService.cs:815) [approx]
      │  │     │  │      /// <summary>
      │  │     │  │      /// Provides information about the current HTTP request and the currently authenticated user (if any).
      │  │     │  │      /// This is often (but not exclusively) parsed from the JWT in the current request.
      │  │     │  │  (stopped at depth 6; 1 branch omitted)
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
      │  │        (stopped at depth 5; 2 branches omitted)
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
      │  │  │  (stopped at depth 4; 2 branches omitted)
      │  │  ├─ call ICollectionRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:1115) [approx]
      │  │  │      public interface ICollectionRepository : IRepository<Collection, Guid>
      │  │  │      Task<int> GetCountByOrganizationIdAsync(Guid organizationId);
      │  │  │      /// <summary>
      │  │  │  (stopped at depth 4; 2 branches omitted)
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
      │        (stopped at depth 4; 10 branches omitted)
      ├─ call CurrentContext.OrganizationUser  (src/Api/Vault/Controllers/CiphersController.cs:178) [verified]
      │      public async Task<bool> OrganizationUser(Guid orgId)
      │      return (Organizations?.Any(o => o.Id == orgId) ?? false) || await OrganizationOwner(orgId);
      ├─ call CipherRequestModel.ToCipherDetails  (src/Api/Vault/Controllers/CiphersController.cs:177) [verified]
      │      public CipherDetails ToCipherDetails(Guid userId, bool allowOrgIdSet = true)
      │      var hasOrgId = !string.IsNullOrWhiteSpace(OrganizationId);
      │      var cipher = new CipherDetails
      │  └─ call ToCipher  (src/Api/Vault/Models/Request/CipherRequestModel.cs:90) [verified]
      │         public Cipher ToCipher(Cipher existingCipher, Guid? userId = null)
      │         // If Data field is provided, use it directly
      │         if (!string.IsNullOrWhiteSpace(Data))
      │     ├─ call Cipher.SetAttachments  (src/Api/Vault/Models/Request/CipherRequestModel.cs:192) [verified]
      │     │      public void SetAttachments(Dictionary<string, CipherAttachment.MetaData> data)
      │     │      if (data == null || data.Count == 0)
      │     │      _attachmentData = null;
      │     ├─ call Cipher.GetAttachments  (src/Api/Vault/Models/Request/CipherRequestModel.cs:161) [verified]
      │     │      public Dictionary<string, CipherAttachment.MetaData> GetAttachments()
      │     │      if (string.IsNullOrWhiteSpace(Attachments))
      │     │      return null;
      │     ├─ call CipherRequestModel.UpdateUserSpecificJsonField  (src/Api/Vault/Models/Request/CipherRequestModel.cs:151) [verified]
      │     │      private static string UpdateUserSpecificJsonField(string existingJson, string userIdKey, object newValue)
      │     │      if (userIdKey == null)
      │     │      return existingJson;
      │     ├─ call CipherRequestModel.ToCipherPassportData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:139) [verified]
      │     │      private CipherPassportData ToCipherPassportData()
      │     │      return new CipherPassportData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherDriversLicenseData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:135) [verified]
      │     │      private CipherDriversLicenseData ToCipherDriversLicenseData()
      │     │      return new CipherDriversLicenseData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherBankAccountData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:131) [verified]
      │     │      private CipherBankAccountData ToCipherBankAccountData()
      │     │      return new CipherBankAccountData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherSSHKeyData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:127) [verified]
      │     │      private CipherSSHKeyData ToCipherSSHKeyData()
      │     │      return new CipherSSHKeyData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherSecureNoteData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:124) [verified]
      │     │      private CipherSecureNoteData ToCipherSecureNoteData()
      │     │      return new CipherSecureNoteData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherIdentityData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:120) [verified]
      │     │      private CipherIdentityData ToCipherIdentityData()
      │     │      return new CipherIdentityData
      │     │      Name = Name,
      │     ├─ call CipherRequestModel.ToCipherCardData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:116) [verified]
      │     │      private CipherCardData ToCipherCardData()
      │     │      return new CipherCardData
      │     │      Name = Name,
      │     └─ call CipherRequestModel.ToCipherLoginData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:107) [verified]
      │            private CipherLoginData ToCipherLoginData()
      │            return new CipherLoginData
      │            Name = Name,
      │        └─ call CipherLoginModel.ToCipherLoginFido2CredentialData  (src/Api/Vault/Models/Request/CipherRequestModel.cs:237) [approx]
      └─ call UserService.GetUserByPrincipalAsync  (src/Api/Vault/Controllers/CiphersController.cs:165) [verified]
             public async Task<User> GetUserByPrincipalAsync(ClaimsPrincipal principal)
             var userId = GetProperUserId(principal);
             if (!userId.HasValue)
RESULT   200 OK / 201 Created · failure → 400 Bad Request
