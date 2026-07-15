TRACE  EngineWorker
       src/TradingEngine.Host/EngineServiceCollectionExtensions.cs:269
       TradingEngine.Host
▸ ENTRY  EngineWorker  (src/TradingEngine.Host/EngineServiceCollectionExtensions.cs:269)
   └─ call EngineWorker  (src/TradingEngine.Host/EngineServiceCollectionExtensions.cs:269)
          /// <summary>
          /// Thin hosted shell. All engine run logic lives in <see cref="EngineRunner"/> (no hosting
          /// dependency, directly testable); this type exists only to plug that logic into the generic-host
      └─ call RunAsync  (src/TradingEngine.Host/EngineWorker.cs:15) [verified]
             public async Task RunAsync(CancellationToken ct)
             _logger.LogInformation("Kernel engine starting. Mode={Mode} Strategies={Count}", _engineMode, _strategies.Count);
             if (_signalGate is not null)
         (10 more branches omitted beyond fan-out)
         ├─ call IBrokerAdapter  (src/TradingEngine.Host/EngineRunner.cs:107) [verified]
         │      public interface IBrokerAdapter
         │      ChannelReader<Tick> TickStream { get; }
         │      ChannelReader<Bar> BarStream { get; }
         ├─ call BarEvaluator  (src/TradingEngine.Host/EngineRunner.cs:107) [approx]
         │      /// <summary>
         │      /// The evaluator stage (iter-36 K1). The strategy/indicator evaluation that today lives imperatively in
         │      /// <see cref="TradingLoop.ProcessBarAsync"/> becomes a deterministic <b>event producer</b> feeding the
         ├─ call KernelTrailingEvaluator  (src/TradingEngine.Host/EngineRunner.cs:107) [approx]
         │      /// <summary>
         │      /// The kernel-path adapter for per-bar trailing / breakeven (iter-36 K4 gap-3). It is the impure half of
         │      /// the trailing seam: it reads the per-position management config + recent bars (which the pure kernel
         ├─ call IRiskManager  (src/TradingEngine.Host/EngineRunner.cs:111) [approx]
         │      public interface IRiskManager
         │      decimal InitialBalance { get; }
         │      DrawdownState Drawdown { get; }
         │  └─ di RiskManager [approx]
         │         public sealed class RiskManager(
         │         ISymbolInfoRegistry symbolRegistry,
         │         Func<string, string, decimal> getCrossRate,
         ├─ call EngineRunner.FlushTimingReport  (src/TradingEngine.Host/EngineRunner.cs:197) [verified]
         │      private void FlushTimingReport(KernelBacktestLoop loop)
         │      if (loop.TimingReport is { } timing)
         │      WriteTimingReport(timing);
         │  └─ call EngineRunner.WriteTimingReport  (src/TradingEngine.Host/EngineRunner.cs:243) [verified]
         │         private void WriteTimingReport(TimingReport timing)
         │         try
         │         var profilingDir = Path.Combine(
         ├─ call EngineRunner.FlushBacktestEquityAsync  (src/TradingEngine.Host/EngineRunner.cs:196) [verified]
         │      private async Task FlushBacktestEquityAsync(CancellationToken ct)
         │      if (_engineMode != EngineMode.Backtest || _scopeFactory is null) return;
         │      if (_equitySink is not BufferedEquitySink buffered) return;
         │  ├─ call FlushAsync  (src/TradingEngine.Host/EngineRunner.cs:262) [verified]
         │  │      public static async Task FlushAsync(
         │  │      IReadOnlyList<AccountSnapshot> snapshots, IEquityRepository repo,
         │  │      EngineMode mode, string runId, CancellationToken ct)
         │  │  ├─ call List  (src/TradingEngine.Host/EquitySnapshotFlush.cs:37) [approx]
         │  │  ├─ call IEquityRepository  (src/TradingEngine.Host/EquitySnapshotFlush.cs:38) [approx]
         │  │  │      public interface IEquityRepository
         │  │  │      Task SaveAsync(EquitySnapshot snapshot, string? runId, CancellationToken ct);
         │  │  │      Task SaveBatchAsync(IReadOnlyList<EquitySnapshot> snapshots, string? runId, CancellationToken ct);
         │  │  │  └─ di SqliteEquityRepository  (tests/TradingEngine.Tests.Simulation/Scenarios/InProcessEngineSmokeTests.cs:90)
         │  │  │         public sealed class SqliteEquityRepository(TradingDbContext db) : IEquityRepository
         │  │  │         public async Task SaveAsync(EquitySnapshot snapshot, string? runId, CancellationToken ct)
         │  │  │         var entity = new EquitySnapshotEntity
         │  │  ├─ call SaveBatchAsync  (src/TradingEngine.Host/EquitySnapshotFlush.cs:38) [verified]
         │  │  │      public async Task SaveBatchAsync(IReadOnlyList<EquitySnapshot> snapshots, string? runId, CancellationToken ct)
         │  │  │      foreach (var snapshot in snapshots)
         │  │  │      db.EquitySnapshots.Add(new EquitySnapshotEntity
         │  │  │  ├─ data EquitySnapshotEntity  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:32) [approx]
         │  │  │  │      public sealed class EquitySnapshotEntity : IAuditableEntity
         │  │  │  │      public DateTime CreatedAtUtc { get; set; }
         │  │  │  │      public DateTime UpdatedAtUtc { get; set; }
         │  │  │  ├─ call TradingDbContext  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:32) [approx]
         │  │  │  │      public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
         │  │  │  │      public DbSet<TradeResultEntity> Trades => Set<TradeResultEntity>();
         │  │  │  │      public DbSet<OrderEntity> Orders => Set<OrderEntity>();
         │  │  │  ├─ call DbSet  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:34) [verified]
         │  │  │  ├─ call TradingDbContext.SaveChangesAsync  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:47) [verified]
         │  │  │  ├─ call EquitySnapshot.ToString  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:43) [verified]
         │  │  │  └─ call TradingDbContext.Add  (src/TradingEngine.Infrastructure/Persistence/Repositories/SqliteEquityRepository.cs:32) [verified]
         │  │  └─ call EquitySnapshotFlush.ToEquity  (src/TradingEngine.Host/EquitySnapshotFlush.cs:37) [verified]
         │  │         public static EquitySnapshot ToEquity(AccountSnapshot s, EngineMode mode) => new(
         │  │         TimestampUtc: s.SimTimeUtc,
         │  │         Balance: s.Balance,
         │  └─ call BufferedEquitySink.GetSnapshots  (src/TradingEngine.Host/EngineRunner.cs:255) [verified]
         │         public IReadOnlyList<AccountSnapshot> GetSnapshots() => _snapshots.ToArray();
         ├─ call RunFromBrokerAsync  (src/TradingEngine.Host/EngineRunner.cs:182) [verified]
         │      public async Task<EngineState> RunFromBrokerAsync(EngineState initial, CancellationToken ct)
         │      var state = initial;
         │      try
         │  ├─ call IBrokerAdapter  (src/TradingEngine.Host/KernelBacktestLoop.cs:142) [approx]
         │  │      public interface IBrokerAdapter
         │  │      ChannelReader<Tick> TickStream { get; }
         │  │      ChannelReader<Bar> BarStream { get; }
         │  ├─ call IJournalWriter  (src/TradingEngine.Host/KernelBacktestLoop.cs:153) [approx]
         │  │      /// <summary>
         │  │      /// The single sink for the unified journal (iter-35 A3). Replaces the independent
         │  │      /// <c>PipelineEventWriter</c> and <c>BarEvaluationHandler</c> writers (Kill-List): there must be
         │  │  └─ di NullJournalWriter  (src/TradingEngine.Host/EngineServiceCollectionExtensions.cs:80) [×2 impls]
         │  │         /// <summary>
         │  │         /// A no-op <see cref="IJournalWriter"/> placeholder for the kernel engine until K5 wires the real
         │  │         /// <c>SqliteStepRecordSink</c> (via a lossless <c>ChannelJournalWriter</c>). The decision journal
         │  ├─ call NullJournalWriter.FlushAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:153) [verified]
         │  │      public Task FlushAsync(CancellationToken ct) => Task.CompletedTask;
         │  ├─ call PumpAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:151) [verified]
         │  │      private async Task<EngineState> PumpAsync(EngineState state, CancellationToken ct)
         │  │      var progressed = true;
         │  │      while (progressed)
         │  │  (3 more branches omitted beyond fan-out)
         │  │  ├─ call IEngineEventQueue  (src/TradingEngine.Host/KernelBacktestLoop.cs:362) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The single-threaded, in-order event queue at the heart of the kernel funnel (iter-35 A2).
         │  │  │      ///
         │  │  │  └─ di InMemoryEngineEventQueue [approx]
         │  │  │         /// <summary>
         │  │  │         /// The default in-order FIFO event queue for the kernel funnel (iter-35 A2). Single-threaded by design:
         │  │  │         /// the <see cref="KernelDriver"/> is the only reader, and effect execution enqueues feedback events on
         │  │  ├─ call IKernel  (src/TradingEngine.Host/KernelBacktestLoop.cs:367) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The decision core (iter-35 A2): a pure function <c>(state, event) → (state', effects)</c>.
         │  │  │      /// The <see cref="KernelDriver"/> calls this once per event. Implementations close over the
         │  │  │  └─ di Kernel [approx]
         │  │  │         /// <summary>
         │  │  │         /// The concrete decision core (iter-35 A2). Closes over the run-constant <see cref="KernelConfig"/> and
         │  │  │         /// routes each event. PURE: no I/O, no wall-clock, no Guid.NewGuid (ids/timestamps come off the event).
         │  │  ├─ call IJournalWriter  (src/TradingEngine.Host/KernelBacktestLoop.cs:369) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The single sink for the unified journal (iter-35 A3). Replaces the independent
         │  │  │      /// <c>PipelineEventWriter</c> and <c>BarEvaluationHandler</c> writers (Kill-List): there must be
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IEffectExecutor  (src/TradingEngine.Host/KernelBacktestLoop.cs:378) [approx]
         │  │  │      public interface IEffectExecutor
         │  │  │      Task ExecuteAsync(EngineEffect effect, CancellationToken ct);
         │  │  │  └─ di EffectExecutor [approx]
         │  │  │         public sealed class EffectExecutor : IEffectExecutor
         │  │  │         private readonly IBrokerAdapter _broker;
         │  │  │         private readonly IEventBus _eventBus;
         │  │  │     (stopped at depth 6; 13 branches omitted)
         │  │  ├─ call IBrokerAdapter  (src/TradingEngine.Host/KernelBacktestLoop.cs:384) [approx]
         │  │  │      public interface IBrokerAdapter
         │  │  │      ChannelReader<Tick> TickStream { get; }
         │  │  │      ChannelReader<Bar> BarStream { get; }
         │  │  ├─ call KernelFeedback.FromAccount  (src/TradingEngine.Host/KernelBacktestLoop.cs:404) [verified]
         │  │  │      public static EquityObserved FromAccount(AccountUpdate a) =>
         │  │  │      new(a.Balance, a.Equity, a.FloatingPnL, a.TimestampUtc);
         │  │  ├─ call InMemoryEngineEventQueue.Enqueue  (src/TradingEngine.Host/KernelBacktestLoop.cs:404) [verified]
         │  │  │      public void Enqueue(EngineEvent evt) => _queue.Enqueue(evt);
         │  │  ├─ call BacktestReplayAdapter.TryRead  (src/TradingEngine.Host/KernelBacktestLoop.cs:401) [approx]
         │  │  ├─ call KernelFeedback.FromExecution  (src/TradingEngine.Host/KernelBacktestLoop.cs:389) [verified]
         │  │  │      public static EngineEvent? FromExecution(ExecutionEvent e, Symbol symbol) => e.NewState switch
         │  │  │      OrderState.PartiallyFilled =>
         │  │  │      new OrderPartiallyFilled(e.OrderId, symbol, e.FilledLots, e.FillPrice ?? new Price(0m), e.TimestampUtc),
         │  │  ├─ call ResolveSymbol  (src/TradingEngine.Host/KernelBacktestLoop.cs:388) [verified]
         │  │  │      private static Symbol ResolveSymbol(EngineState state, Guid orderId)
         │  │  │      if (state.Positions.TryGetValue(orderId, out var ps)) return ps.Symbol;
         │  │  │      foreach (var (_, p) in state.Positions) return p.Symbol;
         │  │  │  ├─ call EngineState  (src/TradingEngine.Host/KernelBacktestLoop.cs:417) [approx]
         │  │  │  │      /// <summary>
         │  │  │  │      /// Kernel state — the authoritative, replayable engine state.
         │  │  │  │      ///
         │  │  │  ├─ call Symbol.Parse  (src/TradingEngine.Host/KernelBacktestLoop.cs:419) [verified]
         │  │  │  │      public static Symbol Parse(string value)
         │  │  │  │      ArgumentException.ThrowIfNullOrWhiteSpace(value);
         │  │  │  │      return new Symbol(value.ToUpperInvariant().Trim());
         │  │  │  └─ call EngineState.TryGetValue  (src/TradingEngine.Host/KernelBacktestLoop.cs:417) [approx]
         │  │  ├─ call ExecuteAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:378) [verified]
         │  │  │      public async Task ExecuteAsync(EngineEffect effect, CancellationToken ct)
         │  │  │      switch (effect)
         │  │  │      case SubmitOrder submit:
         │  │  │  (1 more branch omitted beyond fan-out)
         │  │  │  ├─ call IBrokerAdapter  (src/TradingEngine.Host/EffectExecutor.cs:76) [approx]
         │  │  │  │      public interface IBrokerAdapter
         │  │  │  │      ChannelReader<Tick> TickStream { get; }
         │  │  │  │      ChannelReader<Bar> BarStream { get; }
         │  │  │  ├─ call IRiskManager  (src/TradingEngine.Host/EffectExecutor.cs:116) [approx]
         │  │  │  │      public interface IRiskManager
         │  │  │  │      decimal InitialBalance { get; }
         │  │  │  │      DrawdownState Drawdown { get; }
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call IPositionManager  (src/TradingEngine.Host/EffectExecutor.cs:121) [approx]
         │  │  │  │      public interface IPositionManager
         │  │  │  │      IReadOnlyList<PositionModification> Evaluate(
         │  │  │  │      Position position,
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call PositionManager.DeregisterPosition  (src/TradingEngine.Host/EffectExecutor.cs:121) [verified]
         │  │  │  │      public void DeregisterPosition(Guid positionId)
         │  │  │  │      if (_tracked.TryGetValue(positionId, out var entry))
         │  │  │  │      logger.LogInformation("Position state changed. Id={Id} From={From} To=Closed", positionId, entry.State);
         │  │  │  ├─ call RiskManager.DeregisterPosition  (src/TradingEngine.Host/EffectExecutor.cs:120) [verified]
         │  │  │  │      public void DeregisterPosition(Guid positionId) => _openPositionRisk.Remove(positionId);
         │  │  │  ├─ call RiskManager.RegisterPosition  (src/TradingEngine.Host/EffectExecutor.cs:116) [verified]
         │  │  │  │      public void RegisterPosition(Guid positionId, string strategyId, decimal openRiskAmount)
         │  │  │  │      => _openPositionRisk[positionId] = (strategyId, openRiskAmount);
         │  │  │  ├─ call HandlePublishTradeClosed  (src/TradingEngine.Host/EffectExecutor.cs:112) [verified]
         │  │  │  │      private async Task HandlePublishTradeClosed(PublishTradeClosed effect, CancellationToken ct)
         │  │  │  │      var symbolInfo = _symbolRegistry.Get(effect.Symbol);
         │  │  │  │      // P0.3 (F6): trade construction extracted to TradeResultFactory so the LIVE path here and the
         │  │  │  │  (stopped at depth 6; 2 branches omitted)
         │  │  │  ├─ call NullDecisionJournal.Record  (src/TradingEngine.Host/EffectExecutor.cs:108) [verified]
         │  │  │  │      public void Record(DecisionRecord r) { }
         │  │  │  ├─ call ClosePartialPositionAsync  (src/TradingEngine.Host/EffectExecutor.cs:101) [verified]
         │  │  │  │      public Task ClosePartialPositionAsync(Guid positionId, decimal lots, CancellationToken ct)
         │  │  │  │      var fillPrice = new Price(_lastClose > 0 ? _lastClose : 1m);
         │  │  │  │      if (_openTrades.TryGetValue(positionId, out var trade))
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call BacktestReplayAdapter.ClosePositionAsync  (src/TradingEngine.Host/EffectExecutor.cs:96) [verified]
         │  │  │  │      public Task ClosePositionAsync(Guid positionId, CancellationToken ct)
         │  │  │  │      var spread = GetSpread();
         │  │  │  │      var mid = _lastClose > 0 ? _lastClose : 1m;
         │  │  │  ├─ call BacktestReplayAdapter.ClosePositionAtAsync  (src/TradingEngine.Host/EffectExecutor.cs:94) [verified]
         │  │  │  │      public Task ClosePositionAtAsync(Guid positionId, Price exitPrice, CancellationToken ct)
         │  │  │  │      => CloseAtAsync(positionId, exitPrice, ct);
         │  │  │  └─ call BacktestReplayAdapter.ModifyOrderAsync  (src/TradingEngine.Host/EffectExecutor.cs:84) [verified]
         │  │  │         public Task ModifyOrderAsync(Guid orderId, Price newStopLoss, Price? newTakeProfit, CancellationToken ct)
         │  │  │         if (_openTrades.TryGetValue(orderId, out var trade))
         │  │  │         _openTrades[orderId] = trade with { StopLoss = newStopLoss, TakeProfit = newTakeProfit };
         │  │  └─ call KernelBacktestLoop.BuildStepRecord  (src/TradingEngine.Host/KernelBacktestLoop.cs:369) [verified]
         │  │         private StepRecord BuildStepRecord(long seq, EngineEvent evt, EngineDecision decision, EngineState state)
         │  │         var effectKinds = new string[decision.Effects.Count];
         │  │         string? decisionReason = null;
         │  │     ├─ call KernelBacktestLoop._captureRisk  (src/TradingEngine.Host/KernelBacktestLoop.cs:453) [approx]
         │  │     └─ call EventKindFor  (src/TradingEngine.Host/KernelBacktestLoop.cs:449) [verified]
         │  │            private static string EventKindFor(EngineEvent evt) => evt switch
         │  │            StopLossModifyRequested s => s.Kind,
         │  │            PartialCloseRequested => AddOnJournalKinds.Partial,
         │  │        (stopped at depth 6; 1 branch omitted)
         │  ├─ call ProcessBarAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:145) [verified]
         │  │      private async Task<EngineState> ProcessBarAsync(BarClosed bar, EngineState state, CancellationToken ct)
         │  │      var barModel = new Bar(bar.Symbol, bar.Timeframe, bar.BarOpenTimeUtc, bar.Open, bar.High, bar.Low, bar.Close, 0);
         │  │      _advanceVenue(barModel);
         │  │  (7 more branches omitted beyond fan-out)
         │  │  ├─ call IBrokerAdapter  (src/TradingEngine.Host/KernelBacktestLoop.cs:189) [approx]
         │  │  │      public interface IBrokerAdapter
         │  │  │      ChannelReader<Tick> TickStream { get; }
         │  │  │      ChannelReader<Bar> BarStream { get; }
         │  │  ├─ call IJournalWriter  (src/TradingEngine.Host/KernelBacktestLoop.cs:211) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The single sink for the unified journal (iter-35 A3). Replaces the independent
         │  │  │      /// <c>PipelineEventWriter</c> and <c>BarEvaluationHandler</c> writers (Kill-List): there must be
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call IEngineEventQueue  (src/TradingEngine.Host/KernelBacktestLoop.cs:225) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The single-threaded, in-order event queue at the heart of the kernel funnel (iter-35 A2).
         │  │  │      ///
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BarEvaluator  (src/TradingEngine.Host/KernelBacktestLoop.cs:234) [approx]
         │  │  │      /// <summary>
         │  │  │      /// The evaluator stage (iter-36 K1). The strategy/indicator evaluation that today lives imperatively in
         │  │  │      /// <see cref="TradingLoop.ProcessBarAsync"/> becomes a deterministic <b>event producer</b> feeding the
         │  │  ├─ call BacktestReplayAdapter.CompleteBarAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:338) [verified]
         │  │  ├─ call PumpAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:332) [verified]
         │  │  │      private async Task<EngineState> PumpAsync(EngineState state, CancellationToken ct)
         │  │  │      var progressed = true;
         │  │  │      while (progressed)
         │  │  │  (stopped at depth 5; 15 branches omitted)
         │  │  ├─ call InMemoryEngineEventQueue.Enqueue  (src/TradingEngine.Host/KernelBacktestLoop.cs:330) [verified]
         │  │  │      public void Enqueue(EngineEvent evt) => _queue.Enqueue(evt);
         │  │  ├─ call KernelBacktestLoop._evaluateWeekendFlatten  (src/TradingEngine.Host/KernelBacktestLoop.cs:326) [approx]
         │  │  ├─ call KernelBacktestLoop._evaluateDailyDdGuard  (src/TradingEngine.Host/KernelBacktestLoop.cs:311) [approx]
         │  │  ├─ call KernelBacktestLoop._evaluateTimeFlatten  (src/TradingEngine.Host/KernelBacktestLoop.cs:294) [approx]
         │  │  ├─ call KernelBacktestLoop._evaluateTrailing  (src/TradingEngine.Host/KernelBacktestLoop.cs:272) [approx]
         │  │  └─ call KernelBacktestLoop._realizedEquity  (src/TradingEngine.Host/KernelBacktestLoop.cs:260) [approx]
         │  └─ call BacktestReplayAdapter.ReadAllAsync  (src/TradingEngine.Host/KernelBacktestLoop.cs:142) [approx]
         ├─ call EngineRunner.BuildInitialState  (src/TradingEngine.Host/EngineRunner.cs:177) [verified]
         │      private EngineState BuildInitialState(decimal initialBalance)
         │      var drawdownType = _riskManager.ActiveRuleSet?.DrawdownType ?? "Fixed";
         │      return new EngineState(
         │  └─ call DrawdownReducer.CreateInitial  (src/TradingEngine.Host/EngineRunner.cs:417) [verified]
         │         public static DrawdownState CreateInitial(decimal initialBalance, string drawdownType = "Fixed")
         │         return new DrawdownState(
         │         initialBalance,
         ├─ call BuildKernelLoop  (src/TradingEngine.Host/EngineRunner.cs:176) [verified]
         │      private KernelBacktestLoop BuildKernelLoop(decimal initialBalance)
         │      var profile = ResolveActiveProfile();
         │      var ruleSet = _riskManager.ActiveRuleSet ?? DefaultRuleSet();
         │  ├─ call IBrokerAdapter  (src/TradingEngine.Host/EngineRunner.cs:310) [approx]
         │  │      public interface IBrokerAdapter
         │  │      ChannelReader<Tick> TickStream { get; }
         │  │      ChannelReader<Bar> BarStream { get; }
         │  ├─ call ResetConfig.FromRuleSet  (src/TradingEngine.Host/EngineRunner.cs:322) [verified]
         │  │      public static ResetConfig FromRuleSet(string? dailyResetTime, string? timezone, DayOfWeek weekStartsOn = DayOfWeek.Monday)
         │  │      var time = TimeOnly.TryParse(dailyResetTime, out var t) ? t : new TimeOnly(0, 0);
         │  │      return new ResetConfig(time, string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone, weekStartsOn);
         │  ├─ call BacktestReplayAdapter.OnBarObserved  (src/TradingEngine.Host/EngineRunner.cs:310) [verified]
         │  │      public void OnBarObserved(Bar bar)
         │  │      _lastClose = bar.Close;
         │  │      _currentSpread = bar.Spread;
         │  │  ├─ call BacktestReplayAdapter.EmitAccountUpdate  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:347) [verified]
         │  │  │      private void EmitAccountUpdate(DateTime ts)
         │  │  │      var floatingPnL = ComputeFloatingPnL(_lastClose);
         │  │  │      var equity = _balance + floatingPnL;
         │  │  │  └─ call ComputeFloatingPnL  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:477) [verified]
         │  │  │         private decimal ComputeFloatingPnL(decimal close)
         │  │  │         if (_openTrades.Count == 0) return 0m;
         │  │  │         try
         │  │  │     (stopped at depth 6; 1 branch omitted)
         │  │  ├─ call ProcessSlTpHits  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:346) [verified]
         │  │  │      private void ProcessSlTpHits(Bar bar)
         │  │  │      if (_openTrades.Count == 0) return;
         │  │  │      var spread = GetSpread();
         │  │  │  ├─ call ILogger  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:448) [verified]
         │  │  │  ├─ call BacktestReplayAdapter.EmitAccountUpdate  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:447) [verified]
         │  │  │  │      private void EmitAccountUpdate(DateTime ts)
         │  │  │  │      var floatingPnL = ComputeFloatingPnL(_lastClose);
         │  │  │  │      var equity = _balance + floatingPnL;
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call BacktestReplayAdapter.EmitExecutionEvent  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:436) [verified]
         │  │  │  │      private void EmitExecutionEvent(ExecutionEvent evt)
         │  │  │  │      if (!_executionChannel.Writer.TryWrite(evt))
         │  │  │  │      _logger.LogError("BacktestReplay: execution channel full — event dropped; orderId={OrderId}", evt.OrderId);
         │  │  │  ├─ call ComputeCosts  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:432) [verified]
         │  │  │  │      private TradeCosts ComputeCosts(OpenTrade trade, decimal exitPrice)
         │  │  │  │      try
         │  │  │  │      var symbolInfo = _symbolRegistry.Get(_symbol);
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call VenueFillModel.FirstBreachingTick  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:430) [verified]
         │  │  │  │      public static decimal FirstBreachingTick(Bar sideBar, decimal level, bool fallsToLevel)
         │  │  │  │      => fallsToLevel
         │  │  │  │      // The open tick already breached (price gapped past the level before the bar began) → the
         │  │  │  ├─ call EngineReducer.DetectSlTpExit  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:415) [verified]
         │  │  │  │      public static string? DetectSlTpExit(PositionState state, BarClosed bar)
         │  │  │  │      return DetectSlTpExit(state.Direction, state.CurrentStopLoss, state.TakeProfit,
         │  │  │  │      bar.High, bar.Low);
         │  │  │  ├─ call SpreadConvention.AskBar  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:412) [verified]
         │  │  │  │      public static Bar AskBar(Bar bidBar, decimal spread) => new(
         │  │  │  │      bidBar.Symbol, bidBar.Timeframe, bidBar.OpenTimeUtc,
         │  │  │  │      bidBar.Open + spread, bidBar.High + spread, bidBar.Low + spread, bidBar.Close + spread, bidBar.Volume);
         │  │  │  └─ call GetSpread  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:405) [verified]
         │  │  │         private decimal GetSpread()
         │  │  │         if (_currentSpread is { } s) return s;
         │  │  │         try { return _symbolRegistry.Get(_symbol).TypicalSpread; }
         │  │  │     (stopped at depth 6; 2 branches omitted)
         │  │  ├─ call ProcessPendingStops  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:345) [verified]
         │  │  │      private void ProcessPendingStops(Bar bar)
         │  │  │      if (_pendingStops.Count == 0) return;
         │  │  │      var spread = GetSpread();
         │  │  │  ├─ call ILogger  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:382) [verified]
         │  │  │  ├─ call BacktestReplayAdapter.EmitExecutionEvent  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:391) [verified]
         │  │  │  │      private void EmitExecutionEvent(ExecutionEvent evt)
         │  │  │  │      if (!_executionChannel.Writer.TryWrite(evt))
         │  │  │  │      _logger.LogError("BacktestReplay: execution channel full — event dropped; orderId={OrderId}", evt.OrderId);
         │  │  │  ├─ call FillEntry  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:381) [verified]
         │  │  │  │      private void FillEntry(Guid orderId, TradeDirection direction, decimal fillPrice, decimal lots, Price sl, Price? tp)
         │  │  │  │      var symbolInfo = _symbolRegistry.Get(_symbol);
         │  │  │  │      var entryCommission = TradeCostCalculator.ComputeEntryCommission(
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call VenueFillModel.FirstBreachingTick  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:380) [verified]
         │  │  │  │      public static decimal FirstBreachingTick(Bar sideBar, decimal level, bool fallsToLevel)
         │  │  │  │      => fallsToLevel
         │  │  │  │      // The open tick already breached (price gapped past the level before the bar began) → the
         │  │  │  ├─ call SpreadConvention.AskBar  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:368) [verified]
         │  │  │  │      public static Bar AskBar(Bar bidBar, decimal spread) => new(
         │  │  │  │      bidBar.Symbol, bidBar.Timeframe, bidBar.OpenTimeUtc,
         │  │  │  │      bidBar.Open + spread, bidBar.High + spread, bidBar.Low + spread, bidBar.Close + spread, bidBar.Volume);
         │  │  │  └─ call GetSpread  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:361) [verified]
         │  │  │         private decimal GetSpread()
         │  │  │         if (_currentSpread is { } s) return s;
         │  │  │         try { return _symbolRegistry.Get(_symbol).TypicalSpread; }
         │  │  │     (stopped at depth 6; 2 branches omitted)
         │  │  ├─ call ProcessPendingLimits  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:344) [verified]
         │  │  │      private void ProcessPendingLimits(Bar bar)
         │  │  │      if (_pendingLimits.Count == 0) return;
         │  │  │      var spread = GetSpread();
         │  │  │  ├─ call ILogger  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:301) [verified]
         │  │  │  ├─ call BacktestReplayAdapter.EmitExecutionEvent  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:310) [verified]
         │  │  │  │      private void EmitExecutionEvent(ExecutionEvent evt)
         │  │  │  │      if (!_executionChannel.Writer.TryWrite(evt))
         │  │  │  │      _logger.LogError("BacktestReplay: execution channel full — event dropped; orderId={OrderId}", evt.OrderId);
         │  │  │  ├─ call FillEntry  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:300) [verified]
         │  │  │  │      private void FillEntry(Guid orderId, TradeDirection direction, decimal fillPrice, decimal lots, Price sl, Price? tp)
         │  │  │  │      var symbolInfo = _symbolRegistry.Get(_symbol);
         │  │  │  │      var entryCommission = TradeCostCalculator.ComputeEntryCommission(
         │  │  │  │  (stopped at depth 6; 1 branch omitted)
         │  │  │  ├─ call VenueFillModel.FirstBreachingTick  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:299) [verified]
         │  │  │  │      public static decimal FirstBreachingTick(Bar sideBar, decimal level, bool fallsToLevel)
         │  │  │  │      => fallsToLevel
         │  │  │  │      // The open tick already breached (price gapped past the level before the bar began) → the
         │  │  │  ├─ call SpreadConvention.AskBar  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:289) [verified]
         │  │  │  │      public static Bar AskBar(Bar bidBar, decimal spread) => new(
         │  │  │  │      bidBar.Symbol, bidBar.Timeframe, bidBar.OpenTimeUtc,
         │  │  │  │      bidBar.Open + spread, bidBar.High + spread, bidBar.Low + spread, bidBar.Close + spread, bidBar.Volume);
         │  │  │  └─ call GetSpread  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:282) [verified]
         │  │  │         private decimal GetSpread()
         │  │  │         if (_currentSpread is { } s) return s;
         │  │  │         try { return _symbolRegistry.Get(_symbol).TypicalSpread; }
         │  │  │     (stopped at depth 6; 2 branches omitted)
         │  │  └─ call BacktestReplayAdapter.BarDuration  (src/TradingEngine.Infrastructure/Adapters/BacktestReplayAdapter.cs:343) [verified]
         │  │         private static TimeSpan BarDuration(Timeframe tf) => tf switch
         │  │         Timeframe.M1 => TimeSpan.FromMinutes(1),
         │  │         Timeframe.M5 => TimeSpan.FromMinutes(5),
         │  ├─ call UpdateCrossRates  (src/TradingEngine.Host/EngineRunner.cs:310) [verified]
         │  │      private void UpdateCrossRates(Bar bar)
         │  │      _crossRateFeed?.Advance(bar.OpenTimeUtc);
         │  │      var si = _symbolRegistry.Get(bar.Symbol);
         │  │  ├─ call ISymbolInfoRegistry  (src/TradingEngine.Host/EngineRunner.cs:435) [approx]
         │  │  │      public interface ISymbolInfoRegistry
         │  │  │      SymbolInfo Get(Symbol symbol);
         │  │  │      void Register(SymbolInfo info);
         │  │  │  └─ di SymbolInfoRegistry  (src/TradingEngine.Infrastructure/ServiceCollectionExtensions.cs:45)
         │  │  │         public sealed class SymbolInfoRegistry : ISymbolInfoRegistry
         │  │  │         private readonly ConcurrentDictionary<Symbol, SymbolInfo> _symbols = new();
         │  │  │         private readonly ConcurrentDictionary<Symbol, VenueSymbolSpec> _venueSpecs = new();
         │  │  ├─ call CrossRateStore  (src/TradingEngine.Host/EngineRunner.cs:436) [approx]
         │  │  │      /// <summary>
         │  │  │      /// Cross-rate table pivoted on USD. Every currency is held as "how many USD one unit of it buys"
         │  │  │      /// (USD itself = 1), so an arbitrary pair converts by chaining two legs. Adding a currency means
         │  │  ├─ call SymbolInfoRegistry.Get  (src/TradingEngine.Host/EngineRunner.cs:435) [verified]
         │  │  │      public SymbolInfo Get(Symbol symbol)
         │  │  │      if (_symbols.TryGetValue(symbol, out var info))
         │  │  │      return info;
         │  │  └─ call Advance  (src/TradingEngine.Host/EngineRunner.cs:433) [verified]
         │  │         public void Advance(DateTime simTimeUtc)
         │  │         foreach (var (currency, points) in series)
         │  │         if (points.Count == 0) continue;
         │  │     └─ call CrossRateStore  (src/TradingEngine.Application/CrossRateFeed.cs:33) [approx]
         │  │            /// <summary>
         │  │            /// Cross-rate table pivoted on USD. Every currency is held as "how many USD one unit of it buys"
         │  │            /// (USD itself = 1), so an arbitrary pair converts by chaining two legs. Adding a currency means
         │  ├─ call ConstraintSet.Resolve  (src/TradingEngine.Host/EngineRunner.cs:293) [verified]
         │  │      public static ConstraintSet Resolve(RiskProfile profile, PropFirmRuleSet ruleSet)
         │  │      return new ConstraintSet(
         │  │      Id: ruleSet.Id,
         │  ├─ call EngineRunner.DefaultRuleSet  (src/TradingEngine.Host/EngineRunner.cs:292) [verified]
         │  │      private static PropFirmRuleSet DefaultRuleSet() => new(
         │  │      "none", "None", "Fixed", 0.05, 0.10, 0.10, 0,
         │  │      "BalancePlusFloating", "22:00:00", "UTC", false, "High", 0, 0,
         │  └─ call ResolveActiveProfile  (src/TradingEngine.Host/EngineRunner.cs:291) [verified]
         │         private RiskProfile ResolveActiveProfile()
         │         var profileId = _strategies
         │         .Select(s => s.Config.RiskProfileId)
         │     ├─ call IRiskProfileResolver  (src/TradingEngine.Host/EngineRunner.cs:408) [approx]
         │     │      public interface IRiskProfileResolver
         │     │      RiskProfile Resolve(string riskProfileId);
         │     │  └─ di RiskProfileResolver [approx]
         │     │         public sealed class RiskProfileResolver(IReadOnlyList<RiskProfile> riskProfiles) : IRiskProfileResolver
         │     │         public RiskProfile Resolve(string riskProfileId)
         │     │         var profile = riskProfiles.FirstOrDefault(p => p.Id == riskProfileId);
         │     └─ call RiskProfileResolver.Resolve  (src/TradingEngine.Host/EngineRunner.cs:408) [verified]
         │            public RiskProfile Resolve(string riskProfileId)
         │            var profile = riskProfiles.FirstOrDefault(p => p.Id == riskProfileId);
         │            if (profile is null)
         ├─ call IndicatorSnapshotService.SetAuxBarSource  (src/TradingEngine.Host/EngineRunner.cs:169) [verified]
         │      public void SetAuxBarSource(Symbol symbol, Timeframe tf, IReadOnlyList<Bar> allBars)
         │      _auxSources[(symbol, tf)] = new AuxBarCursor { All = allBars };
         ├─ call Symbol.Parse  (src/TradingEngine.Host/EngineRunner.cs:166) [verified]
         │      public static Symbol Parse(string value)
         │      ArgumentException.ThrowIfNullOrWhiteSpace(value);
         │      return new Symbol(value.ToUpperInvariant().Trim());
         └─ call IndicatorSnapshotService.WarmUpIndicatorsAsync  (src/TradingEngine.Host/EngineRunner.cs:155) [verified]
                public Task WarmUpIndicatorsAsync(CancellationToken ct)
                foreach (var (symbol, byTf) in Bars)
                foreach (var (tf, _) in byTf)
            └─ call IndicatorSnapshotService.RecomputeIndicatorsAsync  (src/TradingEngine.Host/IndicatorSnapshotService.cs:204) [verified]
                   public Task RecomputeIndicatorsAsync(Symbol symbol, Timeframe tf, CancellationToken ct)
                   ct.ThrowIfCancellationRequested();
                   if (!Bars.TryGetValue(symbol, out var byTf)) return Task.CompletedTask;
               ├─ call IndicatorSnapshotService.Emit  (src/TradingEngine.Host/IndicatorSnapshotService.cs:190) [verified]
               │      private void Emit(string key, double value)
               │      IndicatorValues[key] = value;
               │      var q = _series.GetOrAdd(key, _ => new Queue<double>());
               ├─ call SkenderIndicatorService.SuperTrend  (src/TradingEngine.Host/IndicatorSnapshotService.cs:188) [verified]
               │      public IndSuperTrendResult SuperTrend(IReadOnlyList<Bar> bars, int period, double multiplier)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return SuperTrend(quotes, period, multiplier);
               ├─ call SkenderIndicatorService.Macd  (src/TradingEngine.Host/IndicatorSnapshotService.cs:181) [verified]
               │      public IndMacdResult Macd(IReadOnlyList<Bar> bars, int fast, int slow, int signal)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return Macd(quotes, fast, slow, signal);
               ├─ call SkenderIndicatorService.BollingerBands  (src/TradingEngine.Host/IndicatorSnapshotService.cs:172) [verified]
               │      public (double Upper, double Middle, double Lower) BollingerBands(IReadOnlyList<Bar> bars, int period, double stdDev)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return BollingerBands(quotes, period, stdDev);
               ├─ call SkenderIndicatorService.Adx  (src/TradingEngine.Host/IndicatorSnapshotService.cs:169) [verified]
               │      public double Adx(IReadOnlyList<Bar> bars, int period)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return quotes.GetAdx(period).LastOrDefault()?.Adx ?? 0;
               ├─ call SkenderIndicatorService.Sma  (src/TradingEngine.Host/IndicatorSnapshotService.cs:166) [verified]
               │      public double Sma(IReadOnlyList<Bar> bars, int period)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return quotes.GetSma(period).LastOrDefault()?.Sma ?? 0;
               ├─ call SkenderIndicatorService.Rsi  (src/TradingEngine.Host/IndicatorSnapshotService.cs:163) [verified]
               │      public double Rsi(IReadOnlyList<Bar> bars, int period)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return quotes.GetRsi(period).LastOrDefault()?.Rsi ?? 50;
               ├─ call SkenderIndicatorService.Ema  (src/TradingEngine.Host/IndicatorSnapshotService.cs:160) [verified]
               │      public double Ema(IReadOnlyList<Bar> bars, int period)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return quotes.GetEma(period).LastOrDefault()?.Ema ?? 0;
               ├─ call SkenderIndicatorService.Atr  (src/TradingEngine.Host/IndicatorSnapshotService.cs:157) [verified]
               │      public double Atr(IReadOnlyList<Bar> bars, int period)
               │      var quotes = bars.Select(b => new SkenderQuote(b)).ToList();
               │      return quotes.GetAtr(period).LastOrDefault()?.Atr ?? 0;
               ├─ call IndicatorCache.BuildKey  (src/TradingEngine.Host/IndicatorSnapshotService.cs:152) [verified]
               │      public static string BuildKey(Symbol symbol, Timeframe tf, string indicatorName, int period, int barCount)
               │      => $"{symbol}:{tf}:{indicatorName}:{period}:{barCount}";
               └─ call SkenderIndicatorService.ToQuotes  (src/TradingEngine.Host/IndicatorSnapshotService.cs:144) [verified]
                      public static IReadOnlyList<SkenderQuote> ToQuotes(IReadOnlyList<Bar> bars) =>
                      bars.Select(b => new SkenderQuote(b)).ToList();

TOUCHES  EquitySnapshotEntity
