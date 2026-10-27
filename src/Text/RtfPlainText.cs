using System.Text;

namespace ERuDict;

/// <summary>
/// Запасной разбор RTF, если в CF_UNICODETEXT попала разметка.
/// Основной путь — уже готовый Unicode; здесь только текст без таблиц шрифтов.
/// </summary>
internal static class RtfPlainText
{
    public static string Convert(string rtf)
    {
        var result = new StringBuilder(rtf.Length);
        var depth = 0;
        var skipUntil = 0;
        for (var i = 0; i < rtf.Length; i++)
        {
            var ch = rtf[i];
            if (ch == '{')
            {
                depth++;
                if (skipUntil == 0 && IsSkippedGroup(rtf, i))
                {
                    skipUntil = depth;
                }

                continue;
            }

            if (ch == '}')
            {
                if (skipUntil == depth)
                {
                    skipUntil = 0;
                }

                depth = Math.Max(0, depth - 1);
                continue;
            }

            if (skipUntil != 0)
            {
                continue;
            }

            if (ch == '\\')
            {
                i = AppendControl(rtf, i, result);
                continue;
            }

            if (ch is not ('\r' or '\n'))
            {
                result.Append(ch);
            }
        }

        return result.ToString().Trim();
    }

    private static bool IsSkippedGroup(string rtf, int brace)
    {
        var i = brace + 1;
        if (i >= rtf.Length || rtf[i] != '\\')
        {
            return false;
        }

        if (i + 1 < rtf.Length && rtf[i + 1] == '*')
        {
            return true;
        }

        var word = ReadWord(rtf, i + 1);
        return word is "fonttbl" or "colortbl" or "stylesheet" or "info" or "pict" or "object" or "header" or "footer" or "themedata" or "datastore";
    }

    private static int AppendControl(string rtf, int slash, StringBuilder result)
    {
        var i = slash + 1;
        if (i >= rtf.Length)
        {
            return slash;
        }

        var ch = rtf[i];
        if (ch == '\'')
        {
            if (i + 2 < rtf.Length && IsHex(rtf[i + 1]) && IsHex(rtf[i + 2]))
            {
                var value = System.Convert.ToInt32(rtf.Substring(i + 1, 2), 16);
                result.Append((char)value);
                return i + 2;
            }

            return i;
        }

        if (ch is '\\' or '{' or '}')
        {
            result.Append(ch);
            return i;
        }

        if (ch == 'u' && i + 1 < rtf.Length && (char.IsDigit(rtf[i + 1]) || rtf[i + 1] == '-'))
        {
            var numberStart = i + 1;
            var numberEnd = numberStart + (rtf[numberStart] == '-' ? 1 : 0);
            while (numberEnd < rtf.Length && char.IsDigit(rtf[numberEnd]))
            {
                numberEnd++;
            }

            if (int.TryParse(rtf[numberStart..numberEnd], out var code))
            {
                if (code < 0)
                {
                    code += 65536;
                }

                result.Append((char)code);
            }

            if (numberEnd < rtf.Length && rtf[numberEnd] == '?')
            {
                return numberEnd;
            }

            if (numberEnd < rtf.Length && rtf[numberEnd] == ' ')
            {
                return numberEnd;
            }

            return numberEnd - 1;
        }

        var word = ReadWord(rtf, i);
        var after = i + word.Length;
        if (after < rtf.Length && rtf[after] == ' ')
        {
            after++;
        }
        else if (after < rtf.Length && rtf[after] == '-')
        {
            after++;
            while (after < rtf.Length && char.IsDigit(rtf[after]))
            {
                after++;
            }
        }
        else
        {
            while (after < rtf.Length && char.IsDigit(rtf[after]))
            {
                after++;
            }
        }

        switch (word)
        {
            case "par" or "line":
                result.Append('\n');
                break;
            case "tab":
                result.Append('\t');
                break;
        }

        return Math.Max(slash, after - 1);
    }

    private static string ReadWord(string text, int start)
    {
        var end = start;
        while (end < text.Length && char.IsLetter(text[end]))
        {
            end++;
        }

        return text[start..end];
    }

    private static bool IsHex(char ch) =>
        ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
}
