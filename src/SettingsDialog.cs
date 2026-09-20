namespace ERuDict;

internal sealed class SettingsDialog : Form
{
    private static readonly string[] Languages =
    {
        "ru — Русский", "en — English", "de — Deutsch", "fr — Français",
        "es — Español", "it — Italiano", "pt — Português", "pl — Polski",
        "uk — Українська", "tr — Türkçe", "zh — 中文", "ja — 日本語", "ko — 한국어"
    };

    private readonly Settings _settings;
    private readonly ComboBox _uiLanguageCombo;
    private readonly ComboBox _languageCombo;
    private readonly NumericUpDown _intervalBox;
    private readonly NumericUpDown _timeoutBox;
    private readonly ComboBox _hotkeyCombo;
    private readonly ComboBox _translatorCombo;
    private readonly CheckBox _copyCheck;
    private readonly CheckBox _autostartCheck;
    private readonly CheckBox _autoDirectionCheck;

    public SettingsDialog(Settings settings)
    {
        _settings = settings;
        Text = I18n.SettingsTitle;
        ClientSize = new Size(500, 500);
        Theme.ApplyDialog(this);
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? Application.ExecutablePath);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 11,
            Padding = new Padding(18, 16, 18, 12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        for (var i = 0; i < 9; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        }

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _uiLanguageCombo = new ComboBox();
        Theme.StyleCombo(_uiLanguageCombo);
        _uiLanguageCombo.Items.AddRange(new object[] { I18n.UiLangSystem, I18n.UiLangRussian, I18n.UiLangEnglish });
        _uiLanguageCombo.SelectedIndex = settings.UiLanguage switch
        {
            "ru" => 1,
            "en" => 2,
            _ => 0
        };
        _uiLanguageCombo.Dock = DockStyle.Fill;

        _languageCombo = new ComboBox();
        Theme.StyleCombo(_languageCombo);
        _languageCombo.Items.AddRange(Languages);
        _languageCombo.SelectedItem = FindLanguageItem(settings.TargetLanguage);
        _languageCombo.Dock = DockStyle.Fill;

        _intervalBox = new NumericUpDown
        {
            Minimum = 150,
            Maximum = 2000,
            Increment = 50,
            Value = settings.DoublePressIntervalMs,
            Dock = DockStyle.Fill
        };
        Theme.StyleNumeric(_intervalBox);

        _timeoutBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 120,
            Value = settings.PopupTimeoutSec,
            Dock = DockStyle.Fill
        };
        Theme.StyleNumeric(_timeoutBox);

        _hotkeyCombo = new ComboBox();
        Theme.StyleCombo(_hotkeyCombo);
        _hotkeyCombo.Items.AddRange(new object[] { I18n.HotkeyDoubleCtrl, I18n.HotkeyCtrlShift });
        _hotkeyCombo.SelectedIndex = settings.HotkeyMode == "ctrlShift" ? 1 : 0;
        _hotkeyCombo.Dock = DockStyle.Fill;

        _translatorCombo = new ComboBox();
        Theme.StyleCombo(_translatorCombo);
        _translatorCombo.Items.AddRange(new object[]
        {
            I18n.TranslatorAuto,
            I18n.TranslatorYandex,
            I18n.TranslatorGoogle
        });
        _translatorCombo.SelectedIndex = settings.Translator switch
        {
            "yandex" => 1,
            "google" => 2,
            _ => 0
        };
        _translatorCombo.Dock = DockStyle.Fill;

        _copyCheck = new CheckBox { Text = I18n.CheckCopy, Checked = settings.CopyToClipboard };
        Theme.StyleCheckBox(_copyCheck);

        _autostartCheck = new CheckBox { Text = I18n.CheckAutostart, Checked = settings.StartWithWindows };
        Theme.StyleCheckBox(_autostartCheck);

        _autoDirectionCheck = new CheckBox { Text = I18n.CheckAutoDirection, Checked = settings.AutoDetectDirection };
        Theme.StyleCheckBox(_autoDirectionCheck);

        layout.Controls.Add(CreateLabel(I18n.LabelUiLanguage), 0, 0);
        layout.Controls.Add(_uiLanguageCombo, 1, 0);
        layout.Controls.Add(CreateLabel(I18n.LabelTargetLanguage), 0, 1);
        layout.Controls.Add(_languageCombo, 1, 1);
        layout.Controls.Add(CreateLabel(I18n.LabelTranslator), 0, 2);
        layout.Controls.Add(_translatorCombo, 1, 2);
        layout.Controls.Add(CreateLabel(I18n.LabelInterval), 0, 3);
        layout.Controls.Add(_intervalBox, 1, 3);
        layout.Controls.Add(CreateLabel(I18n.LabelTimeout), 0, 4);
        layout.Controls.Add(_timeoutBox, 1, 4);
        layout.Controls.Add(CreateLabel(I18n.LabelHotkey), 0, 5);
        layout.Controls.Add(_hotkeyCombo, 1, 5);

        layout.SetColumnSpan(_copyCheck, 2);
        layout.Controls.Add(_copyCheck, 0, 6);
        layout.SetColumnSpan(_autoDirectionCheck, 2);
        layout.Controls.Add(_autoDirectionCheck, 0, 7);
        layout.SetColumnSpan(_autostartCheck, 2);
        layout.Controls.Add(_autostartCheck, 0, 8);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        var saveButton = Theme.CreateButton(I18n.Save, accent: true);
        saveButton.Width = 128;
        saveButton.Click += (_, _) => SaveAndClose();

        var cancelButton = Theme.CreateButton(I18n.Cancel, accent: false);
        cancelButton.Width = 110;
        cancelButton.Click += (_, _) => Close();

        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(buttons, 0, 10);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static Label CreateLabel(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            Font = Theme.Ui,
            ForeColor = Theme.Text,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

    private void SaveAndClose()
    {
        try
        {
            if (_languageCombo.SelectedItem is string selected)
            {
                _settings.TargetLanguage = GetLanguageCode(selected);
            }

            _settings.UiLanguage = _uiLanguageCombo.SelectedIndex switch
            {
                1 => "ru",
                2 => "en",
                _ => "auto"
            };
            _settings.DoublePressIntervalMs = (int)_intervalBox.Value;
            _settings.PopupTimeoutSec = (int)_timeoutBox.Value;
            _settings.CopyToClipboard = _copyCheck.Checked;
            _settings.StartWithWindows = _autostartCheck.Checked;
            _settings.AutoDetectDirection = _autoDirectionCheck.Checked;
            _settings.HotkeyMode = _hotkeyCombo.SelectedIndex == 1 ? "ctrlShift" : "doubleCtrl";
            _settings.Translator = _translatorCombo.SelectedIndex switch
            {
                1 => "yandex",
                2 => "google",
                _ => "auto"
            };
            _settings.Save();
            I18n.ApplyPreference(_settings.UiLanguage);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to save settings from the dialog.", ex);
            MessageBox.Show(this, I18n.SettingsSaveFailed, I18n.ErrorTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static string FindLanguageItem(string code)
    {
        var prefix = code + " —";
        return Languages.FirstOrDefault(item => item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
               ?? Languages[0];
    }

    private static string GetLanguageCode(string item)
    {
        var space = item.IndexOf(' ');
        return space > 0 ? item[..space] : item;
    }
}
