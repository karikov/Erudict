namespace ERuDict;

internal sealed class PopupWindow : UiWindow
{
    private const int TimerHide = 1;
    private const int TimerEsc = 2;
    private const int TimerCopy = 3;
    private const int CopyButtonId = 101;
    private const int CloseButtonId = 102;

    private readonly Settings _settings;
    private nint _sourceEdit;
    private nint _translationEdit;
    private nint _copyButton;
    private nint _closeButton;
    private bool _autoHide;
    private bool _dismissOnOutside;
    private string _translationCaption = "";
    private string _hint = "";
    private bool _error;
    private UiNative.Rect _sourceCaptionRect;
    private UiNative.Rect _translationCaptionRect;
    private UiNative.Rect _hintRect;

    public PopupWindow(Settings settings)
    {
        _settings = settings;
        Create(
            "",
            UiNative.WS_POPUP | UiNative.WS_CLIPCHILDREN,
            UiNative.WS_EX_TOPMOST | UiNative.WS_EX_TOOLWINDOW | UiNative.WS_EX_NOACTIVATE,
            0,
            0,
            UiDpi.S(900),
            UiDpi.S(440),
            0);
    }

    public void ShowTranslation(string sourceText, string? translation, bool isError = false, string? directionLabel = null, bool autoHide = false, bool dismissOnOutside = false)
    {
        _error = isError;
        _autoHide = autoHide;
        _dismissOnOutside = dismissOnOutside;
        ArmOutsideClick();
        _translationCaption = string.IsNullOrWhiteSpace(directionLabel)
            ? I18n.TranslationCaption
            : I18n.TranslationCaption + "  ·  " + directionLabel;
        _hint = dismissOnOutside ? I18n.PopupHintOutside : I18n.PopupHint;
        UiNative.SetText(_sourceEdit, Truncate(sourceText, 600));
        UiNative.SetText(_translationEdit, translation ?? "");
        UiNative.SetText(_copyButton, I18n.Copy);
        UiNative.SetText(_closeButton, I18n.Close);
        UiNative.EnableWindow(_copyButton, !isError && !string.IsNullOrWhiteSpace(translation));

        if (_settings.CopyToClipboard && !isError && !string.IsNullOrWhiteSpace(translation))
        {
            ClipboardManager.SetText(translation);
        }

        LayoutContent();
        UiNative.GetWindowRect(Hwnd, out var bounds);
        UiNative.PlaceNearCursor(Hwnd, bounds.Width, bounds.Height);
        UiNative.RoundWindow(Hwnd, bounds.Width, bounds.Height, UiDpi.S(8));
        ShowWithoutActivation();
        UiNative.InvalidateRect(Hwnd, 0, true);
        StartTimers();
    }

    public void HidePopup()
    {
        UiNative.KillTimer(Hwnd, TimerHide);
        UiNative.KillTimer(Hwnd, TimerEsc);
        UiNative.KillTimer(Hwnd, TimerCopy);
        HideWindow();
    }

    protected override void OnCreate()
    {
        const uint editStyle = UiNative.ES_MULTILINE | UiNative.ES_AUTOVSCROLL | UiNative.ES_READONLY | UiNative.WS_VSCROLL;
        _sourceEdit = UiNative.CreateControl("EDIT", "", editStyle, 0, 0, 10, 10, Hwnd, 111, UiTheme.UiFont);
        _translationEdit = UiNative.CreateControl("EDIT", "", editStyle, 0, 0, 10, 10, Hwnd, 112, UiTheme.TranslationFont);
        _closeButton = UiNative.CreateControl("BUTTON", I18n.Close, UiNative.BS_OWNERDRAW, 0, 0, 10, 10, Hwnd, CloseButtonId, UiTheme.SmallFont);
        _copyButton = UiNative.CreateControl("BUTTON", I18n.Copy, UiNative.BS_OWNERDRAW, 0, 0, 10, 10, Hwnd, CopyButtonId, UiTheme.SmallFont);
    }

    protected override void OnSize() => LayoutContent();

    protected override bool OnClose()
    {
        HidePopup();
        return true;
    }

    protected override void OnCommand(int id, int code, nint control)
    {
        if (id == CopyButtonId)
        {
            CopyTranslation();
        }
        else if (id == CloseButtonId)
        {
            HidePopup();
        }
    }

    protected override void OnTimer(nint id)
    {
        if (id == TimerHide || id == TimerEsc && Win32.IsKeyDown(Win32.VkEscape))
        {
            HidePopup();
            return;
        }

        if (id == TimerEsc && _dismissOnOutside && OutsideClick())
        {
            HidePopup();
            return;
        }

        if (id == TimerCopy)
        {
            UiNative.KillTimer(Hwnd, TimerCopy);
            UiNative.SetText(_copyButton, I18n.Copy);
            UiNative.InvalidateRect(_copyButton, 0, true);
        }
    }

    protected override void OnPaint(nint hdc)
    {
        UiNative.GetClientRect(Hwnd, out var client);
        var accent = new UiNative.Rect { Left = 0, Top = 0, Right = UiDpi.S(5), Bottom = client.Bottom };
        Fill(hdc, accent, UiTheme.AccentBrush);
        var border = client;
        UiNative.FrameRect(hdc, ref border, UiTheme.BorderBrush);
        DrawText(hdc, I18n.SourceCaption, UiTheme.CaptionFont, UiTheme.Muted, _sourceCaptionRect, 0);
        DrawText(hdc, _translationCaption, UiTheme.CaptionFont, UiTheme.Accent, _translationCaptionRect, 0);
        DrawText(hdc, _hint, UiTheme.SmallFont, UiTheme.Muted, _hintRect, UiNative.DT_VCENTER | UiNative.DT_SINGLELINE);
    }

    protected override nint OnColor(uint message, nint hdc, nint child)
    {
        if (child == _sourceEdit)
        {
            return ColorEdit(hdc, UiTheme.Muted);
        }

        if (child == _translationEdit)
        {
            return ColorEdit(hdc, _error ? UiTheme.Error : UiTheme.Text);
        }

        return 0;
    }

    protected override void OnDrawItem(UiNative.DrawItemStruct item)
    {
        if (item.hwndItem != _copyButton && item.hwndItem != _closeButton)
        {
            return;
        }

        var accent = item.hwndItem == _copyButton;
        var pressed = (item.itemState & UiNative.ODS_SELECTED) != 0;
        var brush = accent
            ? (pressed ? UiTheme.AccentHoverBrush : UiTheme.AccentBrush)
            : (pressed ? UiTheme.BorderBrush : UiTheme.SurfaceAltBrush);
        Fill(item.hDC, item.rcItem, brush);
        if (!accent)
        {
            var border = item.rcItem;
            UiNative.FrameRect(item.hDC, ref border, UiTheme.BorderBrush);
        }

        var color = accent ? UiTheme.White : UiTheme.Text;
        DrawText(item.hDC, UiNative.GetText(item.hwndItem), UiTheme.SmallFont, color, item.rcItem,
            UiNative.DT_CENTER | UiNative.DT_VCENTER | UiNative.DT_SINGLELINE);
    }

    private void LayoutContent()
    {
        if (_sourceEdit == 0)
        {
            return;
        }

        UiNative.GetClientRect(Hwnd, out var client);
        var left = UiDpi.S(24);
        var inner = Math.Max(UiDpi.S(80), client.Width - left - UiDpi.S(20));
        var y = UiDpi.S(16);
        _sourceCaptionRect = Line(left, y, inner, UiDpi.S(18));
        y = _sourceCaptionRect.Bottom + UiDpi.S(6);

        var footer = UiDpi.S(36);
        var fixedHeight = UiDpi.S(16) + _sourceCaptionRect.Height + UiDpi.S(6) + UiDpi.S(12)
            + UiDpi.S(18) + UiDpi.S(6) + UiDpi.S(12) + footer + UiDpi.S(12);
        var boxes = Math.Max(UiDpi.S(140), client.Height - fixedHeight);
        var sourceHeight = Math.Max(UiDpi.S(72), (int)(boxes * 0.38));
        var translationHeight = Math.Max(UiDpi.S(88), boxes - sourceHeight);

        UiNative.MoveWindow(_sourceEdit, left, y, inner, sourceHeight, true);
        y += sourceHeight + UiDpi.S(12);
        _translationCaptionRect = Line(left, y, inner, UiDpi.S(18));
        y = _translationCaptionRect.Bottom + UiDpi.S(6);
        UiNative.MoveWindow(_translationEdit, left, y, inner, translationHeight, true);
        y += translationHeight + UiDpi.S(12);

        var buttonWidth = UiDpi.S(120);
        var buttonHeight = UiDpi.S(28);
        var gap = UiDpi.S(8);
        var copyX = left + inner - buttonWidth;
        var closeX = copyX - gap - buttonWidth;
        UiNative.MoveWindow(_closeButton, closeX, y, buttonWidth, buttonHeight, true);
        UiNative.MoveWindow(_copyButton, copyX, y, buttonWidth, buttonHeight, true);
        _hintRect = Line(left, y, Math.Max(UiDpi.S(40), closeX - left - gap), buttonHeight);
        UiNative.RoundWindow(Hwnd, client.Width, client.Height, UiDpi.S(8));
    }

    private void StartTimers()
    {
        UiNative.SetTimer(Hwnd, TimerEsc, 80, 0);
        UiNative.KillTimer(Hwnd, TimerHide);
        if (_autoHide && _settings.PopupTimeoutSec > 0)
        {
            UiNative.SetTimer(Hwnd, TimerHide, (uint)(_settings.PopupTimeoutSec * 1000), 0);
        }
    }

    private void CopyTranslation()
    {
        var text = UiNative.GetText(_translationEdit);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        ClipboardManager.SetText(text);
        UiNative.SetText(_copyButton, I18n.Copied);
        UiNative.InvalidateRect(_copyButton, 0, true);
        UiNative.KillTimer(Hwnd, TimerHide);
        UiNative.SetTimer(Hwnd, TimerCopy, 900, 0);
    }

    private static UiNative.Rect Line(int x, int y, int width, int height) =>
        new() { Left = x, Top = y, Right = x + width, Bottom = y + height };

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text ?? "";
        }

        return text[..maxLength] + "…";
    }
}
