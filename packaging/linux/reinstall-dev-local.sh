#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
APP_DIR="${1:-$ROOT_DIR/dist/linux}"
APP_BIN="$APP_DIR/ppgram"
DESKTOP_SRC="$ROOT_DIR/packaging/linux/ppgram.desktop"
DESKTOP_DST_DIR="$HOME/.local/share/applications"
DESKTOP_DST="$DESKTOP_DST_DIR/ppgram.desktop"
ICON_DST_BASE="$HOME/.local/share/icons/hicolor"
ICON_SRC_DIR="$ROOT_DIR/packaging/icons"

escape_desktop_exec() {
  local value="$1"
  value="${value//\\/\\\\}"
  value="${value// /\\ }"
  value="${value//\"/\\\"}"
  value="${value//\$/\\\$}"
  value="${value//\`/\\\`}"
  printf '%s' "$value"
}

cleanup_old_paths() {
  rm -f -- "$DESKTOP_DST"
  for size in 16 32 48 64 128 256; do
    rm -f -- "$ICON_DST_BASE/${size}x${size}/apps/ppgram.png"
  done
}

install_current_sources() {
  if [[ ! -x "$APP_BIN" ]]; then
    echo "error: expected executable at $APP_BIN" >&2
    exit 1
  fi

  mkdir -p "$DESKTOP_DST_DIR"

  local app_bin_escaped
  app_bin_escaped="$(escape_desktop_exec "$APP_BIN")"
  sed "s|^Exec=.*$|Exec=$app_bin_escaped|" "$DESKTOP_SRC" > "$DESKTOP_DST"

  for size in 16 32 48 64 128 256; do
    local src_png="$ICON_SRC_DIR/ppgram_${size}.png"
    if [[ ! -f "$src_png" ]]; then
      echo "error: missing packaged icon $src_png" >&2
      exit 1
    fi

    local dst_dir="$ICON_DST_BASE/${size}x${size}/apps"
    mkdir -p "$dst_dir"
    cp "$src_png" "$dst_dir/ppgram.png"
  done
}

cleanup_old_paths
install_current_sources

update-desktop-database "$DESKTOP_DST_DIR" >/dev/null 2>&1 || true
gtk-update-icon-cache "$ICON_DST_BASE" >/dev/null 2>&1 || true
echo "reinstalled desktop integration for ppgram: $DESKTOP_DST"
