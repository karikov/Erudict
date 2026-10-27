namespace ERuDict;

internal static class TextMetrics
{
    public const int MiniTooltipWordLimit = 3;

    public static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static bool PreferMiniTooltip(string text) => CountWords(text) is > 0 and <= MiniTooltipWordLimit;
}
