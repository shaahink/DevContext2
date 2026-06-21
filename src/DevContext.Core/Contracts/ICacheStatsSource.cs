using DevContext.Core.Models;

namespace DevContext.Core.Contracts;

/// <summary>Implemented by analysis caches that can report hit/miss statistics for the run report.</summary>
public interface ICacheStatsSource
{
    /// <summary>Returns text/syntax cache hit and miss counts.</summary>
    CacheStats GetStats();
}
