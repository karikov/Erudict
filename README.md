# ERuDict

**ERuDict** (English–Russian Dictionary) — фоновый переводчик выделенного текста для Windows.

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

Обычная сборка (нужен Desktop Runtime на целевом ПК):

```sh
dotnet build -c Release
```

Самодостаточный один файл (Windows x64, .NET внутри):

```sh
dotnet publish -c Release -p:PublishProfile=Win64SelfContained
```

Результат: `bin/Release/publish/ERuDict.exe`.

## Запуск

Запустите `ERuDict.exe`. Повторный запуск не создаёт второй процесс.

## Использование (кратко)

1. Выделите текст.
2. Дважды нажмите Ctrl.
3. Рядом с курсором появится перевод.

Справка также открывается из трея: **Помощь** / **Help**.
