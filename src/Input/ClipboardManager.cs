using System.Net;
using System.Text.RegularExpressions;

namespace ERuDict;

internal static class ClipboardManager
{
    public const int MaxTextLength = 5000;

    public static async Task<string?> GetSelectedTextAsync(CancellationToken cancellationToken = default)
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("Буфер обмена доступен только из STA-потока.");
        }

        string? backup = null;
        uint sequenceBefore = NativeClipboard.SequenceNumber;
        if (await NativeClipboard.TryOpenAsync(cancellationToken))
        {
            try
            {
                backup = NativeClipboard.ReadUnicode();
                sequenceBefore = NativeClipboard.SequenceNumber;
            }
            finally
            {
                NativeClipboard.Close();
            }
        }

        Win32.SendCopyShortcut();

        string? captured = null;
        try
        {
            for (var attempt = 0; attempt < 12; attempt++)
            {
                await Task.Delay(50, cancellationToken).ConfigureAwait(true);
                if (!await NativeClipboard.TryOpenAsync(cancellationToken))
                {
                    continue;
                }

                try
                {
                    if (NativeClipboard.SequenceNumber == sequenceBefore)
                    {
                        continue;
                    }

                    captured = NativeClipboard.ReadUnicode();
                    if (!string.IsNullOrWhiteSpace(captured))
                    {
                        break;
                    }
                }
                finally
                {
                    NativeClipboard.Close();
                }
            }
        }
        finally
        {
            await RestoreUnicodeAsync(backup, cancellationToken);
        }

        return NormalizeCapturedText(captured);
    }

    public static async void SetText(string text)
    {
        try
        {
            await RestoreUnicodeAsync(text, CancellationToken.None);
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось записать текст в буфер обмена.", ex);
        }
    }

    private static async Task RestoreUnicodeAsync(string? text, CancellationToken cancellationToken)
    {
        if (!await NativeClipboard.TryOpenAsync(cancellationToken))
        {
            AppLogger.Error("Не удалось открыть буфер обмена для восстановления.");
            return;
        }

        try
        {
            NativeClipboard.TrySetUnicode(text ?? string.Empty);
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось восстановить буфер обмена.", ex);
        }
        finally
        {
            NativeClipboard.Close();
        }
    }

    private static string? NormalizeCapturedText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = text.Replace("\0", string.Empty);
        text = ToPlainText(text).Trim();
        if (text.Length == 0)
        {
            return null;
        }

        return text.Length <= MaxTextLength ? text : text[..MaxTextLength];
    }

    private static string ToPlainText(string text)
    {
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return RtfPlainText.Convert(trimmed);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Не удалось разобрать RTF из буфера.", ex);
            }
        }

        if (trimmed.Contains("StartHTML:", StringComparison.Ordinal) ||
            trimmed.Contains("<html", StringComparison.OrdinalIgnoreCase))
        {
            return StripHtml(trimmed);
        }

        return text;
    }

    private static string StripHtml(string html)
    {
        var fragment = ExtractHtmlFragment(html);
        fragment = Regex.Replace(fragment, @"(?i)<br\s*/?>", "\n");
        fragment = Regex.Replace(fragment, @"(?i)</p>", "\n");
        fragment = Regex.Replace(fragment, @"(?i)</div>", "\n");
        fragment = Regex.Replace(fragment, "<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(fragment);
    }

    private static string ExtractHtmlFragment(string html)
    {
        var startToken = "<!--StartFragment-->";
        var endToken = "<!--EndFragment-->";
        var start = html.IndexOf(startToken, StringComparison.Ordinal);
        var end = html.IndexOf(endToken, StringComparison.Ordinal);
        if (start >= 0 && end > start)
        {
            return html[(start + startToken.Length)..end];
        }

        return html;
    }
}
