namespace ERuDict;

internal sealed class MiniTooltipWindow : UiWindow
{
    private const int TimerHide = 1;
    private const int TimerEsc = 2;

    private readonly Settings _settings;
    private string _sourceText = "";
    private string _translation = "";
    private string? _direction;
    private bool _error;
    private bool _autoHide;
    private bool _dismissOnOutside;
    private long _ignoreClicksUntil;
    private UiNative.Rect _textRect;
    private UiNative.Rect _directionRect;
    private UiNative.Rect _buttonRect;

    public event Action<string, string, bool, string?>? ExpandRequested;

    public MiniTooltipWindow(Settings settings)
    {
        _settings = settings;
        Create(
            "",
            UiNative.WS_POPUP | UiNative.WS_CLIPCHILDREN,
            UiNative.WS_EX_TOPMOST | UiNative.WS_EX_TOOLWINDOW | UiNative.WS_EX_NOACTIVATE,
            0,
            0,
            UiDpi.S(280),
            UiDpi.S(72),
            0);
    }

    public void ShowTranslation(string sourceText, string? translation, bool isError, string? directionLabel, bool autoHide = false, bool dismissOnOutside = false)
    {
        _sourceText = sourceText;
        _translation = string.IsNullOrWhiteSpace(translation) ? "—" : translation;
        _direction = directionLabel;
        _error = isError;
        _autoHide = autoHide;
        _dismissOnOutside = dismissOnOutside;
        _ignoreClicksUntil = Environment.TickCount64 + 250;
        ArmOutsideClick();

        if (_settings.CopyToClipboard && !isError && !string.IsNullOrWhiteSpace(translation))
        {
            ClipboardManager.SetText(translation);
        }

        var size = Measure();
        UiNative.PlaceNearCursor(Hwnd, size.Width, size.Height);
        UiNative.RoundWindow(Hwnd, size.Width, size.Height, UiDpi.S(8));
        ShowWithoutActivation();
        UiNative.InvalidateRect(Hwnd, 0, true);
        UiNative.SetTimer(Hwnd, TimerEsc, 80, 0);
        UiNative.KillTimer(Hwnd, TimerHide);
        if (_autoHide && _settings.PopupTimeoutSec > 0)
        {
            UiNative.SetTimer(Hwnd, TimerHide, (uint)(_settings.PopupTimeoutSec * 1000), 0);
        }
    }

    public void HideTooltip()
    {
        UiNative.KillTimer(Hwnd, TimerHide);
        UiNative.KillTimer(Hwnd, TimerEsc);
        HideWindow();
    }

    protected override bool OnClose()
    {
        HideTooltip();
        return true;
    }

    protected override void OnTimer(nint id)
    {
        if (id == TimerHide || id == TimerEsc && Win32.IsKeyDown(Win32.VkEscape))
        {
            HideTooltip();
            return;
        }

        if (id == TimerEsc && _dismissOnOutside && OutsideClick())
        {
            HideTooltip();
        }
    }

    protected override void OnClick(int x, int y)
    {
        if (Environment.TickCount64 < _ignoreClicksUntil)
        {
            return;
        }

        if (_buttonRect.Contains(x, y))
        {
            HideTooltip();
            ExpandRequested?.Invoke(_sourceText, _translation == "—" ? "" : _translation, _error, _direction);
        }
    }

    protected override nint HandleMessage(uint message, nint wParam, nint lParam)
    {
        if (message == UiNative.WM_SETCURSOR && _buttonRect.Width > 0)
        {
            UiNative.GetCursorPos(out var screen);
            UiNative.GetWindowRect(Hwnd, out var window);
            var x = screen.X - window.Left;
            var y = screen.Y - window.Top;
            if (_buttonRect.Contains(x, y))
            {
                UiNative.SetCursor(UiNative.LoadCursor(0, 32649));
                return 1;
            }
        }

        return base.HandleMessage(message, wParam, lParam);
    }

    protected override void OnPaint(nint hdc)
    {
        UiNative.GetClientRect(Hwnd, out var client);
        var accent = new UiNative.Rect { Left = 0, Top = 0, Right = UiDpi.S(4), Bottom = client.Bottom };
        Fill(hdc, accent, UiTheme.AccentBrush);
        var border = client;
        UiNative.FrameRect(hdc, ref border, UiTheme.BorderBrush);
        DrawText(hdc, _translation, UiTheme.TranslationFont, _error ? UiTheme.Error : UiTheme.Text, _textRect, UiNative.DT_WORDBREAK);
        if (!string.IsNullOrWhiteSpace(_direction))
        {
            DrawText(hdc, _direction, UiTheme.SmallFont, UiTheme.Muted, _directionRect, UiNative.DT_SINGLELINE | UiNative.DT_VCENTER);
        }

        Fill(hdc, _buttonRect, UiTheme.SurfaceAltBrush);
        var buttonBorder = _buttonRect;
        UiNative.FrameRect(hdc, ref buttonBorder, UiTheme.BorderBrush);
        DrawText(hdc, I18n.Expand, UiTheme.SmallFont, UiTheme.Text, _buttonRect, UiNative.DT_CENTER | UiNative.DT_VCENTER | UiNative.DT_SINGLELINE);
    }

    private (int Width, int Height) Measure()
    {
        var dc = UiNative.GetDC(Hwnd);
        try
        {
            var text = UiNative.MeasureText(dc, _translation, UiTheme.TranslationFont, UiDpi.S(420), UiNative.DT_WORDBREAK);
            var textWidth = Math.Max(text.Width, UiDpi.S(40));
            var textHeight = Math.Max(text.Height, UiDpi.S(20));
            _textRect = new UiNative.Rect
            {
                Left = UiDpi.S(16),
                Top = UiDpi.S(12),
                Right = UiDpi.S(16) + textWidth,
                Bottom = UiDpi.S(12) + textHeight
            };

            var buttonWidth = UiDpi.S(118);
            var buttonHeight = UiDpi.S(26);
            var directionWidth = 0;
            if (!string.IsNullOrWhiteSpace(_direction))
            {
                directionWidth = UiNative.MeasureText(dc, _direction, UiTheme.SmallFont, UiDpi.S(240), UiNative.DT_SINGLELINE).Width;
            }

            var footerWidth = UiDpi.S(16) + directionWidth + UiDpi.S(12) + buttonWidth + UiDpi.S(16);
            var width = Math.Min(UiDpi.S(460), Math.Max(Math.Max(_textRect.Right + UiDpi.S(16), footerWidth), UiDpi.S(220)));
            var buttonTop = _textRect.Bottom + UiDpi.S(8);
            _buttonRect = new UiNative.Rect
            {
                Left = width - UiDpi.S(16) - buttonWidth,
                Top = buttonTop,
                Right = width - UiDpi.S(16),
                Bottom = buttonTop + buttonHeight
            };
            _directionRect = new UiNative.Rect
            {
                Left = UiDpi.S(16),
                Top = buttonTop,
                Right = _buttonRect.Left - UiDpi.S(8),
                Bottom = _buttonRect.Bottom
            };
            return (width, _buttonRect.Bottom + UiDpi.S(12));
        }
        finally
        {
            UiNative.ReleaseDC(Hwnd, dc);
        }
    }
}
