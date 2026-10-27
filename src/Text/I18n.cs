using System.Globalization;

namespace ERuDict;

internal static class I18n
{
    public static string Code { get; private set; } = "en";

    public static event Action? Changed;

    public static bool IsRussian => Code == "ru";

    public static string AppName => "ERuDict";

    public static string AppTagline => T(
        "English–Russian translator for selected text",
        "Англо-русский переводчик выделенного текста");

    public static void ApplyPreference(string? preference)
    {
        var resolved = Resolve(preference);
        if (resolved == Code)
        {
            return;
        }

        Code = resolved;
        Changed?.Invoke();
    }

    public static string Resolve(string? preference)
    {
        var value = preference?.Trim().ToLowerInvariant();
        if (value is "ru" or "en")
        {
            return value;
        }

        return DetectSystemLanguage();
    }

    public static string DetectSystemLanguage()
    {
        for (var culture = CultureInfo.CurrentUICulture;
             culture is not null && !Equals(culture, CultureInfo.InvariantCulture);
             culture = culture.Parent)
        {
            if (culture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase))
            {
                return "ru";
            }
        }

        return "en";
    }

    public static string T(string english, string russian) => IsRussian ? russian : english;

    public static string TrayTooltip => AppName + " — " + AppTagline;
    public static string MenuTranslate => T("Translate selection", "Перевести выделенное");
    public static string MenuSettings => T("Settings…", "Настройки…");
    public static string MenuHelp => T("Help", "Помощь");
    public static string MenuExit => T("Exit", "Выход");

    public static string SettingsTitle => T("ERuDict settings", "Настройки ERuDict");
    public static string LabelUiLanguage => T("Interface language", "Язык интерфейса");
    public static string LabelTargetLanguage => T("Translation language", "Язык перевода");
    public static string LabelTranslator => T("Translation service", "Сервис перевода");
    public static string LabelInterval => T("Double Ctrl interval, ms", "Интервал двойного Ctrl, мс");
    public static string LabelTimeout => T("Auto-hide, sec", "Автоскрытие, сек");
    public static string LabelHotkey => T("Hotkey", "Горячая клавиша");
    public static string CheckCopy => T("Copy translation to the clipboard", "Копировать перевод в буфер обмена");
    public static string CheckAutoDirection => T(
        "Auto direction: Latin → my language, Cyrillic → EN",
        "Автонаправление: латиница → мой язык, кириллица → EN");
    public static string CheckAutostart => T("Start with Windows", "Запускать вместе с Windows");
    public static string Save => T("Save", "Сохранить");
    public static string Cancel => T("Cancel", "Отмена");
    public static string ErrorTitle => T("Error", "Ошибка");
    public static string SettingsSaveFailed => T(
        "Could not save settings. Check that the program folder is writable.",
        "Не удалось сохранить настройки. Проверьте права на запись рядом с программой.");

    public static string UiLangSystem => T("System (Russian if Windows is ru)", "Системный (русский, если Windows на ru)");
    public static string UiLangRussian => "Русский";
    public static string UiLangEnglish => "English";
    public static string HotkeyDoubleCtrl => T("Double Ctrl", "Двойной Ctrl");
    public static string HotkeyCtrlShift => "Ctrl + Shift";
    public static string TranslatorAuto => T("Auto (Yandex, then Google)", "Авто (Яндекс, затем Google)");
    public static string TranslatorYandex => T("Yandex", "Яндекс");
    public static string TranslatorGoogle => "Google";

    public static string SourceCaption => T("SOURCE", "ИСХОДНЫЙ ТЕКСТ");
    public static string TranslationCaption => T("TRANSLATION", "ПЕРЕВОД");
    public static string PopupHint => T("Esc also closes", "Esc тоже закрывает");
    public static string PopupHintOutside => T("Esc or a click outside", "Esc или клик снаружи");
    public static string Copy => T("Copy", "Копировать");
    public static string Copied => T("Copied", "Скопировано");
    public static string Expand => T("Expand", "Развернуть");

    public static string AlreadyRunning => T(
        "ERuDict is already running in the system tray.",
        "ERuDict уже запущен и находится в системном трее.");
    public static string StartupFailed => T("Could not start ERuDict:", "Не удалось запустить ERuDict:");
    public static string TranslateFailed => T("Translation failed: ", "Не удалось перевести: ");
    public static string GenericError => T("Error: ", "Ошибка: ");

    public static string EngineYandex => T("Yandex", "Яндекс");
    public static string EngineGoogle => "Google";
    public static string EngineOffline => T("Offline", "Офлайн");
    public static string OfflineUnknown => T("not found", "нет в словаре");
    public static string OfflineDictionaryMissing => T(
        "Offline dictionary files were not found next to the program.",
        "Файлы офлайн-словаря не найдены рядом с программой.");
    public static string OfflineNoWords => T("No words to look up.", "В выделении нет слов для словаря.");
    public static string OfflineRemark => T(
        "Online translation services are unavailable. Offline mode can only translate words — at most 15 per request.",
        "Онлайн сервисы для перевода недоступны. В оффлайн режиме работает только перевод слов - не более 15 за один раз.");

    public static string HelpTitle => T("ERuDict — Help", "ERuDict — Помощь");
    public static string Close => T("Close", "Закрыть");

    public static string HelpBody => IsRussian ? HelpRu : HelpEn;

    private const string HelpEn =
        """
        ERuDict (English–Russian Dictionary) translates selected text from any Windows app.

        Quick start
        1. Leave ERuDict running in the system tray.
        2. Select text in a browser, editor, Outlook, or terminal.
        3. Double-press Ctrl (or Ctrl+Shift if you changed the hotkey).
        4. A popup appears near the cursor with the translation.

        Short phrases
        If the selection is 1–3 words, a compact tooltip shows only the translation.
        Click Expand to open the full popup with the original text.

        Direction
        • About 80% Latin letters → English → your language (usually Russian).
        • About 80% Cyrillic letters → Russian → English.
        • Mixed text → auto-detect → the language from Settings.

        Services
        Auto tries Yandex first, then Google. You can lock a single service in Settings.

        Offline
        If the online services cannot be reached, ERuDict looks up words in the local FreeDict (folder dict next to the exe).
        Only words are translated, at most 15 per request.

        Tips
        • The original clipboard is restored after capture (Unicode text).
        • A short popup and an offline dictionary result close on their own after the timeout, with Esc, or with a click outside the window.
        • A full online translation stays until you press Close or Esc.
        • Settings, Help, and Exit are in the tray icon menu.
        • Logs: erudict.log next to the executable.
        """;

    private const string HelpRu =
        """
        ERuDict (English–Russian Dictionary) переводит выделенный текст из любого приложения Windows.

        Быстрый старт
        1. Оставьте ERuDict в системном трее.
        2. Выделите текст в браузере, редакторе, Outlook или терминале.
        3. Дважды нажмите Ctrl (или Ctrl+Shift, если сменили горячую клавишу).
        4. Рядом с курсором появится окно с переводом.

        Короткие фразы
        Если выделено 1–3 слова, показывается мини-тултип только с переводом.
        Кнопка «Развернуть» открывает полный попап с исходным текстом.

        Направление
        • Около 80% латинских букв → английский → ваш язык (обычно русский).
        • Около 80% кириллицы → русский → английский.
        • Смешанный текст → автоопределение → язык из настроек.

        Сервисы
        Режим «Авто» сначала спрашивает Яндекс, при сбое — Google. В настройках можно закрепить один сервис.

        Офлайн
        Если онлайн-сервисы недоступны, ERuDict ищет слова в локальном FreeDict (папка dict рядом с exe).
        Переводятся только слова, не больше 15 за раз.

        Подсказки
        • Прежний буфер обмена восстанавливается после захвата (Unicode-текст).
        • Короткий попап и перевод из словаря закрываются сами по таймауту, по Esc и по клику за пределами окна.
        • Полный онлайн-перевод остаётся, пока его не закроют кнопкой «Закрыть» или Esc.
        • Настройки, помощь и выход — в меню иконки в трее.
        • Журнал: erudict.log рядом с программой.
        """;
}
