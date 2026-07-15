using Web.Models;

namespace Web.Services;

// Primary-constructor service (mirrors DntSite's FeedsService). Auto-registered by convention.
public class FeedsService(ISettingsProvider settings) : IFeedsService
{
    public async Task<FeedChannel> GetNewsAsync(bool showBrief)
    {
        var brief = await settings.ShowBriefAsync();
        return new FeedChannel($"news brief={brief} showBrief={showBrief}");
    }

    public async Task<FeedChannel> GetCommentsAsync(bool showBrief)
    {
        var brief = await settings.ShowBriefAsync();
        return new FeedChannel($"comments brief={brief} showBrief={showBrief}");
    }
}

public class SettingsProvider : ISettingsProvider
{
    public Task<bool> ShowBriefAsync() => Task.FromResult(true);
}
