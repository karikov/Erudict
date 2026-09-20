namespace ERuDict;

internal readonly record struct TranslationDirection(string SourceLanguage, string TargetLanguage, string Label)
{
    public static TranslationDirection Fallback(string targetLanguage) =>
        new("auto", targetLanguage, "AUTO → " + targetLanguage.ToUpperInvariant());
}

internal static class LanguageDetector
{
    public const double ScriptThreshold = 0.8;

    private static readonly HashSet<string> CyrillicLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "ru", "uk", "bg", "sr", "be", "mk", "kk", "ky", "tg", "mn"
    };

    public static TranslationDirection Resolve(string text, string homeLanguage, bool enabled)
    {
        if (!enabled)
        {
            return TranslationDirection.Fallback(homeLanguage);
        }

        CountScripts(text, out var latin, out var cyrillic, out var letters);
        if (letters == 0)
        {
            return TranslationDirection.Fallback(homeLanguage);
        }

        var latinShare = (double)latin / letters;
        var cyrillicShare = (double)cyrillic / letters;
        var home = string.IsNullOrWhiteSpace(homeLanguage) ? "ru" : homeLanguage;

        if (latinShare >= ScriptThreshold)
        {
            var target = home.Equals("en", StringComparison.OrdinalIgnoreCase) ? "ru" : home;
            return new TranslationDirection("en", target, "EN → " + target.ToUpperInvariant());
        }

        if (cyrillicShare >= ScriptThreshold)
        {
            var source = CyrillicLanguages.Contains(home) ? home : "ru";
            var target = CyrillicLanguages.Contains(home) || home.Equals("en", StringComparison.OrdinalIgnoreCase)
                ? "en"
                : home;
            return new TranslationDirection(source, target, source.ToUpperInvariant() + " → " + target.ToUpperInvariant());
        }

        return TranslationDirection.Fallback(home);
    }

    private static void CountScripts(string text, out int latin, out int cyrillic, out int letters)
    {
        latin = 0;
        cyrillic = 0;
        letters = 0;

        foreach (var ch in text)
        {
            if (!char.IsLetter(ch))
            {
                continue;
            }

            letters++;
            if (IsLatin(ch))
            {
                latin++;
            }
            else if (IsCyrillic(ch))
            {
                cyrillic++;
            }
        }
    }

    private static bool IsLatin(char ch) =>
        ch is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '\u00C0' and <= '\u024F');

    private static bool IsCyrillic(char ch) =>
        ch is (>= '\u0400' and <= '\u04FF') or (>= '\u0500' and <= '\u052F');
}
