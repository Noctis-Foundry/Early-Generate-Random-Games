#!/usr/bin/env bash

OWNER="Noctis-Foundry"
REPO="Early-Generate-Random-Games"
TAG="v0.4.0"
FILE_PATTERN="ChallangeRunner-x86_64-.*\.AppImage"
OUTPUT_FILE="ChallangeRunner.AppImage"

mkdir -p /tmp/challange_runner_tmp
cd /tmp/challange_runner_tmp

DOWNLOAD_URL=$(curl -sL "https://api.github.com/repos/${OWNER}/${REPO}/releases/tags/${TAG}" | \
  jq -r --arg pat "$FILE_PATTERN" '.assets[] | select(.name | test($pat)) | .browser_download_url')

if [[ -z "$DOWNLOAD_URL" || "$DOWNLOAD_URL" == "null" ]]; then
  echo "Ошибка: не удалось найти в релизе ${TAG} файл, соответствующий шаблону '${FILE_PATTERN}'." >&2
  exit 1
fi

echo "Скачивание: $DOWNLOAD_URL"
echo "Файл будет сохранён в: $(pwd)/$OUTPUT_FILE"
curl -L -o "$OUTPUT_FILE" "DOWNLOAD_URL"
chmod +x "$OUTPUT_FILE"

mkdir -p ~/.local/share/ChallangeRunner
mv -p "$OUTPUT_FILE" $HOME/.local/share/ChallangeRunner.AppImage




