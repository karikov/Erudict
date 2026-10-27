using System.Runtime.InteropServices;

namespace ERuDict;

internal static class UiTheme
{
    public const int Background = 0x00312D2B;
    public const int Surface = 0x003C3735;
    public const int SurfaceAlt = 0x0047413F;
    public const int Border = 0x0057504E;
    public const int Accent = 0x00227EE6;
    public const int AccentHover = 0x003A96F5;
    public const int Text = 0x00F5F3F2;
    public const int Muted = 0x00C1BAB5;
    public const int Error = 0x003C4CE7;
    public const int White = 0x00FFFFFF;

    public static nint BackgroundBrush { get; private set; }
    public static nint SurfaceBrush { get; private set; }
    public static nint SurfaceAltBrush { get; private set; }
    public static nint BorderBrush { get; private set; }
    public static nint AccentBrush { get; private set; }
    public static nint AccentHoverBrush { get; private set; }
    public static nint UiFont { get; private set; }
    public static nint SmallFont { get; private set; }
    public static nint CaptionFont { get; private set; }
    public static nint TranslationFont { get; private set; }
    public static nint LargeIcon { get; private set; }
    public static nint SmallIcon { get; private set; }

    public static void Ensure()
    {
        if (BackgroundBrush != 0)
        {
            return;
        }

        BackgroundBrush = UiNative.CreateSolidBrush(Background);
        SurfaceBrush = UiNative.CreateSolidBrush(Surface);
        SurfaceAltBrush = UiNative.CreateSolidBrush(SurfaceAlt);
        BorderBrush = UiNative.CreateSolidBrush(Border);
        AccentBrush = UiNative.CreateSolidBrush(Accent);
        AccentHoverBrush = UiNative.CreateSolidBrush(AccentHover);
        UiFont = CreateUiFont(10, UiNative.FW_NORMAL);
        SmallFont = CreateUiFont(8, UiNative.FW_NORMAL);
        CaptionFont = CreateUiFont(8, UiNative.FW_SEMIBOLD);
        TranslationFont = CreateUiFont(13, UiNative.FW_NORMAL);
        LoadIcons();
    }

    private static nint CreateUiFont(int points, int weight) =>
        UiNative.CreateFont(
            UiDpi.FontHeight(points), 0, 0, 0, weight,
            0, 0, 0,
            1, 0, 0, 5, 0, "Segoe UI");

    private static void LoadIcons()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path))
        {
            UiNative.ExtractIconEx(path, 0, out var large, out var small, 1);
            LargeIcon = large;
            SmallIcon = small;
        }
    }
}
