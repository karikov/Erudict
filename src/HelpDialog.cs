namespace ERuDict;

internal sealed class HelpDialog : Form
{
    public HelpDialog()
    {
        Text = I18n.HelpTitle;
        ClientSize = new Size(560, 520);
        Theme.ApplyDialog(this);
        MaximizeBox = true;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(480, 360);
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? Application.ExecutablePath);

        var text = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            ForeColor = Theme.Text,
            Font = Theme.Ui,
            Text = I18n.HelpBody.Replace("\n", Environment.NewLine),
            TabStop = true
        };

        var close = Theme.CreateButton(I18n.Close, accent: true);
        close.Width = 120;
        close.DialogResult = DialogResult.OK;
        close.Anchor = AnchorStyles.Right;

        var buttons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(16, 8, 16, 12)
        };
        close.Location = new Point(buttons.ClientSize.Width - 16 - close.Width, 10);
        buttons.Resize += (_, _) => close.Location = new Point(buttons.ClientSize.Width - 16 - close.Width, 10);
        buttons.Controls.Add(close);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 16, 16, 8)
        };
        body.Controls.Add(text);

        Controls.Add(body);
        Controls.Add(buttons);
        AcceptButton = close;
        CancelButton = close;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = false;
    }
}
