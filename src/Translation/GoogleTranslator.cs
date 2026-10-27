using System.Net;
using System.Text;
using System.Text.Json;

namespace ERuDict;

internal static class GoogleTranslator
{
    private static readonly Uri Endpoint = new("https://translate.googleapis.com/translate_a/single");

    public static async Task<string> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client"] = "gtx",
            ["sl"] = sourceLanguage,
            ["tl"] = targetLanguage,
            ["dt"] = "t",
            ["q"] = text
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = content };
        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");

        using var response = await Translator.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Google Translate HTTP {(int)response.StatusCode}: {body}");
        }

        return Parse(body);
    }

    private static string Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Некорректный ответ Google Translate");
        }

        var sentences = root[0];
        if (sentences.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Пустой ответ Google Translate");
        }

        var builder = new StringBuilder();
        foreach (var item in sentences.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
            {
                var part = item[0].GetString();
                if (!string.IsNullOrEmpty(part))
                {
                    builder.Append(part);
                }
            }
        }

        var translation = builder.ToString();
        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new InvalidOperationException("Пустой ответ Google Translate");
        }

        return translation;
    }
}
