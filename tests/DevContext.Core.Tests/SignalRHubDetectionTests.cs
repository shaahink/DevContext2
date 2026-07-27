using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

/// <summary>B1 (Prism D1.2a) — in-framework SignalR. Both audit repos (dotnet-podcasts, bitwarden)
/// declare hubs with the namespace-QUALIFIED base (": Microsoft.AspNetCore.SignalR.Hub") and no
/// SignalR package reference; the bare-name matcher missed them entirely.</summary>
public sealed class SignalRHubDetectionTests
{
    [Theory]
    [InlineData("Hub", true)]
    [InlineData("Hub<IListenTogetherHubClient>", true)]
    [InlineData("Microsoft.AspNetCore.SignalR.Hub", true)]                       // podcasts + bitwarden form
    [InlineData("Microsoft.AspNetCore.SignalR.Hub<IChatClient>", true)]
    [InlineData("EventHub", false)]                                              // Azure Event Hubs client type
    [InlineData("My.Messaging.Hub", false)]                                      // non-SignalR qualified Hub
    [InlineData("HubConnection", false)]                                         // the SignalR CLIENT type
    [InlineData("INotificationHub", false)]
    public void IsHubBaseName_matches_bare_and_signalr_qualified_only(string baseName, bool expected)
    {
        Assert.Equal(expected, SignalRHubExtractor.IsHubBaseName(baseName));
    }
}
