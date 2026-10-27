namespace ERuDict;

internal sealed class SettingsDialog : UiWindow
{
    private static readonly string[] Languages =
    {
        "ru — Русский", "en — English", "de — Deutsch", "fr — Français",
        "es — Español", "it — Italiano", "pt — Português", "pl — Polski",
        "uk — Українська", "tr — Türkçe", "zh — 中文", "ja — 日本語", "ko — 한국어"
    };

    private readonly Settings _settings;
    private nint _uiLanguage;
    private nint _language;
    private nint _translator;
    private nint _interval;
    private nint _timeout;
    private nint _hotkey;
    private nint _copy;
    private nint _autoDirection;
    private nint _autostart;

    public SettingsDialog(Settings settings, nint owner)
    {
        _settings = settings;
        var width = UiDpi.S(540);
        var height = UiDpi.S(500);
        Create(
            I18n.SettingsTitle,
            UiNative.WS_CAPTION | UiNative.WS_SYSMENU | UiNative.WS_CLIPCHILDREN,
            UiNative.WS_EX_DLGMODALFRAME,
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
        UiNative.GetClientRect(Hwnd, out var client);
        var labelX = UiDpi.S(18);
        var controlX = UiDpi.S(250);
        var labelW = UiDpi.S(220);
        var controlW = Math.Max(UiDpi.S(160), client.Width - controlX - UiDpi.S(18));
        var row = UiDpi.S(40);
        var y = UiDpi.S(16);
        var font = UiTheme.UiFont;

        AddLabel(I18n.LabelUiLanguage, labelX, y, labelW, font);
        _uiLanguage = Combo(controlX, y, controlW, font);
        UiNative.ComboAdd(_uiLanguage, I18n.UiLangSystem);
        UiNative.ComboAdd(_uiLanguage, I18n.UiLangRussian);
        UiNative.ComboAdd(_uiLanguage, I18n.UiLangEnglish);
        UiNative.ComboSelect(_uiLanguage, _settings.UiLanguage switch
        {
            "ru" => 1,
            "en" => 2,
            _ => 0
        });

        y += row;
        AddLabel(I18n.LabelTargetLanguage, labelX, y, labelW, font);
        _language = Combo(controlX, y, controlW, font);
        foreach (var language in Languages)
        {
            UiNative.ComboAdd(_language, language);
        }

        UiNative.ComboSelect(_language, FindLanguageIndex(_settings.TargetLanguage));

        y += row;
        AddLabel(I18n.LabelTranslator, labelX, y, labelW, font);
        _translator = Combo(controlX, y, controlW, font);
        UiNative.ComboAdd(_translator, I18n.TranslatorAuto);
        UiNative.ComboAdd(_translator, I18n.TranslatorYandex);
        UiNative.ComboAdd(_translator, I18n.TranslatorGoogle);
        UiNative.ComboSelect(_translator, _settings.Translator switch
        {
            "yandex" => 1,
            "google" => 2,
            _ => 0
        });

        y += row;
        AddLabel(I18n.LabelInterval, labelX, y, labelW, font);
        _interval = Edit(controlX, y, UiDpi.S(120), _settings.DoublePressIntervalMs.ToString(), font);

        y += row;
        AddLabel(I18n.LabelTimeout, labelX, y, labelW, font);
        _timeout = Edit(controlX, y, UiDpi.S(120), _settings.PopupTimeoutSec.ToString(), font);

        y += row;
        AddLabel(I18n.LabelHotkey, labelX, y, labelW, font);
        _hotkey = Combo(controlX, y, controlW, font);
        UiNative.ComboAdd(_hotkey, I18n.HotkeyDoubleCtrl);
        UiNative.ComboAdd(_hotkey, I18n.HotkeyCtrlShift);
        UiNative.ComboSelect(_hotkey, _settings.HotkeyMode == "ctrlShift" ? 1 : 0);

        y += row + UiDpi.S(6);
        _copy = Check(I18n.CheckCopy, labelX, y, client.Width - UiDpi.S(36), _settings.CopyToClipboard, font);
        y += row;
        _autoDirection = Check(I18n.CheckAutoDirection, labelX, y, client.Width - UiDpi.S(36), _settings.AutoDetectDirection, font);
        y += row;
        _autostart = Check(I18n.CheckAutostart, labelX, y, client.Width - UiDpi.S(36), _settings.StartWithWindows, font);

        var buttonY = client.Height - UiDpi.S(52);
        var buttonW = UiDpi.S(128);
        UiNative.CreateControl("BUTTON", I18n.Cancel, UiNative.BS_PUSHBUTTON | UiNative.WS_TABSTOP,
            client.Width - UiDpi.S(18) - buttonW - UiDpi.S(8) - UiDpi.S(110), buttonY, UiDpi.S(110), UiDpi.S(32),
            Hwnd, UiNative.IDCANCEL, font);
        UiNative.CreateControl("BUTTON", I18n.Save, UiNative.BS_DEFPUSHBUTTON | UiNative.WS_TABSTOP,
            client.Width - UiDpi.S(18) - buttonW, buttonY, buttonW, UiDpi.S(32),
            Hwnd, UiNative.IDOK, font);
    }

    protected override void OnCommand(int id, int code, nint control)
    {
        if (id == UiNative.IDOK)
        {
            SaveAndClose();
        }
        else if (id == UiNative.IDCANCEL)
        {
            CloseModal();
        }
    }

    protected override nint OnColor(uint message, nint hdc, nint child)
    {
        if (message == UiNative.WM_CTLCOLORSTATIC)
        {
            return ColorStatic(hdc, UiTheme.Text);
        }

        if (message is UiNative.WM_CTLCOLOREDIT or UiNative.WM_CTLCOLORLISTBOX)
        {
            return ColorEdit(hdc, UiTheme.Text);
        }

        return 0;
    }

    private void SaveAndClose()
    {
        try
        {
            var languageIndex = UiNative.ComboSelection(_language);
            if (languageIndex >= 0 && languageIndex < Languages.Length)
            {
                _settings.TargetLanguage = GetLanguageCode(Languages[languageIndex]);
            }

            _settings.UiLanguage = UiNative.ComboSelection(_uiLanguage) switch
            {
                1 => "ru",
                2 => "en",
                _ => "auto"
            };
            _settings.DoublePressIntervalMs = ReadInt(_interval, _settings.DoublePressIntervalMs);
            _settings.PopupTimeoutSec = ReadInt(_timeout, _settings.PopupTimeoutSec);
            _settings.CopyToClipboard = UiNative.IsChecked(_copy);
            _settings.StartWithWindows = UiNative.IsChecked(_autostart);
            _settings.AutoDetectDirection = UiNative.IsChecked(_autoDirection);
            _settings.HotkeyMode = UiNative.ComboSelection(_hotkey) == 1 ? "ctrlShift" : "doubleCtrl";
            _settings.Translator = UiNative.ComboSelection(_translator) switch
            {
                1 => "yandex",
                2 => "google",
                _ => "auto"
            };
            _settings.Save();
            I18n.ApplyPreference(_settings.UiLanguage);
            CloseModal();
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to save settings from the dialog.", ex);
            UiNative.Warn(Hwnd, I18n.SettingsSaveFailed, I18n.ErrorTitle);
        }
    }

    private nint Combo(int x, int y, int width, nint font) =>
        UiNative.CreateControl(
            "COMBOBOX",
            "",
            UiNative.CBS_DROPDOWNLIST | UiNative.CBS_HASSTRINGS | UiNative.WS_VSCROLL | UiNative.WS_TABSTOP,
            x,
            y,
            width,
            UiDpi.S(240),
            Hwnd,
            200,
            font);

    private nint Edit(int x, int y, int width, string value, nint font)
    {
        var hwnd = UiNative.CreateControl("EDIT", value, UiNative.ES_NUMBER | UiNative.WS_TABSTOP, x, y, width, UiDpi.S(24), Hwnd, 201, font);
        return hwnd;
    }

    private nint Check(string text, int x, int y, int width, bool value, nint font)
    {
        var hwnd = UiNative.CreateControl("BUTTON", text, UiNative.BS_AUTOCHECKBOX | UiNative.WS_TABSTOP, x, y, width, UiDpi.S(24), Hwnd, 202, font);
        UiNative.SetChecked(hwnd, value);
        return hwnd;
    }

    private void AddLabel(string text, int x, int y, int width, nint font) =>
        UiNative.CreateControl("STATIC", text, UiNative.SS_NOPREFIX | UiNative.SS_CENTERIMAGE, x, y, width, UiDpi.S(24), Hwnd, 203, font);

    private static int ReadInt(nint edit, int fallback) =>
        int.TryParse(UiNative.GetText(edit), out var value) ? value : fallback;

    private static int FindLanguageIndex(string code)
    {
        var prefix = code + " —";
        for (var i = 0; i < Languages.Length; i++)
        {
            if (Languages[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 0;
    }

    private static string GetLanguageCode(string item)
    {
        var space = item.IndexOf(' ');
        return space > 0 ? item[..space] : item;
    }
}
