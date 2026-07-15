TRACE  DELETE /api/runs
       src/TradingEngine.Web/Api/RunsController.cs:215
       TradingEngine.Web
▸ ENTRY  DELETE /api/runs  (src/TradingEngine.Web/Api/RunsController.cs:215)
   └─ call RunsController.Cancel  (src/TradingEngine.Web/Api/RunsController.cs:215)
          [HttpDelete("{runId}")]
          public async Task<IActionResult> Cancel(string runId)
          _orchestrator.Cancel(runId);
      └─ call BacktestOrchestrator.Cancel  (src/TradingEngine.Web/Api/RunsController.cs:218) [verified]
             public void Cancel(string runId)
             if (!_runs.TryGetValue(runId, out var state))
             return;
         ├─ call BacktestOrchestrator.KillCtraderProcessTreeAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:584) [verified]
         │      private Task KillCtraderProcessTreeAsync(string runId, string reason) => Task.Run(() =>
         │      foreach (var image in new[] { "ctrader-cli", "cTrader.Automate" })
         │      System.Diagnostics.Process[] procs;
         │  └─ call RecordReap  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:1980) [verified]
         │         private void RecordReap(string runId, string image, int pid, string reason)
         │         _ = Task.Run(async () =>
         │         try
         │     ├─ data VenueSessionEntity  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2000) [approx]
         │     │      public sealed class VenueSessionEntity : IAuditableEntity
         │     │      public Guid Id { get; set; } = Guid.NewGuid();
         │     │      public string RunId { get; set; } = "";
         │     ├─ call DbSet  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2000) [verified]
         │     └─ call TradingDbContext.SaveChangesAsync  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:2005) [verified]
         └─ call RunStateMachine.IsTerminal  (src/TradingEngine.Web/Services/BacktestOrchestrator.cs:573) [verified]
                public static bool IsTerminal(string state) => TerminalStates.Contains(state);

TOUCHES  VenueSessionEntity
RESULT   200 OK / 204 No Content · failure → 404 Not Found
