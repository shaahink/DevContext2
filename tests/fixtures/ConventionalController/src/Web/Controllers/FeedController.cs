using Microsoft.AspNetCore.Mvc;

using Web.Models;
using Web.Services;

namespace Web.Controllers;

// Mirrors DntSite's FeedController: primary-constructor injection, conventional [controller]/[action]
// routing with NO HTTP verb attributes, and expression-bodied actions wrapping the service call in a
// `new FeedResult<T>(await feedsService.GetXAsync(await ShowBriefDescriptionAsync()))`.
[ApiController]
[Route("[controller]/[action]")]
public class FeedController(IFeedsService feedsService, ISettingsProvider settingsProvider)
    : ControllerBase
{
    public async Task<IActionResult> News()
        => new FeedResult<FeedChannel>(await feedsService.GetNewsAsync(await ShowBriefDescriptionAsync()));

    public async Task<IActionResult> Comments()
        => new FeedResult<FeedChannel>(await feedsService.GetCommentsAsync(await ShowBriefDescriptionAsync()));

    private async Task<bool> ShowBriefDescriptionAsync()
        => await settingsProvider.ShowBriefAsync();
}
