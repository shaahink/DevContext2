namespace DevContext.Desktop.Helpers;

/// <summary>Wraps the cancel-previous + identity-guarded disposal pattern used by _cts, _renderCts, etc.</summary>
public sealed class CancellableOperation : IDisposable
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _linkedCts;

    /// <summary>Cancels and disposes the previous operation, then starts a new one. Returns the new token.</summary>
    public CancellationToken Begin()
    {
        lock (_lock)
        {
            CancelAndDispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }

    /// <summary>Cancels the current operation without starting a new one.</summary>
    public void Cancel()
    {
        lock (_lock)
        {
            CancelAndDispose();
        }
    }

    /// <summary>Disposes this operation only if it's still the current instance. Call from finally blocks.</summary>
    public void End(CancellationTokenSource? myCts)
    {
        lock (_lock)
        {
            if (_cts == myCts && _cts is not null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    /// <summary>Creates a linked token from an external token and the internal one.
    /// The linked source is disposed on the next Begin/Cancel/Dispose call.</summary>
    public CancellationToken Link(CancellationToken external)
    {
        lock (_lock)
        {
            _linkedCts?.Dispose();
            _linkedCts = null;
            if (_cts is not null)
            {
                _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(external, _cts.Token);
                return _linkedCts.Token;
            }
            return external;
        }
    }

    /// <summary>Gets the current token, or default if no operation is active.</summary>
    public CancellationToken Token
    {
        get
        {
            lock (_lock)
            {
                return _cts?.Token ?? CancellationToken.None;
            }
        }
    }

    private void CancelAndDispose()
    {
        _linkedCts?.Dispose();
        _linkedCts = null;
        if (_cts is null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            CancelAndDispose();
        }
    }
}
