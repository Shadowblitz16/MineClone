namespace MineClone.Core;

public abstract class Disposable : IDisposable
{
    private bool _disposed;

    ~Disposable()
    {
        Dispose();
    }

    public bool IsDisposed() => _disposed;
    
    protected virtual void OnDispose()
    {
        
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        OnDispose();
        GC.SuppressFinalize(this);
    }
}