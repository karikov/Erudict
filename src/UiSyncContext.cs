namespace ERuDict;

internal sealed class UiSyncContext : SynchronizationContext
{
    private readonly nint _hwnd;

    public UiSyncContext(nint hwnd) => _hwnd = hwnd;

    public override void Post(SendOrPostCallback d, object? state)
    {
        var box = new Work(d, state);
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(box);
        if (!UiNative.PostMessage(_hwnd, UiNative.WM_EXECUTE, 0, System.Runtime.InteropServices.GCHandle.ToIntPtr(handle)))
        {
            handle.Free();
        }
    }

    public static void Execute(nint lParam)
    {
        var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(lParam);
        try
        {
            if (handle.Target is Work work)
            {
                work.Invoke();
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("Ошибка в очереди интерфейса.", ex);
        }
        finally
        {
            handle.Free();
        }
    }

    private sealed class Work
    {
        private readonly SendOrPostCallback _callback;
        private readonly object? _state;

        public Work(SendOrPostCallback callback, object? state)
        {
            _callback = callback;
            _state = state;
        }

        public void Invoke() => _callback(_state);
    }
}
