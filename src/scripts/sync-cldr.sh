#!/bin/bash
set -e

# Script to sync CLDR data with caching support
# Handles downloading, extracting, and calling the generator

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="$(dirname "$SCRIPT_DIR")"
REPO_ROOT="$(dirname "$SRC_DIR")"
GENERATOR_DIR="$SRC_DIR/Rh.MessageFormat.CldrGenerator"

# Predefined output directory
OUTPUT_DIR="$SRC_DIR/Rh.MessageFormat.CldrData/Generated"

# Default working directory (OS temp folder + subfolder)
if [[ -n "$TMPDIR" ]]; then
    # Remove trailing slash from TMPDIR if present
    TMPDIR_CLEAN="${TMPDIR%/}"
    DEFAULT_WORKING_DIR="$TMPDIR_CLEAN/rh-messageformat-cldr"
else
    DEFAULT_WORKING_DIR="/tmp/rh-messageformat-cldr"
fi

# GitHub URLs
GITHUB_API_URL="https://api.github.com/repos/unicode-org/cldr-json/releases/latest"
ARCHIVE_URL_PATTERN="https://github.com/unicode-org/cldr-json/archive/refs/tags/{version}.zip"

# Parse arguments
VERSION=""
WORKING_DIR=""
LOCALES=""
KEEP_FILES="true"  # Default: keep files for caching
FORCE="false"
CLDR_ROOT=""

show_help() {
    echo "Usage: sync-cldr.sh [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -v, --version VERSION    CLDR version (e.g., '46.0.0' or 'v46.0.0')"
    echo "                           Default: fetches latest from GitHub API"
    echo "  -l, --locales LOCALES    Comma-separated list of locales to include"
    echo "                           Default: all .NET-supported locales"
    echo "  -w, --working-dir DIR    Working directory for downloads/extraction"
    echo "                           Default: \$TMPDIR/rh-messageformat-cldr/"
    echo "  -k, --keep-files         Keep downloaded archive after generation (default: true)"
    echo "  --no-keep-files          Delete downloaded archive after generation"
    echo "  -f, --force              Force re-download and re-extract (removes cached data)"
    echo "  -r, --cldr-root DIR      Use pre-extracted CLDR data (skip download/unpack)"
    echo "  -h, --help               Show this help message"
    echo ""
    echo "Output directory is predefined: $OUTPUT_DIR"
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -l|--locales)
            LOCALES="$2"
            shift 2
            ;;
        -w|--working-dir)
            WORKING_DIR="$2"
            shift 2
            ;;
        -k|--keep-files)
            KEEP_FILES="true"
            shift
            ;;
        --no-keep-files)
            KEEP_FILES="false"
            shift
            ;;
        -f|--force)
            FORCE="true"
            shift
            ;;
        -r|--cldr-root)
            CLDR_ROOT="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Resolve working directory
if [[ -z "$WORKING_DIR" ]]; then
    WORKING_DIR="$DEFAULT_WORKING_DIR"
fi

# Create working directory if it doesn't exist
mkdir -p "$WORKING_DIR"

echo "Working directory: $WORKING_DIR"
echo "Output directory: $OUTPUT_DIR"

# Function to fetch latest version from GitHub API
fetch_latest_version() {
    echo "Fetching latest CLDR version from GitHub..." >&2
    local response
    response=$(curl -s -H "User-Agent: Rh.MessageFormat.CldrGenerator" "$GITHUB_API_URL")
    local tag_name
    tag_name=$(echo "$response" | grep -o '"tag_name"[[:space:]]*:[[:space:]]*"[^"]*"' | sed 's/.*"tag_name"[[:space:]]*:[[:space:]]*"\([^"]*\)"/\1/')

    if [[ -z "$tag_name" ]]; then
        echo "Error: Failed to fetch latest version from GitHub API" >&2
        exit 1
    fi

    echo "Latest CLDR version: $tag_name" >&2
    echo "$tag_name"
}

# Function to normalize version (remove leading 'v' if present for folder names)
normalize_version() {
    local version="$1"
    # Remove leading 'v' for consistency in folder names
    echo "${version#v}"
}

# If user provided pre-extracted data, use it directly
if [[ -n "$CLDR_ROOT" ]]; then
    if [[ ! -d "$CLDR_ROOT" ]]; then
        echo "Error: CLDR root directory does not exist: $CLDR_ROOT"
        exit 1
    fi
    echo "Using pre-extracted CLDR data: $CLDR_ROOT"
else
    # Fetch version if not specified
    if [[ -z "$VERSION" ]]; then
        VERSION=$(fetch_latest_version)
    else
        echo "Using specified CLDR version: $VERSION"
    fi

    # Normalize version for paths (without 'v' prefix)
    NORMALIZED_VERSION=$(normalize_version "$VERSION")

    # Determine paths
    ARCHIVE_FILE="cldr-json-$VERSION.zip"
    ARCHIVE_PATH="$WORKING_DIR/$ARCHIVE_FILE"
    VERSION_DIR="$WORKING_DIR/$NORMALIZED_VERSION"
    EXTRACTED_FOLDER="cldr-json-$NORMALIZED_VERSION"
    # CLDR root is the extracted folder itself (appsettings.json paths include cldr-json/ prefix)
    CLDR_ROOT="$VERSION_DIR/$EXTRACTED_FOLDER"

    # Handle --force option: remove cached data
    if [[ "$FORCE" == "true" ]]; then
        if [[ -f "$ARCHIVE_PATH" ]]; then
            echo "Force mode: Removing cached archive: $ARCHIVE_PATH"
            rm -f "$ARCHIVE_PATH"
        fi
        if [[ -d "$VERSION_DIR" ]]; then
            echo "Force mode: Removing cached extraction: $VERSION_DIR"
            rm -rf "$VERSION_DIR"
        fi
    fi

    # Check if already extracted
    if [[ -d "$CLDR_ROOT" ]]; then
        echo "Using cached CLDR data: $CLDR_ROOT"
    else
        # Check if archive exists
        if [[ -f "$ARCHIVE_PATH" ]]; then
            echo "Archive found: $ARCHIVE_PATH"
        else
            # Download archive
            DOWNLOAD_URL="${ARCHIVE_URL_PATTERN//\{version\}/$VERSION}"
            echo "Downloading CLDR $VERSION from $DOWNLOAD_URL..."
            curl -L -o "$ARCHIVE_PATH" "$DOWNLOAD_URL"
            echo "Downloaded to $ARCHIVE_PATH"
        fi

        # Extract archive
        echo "Extracting to $VERSION_DIR..."
        mkdir -p "$VERSION_DIR"
        unzip -q -o "$ARCHIVE_PATH" -d "$VERSION_DIR"
        echo "Extracted to $CLDR_ROOT"

        # Cleanup archive if --no-keep-files
        if [[ "$KEEP_FILES" == "false" ]]; then
            echo "Removing archive: $ARCHIVE_PATH"
            rm -f "$ARCHIVE_PATH"
        fi
    fi
fi

# Build command arguments for the generator
CMD_ARGS=(
    --cldr-root "$CLDR_ROOT"
    --output "$OUTPUT_DIR"
)

if [[ -n "$LOCALES" ]]; then
    CMD_ARGS+=(--locales "$LOCALES")
fi

# Run the .NET generator
echo ""
echo "Running CLDR generator..."
dotnet run --project "$GENERATOR_DIR" -- "${CMD_ARGS[@]}"

echo ""
echo "CLDR sync completed."
