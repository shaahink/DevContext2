namespace DevContext.Desktop.Helpers;

/// <summary>UI-thread debounce using a captured SynchronizationContext. No WPF dependency.</summary>
public sealed class Debouncer : IDisposable
{
    private readonly int _delayMs;
    private readonly CancellableOperation _op = new();
    private readonly SynchronizationContext? _syncContext;

    public Debouncer(int delayMs = 500)
    {
        _delayMs = delayMs;
        _syncContext = SynchronizationContext.Current;
    }

    /// <summary>Debounces an action: cancels any pending execution and schedules a new one after the delay.</summary>
    public void Invoke(Action action)
    {
        var ct = _op.Begin();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_delayMs, ct).ConfigureAwait(false);
                if (!ct.IsCancellationRequested)
                {
                    if (_syncContext is not null)
                        _syncContext.Post(_ => action(), null);
                    else
                        action();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { /* best-effort; debounced action failures are not surfaced */ }
        }, ct);
    }

    /// <summary>Cancels any pending debounced execution.</summary>
    public void Cancel() => _op.Cancel();

    public void Dispose() => _op.Dispose();
}
