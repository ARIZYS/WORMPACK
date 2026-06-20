# Wormcraft Launcher — инструкция (GitHub + jsDelivr)

## Структура проекта

```
WormcraftLauncher/
├── Backend/                    ← это станет твоим GitHub-репозиторием
│   ├── generate_manifest.py
│   └── files/
│       ├── mods/                ← сюда твои .jar моды
│       └── config/               ← сюда конфиги модов
│
└── Launcher/                   ← исходники C# лаунчера, собираешь в .exe у себя
    ├── WormcraftLauncher.csproj
    ├── Program.cs
    ├── MainForm.cs
    ├── Models.cs
    ├── UpdateManager.cs
    └── GameLauncher.cs
```

## Шаг 1. Создаём GitHub-репозиторий

1. Заходишь на github.com, создаёшь аккаунт (если нет).
2. New repository → название, например `wormcraft-launcher` → **Public**
   (jsDelivr раздаёт только публичные репозитории, это нормально — там просто файлы модов,
   ничего секретного).
3. Клонируешь его к себе на компьютер (или просто перетаскиваешь файлы через веб-интерфейс
   GitHub — кнопка "Add file → Upload files", без консоли тоже можно).
4. Кидаешь туда содержимое папки `Backend/`: `generate_manifest.py` и папку `files/`,
   а внутри `files/` — **все** папки твоей сборки, какие у тебя есть, например:
   ```
   files/
     mods/
     config/
     shaderpacks/
     scripts/
     emotes/
     resourcepacks/
   ```
   Лаунчер скопирует их все автоматически в `.minecraft` игрока — ничего отдельно
   прописывать для новых папок не нужно, он сам читает структуру из манифеста.

## Шаг 2. Настраиваем генератор манифеста

Открой `generate_manifest.py`, поправь сверху:

```python
GITHUB_USER = "твой_github_ник"
GITHUB_REPO = "wormcraft-launcher"
FORGE_VERSION = "1.19.2-43.4.0"   # точная версия форджа на сервере
```

Нужен Python 3 (ставится с python.org, дальше просто двойной клик / `python generate_manifest.py`
в консоли). Никаких библиотек ставить не надо.

## Шаг 3. Обновление сборки (рабочий процесс)

Каждый раз когда меняешь моды/конфиги:

1. Кладёшь/удаляешь файлы в `files/mods/` или `files/config/` локально.
2. Запускаешь:
   ```
   python generate_manifest.py
   ```
   Это пересоберёт `manifest.json` с новыми хешами и версией сборки.
3. Заливаешь изменения на GitHub:
   ```
   git add .
   git commit -m "update build"
   git push
   ```
   (или через веб-интерфейс GitHub, если без консоли — просто перезалить изменённые файлы).
4. **Важно:** jsDelivr кеширует файлы на ~12 часов. Чтобы игроки увидели обновление
   сразу, после пуша зайди по ссылке (можно прямо в браузере):
   ```
   https://purge.jsdelivr.net/gh/твой_ник/wormcraft-launcher@main/manifest.json
   ```
   Это сбросит кеш для манифеста. Если поменялись и сами файлы модов — для каждого
   изменённого файла можно так же дёрнуть purge-ссылку, либо просто подождать.

## Шаг 4. Сборка лаунчера (Launcher)

Нужен **Visual Studio 2022** (Community, бесплатно) с компонентом ".NET desktop development",
или просто .NET 6 SDK + командная строка.

### Через Visual Studio
1. Открой `WormcraftLauncher.csproj` в Visual Studio.
2. Она сама подтянет нужные NuGet-пакеты (CmlLib.Core и т.д.) при первой сборке.
3. В `UpdateManager.cs` поправь константу `ManifestUrl` на свою реальную ссылку:
   ```csharp
   private const string ManifestUrl =
       "https://cdn.jsdelivr.org/gh/твой_ник/wormcraft-launcher@main/manifest.json";
   ```
4. Собери (Build → Build Solution), затем Build → Publish для готового `.exe`,
   либо просто запусти Debug/Release сборку — `WormcraftLauncher.exe` появится в
   `Launcher/bin/Release/net6.0-windows/`.

### Через консоль
```bash
cd Launcher
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```
Готовый `.exe` будет в `Launcher/bin/Release/net6.0-windows/win-x64/publish/`.

## Шаг 5. Раздать лаунчер игрокам

Залей собранный `WormcraftLauncher.exe` куда угодно (Яндекс.Диск, Google Drive,
тот же GitHub в Releases) и дай игрокам ссылку на скачивание.

При первом запуске игрок:
1. Вводит любой никнейм (офлайн-режим, как в TLauncher — без лицензии).
2. Жмёт «ИГРАТЬ» — лаунчер скачивает Forge 1.19.2, ставит его,
   скачивает твою сборку модов с jsDelivr, и запускает игру с автоподключением
   к `wormcraft.20tps.ru`.

При каждом следующем запуске лаунчер сравнивает версию сборки с той,
что в манифесте, и докачивает только изменения.

## Важные нюансы

- **Forge версия.** В `generate_manifest.py` и логике лаунчера используется формат
  `1.19.2-43.4.0`. Проверь точную версию форджа, которая стоит на твоём сервере,
  и впиши именно её в обоих местах.
- **Java.** CmlLib сам подтянет подходящую Java через систему Mojang, отдельно ставить
  не нужно.
- **Авторизация.** Сейчас офлайн-режим (`MSession.CreateOfflineSession`) — ник не
  проверяется, как просил. Библиотека для Microsoft-авторизации уже подключена
  в проект на будущее, просто не используется.
- **Лимиты GitHub/jsDelivr.** Каждый файл мода — до 100 МБ (более чем достаточно),
  суммарный размер репозитория — желательно не больше пары ГБ. jsDelivr — бесплатный
  публичный CDN, без скрытых платежей.
- **Защита от лишних скачиваний.** Манифест хранит SHA1-хеши всех файлов, поэтому
  при повторном запуске лаунчер не перекачивает то, что не менялось — только diff.

