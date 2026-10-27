using System.Runtime.InteropServices;

namespace ERuDict;

internal class UiWindow : IDisposable
{
    private const string ClassName = "ERuDictWindow";

    private static readonly Dictionary<nint, UiWindow> Windows = new();
    private static readonly WndProcDelegate Proc = Dispatch;
    private static bool _classRegistered;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WndProcDelegate(nint hwnd, uint msg, nint wParam, nint lParam);

    protected nint Hwnd { get; private set; }
    private bool _modalClosed;
    private bool _outsideArmed;
    private bool _outsideWasDown;

    protected void Create(string? title, uint style, uint exStyle, int x, int y, int width, int height, nint owner)
    {
        UiTheme.Ensure();
        EnsureClass();
        var handle = GCHandle.Alloc(this);
        try
        {
            var hwnd = UiNative.CreateWindowEx(
                exStyle,
                ClassName,
                title,
                style,
                x,
                y,
                width,
                height,
                owner,
                0,
                Win32.GetModuleHandle(null),
                GCHandle.ToIntPtr(handle));
            if (hwnd == 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            Hwnd = hwnd;
            if (UiTheme.LargeIcon != 0)
            {
                UiNative.SendMessage(hwnd, UiNative.WM_SETICON, 1, UiTheme.LargeIcon);
                UiNative.SendMessage(hwnd, UiNative.WM_SETICON, 0, UiTheme.SmallIcon != 0 ? UiTheme.SmallIcon : UiTheme.LargeIcon);
            }
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        if (Hwnd != 0 && UiNative.IsWindow(Hwnd))
        {
            UiNative.DestroyWindow(Hwnd);
        }

        Hwnd = 0;
        GC.SuppressFinalize(this);
    }

    protected void ShowWithoutActivation()
    {
        UiNative.ShowWindow(Hwnd, UiNative.SW_SHOWNOACTIVATE);
        UiNative.UpdateWindow(Hwnd);
    }

    protected void HideWindow()
    {
        UiNative.ShowWindow(Hwnd, UiNative.SW_HIDE);
        UiNative.SetWindowPos(
            Hwnd,
            0,
            0,
            0,
            0,
            0,
            UiNative.SWP_NOMOVE | UiNative.SWP_NOSIZE | UiNative.SWP_NOZORDER | UiNative.SWP_NOACTIVATE | UiNative.SWP_HIDEWINDOW);
    }

    protected void ShowModal()
    {
        _modalClosed = false;
        UiNative.ShowWindow(Hwnd, UiNative.SW_SHOW);
        UiNative.SetForegroundWindow(Hwnd);
        while (!_modalClosed)
        {
            var result = UiNative.GetMessage(out var message, 0, 0, 0);
            if (result == 0)
            {
                UiNative.PostQuitMessage((int)message.wParam);
                break;
            }

            if (result < 0)
            {
                break;
            }

            if (UiNative.IsDialogMessage(Hwnd, ref message))
            {
                continue;
            }

            UiNative.TranslateMessage(ref message);
            UiNative.DispatchMessage(ref message);
        }
    }

    protected void CloseModal()
    {
        _modalClosed = true;
        if (Hwnd != 0 && UiNative.IsWindow(Hwnd))
        {
            UiNative.DestroyWindow(Hwnd);
        }
    }

    protected virtual void OnCreate()
    {
    }

    protected virtual void OnPaint(nint hdc)
    {
    }

    protected virtual void OnSize()
    {
    }

    protected virtual void OnClick(int x, int y)
    {
    }

    protected virtual void OnCommand(int id, int code, nint control)
    {
    }

    protected virtual void OnTimer(nint id)
    {
    }

    protected virtual void OnDrawItem(UiNative.DrawItemStruct item)
    {
    }

    protected virtual nint OnColor(uint message, nint hdc, nint child) => 0;

    protected virtual bool OnClose() => false;

    protected void ArmOutsideClick()
    {
        var down = MouseDown();
        _outsideArmed = !down;
        _outsideWasDown = down;
    }

    protected bool OutsideClick()
    {
        var down = MouseDown();
        if (!_outsideArmed)
        {
            if (!down)
            {
                _outsideArmed = true;
            }

            _outsideWasDown = down;
            return false;
        }

        var pressed = down && !_outsideWasDown;
        _outsideWasDown = down;
        if (!pressed)
        {
            return false;
        }

        UiNative.GetCursorPos(out var cursor);
        UiNative.GetWindowRect(Hwnd, out var window);
        return !window.Contains(cursor.X, cursor.Y);
    }

    private static bool MouseDown() =>
        Win32.IsKeyDown(Win32.VkLButton) || Win32.IsKeyDown(Win32.VkRButton);

    protected virtual void OnDestroy()
    {
    }

    protected virtual nint HandleMessage(uint message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case UiNative.WM_CREATE:
                OnCreate();
                return 0;
            case UiNative.WM_CLOSE:
                if (OnClose())
                {
                    return 0;
                }

                break;
            case UiNative.WM_DESTROY:
                Windows.Remove(Hwnd);
                OnDestroy();
                return 0;
            case UiNative.WM_ERASEBKGND:
                PaintBackground(wParam);
                return 1;
            case UiNative.WM_PAINT:
            {
                var hdc = UiNative.BeginPaint(Hwnd, out var paint);
                try
                {
                    OnPaint(hdc);
                }
                finally
                {
                    UiNative.EndPaint(Hwnd, ref paint);
                }

                return 0;
            }
            case UiNative.WM_SIZE:
                OnSize();
                return 0;
            case UiNative.WM_LBUTTONDOWN:
                OnClick(UiNative.SignedLow(lParam), UiNative.SignedHigh(lParam));
                return 0;
            case UiNative.WM_COMMAND:
                OnCommand(UiNative.LowWord(wParam), UiNative.HighWord(wParam), lParam);
                return 0;
            case UiNative.WM_TIMER:
                OnTimer(wParam);
                return 0;
            case UiNative.WM_DRAWITEM:
                OnDrawItem(Marshal.PtrToStructure<UiNative.DrawItemStruct>(lParam));
                return 1;
            case UiNative.WM_CTLCOLOREDIT:
            case UiNative.WM_CTLCOLORSTATIC:
            case UiNative.WM_CTLCOLORLISTBOX:
            {
                var brush = OnColor(message, wParam, lParam);
                if (brush != 0)
                {
                    return brush;
                }

                break;
            }
        }

        return UiNative.DefWindowProc(Hwnd, message, wParam, lParam);
    }

    protected void PaintBackground(nint hdc)
    {
        UiNative.GetClientRect(Hwnd, out var rect);
        UiNative.FillRect(hdc, ref rect, UiTheme.BackgroundBrush);
    }

    protected void Fill(nint hdc, UiNative.Rect rect, nint brush) => UiNative.FillRect(hdc, ref rect, brush);

    protected void DrawText(nint hdc, string text, nint font, int color, UiNative.Rect rect, uint format)
    {
        var previous = UiNative.SelectObject(hdc, font);
        UiNative.SetBkMode(hdc, UiNative.TRANSPARENT);
        UiNative.SetTextColor(hdc, color);
        UiNative.DrawText(hdc, text ?? "", -1, ref rect, format | UiNative.DT_NOPREFIX);
        UiNative.SelectObject(hdc, previous);
    }

    protected nint ColorEdit(nint hdc, int textColor)
    {
        UiNative.SetBkMode(hdc, UiNative.OPAQUE);
        UiNative.SetBkColor(hdc, UiTheme.Surface);
        UiNative.SetTextColor(hdc, textColor);
        return UiTheme.SurfaceBrush;
    }

    protected nint ColorStatic(nint hdc, int textColor)
    {
        UiNative.SetBkMode(hdc, UiNative.TRANSPARENT);
        UiNative.SetBkColor(hdc, UiTheme.Background);
        UiNative.SetTextColor(hdc, textColor);
        return UiTheme.BackgroundBrush;
    }

    private static void EnsureClass()
    {
        if (_classRegistered)
        {
            return;
        }

        var cls = new UiNative.WndClassEx
        {
            cbSize = Marshal.SizeOf<UiNative.WndClassEx>(),
            style = 0x0001 | 0x0002 | 0x0008,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(Proc),
            hInstance = Win32.GetModuleHandle(null),
            hCursor = UiNative.LoadCursor(0, 32512),
            hbrBackground = UiTheme.BackgroundBrush,
            lpszClassName = ClassName,
            hIcon = UiTheme.LargeIcon,
            hIconSm = UiTheme.SmallIcon
        };
        if (UiNative.RegisterClassEx(ref cls) == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        _classRegistered = true;
    }

    private static nint Dispatch(nint hwnd, uint message, nint wParam, nint lParam)
    {
        try
        {
            if (message == UiNative.WM_NCCREATE)
            {
                var created = Marshal.PtrToStructure<UiNative.CreateStruct>(lParam);
                var handle = GCHandle.FromIntPtr(created.lpCreateParams);
                if (handle.Target is UiWindow window)
                {
                    window.Hwnd = hwnd;
                    Windows[hwnd] = window;
                }

                return UiNative.DefWindowProc(hwnd, message, wParam, lParam);
            }

            if (Windows.TryGetValue(hwnd, out var found))
            {
                return found.HandleMessage(message, wParam, lParam);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("Ошибка окна.", ex);
        }

        return UiNative.DefWindowProc(hwnd, message, wParam, lParam);
    }
}
