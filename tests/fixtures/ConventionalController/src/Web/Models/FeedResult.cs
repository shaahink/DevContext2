using Microsoft.AspNetCore.Mvc;

namespace Web.Models;

public sealed record FeedChannel(string Payload);

// Mirrors DntSite's FeedResult<T> — the action wraps its service call in a `new FeedResult<T>(await ...)`.
public sealed class FeedResult<T>(FeedChannel channel) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "application/xml";
        return context.HttpContext.Response.WriteAsync(channel.Payload);
    }
}
