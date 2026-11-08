#!/usr/bin/env bash

# 1. Переменные
OWNER="Noctis-Foundry"
REPO="Early-Generate-Random-Games"
TAG="v0.4.0"
FILE_PATTERN="ChallangeRunner-x86_64-.*\.AppImage"
OUTPUT_FILE="ChallangeRunner.AppImage"

# Определяем пути
BIN_DIR="$HOME/.local/share"
APPS_DIR="$HOME/.local/share/applications"
DESKTOP_FILE="$APPS_DIR/ChallangeRunner.desktop"
FINAL_APPIMAGE="$BIN_DIR/ChallangeRunner.AppImage"

# Создаём необходимые директории
mkdir -p /tmp/challange_runner_tmp
mkdir -p "$APPS_DIR"
cd /tmp/challange_runner_tmp

# 2. Поиск URL
DOWNLOAD_URL=$(curl -sL "https://api.github.com/repos/${OWNER}/${REPO}/releases/tags/${TAG}" | \
  jq -r --arg pat "$FILE_PATTERN" '.assets[] | select(.name | test($pat)) | .browser_download_url')

if [[ -z "$DOWNLOAD_URL" || "$DOWNLOAD_URL" == "null" ]]; then
  echo "Ошибка: не удалось найти в релизе ${TAG} файл, соответствующий шаблону '${FILE_PATTERN}'." >&2
  exit 1
fi

# 3. Скачивание (ИСПРАВЛЕНА ОПЕЧАТКА С $)
echo "Скачивание: $DOWNLOAD_URL"
echo "Файл будет сохранён в: $(pwd)/$OUTPUT_FILE"
curl -L -o "$OUTPUT_FILE" "$DOWNLOAD_URL"
chmod +x "$OUTPUT_FILE"

# 4. Генерация .desktop файла
cat > "$DESKTOP_FILE" <<EOF
[Desktop Entry]
Type=Application
Name=ChallangeRunner
Comment=Generating events for friends party
Exec=$FINAL_APPIMAGE %U
Icon=$BIN_DIR/ChallangeRunner.png
Terminal=false
Categories=Utility;
EOF
chmod +x "$DESKTOP_FILE" # Делаем .desktop исполняемым

# 5. Перемещение AppImage
echo "Установка в $FINAL_APPIMAGE"
mv "$OUTPUT_FILE" "$FINAL_APPIMAGE"

# 6. Скачивание иконки (через raw URL)
echo "Скачивание иконки..."
curl -L -o "$BIN_DIR/ChallangeRunner.png" "https://raw.githubusercontent.com/Noctis-Foundry/Early-Generate-Random-Games/refs/heads/main/appimage_setup/ChallangeRunner.png"

# 7. Обновление кэша приложений (чтобы rofi/меню сразу увидели программу)
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$APPS_DIR" >/dev/null 2>&1 || true
fi

echo "Установка завершена успешно!"
