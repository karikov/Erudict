using System.Net;
using System.Text.Json;

namespace ERuDict;

internal static class YandexTranslator
{
    private static readonly Uri Endpoint = new("https://translate.yandex.net/api/v1/tr.json/translate");

    public static async Task<string> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken)
    {
        var lang = BuildLangPair(sourceLanguage, targetLanguage);
        var ucid = Guid.NewGuid().ToString("N");
        var url = $"{Endpoint}?ucid={ucid}&srv=android&format=text";

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["text"] = text,
            ["lang"] = lang
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.TryAddWithoutValidation("User-Agent", "ru.yandex.translate/3.20.2024");

        using var response = await Translator.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Яндекс.Переводчик HTTP {(int)response.StatusCode}: {TrimBody(body)}");
        }

        return Parse(body);
    }

    private static string BuildLangPair(string sourceLanguage, string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguage) ||
            sourceLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return targetLanguage;
        }

        return sourceLanguage + "-" + targetLanguage;
    }

    private static string Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("code", out var codeElement) &&
            codeElement.ValueKind == JsonValueKind.Number &&
            codeElement.GetInt32() is not 200)
        {
            throw new InvalidOperationException("Яндекс.Переводчик вернул код " + codeElement.GetInt32());
        }

        if (!root.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Некорректный ответ Яндекс.Переводчика");
        }

        var parts = textElement.EnumerateArray()
            .Select(item => item.GetString())
            .Where(part => !string.IsNullOrEmpty(part));
        var translation = string.Join("", parts);
        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new InvalidOperationException("Пустой ответ Яндекс.Переводчика");
        }

        return translation;
    }

    private static string TrimBody(string body) =>
        body.Length <= 240 ? body : body[..240] + "…";
}
