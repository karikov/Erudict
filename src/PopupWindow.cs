namespace ERuDict;

internal sealed class PopupWindow : Form
{
    private const int CornerRadius = 4;
    private const int PadX = 24;
    private const int PadTop = 20;
    private const int PadBottom = 18;

    private readonly Settings _settings;
    private readonly Label _sourceCaption;
    private readonly TextBox _sourceBox;
    private readonly Panel _sourcePanel;
    private readonly Label _translationCaption;
    private readonly TextBox _translationBox;
    private readonly Panel _translationPanel;
    private readonly Label _hint;
    private readonly Button _copyButton;
    private readonly Panel _content;
    private readonly System.Windows.Forms.Timer _hideTimer;
    private readonly System.Windows.Forms.Timer _escTimer;

    public PopupWindow(Settings settings)
    {
        _settings = settings;

        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        Size = new Size(960, 438);
        Padding = new Padding(0);
        DoubleBuffered = true;

        var accent = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = Theme.Accent
        };

        _content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Background,
            Padding = new Padding(0)
        };

        _sourceCaption = new Label
        {
            Text = "ИСХОДНЫЙ ТЕКСТ",
            AutoSize = true,
            Font = Theme.Caption,
            ForeColor = Theme.Muted,
            Location = new Point(0, 0)
        };

        _sourceBox = CreateReadOnlyBox(Theme.Ui, Theme.Muted);
        _sourcePanel = WrapBox(_sourceBox);

        _translationCaption = new Label
        {
            Text = "ПЕРЕВОД",
            AutoSize = true,
            Font = Theme.Caption,
            ForeColor = Theme.Accent,
            Location = new Point(0, 86)
        };

        _translationBox = CreateReadOnlyBox(Theme.Translation, Theme.Text);
        _translationPanel = WrapBox(_translationBox);

        _hint = new Label
        {
            Text = "Esc или клик вне текста — закрыть",
            AutoSize = true,
            Font = Theme.UiSmall,
            ForeColor = Theme.Muted,
            Location = new Point(0, 206)
        };

        _copyButton = Theme.CreateButton(I18n.Copy, accent: false);
        _copyButton.Size = new Size(120, 28);
        _copyButton.Font = Theme.UiSmall;
        _copyButton.Click += (_, _) => CopyTranslation();

        _content.Controls.Add(_sourceCaption);
        _content.Controls.Add(_sourcePanel);
        _content.Controls.Add(_translationCaption);
        _content.Controls.Add(_translationPanel);
        _content.Controls.Add(_hint);
        _content.Controls.Add(_copyButton);

        Controls.Add(_content);
        Controls.Add(accent);

        Load += (_, _) => LayoutContent();
        Resize += (_, _) =>
        {
            this.RoundRegion(CornerRadius);
            LayoutContent();
        };
        MouseDown += (_, _) => HidePopup();
        _content.MouseDown += (_, _) => HidePopup();
        _sourceCaption.MouseDown += (_, _) => HidePopup();
        _translationCaption.MouseDown += (_, _) => HidePopup();
        _hint.MouseDown += (_, _) => HidePopup();

        _hideTimer = new System.Windows.Forms.Timer();
        _hideTimer.Tick += (_, _) => HidePopup();

        _escTimer = new System.Windows.Forms.Timer { Interval = 80 };
        _escTimer.Tick += (_, _) =>
        {
            if (Visible && Win32.IsKeyDown(Win32.VkEscape))
            {
                HidePopup();
            }
        };
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WsExNoActivate = 0x08000000;
            const int WsExToolWindow = 0x00000080;
            const int WsExTopMost = 0x00000008;
            var cp = base.CreateParams;
            cp.ExStyle |= WsExNoActivate | WsExToolWindow | WsExTopMost;
            return cp;
        }
    }

    public void ShowTranslation(string sourceText, string? translation, bool isError = false, string? directionLabel = null)
    {
        _sourceBox.Text = Truncate(sourceText, 600);
        _translationBox.Text = translation ?? "";
        _translationBox.ForeColor = isError ? Theme.Error : Theme.Text;
        _sourceCaption.Text = I18n.SourceCaption;
        _translationCaption.Text = string.IsNullOrWhiteSpace(directionLabel)
            ? I18n.TranslationCaption
            : I18n.TranslationCaption + "  ·  " + directionLabel;
        _hint.Text = I18n.PopupHint;
        _copyButton.Text = I18n.Copy;
        _copyButton.Enabled = !isError && !string.IsNullOrWhiteSpace(translation);

        if (_settings.CopyToClipboard && !isError && !string.IsNullOrWhiteSpace(translation))
        {
            ClipboardManager.SetText(translation);
        }

        PlaceNearCursor();
        Show();
        Update();
        LayoutContent();
        this.RoundRegion(CornerRadius);

        _escTimer.Start();
        _hideTimer.Stop();
        if (_settings.PopupTimeoutSec > 0)
        {
            _hideTimer.Interval = _settings.PopupTimeoutSec * 1000;
            _hideTimer.Start();
        }
    }

    public void HidePopup()
    {
        _hideTimer.Stop();
        _escTimer.Stop();
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hideTimer.Dispose();
            _escTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LayoutContent()
    {
        var innerWidth = _content.ClientSize.Width - PadX * 2;
        if (innerWidth < 80)
        {
            return;
        }

        const int footerHeight = 36;
        var y = PadTop;
        _sourceCaption.Location = new Point(PadX, y);
        y = _sourceCaption.Bottom + 6;

        var captionsAndGaps = _sourceCaption.Height + _translationCaption.Height + 6 + 12 + 6 + 12 + footerHeight + PadBottom;
        var boxesHeight = Math.Max(120, _content.ClientSize.Height - PadTop - captionsAndGaps);
        var sourceHeight = Math.Max(80, (int)(boxesHeight * 0.38));
        var translationHeight = Math.Max(100, boxesHeight - sourceHeight);

        _sourcePanel.SetBounds(PadX, y, innerWidth, sourceHeight);
        y = _sourcePanel.Bottom + 12;
        _translationCaption.Location = new Point(PadX, y);
        y = _translationCaption.Bottom + 6;
        _translationPanel.SetBounds(PadX, y, innerWidth, translationHeight);
        y = _translationPanel.Bottom + 12;
        _hint.Location = new Point(PadX, y + 4);
        _copyButton.Location = new Point(PadX + innerWidth - _copyButton.Width, y);
    }

    private void PlaceNearCursor()
    {
        var cursor = Cursor.Position;
        var area = Screen.FromPoint(cursor).WorkingArea;
        var x = cursor.X + 16;
        var y = cursor.Y + 16;
        if (x + Width > area.Right)
        {
            x = cursor.X - Width - 16;
        }

        if (y + Height > area.Bottom)
        {
            y = cursor.Y - Height - 16;
        }

        Location = new Point(Math.Max(area.Left, x), Math.Max(area.Top, y));
    }

    private void CopyTranslation()
    {
        if (!string.IsNullOrWhiteSpace(_translationBox.Text))
        {
            ClipboardManager.SetText(_translationBox.Text);
            _copyButton.Text = I18n.Copied;
            _hideTimer.Stop();
            var reset = new System.Windows.Forms.Timer { Interval = 900 };
            reset.Tick += (_, _) =>
            {
                reset.Stop();
                reset.Dispose();
                _copyButton.Text = I18n.Copy;
            };
            reset.Start();
        }
    }

    private static TextBox CreateReadOnlyBox(Font font, Color color) =>
        new()
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            ForeColor = color,
            Font = font,
            ScrollBars = ScrollBars.Vertical,
            TabStop = false,
            Margin = new Padding(0)
        };

    private static Panel WrapBox(TextBox box)
    {
        var panel = new Panel
        {
            BackColor = Theme.Surface,
            Padding = new Padding(10, 8, 10, 8)
        };
        panel.Controls.Add(box);
        return panel;
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text ?? "";
        }

        return text[..maxLength] + "…";
    }
}
