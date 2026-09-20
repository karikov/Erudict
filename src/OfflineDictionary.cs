using System.Text.RegularExpressions;

namespace ERuDict;

internal static class OfflineDictionary
{
    public const int MaxWords = 15;

    private static readonly Regex WordPattern = new(
        @"[\p{L}\p{M}]+(?:['’\-][\p{L}\p{M}]+)*",
        RegexOptions.Compiled);

    private static readonly object Gate = new();
    private static Dictionary<string, string>? _enRu;
    private static Dictionary<string, string>? _ruEn;
    private static bool _loaded;

    public static TranslationOutcome Translate(string text)
    {
        EnsureLoaded();
        if (_enRu is null || _ruEn is null || _enRu.Count == 0 && _ruEn.Count == 0)
        {
            throw new InvalidOperationException(I18n.OfflineDictionaryMissing);
        }

        var matches = WordPattern.Matches(text);
        if (matches.Count == 0)
        {
            throw new InvalidOperationException(I18n.OfflineNoWords);
        }

        var limit = Math.Min(matches.Count, MaxWords);
        var lines = new List<string>(limit + 2);
        for (var i = 0; i < limit; i++)
        {
            var word = matches[i].Value;
            var gloss = Lookup(word);
            lines.Add(word + " — " + (string.IsNullOrEmpty(gloss) ? I18n.OfflineUnknown : gloss));
        }

        lines.Add(string.Empty);
        lines.Add(I18n.OfflineRemark);
        return new TranslationOutcome(string.Join(Environment.NewLine, lines), "offline");
    }

    private static string? Lookup(string word)
    {
        var key = Normalize(word);
        if (key.Length == 0)
        {
            return null;
        }

        var cyrillic = key.Any(static ch => ch is >= '\u0400' and <= '\u04FF');
        if (cyrillic)
        {
            return Find(_ruEn!, key);
        }

        return Find(_enRu!, key) ?? FindEnglishStem(_enRu!, key);
    }

    private static string? FindEnglishStem(Dictionary<string, string> map, string key)
    {
        if (key.Length < 5)
        {
            return null;
        }

        if (key.EndsWith("ies", StringComparison.Ordinal) && key.Length > 4)
        {
            var found = Find(map, key[..^3] + "y");
            if (found is not null)
            {
                return found;
            }
        }

        foreach (var suffix in new[] { "ing", "ed", "es", "s" })
        {
            if (key.EndsWith(suffix, StringComparison.Ordinal) && key.Length - suffix.Length >= 3)
            {
                var found = Find(map, key[..^suffix.Length]);
                if (found is not null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static string? Find(Dictionary<string, string> map, string key) =>
        map.TryGetValue(key, out var value) ? value : null;

    private static string Normalize(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Replace('ё', 'е').ToCharArray();
        var written = 0;
        foreach (var ch in chars)
        {
            if (ch is not '\u0301')
            {
                chars[written++] = ch;
            }
        }

        return new string(chars, 0, written);
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (Gate)
        {
            if (_loaded)
            {
                return;
            }

            var folder = Path.Combine(AppPaths.Directory, "dict");
            _enRu = LoadTsv(Path.Combine(folder, "en-ru.tsv"));
            _ruEn = LoadTsv(Path.Combine(folder, "ru-en.tsv"));
            _loaded = true;
        }
    }

    private static Dictionary<string, string> LoadTsv(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(path))
        {
            AppLogger.Error("Dictionary file not found: " + path);
            return map;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var tab = line.IndexOf('\t');
            if (tab <= 0 || tab == line.Length - 1)
            {
                continue;
            }

            var key = Normalize(line[..tab]);
            var value = line[(tab + 1)..].Trim();
            if (key.Length == 0 || value.Length == 0 || map.ContainsKey(key))
            {
                continue;
            }

            map[key] = value;
        }

        return map;
    }
}
