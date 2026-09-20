using System.Runtime.InteropServices;

namespace ERuDict;

internal sealed class HotkeyManager : IDisposable
{
    private readonly Settings _settings;
    private readonly Action _onTrigger;
    private readonly SynchronizationContext _sync;
    private readonly Win32.LowLevelKeyboardProc _hookProc;

    private IntPtr _hook = IntPtr.Zero;
    private bool _ctrlWasDown;
    private bool _shiftWasDown;
    private long _lastPressTime;
    private long _lastTriggerTime;
    private bool _disposed;

    public HotkeyManager(Settings settings, Action onTrigger)
    {
        _settings = settings;
        _onTrigger = onTrigger;
        _sync = SynchronizationContext.Current ?? new SynchronizationContext();
        _hookProc = HookCallback;
    }

    public void Start()
    {
        if (_hook != IntPtr.Zero)
        {
            return;
        }

        _hook = Win32.SetWindowsHookEx(
            Win32.WhKeyboardLl,
            _hookProc,
            Win32.GetModuleHandle(null),
            0);

        if (_hook == IntPtr.Zero)
        {
            AppLogger.Error("Не удалось установить клавиатурный хук.", new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
        }
    }

    public void Stop()
    {
        if (_hook == IntPtr.Zero)
        {
            return;
        }

        Win32.UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var info = Marshal.PtrToStructure<Win32.KbdLlHookStruct>(lParam);
            if ((info.Flags & Win32.LlkhfInjected) == 0)
            {
                var message = wParam.ToInt32();
                var isDown = message is Win32.WmKeyDown or Win32.WmSysKeyDown;
                HandleKey(info.VkCode, isDown);
            }
        }

        return Win32.CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    private void HandleKey(uint vkCode, bool isDown)
    {
        var now = Environment.TickCount64;
        var ctrlDown = Win32.IsControl(vkCode) ? isDown : Win32.IsKeyDown(Win32.VkControl);
        var shiftDown = vkCode == Win32.VkShift ? isDown : Win32.IsKeyDown(Win32.VkShift);

        if (_settings.HotkeyMode == "ctrlShift")
        {
            if (ctrlDown && shiftDown && !(_ctrlWasDown && _shiftWasDown))
            {
                TryTrigger(now);
            }
        }
        else if (Win32.IsControl(vkCode) && isDown && !_ctrlWasDown)
        {
            if (now - _lastPressTime < _settings.DoublePressIntervalMs && now - _lastTriggerTime > 500)
            {
                TryTrigger(now);
            }

            _lastPressTime = now;
        }

        _ctrlWasDown = ctrlDown;
        _shiftWasDown = shiftDown;
    }

    private void TryTrigger(long now)
    {
        _lastTriggerTime = now;
        _sync.Post(_ => _onTrigger(), null);
    }
}
