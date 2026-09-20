using System.Net.Http.Headers;

namespace ERuDict;

internal readonly record struct TranslationOutcome(string Text, string Engine)
{
    public string EngineLabel => Engine switch
    {
        "yandex" => I18n.EngineYandex,
        "google" => I18n.EngineGoogle,
        "offline" => I18n.EngineOffline,
        _ => Engine
    };
}

internal static class Translator
{
    private const int TimeoutSeconds = 10;
    private static readonly HttpClient Http = CreateClient();

    internal static HttpClient Client => Http;

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return client;
    }

    public static async Task<TranslationOutcome> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        string provider,
        CancellationToken cancellationToken = default)
    {
        provider = (provider ?? "auto").Trim().ToLowerInvariant();
        var engines = provider switch
        {
            "google" => new[] { "google" },
            "yandex" => new[] { "yandex" },
            _ => new[] { "yandex", "google" }
        };

        Exception? lastError = null;
        foreach (var engine in engines)
        {
            try
            {
                var translated = engine == "google"
                    ? await GoogleTranslator.TranslateAsync(text, sourceLanguage, targetLanguage, cancellationToken)
                        .ConfigureAwait(false)
                    : await YandexTranslator.TranslateAsync(text, sourceLanguage, targetLanguage, cancellationToken)
                        .ConfigureAwait(false);
                return new TranslationOutcome(translated, engine);
            }
            catch (Exception ex)
            {
                lastError = ex;
                AppLogger.Error(engine + " translator failed.", ex);
            }
        }

        try
        {
            return OfflineDictionary.Translate(text);
        }
        catch (Exception offlineError)
        {
            AppLogger.Error("Offline dictionary failed.", offlineError);
            throw lastError ?? offlineError;
        }
    }
}
