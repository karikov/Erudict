namespace ERuDict;

internal sealed class MiniTooltipWindow : Form
{
    private const int CornerRadius = 4;
    private const int MaxTextWidth = 420;

    private readonly Settings _settings;
    private readonly Label _translationLabel;
    private readonly Label _directionLabel;
    private readonly Button _expandButton;
    private readonly System.Windows.Forms.Timer _hideTimer;
    private readonly System.Windows.Forms.Timer _escTimer;

    private string _sourceText = "";
    private string _translation = "";
    private string? _directionLabelText;
    private bool _isError;

    public event Action<string, string, bool, string?>? ExpandRequested;

    public MiniTooltipWindow(Settings settings)
    {
        _settings = settings;

        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        Size = new Size(280, 72);
        DoubleBuffered = true;

        var accent = new Panel
        {
            Dock = DockStyle.Left,
            Width = 4,
            BackColor = Theme.Accent
        };

        _translationLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(MaxTextWidth, 0),
            Font = Theme.Translation,
            ForeColor = Theme.Text,
            BackColor = Theme.Background,
            Location = new Point(16, 12)
        };

        _directionLabel = new Label
        {
            AutoSize = true,
            Font = Theme.UiSmall,
            ForeColor = Theme.Muted,
            BackColor = Theme.Background
        };

        _expandButton = Theme.CreateButton(I18n.Expand, accent: false);
        _expandButton.Size = new Size(118, 26);
        _expandButton.Font = Theme.UiSmall;
        _expandButton.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Expand();
            }
        };

        Controls.Add(_translationLabel);
        Controls.Add(_directionLabel);
        Controls.Add(_expandButton);
        Controls.Add(accent);

        MouseDown += (_, _) => HideTooltip();

        _hideTimer = new System.Windows.Forms.Timer();
        _hideTimer.Tick += (_, _) => HideTooltip();
        _escTimer = new System.Windows.Forms.Timer { Interval = 80 };
        _escTimer.Tick += (_, _) =>
        {
            if (Visible && Win32.IsKeyDown(Win32.VkEscape))
            {
                HideTooltip();
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

    public void ShowTranslation(string sourceText, string? translation, bool isError, string? directionLabel)
    {
        _sourceText = sourceText;
        _translation = translation ?? "";
        _directionLabelText = directionLabel;
        _isError = isError;

        _translationLabel.Text = string.IsNullOrWhiteSpace(_translation) ? "—" : _translation;
        _translationLabel.ForeColor = isError ? Theme.Error : Theme.Text;
        _expandButton.Text = I18n.Expand;
        _directionLabel.Text = directionLabel ?? "";
        _directionLabel.Visible = !string.IsNullOrWhiteSpace(directionLabel);

        if (_settings.CopyToClipboard && !isError && !string.IsNullOrWhiteSpace(_translation))
        {
            ClipboardManager.SetText(_translation);
        }

        LayoutTooltip();
        PlaceNearCursor();
        Show();
        Update();
        this.RoundRegion(CornerRadius);

        _escTimer.Start();
        _hideTimer.Stop();
        if (_settings.PopupTimeoutSec > 0)
        {
            _hideTimer.Interval = _settings.PopupTimeoutSec * 1000;
            _hideTimer.Start();
        }
    }

    public void HideTooltip()
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

    private void Expand()
    {
        HideTooltip();
        ExpandRequested?.Invoke(_sourceText, _translation, _isError, _directionLabelText);
    }

    private void LayoutTooltip()
    {
        _translationLabel.MaximumSize = new Size(MaxTextWidth, 0);
        _translationLabel.Location = new Point(16, 12);

        var textRight = _translationLabel.Right;
        var textBottom = _translationLabel.Bottom;
        var buttonY = Math.Max(textBottom + 8, 40);
        _expandButton.Location = new Point(Math.Max(16, textRight - _expandButton.Width), buttonY);
        _directionLabel.Location = new Point(16, _expandButton.Top + 5);

        var footerWidth = 16 + _directionLabel.PreferredWidth + 12 + _expandButton.Width + 16;
        var width = Math.Max(Math.Max(_translationLabel.Right + 16, footerWidth), 220);
        width = Math.Min(width, MaxTextWidth + 40);
        var height = _expandButton.Bottom + 12;
        Size = new Size(width, height);
        _expandButton.Location = new Point(Width - 16 - _expandButton.Width, _expandButton.Top);
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
}
