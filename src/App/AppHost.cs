namespace ERuDict;

internal sealed class AppHost : UiWindow
{
    private const uint TrayId = 1;
    private const int MenuTranslate = 1;
    private const int MenuSettings = 2;
    private const int MenuHelp = 3;
    private const int MenuExit = 4;

    private readonly Settings _settings;
    private readonly PopupWindow _popup;
    private readonly MiniTooltipWindow _miniTooltip;
    private HotkeyManager? _hotkeys;
    private bool _translating;
    private bool _cleaned;
    private nint _trayIcon;

    public AppHost(Settings settings)
    {
        _settings = settings;
        Create(
            "ERuDict",
            UiNative.WS_POPUP,
            UiNative.WS_EX_TOOLWINDOW | UiNative.WS_EX_NOACTIVATE,
            -32000,
            -32000,
            0,
            0,
            0);
        HideWindow();
        _popup = new PopupWindow(settings);
        _miniTooltip = new MiniTooltipWindow(settings);
        _miniTooltip.ExpandRequested += (source, translation, isError, direction) =>
            SafeShowPopup(source, translation, isError, direction, forceFull: true);
    }

    public void Run()
    {
        SynchronizationContext.SetSynchronizationContext(new UiSyncContext(Hwnd));
        _hotkeys = new HotkeyManager(_settings, TranslateSelected);
        _hotkeys.Start();
        AddTray();
        I18n.Changed += UpdateTrayTip;

        while (UiNative.GetMessage(out var message, 0, 0, 0) > 0)
        {
            UiNative.TranslateMessage(ref message);
            UiNative.DispatchMessage(ref message);
        }
    }

    protected override nint HandleMessage(uint message, nint wParam, nint lParam)
    {
        if (message == UiNative.WM_EXECUTE)
        {
            UiSyncContext.Execute(lParam);
            return 0;
        }

        if (message == UiNative.WM_TRAY)
        {
            OnTray((uint)lParam.ToInt32());
            return 0;
        }

        return base.HandleMessage(message, wParam, lParam);
    }

    protected override bool OnClose()
    {
        Cleanup();
        return false;
    }

    protected override void OnDestroy() => UiNative.PostQuitMessage(0);

    private void OnTray(uint mouse)
    {
        if (mouse == UiNative.WM_LBUTTONDBLCLK)
        {
            TranslateSelected();
            return;
        }

        if (mouse == UiNative.WM_RBUTTONUP)
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        var menu = UiNative.CreatePopupMenu();
        UiNative.AppendMenu(menu, UiNative.MF_STRING, MenuTranslate, I18n.MenuTranslate);
        UiNative.AppendMenu(menu, UiNative.MF_STRING, MenuSettings, I18n.MenuSettings);
        UiNative.AppendMenu(menu, UiNative.MF_STRING, MenuHelp, I18n.MenuHelp);
        UiNative.AppendMenu(menu, UiNative.MF_SEPARATOR, 0, null);
        UiNative.AppendMenu(menu, UiNative.MF_STRING, MenuExit, I18n.MenuExit);
        UiNative.GetCursorPos(out var cursor);
        UiNative.SetForegroundWindow(Hwnd);
        var command = UiNative.TrackPopupMenu(
            menu,
            UiNative.TPM_RETURNCMD | UiNative.TPM_NONOTIFY,
            cursor.X,
            cursor.Y,
            0,
            Hwnd,
            0);
        UiNative.PostMessage(Hwnd, UiNative.WM_NULL, 0, 0);
        UiNative.DestroyMenu(menu);

        switch (command)
        {
            case MenuTranslate:
                TranslateSelected();
                break;
            case MenuSettings:
                using (var dialog = new SettingsDialog(_settings, Hwnd))
                {
                    dialog.ShowDialog();
                }

                break;
            case MenuHelp:
                using (var dialog = new HelpDialog(Hwnd))
                {
                    dialog.ShowDialog();
                }

                break;
            case MenuExit:
                UiNative.PostMessage(Hwnd, UiNative.WM_CLOSE, 0, 0);
                break;
        }
    }

    private async void TranslateSelected()
    {
        if (_translating)
        {
            return;
        }

        _translating = true;
        _hotkeys?.Suspend();
        try
        {
            var text = await ClipboardManager.GetSelectedTextAsync();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            try
            {
                var direction = LanguageDetector.Resolve(text, _settings.TargetLanguage, _settings.AutoDetectDirection);
                var outcome = await Translator.TranslateAsync(
                    text,
                    direction.SourceLanguage,
                    direction.TargetLanguage,
                    _settings.Translator);
                var label = direction.Label + " · " + outcome.EngineLabel;
                var offline = outcome.Engine == "offline";
                SafeShowPopup(text, outcome.Text, isError: false, label, forceFull: offline, autoHide: offline);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Ошибка перевода.", ex);
                SafeShowPopup(text, I18n.TranslateFailed + ex.Message, isError: true);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to capture selected text.", ex);
            SafeShowPopup("", I18n.GenericError + ex.Message, isError: true);
        }
        finally
        {
            _hotkeys?.Resume();
            _translating = false;
        }
    }

    private void SafeShowPopup(string source, string? message, bool isError, string? directionLabel = null, bool forceFull = false, bool autoHide = false)
    {
        try
        {
            _popup.HidePopup();
            _miniTooltip.HideTooltip();
            var useMini = !forceFull && TextMetrics.PreferMiniTooltip(source);
            if (useMini)
            {
                _miniTooltip.ShowTranslation(source, message, isError, directionLabel, autoHide: !isError, dismissOnOutside: true);
            }
            else
            {
                _popup.ShowTranslation(source, message, isError, directionLabel, autoHide, dismissOnOutside: autoHide);
            }

            HideWindow();
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось показать окно перевода.", ex);
        }
    }

    private void AddTray()
    {
        _trayIcon = UiTheme.SmallIcon != 0 ? UiTheme.SmallIcon : UiTheme.LargeIcon;
        var data = CreateTrayData();
        if (!UiNative.Shell_NotifyIcon(UiNative.NIM_ADD, ref data))
        {
            AppLogger.Error("Не удалось добавить иконку в трей.");
        }
    }

    private void UpdateTrayTip()
    {
        var data = CreateTrayData();
        UiNative.Shell_NotifyIcon(UiNative.NIM_MODIFY, ref data);
    }

    private UiNative.NotifyIconData CreateTrayData()
    {
        var tip = I18n.TrayTooltip;
        if (tip.Length > 127)
        {
            tip = tip[..127];
        }

        return new UiNative.NotifyIconData
        {
            cbSize = System.Runtime.InteropServices.Marshal.SizeOf<UiNative.NotifyIconData>(),
            hWnd = Hwnd,
            uID = TrayId,
            uFlags = UiNative.NIF_MESSAGE | UiNative.NIF_ICON | UiNative.NIF_TIP,
            uCallbackMessage = UiNative.WM_TRAY,
            hIcon = _trayIcon,
            szTip = tip
        };
    }

    private void RemoveTray()
    {
        var data = CreateTrayData();
        UiNative.Shell_NotifyIcon(UiNative.NIM_DELETE, ref data);
    }

    private void Cleanup()
    {
        if (_cleaned)
        {
            return;
        }

        _cleaned = true;
        I18n.Changed -= UpdateTrayTip;
        _hotkeys?.Dispose();
        RemoveTray();
        _miniTooltip.Dispose();
        _popup.Dispose();
    }
}
