using System.Collections.Concurrent;
using System.Threading.Channels;
using DevContext.Protos;
using DevContext.Server.Endpoints;

namespace DevContext.Server.Services;

public sealed class McpObservabilityService
{
    private readonly ConcurrentDictionary<string, ChannelWriter<ToolCallEvent>> _observers = new();
    private long _agentCallCount;
    private long _lastAgentCallAtUtcMs;
    private string _lastAgentTool = string.Empty;

    public IDisposable Subscribe(string observerId, ChannelWriter<ToolCallEvent> writer)
    {
        _observers[observerId] = writer;
        return new Unsubscriber(this, observerId);
    }

    public void Notify(ToolCallEvent evt)
    {
        // N4.1 — MEASURE FIRST, forward second. This used to open with `if (!_running) return;`,
        // a global mute the MCP page's Stop button flipped, so a human watching one page could
        // silently erase what every other watcher saw AND what the server knew. The
        // last-agent-call figures the status card reports are facts about traffic; they cannot
        // depend on whether anyone happened to be looking.
        if (evt.Origin == OriginTag.Agent)
        {
            Interlocked.Increment(ref _agentCallCount);
            Interlocked.Exchange(ref _lastAgentCallAtUtcMs, evt.TimestampUtcMs);
            Volatile.Write(ref _lastAgentTool, evt.Tool);
        }

        var stale = new List<string>();
        foreach (var (id, writer) in _observers)
        {
            if (!writer.TryWrite(evt))
            {
                stale.Add(id); // G6 — channel full or completed → mark stale
            }
        }

        // G6 — cleanup stale observers outside the enumeration
        foreach (var id in stale)
            _observers.TryRemove(id, out _);
    }

    public int ObserverCount => _observers.Count;

    /// <summary>N4.1 — agent-origin tool calls served since this server started.</summary>
    public long AgentCallCount => Interlocked.Read(ref _agentCallCount);

    /// <summary>N4.1 — when the last agent-origin call was served (unix ms); 0 = never.</summary>
    public long LastAgentCallAtUtcMs => Interlocked.Read(ref _lastAgentCallAtUtcMs);

    /// <summary>N4.1 — the tool name of that call; empty when no agent has called.</summary>
    public string LastAgentTool => Volatile.Read(ref _lastAgentTool);

    private void Remove(string observerId)
    {
        _observers.TryRemove(observerId, out _);
    }

    private sealed class Unsubscriber(McpObservabilityService svc, string id) : IDisposable
    {
        public void Dispose() => svc.Remove(id);
    }
}
