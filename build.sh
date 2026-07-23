#!/usr/bin/env bash
# Build script for ZipAll.
#
# Usage:
#   ./build.sh                     # debug build
#   ./build.sh --publish           # self-contained, single-file release publish (win-x64)
#   ./build.sh --publish --build-number 123   # override the auto build number (e.g. from CI)
#   ./build.sh --installer         # publish, then build the Inno Setup installer (requires
#                                   # Windows + Inno Setup 6 with ISCC on PATH, or ISCC under Wine)

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT_DIR/src/ZipAll/ZipAll.csproj"
PUBLISH_DIR="$ROOT_DIR/publish"
INSTALLER_ISS="$ROOT_DIR/installer/ZipAll.iss"
DIST_DIR="$ROOT_DIR/dist"

PUBLISH=false
INSTALLER=false
BUILD_NUMBER=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --publish) PUBLISH=true; shift ;;
        --installer) INSTALLER=true; shift ;;
        --build-number) BUILD_NUMBER="$2"; shift 2 ;;
        *) echo "Unknown argument: $1"; exit 1 ;;
    esac
done

VERSION_ARG=()
if [[ -n "$BUILD_NUMBER" ]]; then
    VERSION_ARG=(/p:BuildNumber="$BUILD_NUMBER")
fi

do_publish() {
    echo "Publishing ZipAll (self-contained, single-file, win-x64, Release)..."
    dotnet publish "$PROJECT" \
        -c Release \
        -r win-x64 \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o "$PUBLISH_DIR" \
        "${VERSION_ARG[@]}"
    echo "Published to: $PUBLISH_DIR"
}

if [[ "$INSTALLER" == true ]]; then
    do_publish

    if ! command -v ISCC.exe >/dev/null 2>&1 && ! command -v iscc >/dev/null 2>&1; then
        echo "ISCC (Inno Setup Compiler) was not found on PATH."
        echo "Install Inno Setup 6 (https://jrsoftware.org/isinfo.php) on Windows and re-run --installer,"
        echo "or run it under Wine on Linux/macOS."
        exit 1
    fi
    ISCC_BIN="$(command -v ISCC.exe || command -v iscc)"

    # Read the version that was just embedded in the .NET assembly metadata,
    # so the installer's version always matches the app it packages.
    ASSEMBLY_INFO=$(dotnet build "$PROJECT" -c Release "${VERSION_ARG[@]}" -getProperty:Version 2>/dev/null || true)
    FILE_VERSION="${ASSEMBLY_INFO:-0.8.0.0}"

    mkdir -p "$DIST_DIR"
    echo "Building installer (Inno Setup) for version $FILE_VERSION..."
    "$ISCC_BIN" "/DMyAppVersion=$FILE_VERSION" "$INSTALLER_ISS"
    echo "Installer written to: $DIST_DIR"
elif [[ "$PUBLISH" == true ]]; then
    do_publish
else
    echo "Building ZipAll (Debug)..."
    dotnet build "$PROJECT" -c Debug "${VERSION_ARG[@]}"
fi
