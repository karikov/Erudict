# ERuDict — инструкция по использованию

ERuDict (English–Russian Dictionary) — фоновый переводчик выделенного текста для Windows.

---

## Русский

### Установка

Скопируйте `ERuDict.exe` и папку `dict` в любую папку и запустите. Устанавливать .NET не нужно. В трее появится иконка. Файл настроек `appsettings.json` создаётся рядом с программой при первом запуске.

### Как перевести текст

1. Выделите текст в любом приложении (браузер, Word, Outlook, мессенджер, терминал).
2. Дважды быстро нажмите **Ctrl** (или **Ctrl+Shift**, если так задано в настройках).
3. Рядом с курсором появится перевод.

Альтернатива: правый клик по иконке в трее → **Перевести выделенное**.

### Мини-тултип и полный попап

- **1–3 слова** — компактный тултип только с переводом. Кнопка **Развернуть** открывает большое окно с исходником.
- **Длиннее** — сразу большое окно: исходный текст и перевод, направление (`EN → RU`) и сервис (`Яндекс` / `Google`).

Закрыть окно: **Esc**, клик по рамке или ожидание автоскрытия.

### Направление перевода

Если в настройках включено автонаправление:

- текст в основном **латиницей** → с английского на ваш язык (обычно русский);
- текст в основном **кириллицей** → с русского на английский;
- смесь языков → автоопределение сервиса и язык из поля «Язык перевода».

### Меню трея

| Пункт | Действие |
|---|---|
| Перевести выделенное | Перевести текущее выделение |
| Настройки… | Язык интерфейса, сервис, горячая клавиша, автозагрузка |
| Помощь | Эта инструкция в окне программы |
| Выход | Закрыть ERuDict |

### Язык интерфейса

По умолчанию берётся **язык Windows**: если система на русском — интерфейс русский, иначе английский. В настройках можно зафиксировать Русский или English. Смена применяется сразу.

### Сервис перевода

- **Авто** — сначала Яндекс, при ошибке Google.
- **Яндекс** или **Google** — только выбранный сервис.

### Офлайн

Если Яндекс и Google недоступны, ERuDict переводит **отдельные слова** по словарю FreeDict (папка `dict` рядом с exe). Не больше **15 слов** за раз; остальное отбрасывается, в окне появляется ремарка о лимите.

### Если не срабатывает

- Иконка должна быть в трее (проверьте скрытые значки Windows).
- Некоторые программы не принимают синтетический Ctrl+C — перевод из них может не получиться.
- Журнал ошибок: `erudict.log` в папке с `ERuDict.exe`.
- Второй экземпляр не запускается: уже работающая копия остаётся в трее.

---

## English

### Install

Copy `ERuDict.exe` and the `dict` folder anywhere and run the program. No .NET install is required. A tray icon appears. `appsettings.json` is created next to the executable on first run.

### Translate text

1. Select text in any app (browser, Word, Outlook, messenger, terminal).
2. Double-press **Ctrl** (or **Ctrl+Shift** if you changed the hotkey).
3. The translation appears near the cursor.

Alternatively: right-click the tray icon → **Translate selection**.

### Mini tooltip vs full popup

- **1–3 words** — a compact tooltip with the translation only. **Expand** opens the full popup with the original.
- **Longer text** — the full popup: source, translation, direction (`EN → RU`) and engine (`Yandex` / `Google`).

Close with **Esc**, a click on the frame, or wait for auto-hide.

### Direction

With auto-direction enabled:

- mostly **Latin** letters → English → your language (usually Russian);
- mostly **Cyrillic** → Russian → English;
- mixed text → the service auto-detects and uses **Translation language** from Settings.

### Tray menu

| Item | Action |
|---|---|
| Translate selection | Translate the current selection |
| Settings… | UI language, engine, hotkey, autostart |
| Help | This guide inside the app |
| Exit | Quit ERuDict |

### Interface language

By default the **Windows UI language** is used: Russian Windows → Russian UI, otherwise English. You can lock Русский or English in Settings. The change applies immediately.

### Translation service

- **Auto** — Yandex first, Google on failure.
- **Yandex** or **Google** — that service only.

No API key. Internet access is required for sentence translation.

### Offline

If Yandex and Google cannot be reached, ERuDict looks up **individual words** in the FreeDict files in the `dict` folder. At most **15 words** per request; a remark is shown when this limit applies.

### Troubleshooting

- Keep the icon in the tray (check Windows overflow icons).
- A few apps ignore synthetic Ctrl+C, so capture may fail there.
- Error log: `erudict.log` next to `ERuDict.exe`.
- A second launch does not start another copy; the running instance stays in the tray.
