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
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:572) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:602) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:608) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:615) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 15 branches omitted)
   │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:638) [approx]
   │  │      public interface ICollectionCipherRepository
   │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IUserRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:653) [approx]
   │  │      public interface IUserRepository : IRepository<User, Guid>
   │  │      Task<User?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<User?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:654) [approx]
   │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:668) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:668) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 34 branches omitted)
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
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:142) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IPolicyRequirementQuery  (src/Core/Vault/Services/Implementations/CipherService.cs:147) [approx]
   │  │      public interface IPolicyRequirementQuery
   │  │      /// <summary>
   │  │      /// Get a policy requirement for a specific user.
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:155) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call IOrganizationRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:159) [approx]
   │  │      public interface IOrganizationRepository : IRepository<Organization, Guid>
   │  │      Task<Organization?> GetByGatewayCustomerIdAsync(string gatewayCustomerId);
   │  │      Task<Organization?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId);
   │  │  (stopped at depth 2; 7 branches omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:167) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call CipherSyncPushService.PushSyncCipherUpdateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:178) [verified]
   │  │      public Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │  │      => PushCipherAsync(cipher, PushType.SyncCipherUpdate, collectionIds);
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call LogCipherEventAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:175) [verified]
   │  │      public async Task LogCipherEventAsync(Cipher cipher, EventType type, DateTime? date = null)
   │  │      var e = await BuildCipherEventMessageAsync(cipher, type, date);
   │  │      if (e != null)
   │  │  (stopped at depth 2; 30 branches omitted)
   │  ├─ call CipherRepository.ReplaceAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:174) [verified]
   │  │      public async Task ReplaceAsync(CipherDetails obj)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call ValidateChangeInCollectionsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:173) [verified]
   │  │      private async Task ValidateChangeInCollectionsAsync(Cipher updatedCipher, IEnumerable<Guid>? newCollectionIds, Guid userId)
   │  │      if (updatedCipher.Id == Guid.Empty || !updatedCipher.OrganizationId.HasValue)
   │  │      return;
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call CipherService.ValidateCipherLastKnownRevisionDate  (src/Core/Vault/Services/Implementations/CipherService.cs:171) [verified]
   │  │      private void ValidateCipherLastKnownRevisionDate(Cipher cipher, DateTime? lastKnownRevisionDate)
   │  │      if (cipher.Id == default || !lastKnownRevisionDate.HasValue)
   │  │      return;
   │  └─ call CipherSyncPushService.PushSyncCipherCreateAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:167) [verified]
   │         public Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds)
   │         => PushCipherAsync(cipher, PushType.SyncCipherCreate, collectionIds);
   │     (stopped at depth 2; 1 branch omitted)
   ├─ call CreateAttachmentAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:238)
   │      public async Task CreateAttachmentAsync(Cipher cipher, Stream stream, string fileName, string key,
   │      long requestLength, Guid savingUserId, bool orgAdmin = false, DateTime? lastKnownRevisionDate = null)
   │      ValidateCipherLastKnownRevisionDate(cipher, lastKnownRevisionDate);
   │  (5 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:252) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:267) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:282) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:289) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 4 branches omitted)
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
   │  │  (stopped at depth 2; 19 branches omitted)
   │  └─ call Cipher.AddAttachment  (src/Core/Vault/Services/Implementations/CipherService.cs:268) [verified]
   │         public void AddAttachment(string id, CipherAttachment.MetaData data)
   │         var attachments = GetAttachments();
   │         if (attachments == null)
   │     (stopped at depth 2; 2 branches omitted)
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
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:215) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:217) [verified]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:225) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:227) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call ICipherSyncPushService  (src/Core/Vault/Services/Implementations/CipherService.cs:233) [approx]
   │  │      public interface ICipherSyncPushService
   │  │      Task PushSyncCipherCreateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │      Task PushSyncCipherUpdateAsync(Cipher cipher, IEnumerable<Guid> collectionIds);
   │  │  (stopped at depth 2; 4 branches omitted)
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
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:453) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IAttachmentStorageService  (src/Core/Vault/Services/Implementations/CipherService.cs:468) [approx]
   │  │      public interface IAttachmentStorageService
   │  │      FileUploadType FileUploadType { get; }
   │  │      Task UploadNewAttachmentAsync(Stream stream, Cipher cipher, CipherAttachment.MetaData attachmentData);
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:475) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:479) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:479) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:475) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  (stopped at depth 2; 6 branches omitted)
   │  ├─ call NoopAttachmentStorageService.DeleteAttachmentsForCipherAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:468) [verified]
   │  │      public Task DeleteAttachmentsForCipherAsync(Guid cipherId)
   │  │      return Task.FromResult(0);
   │  ├─ call CipherRepository.DeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:462) [verified]
   │  │      public async Task DeleteAsync(IEnumerable<Guid> ids, Guid userId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call FilterCiphersByDeletePermission  (src/Core/Vault/Services/Implementations/CipherService.cs:460) [verified]
   │  │      private async Task<List<T>> FilterCiphersByDeletePermission<T>(
   │  │      IEnumerable<T> ciphers,
   │  │      HashSet<Guid> cipherIdsSet,
   │  │  (stopped at depth 2; 23 branches omitted)
   │  ├─ call CipherRepository.GetManyByUserIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:459) [verified]
   │  │      public async Task<ICollection<CipherDetails>> GetManyByUserIdAsync(Guid userId, bool withOrganizations = true)
   │  │      string sprocName = null;
   │  │      if (withOrganizations)
   │  └─ call CipherRepository.DeleteByIdsOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:455) [verified]
   │         public async Task DeleteByIdsOrganizationIdAsync(IEnumerable<Guid> ids, Guid organizationId)
   │         using (var connection = new SqlConnection(ConnectionString))
   │         var results = await connection.ExecuteAsync(
   │     (stopped at depth 2; 1 branch omitted)
   ├─ call SoftDeleteManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:762)
   │      public async Task SoftDeleteManyAsync(IEnumerable<Guid> cipherIds, Guid deletingUserId, Guid? organizationId, bool orgAdmin)
   │      var cipherIdsSet = new HashSet<Guid>(cipherIds);
   │      var deletingCiphers = new List<Cipher>();
   │  (2 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:765) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:769) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call ISecurityTaskRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:781) [approx]
   │  │      public interface ISecurityTaskRepository : IRepository<SecurityTask, Guid>
   │  │      /// <summary>
   │  │      /// Retrieves security tasks for a user based on their organization and cipher access permissions.
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:787) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:791) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 4 branches omitted)
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
   │  │  (stopped at depth 2; 2 branches omitted)
   │  ├─ call CipherRepository.SoftDeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:778) [verified]
   │  │      public async Task SoftDeleteAsync(IEnumerable<Guid> ids, Guid userId)
   │  │      using (var connection = new SqlConnection(ConnectionString))
   │  │      var results = await connection.ExecuteAsync(
   │  │  (stopped at depth 2; 1 branch omitted)
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
   │     (stopped at depth 2; 1 branch omitted)
   ├─ call RestoreManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:817)
   │      public async Task<ICollection<CipherOrganizationDetails>> RestoreManyAsync(IEnumerable<Guid> cipherIds, Guid restoringUserId, Guid? organizationId = null, bool orgAdmin = false)
   │      if (cipherIds == null || !cipherIds.Any())
   │      return new List<CipherOrganizationDetails>();
   │  (1 more branch omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:846) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:830) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:850) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:854) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 4 branches omitted)
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
   │  │  (stopped at depth 2; 1 branch omitted)
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
   │  │  (stopped at depth 2; 1 branch omitted)
   │  ├─ call DomainIcons.Where  (src/Core/Vault/Services/Implementations/CipherService.cs:831) [approx]
   │  └─ call CipherRepository.GetManyOrganizationDetailsByOrganizationIdAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:830) [verified]
   │         public async Task<ICollection<CipherOrganizationDetails>> GetManyOrganizationDetailsByOrganizationIdAsync(
   │         Guid organizationId)
   │         using (var connection = new SqlConnection(ConnectionString))
   ├─ call ShareManyAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:671)
   │      public async Task<IEnumerable<CipherDetails>> ShareManyAsync(IEnumerable<(CipherDetails cipher, DateTime? lastKnownRevisionDate)> cipherInfos,
   │      Guid organizationId, IEnumerable<Guid> collectionIds, Guid sharingUserId)
   │      var cipherIds = new List<Guid>();
   │  (21 more branches omitted beyond fan-out)
   │  ├─ data Cipher  (src/Core/Vault/Services/Implementations/CipherService.cs:690) [approx]
   │  │      public class Cipher : ITableObject<Guid>, ICloneable
   │  │      private Dictionary<string, CipherAttachment.MetaData> _attachmentData;
   │  │      public Guid Id { get; set; }
   │  │  (stopped at depth 2; 3 branches omitted)
   │  ├─ call ICipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:685) [approx]
   │  │      public interface ICipherRepository : IRepository<Cipher, Guid>
   │  │      Task<CipherDetails> GetByIdAsync(Guid id, Guid userId);
   │  │      Task<CipherOrganizationDetails> GetOrganizationDetailsByIdAsync(Guid id);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call ICollectionCipherRepository  (src/Core/Vault/Services/Implementations/CipherService.cs:686) [approx]
   │  │      public interface ICollectionCipherRepository
   │  │      Task<ICollection<CollectionCipher>> GetManyByUserIdAsync(Guid userId);
   │  │      Task<ICollection<CollectionCipher>> GetManyByOrganizationIdAsync(Guid organizationId);
   │  │  (stopped at depth 2; 5 branches omitted)
   │  ├─ call IEventService  (src/Core/Vault/Services/Implementations/CipherService.cs:693) [approx]
   │  │      public interface IEventService
   │  │      /// <summary>
   │  │      /// Logs a user event and creates organization-scoped copies for each org the user belongs to.
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call IPushNotificationService  (src/Core/Vault/Services/Implementations/CipherService.cs:697) [approx]
   │  │      /// <summary>
   │  │      /// Used to Push notifications to end-user devices.
   │  │      /// </summary>
   │  │  (stopped at depth 2; 4 branches omitted)
   │  ├─ call DomainIcons.Select  (src/Core/Vault/Services/Implementations/CipherService.cs:698) [verified]
   │  ├─ call MultiServicePushNotificationService.PushSyncCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:697) [verified]
   │  ├─ call LogCipherEventsAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:693) [verified]
   │  │      public async Task LogCipherEventsAsync(IEnumerable<Tuple<Cipher, EventType, DateTime?>> events)
   │  │      var cipherEvents = new List<IEvent>();
   │  │      foreach (var ev in events)
   │  │  (stopped at depth 2; 3 branches omitted)
   │  └─ call CollectionCipherRepository.UpdateCollectionsForCiphersAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:686) [verified]
   │         public async Task UpdateCollectionsForCiphersAsync(IEnumerable<Guid> cipherIds, Guid userId,
   │         Guid organizationId, IEnumerable<Guid> collectionIds)
   │         using (var connection = new SqlConnection(ConnectionString))
   │     (stopped at depth 2; 1 branch omitted)
   ├─ call CreateAttachmentShareAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:292)
   │      public async Task CreateAttachmentShareAsync(Cipher cipher, Stream stream, string fileName, string key,
   │      long requestLength, string attachmentId, Guid organizationId)
   │      try
   │  (stopped at depth 1; 11 branches omitted)
   ├─ call SaveAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:84)
   │      public async Task SaveAsync(Cipher cipher, Guid savingUserId, DateTime? lastKnownRevisionDate,
   │      IEnumerable<Guid>? collectionIds = null, bool skipPermissionCheck = false, bool limitCollectionScope = true)
   │      if (!skipPermissionCheck && !(await UserCanEditAsync(cipher, savingUserId)))
   │  (stopped at depth 1; 35 branches omitted)
   └─ call DeleteAsync  (src/Core/Vault/Services/Implementations/CipherService.cs:429)
          public async Task DeleteAsync(CipherDetails cipherDetails, Guid deletingUserId, bool orgAdmin = false)
          if (!orgAdmin && !await UserCanDeleteAsync(cipherDetails, deletingUserId))
          throw new BadRequestException("You do not have permissions to delete this.");
      (stopped at depth 1; 18 branches omitted)
NOTE: trace shaped to the ~8000-token budget — 288 deeper step(s) omitted (marked "(N omitted)" in place; raise --max-tokens to widen)

