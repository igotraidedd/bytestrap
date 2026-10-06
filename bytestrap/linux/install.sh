#!/usr/bin/env bash
#
# Bytestrap for Linux - build & install script
#
#   ./linux/install.sh                    build + install to ~/.local
#   ./linux/install.sh --self-contained   bundle the .NET runtime (for PCs without dotnet)
#   ./linux/install.sh --prefix /usr/local --system
#                                         system-wide install (needs sudo)
#   ./linux/install.sh --uninstall         remove installed files
#
set -euo pipefail

PREFIX="${HOME}/.local"
SELF_CONTAINED=0
UNINSTALL=0

while [ $# -gt 0 ]; do
    case "$1" in
        --prefix)      PREFIX="$2"; shift 2 ;;
        --self-contained) SELF_CONTAINED=1; shift ;;
        --system)      PREFIX="/usr/local"; shift ;;
        --uninstall)   UNINSTALL=1; shift ;;
        -h|--help)
            sed -n '2,/^$/p' "$0" | sed 's/^# \?//'
            exit 0 ;;
        *) echo "unknown option: $1" >&2; exit 2 ;;
    esac
done

BIN_DIR="${PREFIX}/bin"
DESKTOP_DIR="${PREFIX}/share/applications"
ICON_DIR="${PREFIX}/share/icons"
PIXMAP_DIR="${PREFIX}/share/pixmaps"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

if [ "${UNINSTALL}" = 1 ]; then
    rm -f "${BIN_DIR}/bytestrap" "${DESKTOP_DIR}/bytestrap.desktop" \
          "${ICON_DIR}/bytestrap.png" "${PIXMAP_DIR}/bytestrap.png"
    echo "uninstalled bytestrap from ${PREFIX}"
    exit 0
fi

if ! command -v dotnet >/dev/null 2>&1; then
    echo "error: the .NET 8 SDK is required to build (https://dot.net)" >&2
    exit 1
fi

echo "==> building bytestrap (linux port)..."

PUBLISH_ARGS=(
    "${REPO_DIR}/Bytestrap.Linux/Bytestrap.Linux.csproj"
    -c Release
    -o "${REPO_DIR}/linux/publish"
)

if [ "${SELF_CONTAINED}" = 1 ]; then
    PUBLISH_ARGS+=(
        --self-contained true
        -r linux-x64
        -p:PublishSingleFile=true
        -p:PublishReadyToRun=true
        -p:IncludeNativeLibrariesForSelfExtract=true
    )
else
    PUBLISH_ARGS+=(--self-contained false)
fi

dotnet publish "${PUBLISH_ARGS[@]}"

echo "==> installing to ${PREFIX}..."
mkdir -p "${BIN_DIR}" "${DESKTOP_DIR}" "${ICON_DIR}"

install -m 755 "${REPO_DIR}/linux/publish/bytestrap" "${BIN_DIR}/bytestrap"

# desktop entry (points at the installed binary)
sed "s|^Exec=.*|Exec=${BIN_DIR}/bytestrap launch|" \
    "${REPO_DIR}/linux/bytestrap.desktop" > "${DESKTOP_DIR}/bytestrap.desktop"

# icon (fall back gracefully when the repo art is missing)
if [ -f "${REPO_DIR}/byteblox.png" ]; then
    install -m 644 "${REPO_DIR}/byteblox.png" "${ICON_DIR}/bytestrap.png"
fi

if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "${DESKTOP_DIR}" >/dev/null 2>&1 || true
fi

echo
echo "installed! make sure ${BIN_DIR} is on your PATH:"
echo "  export PATH=\"\$PATH:${BIN_DIR}\""
echo
echo "then run:  bytestrap status"
