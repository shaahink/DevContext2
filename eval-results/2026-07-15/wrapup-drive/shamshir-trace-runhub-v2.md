TRACE  RunHub
       src/TradingEngine.Web/Hubs/RunHub.cs

▸ ENTRY  RunHub  (src/TradingEngine.Web/Hubs/RunHub.cs)
   └─ call RunHub  (src/TradingEngine.Web/Hubs/RunHub.cs:12)
          /// <summary>
          /// Live run channel (iter-21 U1). Clients join a per-run group keyed by <c>runId</c>; the engine
          /// publishes a throttled <see cref="Services.RunProgress"/> envelope to that group via
