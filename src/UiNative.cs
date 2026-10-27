using System.Runtime.InteropServices;
using System.Text;

namespace ERuDict;

internal static class UiNative
{
    public const uint WM_DESTROY = 0x0002;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_SIZE = 0x0005;
    public const uint WM_PAINT = 0x000F;
    public const uint WM_ERASEBKGND = 0x0014;
    public const uint WM_SETFONT = 0x0030;
    public const uint WM_GETMINMAXINFO = 0x0024;
    public const uint WM_SETICON = 0x0080;
    public const uint WM_NCCREATE = 0x0081;
    public const uint WM_CREATE = 0x0001;
    public const uint WM_NULL = 0x0000;
    public const uint WM_COMMAND = 0x0111;
    public const uint WM_TIMER = 0x0113;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_SETCURSOR = 0x0020;
    public const uint WM_DRAWITEM = 0x002B;
    public const uint WM_CTLCOLOREDIT = 0x0133;
    public const uint WM_CTLCOLORSTATIC = 0x0138;
    public const uint WM_CTLCOLORLISTBOX = 0x0134;
    public const uint WM_LBUTTONDBLCLK = 0x0203;
    public const uint WM_RBUTTONUP = 0x0205;
    public const uint WM_KEYDOWN = 0x0100;

    public const uint WM_TRAY = 0x8000 + 21;
    public const uint WM_EXECUTE = 0x8000 + 22;

    public const uint WS_OVERLAPPED = 0x00000000;
    public const uint WS_POPUP = 0x80000000;
    public const uint WS_CHILD = 0x40000000;
    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_CLIPCHILDREN = 0x02000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_CAPTION = 0x00C00000;
    public const uint WS_SYSMENU = 0x00080000;
    public const uint WS_THICKFRAME = 0x00040000;
    public const uint WS_MINIMIZEBOX = 0x00020000;
    public const uint WS_MAXIMIZEBOX = 0x00010000;
    public const uint WS_TABSTOP = 0x00010000;
    public const uint WS_VSCROLL = 0x00200000;
    public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
    public const uint WS_EX_DLGMODALFRAME = 0x00000001;

    public const uint ES_MULTILINE = 0x0004;
    public const uint ES_AUTOVSCROLL = 0x0040;
    public const uint ES_READONLY = 0x0800;
    public const uint ES_NUMBER = 0x2000;
    public const uint SS_NOPREFIX = 0x0080;
    public const uint SS_CENTERIMAGE = 0x0200;
    public const uint BS_PUSHBUTTON = 0x00000000;
    public const uint BS_DEFPUSHBUTTON = 0x00000001;
    public const uint BS_AUTOCHECKBOX = 0x00000003;
    public const uint BS_OWNERDRAW = 0x0000000B;
    public const uint CBS_DROPDOWNLIST = 0x0003;
    public const uint CBS_HASSTRINGS = 0x0200;

    public const uint CB_ADDSTRING = 0x0143;
    public const uint CB_SETCURSEL = 0x014E;
    public const uint CB_GETCURSEL = 0x0147;
    public const uint BM_GETCHECK = 0x00F0;
    public const uint BM_SETCHECK = 0x00F1;
    public const nint BST_CHECKED = 1;

    public const int SW_HIDE = 0;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_SHOW = 5;

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public static readonly nint HWND_TOPMOST = -1;

    public const uint DT_CENTER = 0x0001;
    public const uint DT_VCENTER = 0x0004;
    public const uint DT_WORDBREAK = 0x0010;
    public const uint DT_SINGLELINE = 0x0020;
    public const uint DT_CALCRECT = 0x0400;
    public const uint DT_NOPREFIX = 0x0800;
    public const int TRANSPARENT = 1;
    public const int OPAQUE = 2;
    public const int FW_NORMAL = 400;
    public const int FW_SEMIBOLD = 600;

    public const uint MF_STRING = 0x0000;
    public const uint MF_SEPARATOR = 0x0800;
    public const uint TPM_NONOTIFY = 0x0080;
    public const uint TPM_RETURNCMD = 0x0100;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;

    public const int IDOK = 1;
    public const int IDCANCEL = 2;
    public const int ODS_SELECTED = 0x0001;

    public static readonly nint HWND_MESSAGE = -3;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaintStruct
    {
        public nint hdc;
        public int fErase;
        public Rect rcPaint;
        public int fRestore;
        public int fIncUpdate;
        public int r0, r1, r2, r3, r4, r5, r6, r7;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInfo
    {
        public Win32.Point ptReserved;
        public Win32.Point ptMaxSize;
        public Win32.Point ptMaxPosition;
        public Win32.Point ptMinTrackSize;
        public Win32.Point ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DrawItemStruct
    {
        public uint CtlType;
        public uint CtlID;
        public uint itemID;
        public uint itemAction;
        public uint itemState;
        public nint hwndItem;
        public nint hDC;
        public Rect rcItem;
        public nint itemData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CreateStruct
    {
        public nint lpCreateParams;
        public nint hInstance;
        public nint hMenu;
        public nint hwndParent;
        public int cy;
        public int cx;
        public int y;
        public int x;
        public int style;
        public nint lpszName;
        public nint lpszClass;
        public uint dwExStyle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WndClassEx
    {
        public int cbSize;
        public uint style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public nint hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Msg
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public Win32.Point pt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NotifyIconData
    {
        public int cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct InitCommonControls
    {
        public int dwSize;
        public uint dwICC;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern ushort RegisterClassEx(ref WndClassEx lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint CreateWindowEx(
        uint dwExStyle, string lpClassName, string? lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(nint hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(nint hWnd, out Rect rect);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(nint hWnd, out Rect rect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool EnableWindow(nint hWnd, bool enable);

    [DllImport("user32.dll")]
    public static extern nint SetFocus(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool SetWindowText(nint hWnd, string text);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    public static extern nint SendMessage(nint hWnd, uint msg, nint wParam, string? lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    public static extern nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    public static extern void PostQuitMessage(int exitCode);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out Msg msg, nint hWnd, uint filterMin, uint filterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref Msg msg);

    [DllImport("user32.dll")]
    public static extern nint DispatchMessage(ref Msg msg);

    [DllImport("user32.dll")]
    public static extern bool IsDialogMessage(nint hDlg, ref Msg msg);

    [DllImport("user32.dll")]
    public static extern nint BeginPaint(nint hWnd, out PaintStruct ps);

    [DllImport("user32.dll")]
    public static extern bool EndPaint(nint hWnd, ref PaintStruct ps);

    [DllImport("user32.dll")]
    public static extern nint GetDC(nint hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(nint hWnd, nint hdc);

    [DllImport("user32.dll")]
    public static extern int FillRect(nint hdc, ref Rect rect, nint brush);

    [DllImport("user32.dll")]
    public static extern int FrameRect(nint hdc, ref Rect rect, nint brush);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int DrawText(nint hdc, string text, int length, ref Rect rect, uint format);

    [DllImport("user32.dll")]
    public static extern bool InvalidateRect(nint hWnd, nint rect, bool erase);

    [DllImport("user32.dll")]
    public static extern nint SetTimer(nint hWnd, nint id, uint elapse, nint proc);

    [DllImport("user32.dll")]
    public static extern bool KillTimer(nint hWnd, nint id);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Win32.Point point);

    [DllImport("user32.dll")]
    public static extern nint MonitorFromPoint(Win32.Point pt, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [DllImport("user32.dll")]
    public static extern nint LoadCursor(nint instance, nint cursor);

    [DllImport("user32.dll")]
    public static extern nint SetCursor(nint cursor);

    [DllImport("user32.dll")]
    public static extern bool SetWindowRgn(nint hWnd, nint region, bool redraw);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern nint CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool AppendMenu(nint menu, uint flags, nuint idNewItem, string? text);

    [DllImport("user32.dll")]
    public static extern int TrackPopupMenu(nint menu, uint flags, int x, int y, int reserved, nint hwnd, nint rect);

    [DllImport("user32.dll")]
    public static extern bool DestroyMenu(nint menu);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(nint owner, string text, string caption, uint type);

    [DllImport("gdi32.dll")]
    public static extern nint CreateSolidBrush(int color);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    public static extern nint CreateFont(
        int height, int width, int escapement, int orientation, int weight,
        uint italic, uint underline, uint strikeOut,
        uint charSet, uint outPrecision, uint clipPrecision, uint quality, uint pitch, string face);

    [DllImport("gdi32.dll")]
    public static extern nint CreateRoundRectRgn(int left, int top, int right, int bottom, int widthEllipse, int heightEllipse);

    [DllImport("gdi32.dll")]
    public static extern nint SelectObject(nint hdc, nint obj);

    [DllImport("gdi32.dll")]
    public static extern int SetBkMode(nint hdc, int mode);

    [DllImport("gdi32.dll")]
    public static extern uint SetBkColor(nint hdc, int color);

    [DllImport("gdi32.dll")]
    public static extern uint SetTextColor(nint hdc, int color);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(nint hdc, int index);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(nint obj);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool Shell_NotifyIcon(uint message, ref NotifyIconData data);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern uint ExtractIconEx(string file, int index, out nint large, out nint small, uint count);

    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(nint icon);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

    [DllImport("comctl32.dll")]
    private static extern bool InitCommonControlsEx(ref InitCommonControls init);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(nint value);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }

    public static void InitializeProcess()
    {
        try
        {
            SetProcessDpiAwarenessContext(-4);
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось включить Per-Monitor DPI.", ex);
        }

        var init = new InitCommonControls
        {
            dwSize = Marshal.SizeOf<InitCommonControls>(),
            dwICC = 0x00004000 | 0x000000FF
        };
        InitCommonControlsEx(ref init);
    }

    public static int LowWord(nint value) => (int)(value.ToInt64() & 0xFFFF);
    public static int HighWord(nint value) => (int)((value.ToInt64() >> 16) & 0xFFFF);
    public static int SignedLow(nint value) => (short)(value.ToInt64() & 0xFFFF);
    public static int SignedHigh(nint value) => (short)((value.ToInt64() >> 16) & 0xFFFF);

    public static string GetText(nint hwnd)
    {
        var length = GetWindowTextLength(hwnd);
        if (length <= 0)
        {
            return "";
        }

        var buffer = new StringBuilder(length + 1);
        GetWindowText(hwnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    public static void SetText(nint hwnd, string? text) => SetWindowText(hwnd, text ?? "");

    public static nint CreateControl(string className, string? text, uint style, int x, int y, int width, int height, nint parent, int id, nint font)
    {
        var hwnd = CreateWindowEx(0, className, text, style | WS_CHILD | WS_VISIBLE, x, y, width, height, parent, id, Win32.GetModuleHandle(null), 0);
        if (hwnd != 0 && font != 0)
        {
            SendMessage(hwnd, WM_SETFONT, font, 0);
        }

        return hwnd;
    }

    public static void ComboAdd(nint combo, string item) => SendMessage(combo, CB_ADDSTRING, 0, item);

    public static void ComboSelect(nint combo, int index) => SendMessage(combo, CB_SETCURSEL, index, 0);

    public static int ComboSelection(nint combo) => (int)SendMessage(combo, CB_GETCURSEL, 0, 0);

    public static bool IsChecked(nint button) => SendMessage(button, BM_GETCHECK, 0, 0) == BST_CHECKED;

    public static void SetChecked(nint button, bool value) =>
        SendMessage(button, BM_SETCHECK, value ? BST_CHECKED : 0, 0);

    public static void Info(string text, string caption) => MessageBox(0, text, caption, 0x40);

    public static void Warn(nint owner, string text, string caption) => MessageBox(owner, text, caption, 0x30);

    public static void Error(string text, string caption) => MessageBox(0, text, caption, 0x10);

    public static void ApplyDarkTitle(nint hwnd)
    {
        var enabled = 1;
        if (DwmSetWindowAttribute(hwnd, 20, ref enabled, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(hwnd, 19, ref enabled, sizeof(int));
        }
    }

    public static void PlaceNearCursor(nint hwnd, int width, int height)
    {
        GetCursorPos(out var cursor);
        var monitor = MonitorFromPoint(cursor, 2);
        var info = new MonitorInfo { cbSize = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref info))
        {
            info.rcWork = new Rect { Left = 0, Top = 0, Right = 1920, Bottom = 1080 };
        }

        var gap = UiDpi.S(16);
        var x = cursor.X + gap;
        var y = cursor.Y + gap;
        if (x + width > info.rcWork.Right)
        {
            x = cursor.X - width - gap;
        }

        if (y + height > info.rcWork.Bottom)
        {
            y = cursor.Y - height - gap;
        }

        x = Math.Max(info.rcWork.Left, x);
        y = Math.Max(info.rcWork.Top, y);
        SetWindowPos(hwnd, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    public static void CenterOnWorkArea(nint hwnd, int width, int height)
    {
        GetCursorPos(out var cursor);
        var monitor = MonitorFromPoint(cursor, 2);
        var info = new MonitorInfo { cbSize = Marshal.SizeOf<MonitorInfo>() };
        GetMonitorInfo(monitor, ref info);
        var x = info.rcWork.Left + Math.Max(0, (info.rcWork.Width - width) / 2);
        var y = info.rcWork.Top + Math.Max(0, (info.rcWork.Height - height) / 2);
        SetWindowPos(hwnd, 0, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public static Rect MeasureText(nint hdc, string text, nint font, int maxWidth, uint format)
    {
        var rect = new Rect { Right = maxWidth };
        var previous = SelectObject(hdc, font);
        DrawText(hdc, text, -1, ref rect, format | DT_CALCRECT | DT_NOPREFIX);
        SelectObject(hdc, previous);
        return rect;
    }

    public static void RoundWindow(nint hwnd, int width, int height, int radius)
    {
        var region = CreateRoundRectRgn(0, 0, width + 1, height + 1, radius, radius);
        if (region != 0)
        {
            SetWindowRgn(hwnd, region, true);
        }
    }
}

internal static class UiDpi
{
    private static int _dpi;

    public static int Dpi
    {
        get
        {
            if (_dpi == 0)
            {
                var dc = UiNative.GetDC(0);
                _dpi = UiNative.GetDeviceCaps(dc, 88);
                UiNative.ReleaseDC(0, dc);
                if (_dpi < 96)
                {
                    _dpi = 96;
                }
            }

            return _dpi;
        }
    }

    public static int S(int px) => px * Dpi / 96;

    public static int FontHeight(int points) => -MulDiv(points, Dpi, 72);

    [DllImport("kernel32.dll")]
    private static extern int MulDiv(int number, int numerator, int denominator);
}
