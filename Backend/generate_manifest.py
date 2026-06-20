"""
Wormcraft Launcher — генератор manifest.json (локальная версия для GitHub)

Запускается у тебя на компьютере перед каждым git push, когда меняешь моды/конфиги.

Структура папки files/ — точная копия того, что должно оказаться в .minecraft:
    files/
      mods/...
      config/...
      shaderpacks/...
      scripts/...
      emotes/...
      resourcepacks/...
      (и любые другие папки, какие у тебя есть в сборке — лаунчер скопирует их все,
       автоматически, без необходимости что-то прописывать в коде)

Использование:
    python generate_manifest.py

Требования: только Python 3 (без сторонних библиотек).
"""

import hashlib
import json
import os
from datetime import datetime, timezone

# ===================== НАСТРОЙКИ =====================

# Твой GitHub юзернейм и название репозитория
GITHUB_USER = "ARIZYS"
GITHUB_REPO = "wWORMPACK"
GITHUB_BRANCH = "main"

# Версия Forge, которая стоит на сервере (поправь под реальную)
FORGE_VERSION = "1.19.2-43.5.2"
MINECRAFT_VERSION = "1.19.2"

# IP сервера для автоподключения
SERVER_IP = "wormcraft.20tps.ru"

# =======================================================

ROOT = os.path.dirname(os.path.abspath(__file__))
FILES_DIR = os.path.join(ROOT, "files")
OUTPUT = os.path.join(ROOT, "manifest.json")

BASE_URL = f"https://cdn.jsdelivr.org/gh/{GITHUB_USER}/{GITHUB_REPO}@{GITHUB_BRANCH}/files/"


def sha1_of_file(path: str) -> str:
    h = hashlib.sha1()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()


def scan_files(base_dir: str):
    result = []
    for dirpath, _, filenames in os.walk(base_dir):
        for name in filenames:
            full_path = os.path.join(dirpath, name)
            rel_path = os.path.relpath(full_path, base_dir).replace("\\", "/")
            result.append({
                "path": rel_path,
                "sha1": sha1_of_file(full_path),
                "size": os.path.getsize(full_path),
            })
    return result


def main():
    if not os.path.isdir(FILES_DIR):
        print(f"Папка {FILES_DIR} не найдена. Создай её и положи туда mods/ и config/")
        return

    file_list = scan_files(FILES_DIR)

    manifest = {
        "buildVersion": datetime.now(timezone.utc).strftime("%Y.%m.%d.%H%M%S"),
        "minecraft": MINECRAFT_VERSION,
        "forge": FORGE_VERSION,
        "serverIp": SERVER_IP,
        "baseUrl": BASE_URL,
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "files": file_list,
    }

    with open(OUTPUT, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    print(f"OK. Манифест обновлён: {OUTPUT}")
    print(f"Файлов в сборке: {len(file_list)}")
    print(f"Версия: {manifest['buildVersion']}")
    print()
    print("Теперь сделай:")
    print("  git add .")
    print(f'  git commit -m "update build {manifest["buildVersion"]}"')
    print("  git push")


if __name__ == "__main__":
    main()
