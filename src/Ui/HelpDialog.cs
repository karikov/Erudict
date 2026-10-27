using System.Runtime.InteropServices;

namespace ERuDict;

internal sealed class HelpDialog : UiWindow
{
    private nint _text;
    private nint _close;

    public HelpDialog(nint owner)
    {
        var width = UiDpi.S(560);
        var height = UiDpi.S(520);
        Create(
            I18n.HelpTitle,
            UiNative.WS_OVERLAPPEDWINDOW | UiNative.WS_CLIPCHILDREN,
            0,
            0,
            0,
            width,
            height,
            owner);
        UiNative.ApplyDarkTitle(Hwnd);
        UiNative.CenterOnWorkArea(Hwnd, width, height);
    }

    public void ShowDialog() => ShowModal();

    protected override bool OnClose()
    {
        CloseModal();
        return true;
    }

    protected override void OnCreate()
    {
        const uint style = UiNative.ES_MULTILINE | UiNative.ES_AUTOVSCROLL | UiNative.ES_READONLY | UiNative.WS_VSCROLL | UiNative.WS_TABSTOP;
        _text = UiNative.CreateControl("EDIT", "", style, 0, 0, 10, 10, Hwnd, 301, UiTheme.UiFont);
        _close = UiNative.CreateControl("BUTTON", I18n.Close, UiNative.BS_DEFPUSHBUTTON | UiNative.WS_TABSTOP, 0, 0, 10, 10, Hwnd, UiNative.IDCANCEL, UiTheme.UiFont);
        UiNative.SetText(_text, I18n.HelpBody.Replace("\n", "\r\n"));
        Layout();
    }

    protected override void OnSize() => Layout();

    protected override void OnCommand(int id, int code, nint control)
    {
        if (id is UiNative.IDCANCEL or UiNative.IDOK)
        {
            CloseModal();
        }
    }

    protected override nint HandleMessage(uint message, nint wParam, nint lParam)
    {
        if (message == UiNative.WM_GETMINMAXINFO)
        {
            var info = Marshal.PtrToStructure<UiNative.MinMaxInfo>(lParam);
            info.ptMinTrackSize = new Win32.Point { X = UiDpi.S(480), Y = UiDpi.S(360) };
            Marshal.StructureToPtr(info, lParam, false);
            return 0;
        }

        return base.HandleMessage(message, wParam, lParam);
    }

    protected override nint OnColor(uint message, nint hdc, nint child)
    {
        if (child == _text)
        {
            return ColorEdit(hdc, UiTheme.Text);
        }

        return ColorStatic(hdc, UiTheme.Text);
    }

    private void Layout()
    {
        if (_text == 0)
        {
            return;
        }

        UiNative.GetClientRect(Hwnd, out var client);
        var pad = UiDpi.S(16);
        var buttonRow = UiDpi.S(52);
        UiNative.MoveWindow(_text, pad, pad, Math.Max(10, client.Width - pad * 2), Math.Max(10, client.Height - buttonRow - pad), true);
        UiNative.MoveWindow(_close, client.Width - pad - UiDpi.S(120), client.Height - UiDpi.S(42), UiDpi.S(120), UiDpi.S(32), true);
    }
}
