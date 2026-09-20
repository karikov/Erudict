namespace ERuDict;

internal sealed class TrayApp : ApplicationContext
{
    private readonly Settings _settings;
    private readonly NotifyIcon _trayIcon;
    private readonly HotkeyManager _hotkeyManager;
    private readonly PopupWindow _popup;
    private readonly MiniTooltipWindow _miniTooltip;
    private bool _translating;
    private bool _cleaned;

    public TrayApp(Settings settings)
    {
        _settings = settings;
        _popup = new PopupWindow(settings);
        _miniTooltip = new MiniTooltipWindow(settings);
        _miniTooltip.ExpandRequested += (source, translation, isError, direction) =>
            SafeShowPopup(source, translation, isError, direction, forceFull: true);

        _trayIcon = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => TranslateSelected();

        I18n.Changed += ApplyUiLanguage;
        ApplyUiLanguage();

        _hotkeyManager = new HotkeyManager(settings, TranslateSelected);
        _hotkeyManager.Start();
    }

    private async void TranslateSelected()
    {
        if (_translating)
        {
            return;
        }

        _translating = true;
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
                SafeShowPopup(text, outcome.Text, isError: false, label, forceFull: outcome.Engine == "offline");
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
            _translating = false;
        }
    }

    private void SafeShowPopup(string source, string? message, bool isError, string? directionLabel = null, bool forceFull = false)
    {
        try
        {
            var useMini = !forceFull && TextMetrics.PreferMiniTooltip(source);
            if (useMini)
            {
                _popup.HidePopup();
                _miniTooltip.ShowTranslation(source, message, isError, directionLabel);
            }
            else
            {
                _miniTooltip.HideTooltip();
                _popup.ShowTranslation(source, message, isError, directionLabel);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось показать окно перевода.", ex);
        }
    }

    private void OpenHelp()
    {
        using var dialog = new HelpDialog();
        dialog.ShowDialog();
    }

    private void ApplyUiLanguage()
    {
        if (_trayIcon is not null)
        {
            _trayIcon.Text = I18n.TrayTooltip;
            RebuildMenu();
        }
    }

    private void RebuildMenu()
    {
        var menu = new ContextMenuStrip
        {
            Renderer = new DarkMenuRenderer(),
            BackColor = Theme.Background,
            ForeColor = Theme.Text,
            Font = Theme.Ui,
            ShowImageMargin = false
        };
        menu.Items.Add(CreateMenuItem(I18n.MenuTranslate, TranslateSelected));
        menu.Items.Add(CreateMenuItem(I18n.MenuSettings, OpenSettings));
        menu.Items.Add(CreateMenuItem(I18n.MenuHelp, OpenHelp));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CreateMenuItem(I18n.MenuExit, ExitApp));
        var previous = _trayIcon.ContextMenuStrip;
        _trayIcon.ContextMenuStrip = menu;
        previous?.Dispose();
    }

    private void OpenSettings()
    {
        using var dialog = new SettingsDialog(_settings);
        dialog.ShowDialog();
    }

    private void ExitApp()
    {
        Cleanup();
        ExitThread();
    }

    protected override void ExitThreadCore()
    {
        Cleanup();
        base.ExitThreadCore();
    }

    private void Cleanup()
    {
        if (_cleaned)
        {
            return;
        }

        _cleaned = true;
        I18n.Changed -= ApplyUiLanguage;
        _hotkeyManager.Dispose();
        _miniTooltip.Dispose();
        _popup.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
    }

    private static ToolStripMenuItem CreateMenuItem(string text, Action action)
    {
        var item = new ToolStripMenuItem(text)
        {
            ForeColor = Theme.Text,
            BackColor = Theme.Background
        };
        item.Click += (_, _) => action();
        return item;
    }

    private static Icon LoadAppIcon()
    {
        var path = Environment.ProcessPath ?? Application.ExecutablePath;
        return Icon.ExtractAssociatedIcon(path) ?? SystemIcons.Application;
    }
}
