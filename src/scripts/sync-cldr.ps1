# Script to sync CLDR data with caching support
# Handles downloading, extracting, and calling the generator

param(
    [Parameter(HelpMessage = "CLDR version (e.g., '46.0.0' or 'v46.0.0'). Default: fetches latest from GitHub API")]
    [string]$Version,

    [Parameter(HelpMessage = "Comma-separated list of locales to include. Default: all .NET-supported locales")]
    [string]$Locales,

    [Parameter(HelpMessage = "Working directory for downloads/extraction. Default: temp folder")]
    [string]$WorkingDir,

    [Parameter(HelpMessage = "Keep downloaded archive after generation (default: true)")]
    [switch]$KeepFiles,

    [Parameter(HelpMessage = "Delete downloaded archive after generation")]
    [switch]$NoKeepFiles,

    [Parameter(HelpMessage = "Force re-download and re-extract (removes cached data)")]
    [switch]$Force,

    [Parameter(HelpMessage = "Use pre-extracted CLDR data (skip download/unpack)")]
    [string]$CldrRoot,

    [Parameter(HelpMessage = "Show help message")]
    [switch]$Help
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SrcDir = Split-Path -Parent $ScriptDir
$GeneratorDir = Join-Path $SrcDir "Rh.MessageFormat.CldrGenerator"

# Predefined output directory
$OutputDir = Join-Path $SrcDir "Rh.MessageFormat.CldrData" "Generated"

# Default working directory (temp folder + subfolder)
# TrimEnd ensures no double slashes if temp path has trailing separator
$TempPath = ([System.IO.Path]::GetTempPath()).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
$DefaultWorkingDir = Join-Path $TempPath "rh-messageformat-cldr"

# GitHub URLs
$GitHubApiUrl = "https://api.github.com/repos/unicode-org/cldr-json/releases/latest"
$ArchiveUrlPattern = "https://github.com/unicode-org/cldr-json/archive/refs/tags/{version}.zip"

function Show-Help {
    Write-Host "Usage: sync-cldr.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Version VERSION       CLDR version (e.g., '46.0.0' or 'v46.0.0')"
    Write-Host "                         Default: fetches latest from GitHub API"
    Write-Host "  -Locales LOCALES       Comma-separated list of locales to include"
    Write-Host "                         Default: all .NET-supported locales"
    Write-Host "  -WorkingDir DIR        Working directory for downloads/extraction"
    Write-Host "                         Default: `$env:TEMP\rh-messageformat-cldr\"
    Write-Host "  -KeepFiles             Keep downloaded archive after generation (default: true)"
    Write-Host "  -NoKeepFiles           Delete downloaded archive after generation"
    Write-Host "  -Force                 Force re-download and re-extract (removes cached data)"
    Write-Host "  -CldrRoot DIR          Use pre-extracted CLDR data (skip download/unpack)"
    Write-Host "  -Help                  Show this help message"
    Write-Host ""
    Write-Host "Output directory is predefined: $OutputDir"
}

if ($Help) {
    Show-Help
    exit 0
}

# Default keep files to true unless -NoKeepFiles is specified
$ShouldKeepFiles = -not $NoKeepFiles

# Resolve working directory
if (-not $WorkingDir) {
    $WorkingDir = $DefaultWorkingDir
}

# Create working directory if it doesn't exist
if (-not (Test-Path $WorkingDir)) {
    New-Item -ItemType Directory -Path $WorkingDir -Force | Out-Null
}

Write-Host "Working directory: $WorkingDir"
Write-Host "Output directory: $OutputDir"

# Function to fetch latest version from GitHub API
function Get-LatestVersion {
    Write-Host "Fetching latest CLDR version from GitHub..."

    $headers = @{
        "User-Agent" = "Rh.MessageFormat.CldrGenerator"
    }

    try {
        $response = Invoke-RestMethod -Uri $GitHubApiUrl -Headers $headers -Method Get
        $tagName = $response.tag_name

        if (-not $tagName) {
            throw "Failed to get tag_name from response"
        }

        Write-Host "Latest CLDR version: $tagName"
        return $tagName
    }
    catch {
        Write-Error "Error: Failed to fetch latest version from GitHub API: $_"
        exit 1
    }
}

# Function to normalize version (remove leading 'v' if present for folder names)
function Get-NormalizedVersion {
    param([string]$Ver)
    if ($Ver.StartsWith("v")) {
        return $Ver.Substring(1)
    }
    return $Ver
}

# If user provided pre-extracted data, use it directly
if ($CldrRoot) {
    if (-not (Test-Path $CldrRoot -PathType Container)) {
        Write-Error "Error: CLDR root directory does not exist: $CldrRoot"
        exit 1
    }
    Write-Host "Using pre-extracted CLDR data: $CldrRoot"
}
else {
    # Fetch version if not specified
    if (-not $Version) {
        $Version = Get-LatestVersion
    }
    else {
        Write-Host "Using specified CLDR version: $Version"
    }

    # Normalize version for paths (without 'v' prefix)
    $NormalizedVersion = Get-NormalizedVersion -Ver $Version

    # Determine paths
    $ArchiveFile = "cldr-json-$Version.zip"
    $ArchivePath = Join-Path $WorkingDir $ArchiveFile
    $VersionDir = Join-Path $WorkingDir $NormalizedVersion
    $ExtractedFolder = "cldr-json-$NormalizedVersion"
    # CLDR root is the extracted folder itself (appsettings.json paths include cldr-json/ prefix)
    $CldrRoot = Join-Path $VersionDir $ExtractedFolder

    # Handle -Force: remove cached data
    if ($Force) {
        if (Test-Path $ArchivePath) {
            Write-Host "Force mode: Removing cached archive: $ArchivePath"
            Remove-Item -Path $ArchivePath -Force
        }
        if (Test-Path $VersionDir) {
            Write-Host "Force mode: Removing cached extraction: $VersionDir"
            Remove-Item -Path $VersionDir -Recurse -Force
        }
    }

    # Check if already extracted
    if (Test-Path $CldrRoot -PathType Container) {
        Write-Host "Using cached CLDR data: $CldrRoot"
    }
    else {
        # Check if archive exists
        if (Test-Path $ArchivePath) {
            Write-Host "Archive found: $ArchivePath"
        }
        else {
            # Download archive
            $DownloadUrl = $ArchiveUrlPattern -replace "\{version\}", $Version
            Write-Host "Downloading CLDR $Version from $DownloadUrl..."

            try {
                Invoke-WebRequest -Uri $DownloadUrl -OutFile $ArchivePath -UseBasicParsing
                Write-Host "Downloaded to $ArchivePath"
            }
            catch {
                Write-Error "Error: Failed to download archive: $_"
                exit 1
            }
        }

        # Extract archive
        Write-Host "Extracting to $VersionDir..."
        if (-not (Test-Path $VersionDir)) {
            New-Item -ItemType Directory -Path $VersionDir -Force | Out-Null
        }

        try {
            Expand-Archive -Path $ArchivePath -DestinationPath $VersionDir -Force
            Write-Host "Extracted to $CldrRoot"
        }
        catch {
            Write-Error "Error: Failed to extract archive: $_"
            exit 1
        }

        # Cleanup archive if -NoKeepFiles
        if (-not $ShouldKeepFiles) {
            Write-Host "Removing archive: $ArchivePath"
            Remove-Item -Path $ArchivePath -Force
        }
    }
}

# Build command arguments for the generator
$cmdArgs = @(
    "--cldr-root", $CldrRoot,
    "--output", $OutputDir
)

if ($Locales) {
    $cmdArgs += "--locales", $Locales
}

# Run the .NET generator
Write-Host ""
Write-Host "Running CLDR generator..."
dotnet run --project $GeneratorDir -- @cmdArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: Generator failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "CLDR sync completed."
