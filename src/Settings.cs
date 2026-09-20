using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERuDict;

internal sealed class Settings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string TargetLanguage { get; set; } = "ru";
    public int DoublePressIntervalMs { get; set; } = 400;
    public int PopupTimeoutSec { get; set; } = 8;
    public bool CopyToClipboard { get; set; }
    public string HotkeyMode { get; set; } = "doubleCtrl";
    public bool StartWithWindows { get; set; }
    public bool AutoDetectDirection { get; set; } = true;
    public string Translator { get; set; } = "auto";
    public string UiLanguage { get; set; } = "auto";

    public static string GetConfigPath() => Path.Combine(AppPaths.Directory, "appsettings.json");

    public static Settings Load()
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
        {
            var defaults = Settings.CreateDefaults();
            try
            {
                defaults.Save();
            }
            catch (Exception ex)
            {
                AppLogger.Error("Не удалось создать файл настроек.", ex);
            }

            return defaults;
        }

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<Settings>(json, JsonOptions) ?? CreateDefaults();
            loaded.Normalize();
            return loaded;
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось прочитать настройки, используются значения по умолчанию.", ex);
            return Settings.CreateDefaults();
        }
    }

    public void Save()
    {
        try
        {
            Normalize();
            File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(this, JsonOptions));
            Autostart.Apply(StartWithWindows);
        }
        catch (Exception ex)
        {
            AppLogger.Error("Не удалось сохранить настройки.", ex);
            throw;
        }
    }

    public static Settings CreateDefaults()
    {
        var systemUi = I18n.DetectSystemLanguage();
        return new Settings
        {
            UiLanguage = "auto",
            TargetLanguage = systemUi == "ru" ? "ru" : "en"
        };
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(TargetLanguage))
        {
            TargetLanguage = "ru";
        }

        DoublePressIntervalMs = Math.Clamp(DoublePressIntervalMs, 150, 2000);
        PopupTimeoutSec = Math.Clamp(PopupTimeoutSec, 0, 120);
        if (HotkeyMode is not "ctrlShift")
        {
            HotkeyMode = "doubleCtrl";
        }

        Translator = Translator?.Trim().ToLowerInvariant() switch
        {
            "google" => "google",
            "yandex" => "yandex",
            _ => "auto"
        };

        UiLanguage = UiLanguage?.Trim().ToLowerInvariant() switch
        {
            "ru" => "ru",
            "en" => "en",
            _ => "auto"
        };
    }
}
