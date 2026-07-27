# D5.3 — laden-dev-server look (unresponsive after ~36 analyses; restart cured)

**Diagnosis: the session cap was already there (capacity 5, LRU + idle eviction) — the unbounded
growth was one level down.** `EngineHostCache` creates one `EngineHost` per analyzed root and
NEVER evicts: each host pins a `ServiceProvider` + `DiscoveryPipeline` + `PersistentAnalysisCache`,
and the latter holds every parsed file's `SyntaxTree` + full text + `XDocument` in memory for the
server's lifetime. Session eviction disposed the session but not its host, so a long-lived dev
server driven across many distinct roots (gate loops, octet drives, ad-hoc analyses) accumulated
per-repo tree caches until the process thrashed. Restart was the only eviction — hence "restart
cures".

**Fix:** a removed session (close, idle-eviction, LRU-eviction — all three removal paths) now
releases its `EngineHost` when no other live session shares the root
(`AnalysisSessionManager.ReleaseHostIfOrphanedAsync` → `EngineHostCache.ReleaseAsync`). The host
cache is thereby bounded by the session cap. A released root re-opens warm via the J2 snapshot
cache (~seconds) instead of holding gigabytes of trees on the off-chance.

**Proof:** `HostReleaseTests` 2/2 — close-releases-host, and capacity-eviction takes the evicted
root's host down with it (real `EngineRunner` + fixtures, snapshot cache redirected). Full server
suite 25/25. Live drive: McpQa serial 2/2 (2m55s) through the changed stack.

Safety note: release fires only when zero live sessions reference the root, so a session's
`Engine.Pipeline` can never be disposed under it; a concurrent analyze on a just-released root
recreates the host via `GetOrCreate` (worst case a cold re-parse, never a wrong result).
