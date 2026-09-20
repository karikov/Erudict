using System.Drawing.Drawing2D;

namespace ERuDict;

internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(0x2B, 0x2D, 0x31);
    public static readonly Color Surface = Color.FromArgb(0x35, 0x37, 0x3C);
    public static readonly Color SurfaceAlt = Color.FromArgb(0x3F, 0x41, 0x47);
    public static readonly Color Border = Color.FromArgb(0x4E, 0x50, 0x57);
    public static readonly Color Accent = Color.FromArgb(0xE6, 0x7E, 0x22);
    public static readonly Color AccentHover = Color.FromArgb(0xF5, 0x96, 0x3A);
    public static readonly Color Text = Color.FromArgb(0xF2, 0xF3, 0xF5);
    public static readonly Color Muted = Color.FromArgb(0xB5, 0xBA, 0xC1);
    public static readonly Color Error = Color.FromArgb(0xE7, 0x4C, 0x3C);

    public static readonly Font Ui = new("Segoe UI", 9.75f);
    public static readonly Font UiSmall = new("Segoe UI", 8.25f);
    public static readonly Font Caption = new("Segoe UI Semibold", 8.25f, FontStyle.Bold);
    public static readonly Font Translation = new("Segoe UI", 12.5f);
    public static readonly Font Button = new("Segoe UI", 9.75f);

    public static void ApplyDialog(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = Text;
        form.Font = Ui;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.ShowInTaskbar = false;
        form.AutoScaleMode = AutoScaleMode.Dpi;
    }

    public static void StyleTextBox(TextBox box)
    {
        box.BorderStyle = BorderStyle.FixedSingle;
        box.BackColor = Surface;
        box.ForeColor = Text;
        box.Font = Ui;
    }

    public static void StyleCombo(ComboBox combo)
    {
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = Surface;
        combo.ForeColor = Text;
        combo.Font = Ui;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public static void StyleNumeric(NumericUpDown numeric)
    {
        numeric.BorderStyle = BorderStyle.FixedSingle;
        numeric.BackColor = Surface;
        numeric.ForeColor = Text;
        numeric.Font = Ui;
    }

    public static void StyleCheckBox(CheckBox check)
    {
        check.ForeColor = Text;
        check.Font = Ui;
        check.AutoSize = true;
    }

    public static Button CreateButton(string text, bool accent)
    {
        var button = new Button
        {
            Text = text,
            Font = Button,
            FlatStyle = FlatStyle.Flat,
            Height = 34,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            BackColor = accent ? Accent : SurfaceAlt,
            ForeColor = accent ? Color.White : Text,
            DialogResult = DialogResult.None
        };
        button.FlatAppearance.BorderColor = accent ? Accent : Border;
        button.FlatAppearance.MouseOverBackColor = accent ? AccentHover : Border;
        return button;
    }
}

internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable())
    {
        RoundedEdges = false;
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected ? Theme.Text : Theme.Text;
        base.OnRenderItemText(e);
    }
}

internal sealed class DarkColorTable : ProfessionalColorTable
{
    public override Color MenuBorder => Theme.Border;
    public override Color MenuItemBorder => Theme.Accent;
    public override Color MenuItemSelected => Theme.SurfaceAlt;
    public override Color MenuItemSelectedGradientBegin => Theme.SurfaceAlt;
    public override Color MenuItemSelectedGradientEnd => Theme.SurfaceAlt;
    public override Color MenuItemPressedGradientBegin => Theme.Surface;
    public override Color MenuItemPressedGradientEnd => Theme.Surface;
    public override Color ImageMarginGradientBegin => Theme.Background;
    public override Color ImageMarginGradientMiddle => Theme.Background;
    public override Color ImageMarginGradientEnd => Theme.Background;
    public override Color ToolStripDropDownBackground => Theme.Background;
    public override Color SeparatorDark => Theme.Border;
    public override Color SeparatorLight => Theme.Border;
}

internal static class ControlExtensions
{
    public static void RoundRegion(this Control control, int radius)
    {
        var bounds = new Rectangle(0, 0, control.Width, control.Height);
        using var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        var region = new Region(path);
        control.Region?.Dispose();
        control.Region = region;
    }
}
