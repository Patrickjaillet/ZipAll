#Requires -Version 5.1
<#
    Build script for ZipAll.

    Usage:
      .\build.ps1                 # debug build
      .\build.ps1 -Publish        # self-contained, single-file release publish
      .\build.ps1 -Publish -BuildNumber 123   # override the auto build number (e.g. from CI)
#>

param(
    [switch]$Publish,
    [switch]$Installer,
    [string]$BuildNumber
)

$ErrorActionPreference = "Stop"

$root        = $PSScriptRoot
$project     = Join-Path $root "src\ZipAll\ZipAll.csproj"
$publishDir  = Join-Path $root "publish"
$installerIss = Join-Path $root "installer\ZipAll.iss"
$distDir     = Join-Path $root "dist"

$versionArg = @()
if ($BuildNumber) {
    $versionArg = @("/p:BuildNumber=$BuildNumber")
}

if ($Installer) {
    # The installer always packages a fresh self-contained publish output.
    Write-Host "Publishing ZipAll (self-contained, single-file, win-x64, Release)..." -ForegroundColor Cyan
    dotnet publish $project `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDir `
        @versionArg
    Write-Host "Published to: $publishDir" -ForegroundColor Green

    $iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if (-not $iscc) {
        throw "ISCC.exe (Inno Setup Compiler) was not found on PATH. Install Inno Setup 6 from https://jrsoftware.org/isinfo.php and re-run with -Installer."
    }

    # Read the version that was just embedded in the published executable so
    # the installer's version always matches the app it packages.
    $exePath = Join-Path $publishDir "ZipAll.exe"
    $fileVersion = (Get-Item $exePath).VersionInfo.FileVersion
    if (-not $fileVersion) {
        throw "Could not read the file version from $exePath"
    }

    Write-Host "Building installer (Inno Setup) for version $fileVersion..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $distDir | Out-Null
    & $iscc.Source "/DMyAppVersion=$fileVersion" $installerIss
    Write-Host "Installer written to: $distDir" -ForegroundColor Green
}
elseif ($Publish) {
    Write-Host "Publishing ZipAll (self-contained, single-file, win-x64, Release)..." -ForegroundColor Cyan
    dotnet publish $project `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDir `
        @versionArg
    Write-Host "Published to: $publishDir" -ForegroundColor Green
}
else {
    Write-Host "Building ZipAll (Debug)..." -ForegroundColor Cyan
    dotnet build $project -c Debug @versionArg
}
