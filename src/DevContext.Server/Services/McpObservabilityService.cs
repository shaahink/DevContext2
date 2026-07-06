using System.Collections.Concurrent;
using System.Threading.Channels;
using DevContext.Protos;

namespace DevContext.Server.Services;

public sealed class McpObservabilityService
{
    private readonly ConcurrentDictionary<string, ChannelWriter<ToolCallEvent>> _observers = new();
    private volatile bool _running;

    public bool IsRunning => _running;

    public string Start()
    {
        _running = true;
        return "MCP endpoint active";
    }

    public string Stop()
    {
        _running = false;
        return "MCP endpoint stopped";
    }

    public IDisposable Subscribe(string observerId, ChannelWriter<ToolCallEvent> writer)
    {
        _observers[observerId] = writer;
        return new Unsubscriber(this, observerId);
    }

    public void Notify(ToolCallEvent evt)
    {
        if (!_running) return;

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

    private void Remove(string observerId)
    {
        _observers.TryRemove(observerId, out _);
    }

    private sealed class Unsubscriber(McpObservabilityService svc, string id) : IDisposable
    {
        public void Dispose() => svc.Remove(id);
    }
}
