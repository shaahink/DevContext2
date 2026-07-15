TRACE  BacktestOrchestrator
       src/TradingEngine.Web/Services/BacktestOrchestrator.cs

▸ ENTRY  BacktestOrchestrator  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs)
       public sealed class BacktestOrchestrator : IBacktestCommandService
       // F34: the currency every money figure in this engine is denominated in — pip values, risk sizing,
       // FTMO limits and the whole tape. A venue account in any other currency is not comparable to a tape
   (106 more branches omitted beyond fan-out)
   ├─ data VenueSessionEntity  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:1696) [approx]
   │      public sealed class VenueSessionEntity : IAuditableEntity
   │      public Guid Id { get; set; } = Guid.NewGuid();
   │      public string RunId { get; set; } = "";
   ├─ call BacktestOrchestrator.AddTeardownWarning  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2210) [verified]
   │      private void AddTeardownWarning(string runId, string code, string detail)
   │      if (_runs.TryGetValue(runId, out var state))
   │      state.Warnings.Enqueue(new RunWarning(code, detail, DateTime.UtcNow));
   │  └─ call BacktestRunState.Enqueue  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:973) [approx]
   ├─ call ReconcileAndBackfillAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2173) [verified]
   │      public async Task<TradePersistenceReconciliation> ReconcileAndBackfillAsync(string runId, CancellationToken ct)
   │      try
   │      var (journalCloses, unreconstructedCloseFills) = await CollectAsync(runId, ct);
   │  ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:52) [approx]
   │  │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
   │  │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
   │  │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
   │  ├─ call ISymbolInfoRegistry  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:77) [approx]
   │  │      public interface ISymbolInfoRegistry
   │  │      SymbolInfo Get(Symbol symbol);
   │  │      void Register(SymbolInfo info);
   │  │  └─ di SymbolInfoRegistry  (src/TradingEngine.Infrastructure/ServiceCollectionExtensions.cs:45)
   │  │         public sealed class SymbolInfoRegistry : ISymbolInfoRegistry
   │  │         private readonly ConcurrentDictionary<Symbol, SymbolInfo> _symbols = new();
   │  │         private readonly ConcurrentDictionary<Symbol, VenueSymbolSpec> _venueSpecs = new();
   │  ├─ call ILogger  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:79) [verified]
   │  ├─ call ITradeRepository  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:86) [approx]
   │  │      public interface ITradeRepository
   │  │      Task SaveAsync(TradeResult trade, string runId, CancellationToken ct);
   │  │      Task<IReadOnlyList<TradeResult>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct);
   │  │  └─ di SqliteTradeRepository  (tests/TradingEngine.Tests.Simulation/Scenarios/InProcessEngineSmokeTests.cs:89)
   │  │         public sealed class SqliteTradeRepository(TradingDbContext db, ISymbolInfoRegistry? symbolRegistry = null) : ITradeRepository
   │  │         public async Task SaveAsync(TradeResult trade, string runId, CancellationToken ct)
   │  │         double? maeR = null;
   │  ├─ call SaveAsync  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:86) [verified]
   │  │      public async Task SaveAsync(TradeResult trade, string runId, CancellationToken ct)
   │  │      double? maeR = null;
   │  │      double? mfeR = null;
   │  │  ├─ data TradeResultEntity  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:27) [verified]
   │  │  │      public sealed class TradeResultEntity : IAuditableEntity
   │  │  │      public DateTime CreatedAtUtc { get; set; }
   │  │  │      public DateTime UpdatedAtUtc { get; set; }
   │  │  ├─ call TradeResult  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:32) [approx]
   │  │  │      public record TradeResult(
   │  │  │      Guid Id,
   │  │  │      Guid PositionId,
   │  │  ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:78) [approx]
   │  │  │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
   │  │  │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
   │  │  │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
   │  │  ├─ call TradingDbContext.SaveChangesAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:79) [verified]
   │  │  ├─ call TradingDbContext.Add  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:78) [approx]
   │  │  ├─ call TradeResult.ToString  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:58) [approx]
   │  │  ├─ call Symbol.ToString  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:32) [verified]
   │  │  │      public override string ToString() => Value;
   │  │  ├─ call MaeMfeNormalizer.Normalize  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:18) [verified]
   │  │  │      public static (double? MaeR, double? MfeR) Normalize(
   │  │  │      double maePips,
   │  │  │      double mfePips,
   │  │  │  ├─ call MaeMfeNormalizer.ComputeMfeR  (src/TradingEngine.Services/Helpers/MaeMfeNormalizer.cs:37) [verified]
   │  │  │  │      public static double? ComputeMfeR(double mfePips, double stopDistancePips)
   │  │  │  │      if (!double.IsFinite(mfePips) || stopDistancePips <= 0)
   │  │  │  │      return null;
   │  │  │  └─ call MaeMfeNormalizer.ComputeMaeR  (src/TradingEngine.Services/Helpers/MaeMfeNormalizer.cs:37) [verified]
   │  │  │         public static double? ComputeMaeR(double maePips, double stopDistancePips)
   │  │  │         if (!double.IsFinite(maePips) || stopDistancePips <= 0)
   │  │  │         return null;
   │  │  └─ call SymbolInfoRegistry.TryGet  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteTradeRepository.cs:12) [verified]
   │  │         public bool TryGet(Symbol symbol, out SymbolInfo info)
   │  │         return _symbols.TryGetValue(symbol, out info!);
   │  ├─ call FromClose  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:85) [verified]
   │  │      public static TradeResult FromClose(
   │  │      PublishTradeClosed effect,
   │  │      SymbolInfo symbolInfo,
   │  │  ├─ call PublishTradeClosed  (src/TradingEngine.Services/Helpers/TradeResultFactory.cs:64) [approx]
   │  │  │      // GrossProfit/NetProfit/Commission/Swap carry the venue-authoritative PnL when known (live);
   │  │  │      // they stay null for the simulated venue, where EffectExecutor recomputes gross from prices.
   │  │  │      // HighWater/LowWater are the most-favorable/most-adverse prices reached over the position's
   │  │  ├─ call PublishTradeClosed.ToString  (src/TradingEngine.Services/Helpers/TradeResultFactory.cs:64) [verified]
   │  │  ├─ call PipCalculator.RMultiple  (src/TradingEngine.Services/Helpers/TradeResultFactory.cs:39) [verified]
   │  │  │      public static double RMultiple(Money netPnL, Money initialRiskAmount)
   │  │  │      if (initialRiskAmount.Amount == 0) return 0;
   │  │  │      return (double)(netPnL.Amount / initialRiskAmount.Amount);
   │  │  ├─ call Money.Add  (src/TradingEngine.Services/Helpers/TradeResultFactory.cs:29) [verified]
   │  │  │      public Money Add(Money other)
   │  │  │      if (other.Currency != Currency)
   │  │  │      throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}");
   │  │  └─ call PipCalculator.GrossPnL  (src/TradingEngine.Services/Helpers/TradeResultFactory.cs:20) [verified]
   │  │         public static Money GrossPnL(
   │  │         TradeDirection direction,
   │  │         Price entryPrice,
   │  │     └─ call PipCalculator.PipValuePerLot  (src/TradingEngine.Services/Helpers/PipCalculator.cs:45) [verified]
   │  │            public static decimal PipValuePerLot(
   │  │            SymbolInfo symbol,
   │  │            decimal currentPrice,
   │  ├─ call SymbolInfoRegistry.TryGet  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:77) [verified]
   │  │      public bool TryGet(Symbol symbol, out SymbolInfo info)
   │  │      return _symbols.TryGetValue(symbol, out info!);
   │  ├─ call TradePersistenceBarrier.Key  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:70) [verified]
   │  │      private static string Key(Guid positionId, DateTime closedAtUtc, string exitReason, decimal lots) =>
   │  │      $"{positionId}|{closedAtUtc.Ticks}|{exitReason}|{lots}";
   │  ├─ call TradingDbContext.Where  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:52) [approx]
   │  └─ call CollectAsync  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:47) [verified]
   │         private async Task<(List<PublishTradeClosed> Closes, int UnreconstructedCloseFills)> CollectAsync(string runId, CancellationToken ct)
   │         var closes = new List<PublishTradeClosed>();
   │         var totalCloseFills = 0;
   │     ├─ call IJournalQueryRepository  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:120) [approx]
   │     │      public interface IJournalQueryRepository
   │     │      Task<IReadOnlyList<StepRecord>> GetByRunAsync(string runId, long? afterSeq, int limit, CancellationToken ct, string? kind = null);
   │     │      IAsyncEnumerable<StepRecord> StreamByRunAsync(string runId, long? afterSeq, CancellationToken ct);
   │     │  └─ di SqliteJournalQueryRepository  (src/TradingEngine.Web/Configuration/ServiceRegistration.cs:127)
   │     │         public sealed class SqliteJournalQueryRepository(TradingDbContext db) : IJournalQueryRepository
   │     │         // Must mirror SqliteStepRecordSink.JsonOpts: the sink serializes enums (e.g. StrategyVerdict.Direction)
   │     │         // as STRINGS via JsonStringEnumConverter, so the reader needs the same converter or the default
   │     ├─ call List  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:133) [approx]
   │     ├─ call ILogger  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:201) [verified]
   │     ├─ call TradePersistenceBarrier.TryParseProposal  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:146) [verified]
   │     │      private static bool TryParseProposal(string eventJson, [NotNullWhen(true)] out ProposalSnapshot? result)
   │     │      result = null;
   │     │      if (string.IsNullOrEmpty(eventJson)) return false;
   │     ├─ call TradePersistenceBarrier.TryParseOpenFill  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:138) [verified]
   │     │      private static bool TryParseOpenFill(string eventJson, [NotNullWhen(true)] out OpenFillSnapshot? result)
   │     │      result = null;
   │     │      if (string.IsNullOrEmpty(eventJson)) return false;
   │     │  └─ call Symbol.Parse  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:256) [verified]
   │     │         public static Symbol Parse(string value)
   │     │         ArgumentException.ThrowIfNullOrWhiteSpace(value);
   │     │         return new Symbol(value.ToUpperInvariant().Trim());
   │     ├─ call TradePersistenceBarrier.TryParseCloseFill  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:132) [verified]
   │     │      private static bool TryParseCloseFill(string eventJson, [NotNullWhen(true)] out CloseFillSnapshot? result)
   │     │      result = null;
   │     │      if (string.IsNullOrEmpty(eventJson)) return false;
   │     ├─ call TradePersistenceBarrier.HasCloseReason  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:125) [verified]
   │     │      private static bool HasCloseReason(string? eventJson)
   │     │      if (string.IsNullOrEmpty(eventJson) || eventJson == "{}")
   │     │      return false;
   │     └─ call StreamByRunAsync  (src/TradingEngine.Infrastructure/Persistence/TradePersistenceBarrier.cs:120) [verified]
   │            public async IAsyncEnumerable<StepRecord> StreamByRunAsync(
   │            string runId, long? afterSeq, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
   │            var query = db.JournalEntries.Where(e => e.RunId == runId);
   │        ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteJournalQueryRepository.cs:38) [approx]
   │        │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
   │        │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
   │        │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
   │        ├─ call SqliteJournalQueryRepository.Map  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteJournalQueryRepository.cs:42) [verified]
   │        │      private static StepRecord Map(JournalEntryEntity e) => new(
   │        │      e.RunId,
   │        │      e.Seq,
   │        └─ call TradingDbContext.Where  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteJournalQueryRepository.cs:38) [approx]
   ├─ call BacktestOrchestrator.TryDequeueNext  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:369) [verified]
   │      private void TryDequeueNext()
   │      if (!_queue.TryPeek(out var peeked)) return;
   │      var (runId, cfg) = peeked;
   │  ├─ call WriteEndRecordAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:420) [verified]
   │  │      private async Task<bool> WriteEndRecordAsync(
   │  │      string runId, BacktestConfig cfg, DateTime startedAt,
   │  │      BacktestResult result, TradeStats stats, string? effectiveConfigJson, string? status = null)
   │  │  ├─ call BacktestConfig  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:931) [approx]
   │  │  │      public sealed record BacktestConfig
   │  │  │      public string RunId { get; init; } = "";
   │  │  │      public required string Symbol { get; init; }
   │  │  ├─ call UpdateAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:936) [verified]
   │  │  │      public async Task UpdateAsync(BacktestRunSummary run, CancellationToken ct)
   │  │  │      await RetryOnBusyAsync(async () =>
   │  │  │      var entity = await db.BacktestRuns.FindAsync([run.RunId], ct);
   │  │  │  ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:78) [approx]
   │  │  │  │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
   │  │  │  │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
   │  │  │  │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
   │  │  │  ├─ call TradingDbContext.SaveChangesAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:99) [verified]
   │  │  │  ├─ call TradingDbContext.FindAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:78) [verified]
   │  │  │  └─ call SqliteBacktestRunRepository.RetryOnBusyAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:76) [verified]
   │  │  │         private static async Task RetryOnBusyAsync(Func<Task> action, CancellationToken ct, int maxRetries = 3)
   │  │  │         for (var attempt = 0; ; attempt++)
   │  │  │         try
   │  │  │     (stopped at depth 4; 1 branch omitted)
   │  │  ├─ call BacktestConfig.GetValueOrDefault  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:934) [approx]
   │  │  ├─ call BacktestOrchestrator.PeriodsJson  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:921) [verified]
   │  │  │      private static string PeriodsJson(IReadOnlyList<string> periods) =>
   │  │  │      JsonSerializer.Serialize(periods ?? []);
   │  │  └─ call BacktestOrchestrator.SymbolsJson  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:921) [verified]
   │  │         private static string SymbolsJson(IReadOnlyList<string> symbols) =>
   │  │         JsonSerializer.Serialize(symbols ?? []);
   │  ├─ call BacktestOrchestrator.EnqueueLog  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:418) [verified]
   │  │      private void EnqueueLog(string runId, ConcurrentQueue<string> queue, string msg)
   │  │      _journal.Write(runId, "LOG", msg, queue);
   │  │  └─ call Write  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:258) [verified]
   │  │         public void Write(string runId, string eventType, string message, ConcurrentQueue<string>? logQueue = null)
   │  │         var json = JsonSerializer.Serialize(new { eventType, message });
   │  │         _progressStore.GetWriter(runId).TryWrite(json);
   │  │     ├─ call BacktestProgressStore  (src/TradingEngine.Web/Services/BacktestJournal.cs:19) [approx]
   │  │     │      public sealed class BacktestProgressStore
   │  │     │      private readonly ConcurrentDictionary<string, Channel<string>> _channels = new();
   │  │     │      public ChannelWriter<string> GetWriter(string runId)
   │  │     └─ call GetWriter  (src/TradingEngine.Web/Services/BacktestJournal.cs:19) [verified]
   │  │            public ChannelWriter<string> GetWriter(string runId)
   │  │            var ch = _channels.GetOrAdd(runId, _ =>
   │  │            Channel.CreateBounded<string>(new BoundedChannelOptions(500)
   │  │        (stopped at depth 4; 1 branch omitted)
   │  ├─ call BacktestOrchestrator.TransitionRun  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:417) [verified]
   │  │      private void TransitionRun(BacktestRunState state, string to)
   │  │      var from = state.Status;
   │  │      switch (RunStateMachine.Classify(from, to))
   │  │  ├─ call Write  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:613) [verified]
   │  │  │      public void Write(string runId, string eventType, string message, ConcurrentQueue<string>? logQueue = null)
   │  │  │      var json = JsonSerializer.Serialize(new { eventType, message });
   │  │  │      _progressStore.GetWriter(runId).TryWrite(json);
   │  │  │  (stopped at depth 3; 2 branches omitted)
   │  │  └─ call RunStateMachine.Classify  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:597) [verified]
   │  │         public static TransitionKind Classify(string from, string to)
   │  │         if (CanTransition(from, to))
   │  │         return TransitionKind.Legal;
   │  │     ├─ call RunStateMachine.IsTerminal  (src/TradingEngine.Domain/RunStateMachine.cs:115) [verified]
   │  │     │      public static bool IsTerminal(string state) => TerminalStates.Contains(state);
   │  │     └─ call RunStateMachine.CanTransition  (src/TradingEngine.Domain/RunStateMachine.cs:107) [verified]
   │  │            public static bool CanTransition(string from, string to) =>
   │  │            Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
   │  ├─ call RunAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:413) [verified]
   │  │      private async Task RunAsync(string runId, BacktestConfig cfg, CancellationToken ct)
   │  │      var state = _runs[runId];
   │  │      var startedAt = state.StartedAt;
   │  │  (12 more branches omitted beyond fan-out)
   │  │  ├─ call BacktestConfig  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:673) [approx]
   │  │  │      public sealed record BacktestConfig
   │  │  │      public string RunId { get; init; } = "";
   │  │  │      public required string Symbol { get; init; }
   │  │  ├─ call ILogger  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:767) [verified]
   │  │  ├─ call BacktestProgressStore  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:814) [approx]
   │  │  │      public sealed class BacktestProgressStore
   │  │  │      private readonly ConcurrentDictionary<string, Channel<string>> _channels = new();
   │  │  │      public ChannelWriter<string> GetWriter(string runId)
   │  │  ├─ call RunProgressBroadcaster  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:820) [verified]
   │  │  │      /// <summary>
   │  │  │      /// Pushes <see cref="RunProgress"/> envelopes to the per-run SignalR group (iter-21 U1). Throttles
   │  │  │      /// to ≈4/sec per run so a fast M1 backtest can't flood the browser (the old per-bar SSE counter
   │  │  ├─ call RunProgressBroadcaster.RemoveRun  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:827) [verified]
   │  │  │      public void RemoveRun(string runId)
   │  │  │      _lastSentTicks.TryRemove(runId, out _);
   │  │  ├─ call BacktestOrchestrator.BuildProgress  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:820) [verified]
   │  │  │      private RunProgress BuildProgress(BacktestRunState state, string status)
   │  │  │      DateTime? simTime = DateTime.TryParse(state.SimTime, out var t) ? t : null;
   │  │  │      var elapsedMs = (long)(DateTime.UtcNow - state.StartedAt).TotalMilliseconds;
   │  │  ├─ call RunProgressBroadcaster.PublishDone  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:820) [verified]
   │  │  │      public void PublishDone(RunProgress progress)
   │  │  │      _lastSentTicks.TryRemove(progress.RunId, out _);
   │  │  │      Send("RunCompleted", progress);
   │  │  │  └─ call RunProgressBroadcaster.Send  (src/TradingEngine.Web/Services/RunProgressBroadcaster.cs:43) [verified]
   │  │  │         private void Send(string method, RunProgress progress)
   │  │  │         _ = _hub.Clients.Group(RunHub.Group(progress.RunId)).SendAsync(method, progress)
   │  │  │         .ContinueWith(t => _logger.LogWarning(t.Exception, "SignalR {Method} failed for run {RunId}", method, progress.RunId),
   │  │  │     (stopped at depth 4; 1 branch omitted)
   │  │  ├─ call BacktestProgressStore.Complete  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:815) [verified]
   │  │  │      public void Complete(string runId)
   │  │  │      if (_channels.TryRemove(runId, out var ch))
   │  │  │      ch.Writer.TryComplete();
   │  │  ├─ call GetWriter  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:814) [verified]
   │  │  │      public ChannelWriter<string> GetWriter(string runId)
   │  │  │      var ch = _channels.GetOrAdd(runId, _ =>
   │  │  │      Channel.CreateBounded<string>(new BoundedChannelOptions(500)
   │  │  │  (stopped at depth 3; 1 branch omitted)
   │  │  ├─ call WriteEndRecordAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:804) [verified]
   │  │  │      private async Task<bool> WriteEndRecordAsync(
   │  │  │      string runId, BacktestConfig cfg, DateTime startedAt,
   │  │  │      BacktestResult result, TradeStats stats, string? effectiveConfigJson, string? status = null)
   │  │  │  (stopped at depth 3; 5 branches omitted)
   │  │  ├─ call BacktestOrchestrator.GetTradeStatsAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:797) [verified]
   │  │  │      private async Task<TradeStats> GetTradeStatsAsync(string runId, decimal initialBalance)
   │  │  │      try
   │  │  │      using var scope = _scopeFactory.CreateScope();
   │  │  └─ call RunDataCache.MarkCompleted  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:788) [verified]
   │  │         public void MarkCompleted(string runId)
   │  │         if (_runs.TryGetValue(runId, out var entry))
   │  │         entry.MarkCompleted();
   │  │     └─ call RunEntry.MarkCompleted  (src/TradingEngine.Infrastructure/Caching/RunDataCache.cs:55) [verified]
   │  │            public void MarkCompleted()
   │  │            CompletedAtUtc = DateTime.UtcNow;
   │  ├─ call BacktestConfig.GetValueOrDefault  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:390) [approx]
   │  └─ call BacktestOrchestrator.ResolveUseCtrader  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:390) [verified]
   │         public static bool ResolveUseCtrader(string? venue) => venue?.ToLowerInvariant() switch
   │         "ctrader" => true,
   │         "replay" or "sim" or "simulated" => false,
   ├─ call WriteStartRecordAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:360) [verified]
   │      private async Task WriteStartRecordAsync(string runId, BacktestConfig cfg, DateTime startedAt, string? effectiveConfigJson,
   │      string? status = null, int? queuePosition = null)
   │      try
   │  ├─ call BacktestConfig  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:851) [approx]
   │  │      public sealed record BacktestConfig
   │  │      public string RunId { get; init; } = "";
   │  │      public required string Symbol { get; init; }
   │  ├─ call SaveAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:896) [verified]
   │  │      public async Task SaveAsync(BacktestRunSummary run, CancellationToken ct)
   │  │      await RetryOnBusyAsync(async () =>
   │  │      var existing = await db.BacktestRuns.FindAsync([run.RunId], ct);
   │  │  ├─ data BacktestRunEntity  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:18) [verified]
   │  │  │      public sealed class BacktestRunEntity : IAuditableEntity
   │  │  │      public DateTime CreatedAtUtc { get; set; }
   │  │  │      public DateTime UpdatedAtUtc { get; set; }
   │  │  ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:11) [approx]
   │  │  │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
   │  │  │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
   │  │  │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
   │  │  ├─ call TradingDbContext.SaveChangesAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:22) [verified]
   │  │  ├─ call TradingDbContext.Add  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:20) [verified]
   │  │  ├─ call SqliteBacktestRunRepository.MapToEntity  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:19) [verified]
   │  │  │      private static void MapToEntity(BacktestRunSummary run, BacktestRunEntity entity)
   │  │  │      entity.RunId = run.RunId;
   │  │  │      entity.StartedAtUtc = run.StartedAtUtc;
   │  │  ├─ call TradingDbContext.FindAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:11) [verified]
   │  │  └─ call SqliteBacktestRunRepository.RetryOnBusyAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteBacktestRunRepository.cs:9) [verified]
   │  │         private static async Task RetryOnBusyAsync(Func<Task> action, CancellationToken ct, int maxRetries = 3)
   │  │         for (var attempt = 0; ; attempt++)
   │  │         try
   │  │     (stopped at depth 3; 1 branch omitted)
   │  ├─ call BacktestConfig.GetValueOrDefault  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:893) [approx]
   │  ├─ call BacktestOrchestrator.PeriodsJson  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:880) [verified]
   │  │      private static string PeriodsJson(IReadOnlyList<string> periods) =>
   │  │      JsonSerializer.Serialize(periods ?? []);
   │  ├─ call BacktestOrchestrator.SymbolsJson  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:880) [verified]
   │  │      private static string SymbolsJson(IReadOnlyList<string> symbols) =>
   │  │      JsonSerializer.Serialize(symbols ?? []);
   │  └─ call ConfigSetHash.Compute  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:876) [verified]
   │         public static string Compute(string json)
   │         var bytes = Encoding.UTF8.GetBytes(json);
   │         var hash = SHA256.HashData(bytes);
   ├─ call BacktestOrchestrator.EnqueueLog  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:354) [verified]
   │      private void EnqueueLog(string runId, ConcurrentQueue<string> queue, string msg)
   │      _journal.Write(runId, "LOG", msg, queue);
   │  (stopped at depth 1; 1 branch omitted)
   ├─ call IJournalWriter.DisposeAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2312) [verified]
   ├─ call BacktestConfig  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:673) [approx]
   │      public sealed record BacktestConfig
   │      public string RunId { get; init; } = "";
   │      public required string Symbol { get; init; }
   ├─ call ILogger  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:767) [verified]
   ├─ call BacktestProgressStore  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:814) [approx]
   │      public sealed class BacktestProgressStore
   │      private readonly ConcurrentDictionary<string, Channel<string>> _channels = new();
   │      public ChannelWriter<string> GetWriter(string runId)
   ├─ call RunProgressBroadcaster  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:820) [verified]
   │      /// <summary>
   │      /// Pushes <see cref="RunProgress"/> envelopes to the per-run SignalR group (iter-21 U1). Throttles
   │      /// to ≈4/sec per run so a fast M1 backtest can't flood the browser (the old per-bar SSE counter
   └─ call RunProgressBroadcaster.RemoveRun  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:827) [verified]
          public void RemoveRun(string runId)
          _lastSentTicks.TryRemove(runId, out _);

TOUCHES  VenueSessionEntity, TradeResultEntity, BacktestRunEntity
