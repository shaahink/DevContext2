using Web.Models;

namespace Web.Services;

public interface IFeedsService
{
    Task<FeedChannel> GetNewsAsync(bool showBrief);
    Task<FeedChannel> GetCommentsAsync(bool showBrief);
}

public interface ISettingsProvider
{
    Task<bool> ShowBriefAsync();
}
