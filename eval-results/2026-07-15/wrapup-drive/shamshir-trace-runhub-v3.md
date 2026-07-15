TRACE  RunHub
       src/TradingEngine.Web/Hubs/RunHub.cs

▸ ENTRY  RunHub  (src/TradingEngine.Web/Hubs/RunHub.cs)
   ├─ call RunHub.Group  (src/TradingEngine.Web/Hubs/RunHub.cs:12)
   │      public static string Group(string runId) => $"run:{runId}";
   ├─ call RunHub.JoinRun  (src/TradingEngine.Web/Hubs/RunHub.cs:12)
   │      public async Task JoinRun(string runId)
   │      await Groups.AddToGroupAsync(Context.ConnectionId, Group(runId));
   │      // iter-redesign P6.1: snapshot-on-join — a page load / reconnect mid-run gets the CURRENT
   │  ├─ call RunHub.Group  (src/TradingEngine.Web/Hubs/RunHub.cs:22) [verified]
   │  │      public static string Group(string runId) => $"run:{runId}";
   │  └─ call BacktestOrchestrator.GetCurrentProgress  (src/TradingEngine.Web/Hubs/RunHub.cs:27) [verified]
   │         public RunProgress? GetCurrentProgress(string runId)
   │         var state = GetState(runId);
   │         if (state is null) return null;
   │     ├─ call BacktestOrchestrator.GetState  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:514) [verified]
   │     │      public BacktestRunState? GetState(string runId) =>
   │     │      _runs.TryGetValue(runId, out var state) ? state : null;
   │     └─ call BacktestOrchestrator.BuildProgress  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:521) [verified]
   │            private RunProgress BuildProgress(BacktestRunState state, string status)
   │            DateTime? simTime = DateTime.TryParse(state.SimTime, out var t) ? t : null;
   │            var elapsedMs = (long)(DateTime.UtcNow - state.StartedAt).TotalMilliseconds;
   └─ call RunHub.LeaveRun  (src/TradingEngine.Web/Hubs/RunHub.cs:12)
          public Task LeaveRun(string runId) =>
          Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(runId));
      └─ call RunHub.Group  (src/TradingEngine.Web/Hubs/RunHub.cs:36) [verified]
             public static string Group(string runId) => $"run:{runId}";
