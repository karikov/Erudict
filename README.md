# ERuDict

**ERuDict** (English–Russian Dictionary) — фоновый переводчик выделенного текста для Windows.

Страница программы: [index.html](index.html). Установщик для Windows: [скачать](https://github.com/karikov/Erudict/releases/latest/download/ERuDict-Setup.exe).

Работает в системном трее. Двойной `Ctrl` переводит выделение через Яндекс или Google и показывает результат у курсора.

Полная инструкция: [docs/USAGE.md](docs/USAGE.md) (русский и English).  
Постановка задачи: [docs/SPEC.md](docs/SPEC.md).

## Возможности

- Перевод через Яндекс и Google (без API-ключа; «Авто» — Яндекс, затем Google)
- Интерфейс на русском и английском (по умолчанию — язык Windows: `ru` → русский, иначе English)
- Горячая клавиша: двойной Ctrl или Ctrl+Shift
- Буфер обмена пользователя восстанавливается (Unicode)
- Полный попап или мини-тултип для 1–3 слов
- Настройки, помощь и выход — в меню иконки трея
- Журнал: `erudict.log` рядом с exe

## Сборка

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download).

Обычная сборка для проверки на этой машине (нужен .NET 8 Desktop Runtime):

```sh
dotnet build -c Release
```

Релиз — Native AOT, один exe для Windows x64, без установленного .NET:

```sh
dotnet publish -c Release -p:PublishProfile=Win64NativeAot
```

Результат: `bin/Release/publish-aot/ERuDict.exe`. Словари лежат рядом, в папке `dict`.

## Запуск

Запустите `ERuDict.exe`. Повторный запуск не создаёт второй процесс.

## Использование (кратко)

1. Выделите текст.
2. Дважды нажмите Ctrl.
3. Рядом с курсором появится перевод.

Справка также открывается из трея: **Помощь** / **Help**.
