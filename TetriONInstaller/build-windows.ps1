#!/usr/bin/env pwsh

param(
    [string]$Platform = "windows-x64",
    [string]$OutputPath = ".\dist\windows",
    [string]$BuildsPath = "..\builds"
)

Write-Host "Building TetriON Windows Installer..." -ForegroundColor Green

# Validate that the platform build exists
$platformBuildPath = Join-Path $BuildsPath $Platform
if (-not (Test-Path $platformBuildPath)) {
    Write-Error "Platform build not found at: $platformBuildPath"
    Write-Error "Please run the build-all script first to create platform builds"
    exit 1
}

# Clean previous builds
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

Write-Host "Using platform build from: $platformBuildPath" -ForegroundColor Yellow

# Create platform-specific game files archive
Write-Host "Creating Windows game files archive..." -ForegroundColor Yellow
$archivePath = ".\game_files_windows.zip"

if (Test-Path $archivePath) {
    Remove-Item $archivePath
}

# Include all files from the windows-x64 build
$itemsToInclude = Get-ChildItem -Path $platformBuildPath -Recurse
if ($itemsToInclude.Count -eq 0) {
    Write-Error "No files found in platform build directory: $platformBuildPath"
    exit 1
}

# Create archive with all Windows build files
Compress-Archive -Path "$platformBuildPath\*" -DestinationPath $archivePath -CompressionLevel Optimal

Write-Host "Archive created with $($itemsToInclude.Count) items" -ForegroundColor Cyan

# Update the project file to use the platform-specific archive
Write-Host "Updating project file for Windows platform..." -ForegroundColor Yellow
$projectFile = "TetriONInstaller.csproj"
$projectContent = Get-Content $projectFile -Raw

# Replace the embedded resource name
$newProjectContent = $projectContent -replace 'game_files\.zip', 'game_files_windows.zip'
Set-Content -Path $projectFile -Value $newProjectContent

# Build installer (Framework-dependent - requires .NET 8.0 on target machine)
Write-Host "Building Windows installer..." -ForegroundColor Yellow
$buildResult = dotnet publish -c Release -p:PublishSingleFile=true --self-contained false -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Installer build failed"
    # Restore original project file
    Set-Content -Path $projectFile -Value $projectContent
    exit 1
}

# Build uninstaller (Framework-dependent - requires .NET 8.0 on target machine)
Write-Host "Building uninstaller..." -ForegroundColor Yellow
Set-Location "..\TetriONUninstaller"
$uninstallerResult = dotnet publish -c Release -p:PublishSingleFile=true --self-contained false -o "..\TetriONInstaller\$OutputPath\uninstaller"

# Copy uninstaller to main output if build succeeded
Set-Location "..\TetriONInstaller"
if (Test-Path "$OutputPath\uninstaller\TetriONUninstaller.exe") {
    Copy-Item "$OutputPath\uninstaller\TetriONUninstaller.exe" "$OutputPath\uninstall.exe" -Force
    Remove-Item -Recurse -Force "$OutputPath\uninstaller"
    Write-Host "Uninstaller included successfully" -ForegroundColor Green
} else {
    Write-Warning "Uninstaller build failed, installer will work without uninstaller"
}

# Clean up temporary files
if (Test-Path $archivePath) {
    Remove-Item $archivePath -Force
}

# Restore original project file
Set-Content -Path $projectFile -Value $projectContent

# Rename installer to be platform-specific
$installerPath = "$OutputPath\TetriONInstaller.exe"
$platformInstallerPath = "$OutputPath\TetriON-Windows-x64-Installer.exe"

if (Test-Path $installerPath) {
    Move-Item $installerPath $platformInstallerPath -Force

    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Windows Installer: $platformInstallerPath" -ForegroundColor Cyan
    $fileSize = (Get-Item $platformInstallerPath).Length / 1MB
    Write-Host "File size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Error "Installer executable not found at: $installerPath"
    exit 1
}

Write-Host "`nWindows installer is ready for distribution!" -ForegroundColor Green
