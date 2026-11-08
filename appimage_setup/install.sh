#!/usr/bin/env bash

# Property
OWNER="Noctis-Foundry"
REPO="Early-Generate-Random-Games"
TAG="v0.4.0"
FILE_PATTERN="ChallangeRunner-x86_64-.*\.AppImage"
OUTPUT_FILE="ChallangeRunner.AppImage"

# Paths
BIN_DIR="$HOME/.local/share"
APPS_DIR="$HOME/.local/share/applications"
DESKTOP_FILE="$APPS_DIR/ChallangeRunner.desktop"
FINAL_APPIMAGE="$BIN_DIR/ChallangeRunner.AppImage"

# Creating dirs
mkdir -p /tmp/challange_runner_tmp
mkdir -p "$APPS_DIR"
cd /tmp/challange_runner_tmp

# Search url
DOWNLOAD_URL=$(curl -sL "https://api.github.com/repos/${OWNER}/${REPO}/releases/tags/${TAG}" | \
  jq -r --arg pat "$FILE_PATTERN" '.assets[] | select(.name | test($pat)) | .browser_download_url')

if [[ -z "$DOWNLOAD_URL" || "$DOWNLOAD_URL" == "null" ]]; then
  echo "Error: Failed to file with tag ${TAG} file, with pattern '${FILE_PATTERN}'." >&2
  exit 1
fi

# Download app image
echo "Download: $DOWNLOAD_URL"
echo "File will saving to: $(pwd)/$OUTPUT_FILE"
curl -L -o "$OUTPUT_FILE" "$DOWNLOAD_URL"
chmod +x "$OUTPUT_FILE"

# Genereting desktop file
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
chmod +x "$DESKTOP_FILE"

# Move AppImage
echo "Install to $FINAL_APPIMAGE"
mv "$OUTPUT_FILE" "$FINAL_APPIMAGE"

# Install icon
echo "Install icon..."
curl -L -o "$BIN_DIR/ChallangeRunner.png" "https://raw.githubusercontent.com/Noctis-Foundry/Early-Generate-Random-Games/refs/heads/main/appimage_setup/ChallangeRunner.png"

# Update apps database
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$APPS_DIR" >/dev/null 2>&1 || true
fi

echo "Install completed!"
