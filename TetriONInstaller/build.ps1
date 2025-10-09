#!/usr/bin/env pwsh

param(
    [string]$GameProjectPath = "..\TetriON",
    [string]$OutputPath = ".\dist"
)

Write-Host "Building TetriON Installer..." -ForegroundColor Green

# Clean previous builds
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build the game in release mode
Write-Host "Building game..." -ForegroundColor Yellow
Set-Location $GameProjectPath
dotnet build -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true

# Create game files archive
Write-Host "Creating game files archive..." -ForegroundColor Yellow
$publishPath = ".\bin\Release\net8.0"
$archivePath = "..\TetriONInstaller\game_files.zip"

if (Test-Path $archivePath) {
    Remove-Item $archivePath
}

# Get all items except win-x64 folder
$itemsToInclude = Get-ChildItem -Path $publishPath | Where-Object { $_.Name -ne "win-x64" }
Compress-Archive -Path $itemsToInclude.FullName -DestinationPath $archivePath -CompressionLevel Optimal

# Copy icon
Copy-Item "Icon.ico" "..\TetriONInstaller\installer_icon.ico" -Force

# Build installer
Write-Host "Building installer..." -ForegroundColor Yellow
Set-Location "..\TetriONInstaller"
dotnet publish -c Release -p:PublishSingleFile=true -o $OutputPath

# Build uninstaller
Write-Host "Building uninstaller..." -ForegroundColor Yellow
Set-Location "..\TetriONUninstaller"
$buildResult = dotnet publish -c Release -p:PublishSingleFile=true -o "..\TetriONInstaller\$OutputPath\uninstaller"

# Copy uninstaller to main output if build succeeded
Set-Location "..\TetriONInstaller"
if (Test-Path "$OutputPath\uninstaller\TetriONUninstaller.exe") {
    Copy-Item "$OutputPath\uninstaller\TetriONUninstaller.exe" "$OutputPath\uninstall.exe" -Force
    Remove-Item -Recurse -Force "$OutputPath\uninstaller"
} else {
    Write-Warning "Uninstaller build failed, skipping..."
}

# Clean up temporary files
Remove-Item "game_files.zip" -Force
Remove-Item "installer_icon.ico" -Force

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Installer: $OutputPath\TetriONInstaller.exe" -ForegroundColor Cyan
Write-Host "File size: $((Get-Item "$OutputPath\TetriONInstaller.exe").Length / 1MB) MB" -ForegroundColor Cyan