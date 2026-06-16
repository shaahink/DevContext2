namespace DevContext.Desktop.Helpers;

/// <summary>Wraps the cancel-previous + identity-guarded disposal pattern used by _cts, _renderCts, etc.</summary>
public sealed class CancellableOperation : IDisposable
{
    private readonly Lock _lock = new();
    private CancellationTokenSource? _cts;

    /// <summary>Cancels and disposes the previous operation, then starts a new one. Returns the new token.</summary>
    public CancellationToken Begin()
    {
        using (_lock.EnterScope())
        {
            CancelAndDispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }

    /// <summary>Cancels the current operation without starting a new one.</summary>
    public void Cancel()
    {
        using (_lock.EnterScope())
        {
            CancelAndDispose();
        }
    }

    /// <summary>Disposes this operation only if it's still the current instance. Call from finally blocks.</summary>
    public void End(CancellationTokenSource? myCts)
    {
        using (_lock.EnterScope())
        {
            if (_cts == myCts && _cts is not null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    /// <summary>Creates a linked token from an external token and the internal one.</summary>
    public CancellationToken Link(CancellationToken external)
    {
        using (_lock.EnterScope())
        {
            return _cts is not null
                ? CancellationTokenSource.CreateLinkedTokenSource(external, _cts.Token).Token
                : external;
        }
    }

    /// <summary>Gets the current token, or default if no operation is active.</summary>
    public CancellationToken Token
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _cts?.Token ?? CancellationToken.None;
            }
        }
    }

    private void CancelAndDispose()
    {
        if (_cts is null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        using (_lock.EnterScope())
        {
            CancelAndDispose();
        }
    }
}
