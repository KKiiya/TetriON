#!/usr/bin/env pwsh

param(
    [string[]]$Platforms = @("windows-x64"),
    [string]$OutputPath = ".\dist",
    [string]$BuildsPath = "..\builds"
)

Write-Host "Building TetriON Platform-Specific Installers..." -ForegroundColor Green

# Validate that builds exist
if (-not (Test-Path $BuildsPath)) {
    Write-Error "Builds directory not found at: $BuildsPath"
    Write-Error "Please run build-all script first to create platform builds"
    exit 1
}

$availablePlatforms = Get-ChildItem -Path $BuildsPath -Directory | Select-Object -ExpandProperty Name
Write-Host "Available platform builds: $($availablePlatforms -join ', ')" -ForegroundColor Cyan

$successfulBuilds = @()
$failedBuilds = @()

foreach ($platform in $Platforms) {
    Write-Host "`n" + "="*60 -ForegroundColor Blue
    Write-Host "Building installer for: $platform" -ForegroundColor Blue
    Write-Host "="*60 -ForegroundColor Blue

    if ($platform -notin $availablePlatforms) {
        Write-Warning "Platform '$platform' not found in builds directory"
        Write-Warning "Available platforms: $($availablePlatforms -join ', ')"
        $failedBuilds += $platform
        continue
    }

    try {
        switch ($platform) {
            "windows-x64" {
                $platformOutput = Join-Path $OutputPath "windows"
                & .\build-windows.ps1 -Platform $platform -OutputPath $platformOutput -BuildsPath $BuildsPath
                if ($LASTEXITCODE -eq 0) {
                    $successfulBuilds += $platform
                    Write-Host "✓ Windows installer completed successfully" -ForegroundColor Green
                } else {
                    throw "Windows installer build failed with exit code $LASTEXITCODE"
                }
            }
            "linux-x64" {
                Write-Warning "Linux installer not yet implemented - will be added in future update"
                # TODO: Implement Linux installer (AppImage, .deb, or .rpm)
                $failedBuilds += $platform
            }
            "macos-x64" {
                Write-Warning "macOS x64 installer not yet implemented - will be added in future update"
                # TODO: Implement macOS installer (.pkg or .dmg)
                $failedBuilds += $platform
            }
            "macos-arm64" {
                Write-Warning "macOS ARM installer not yet implemented - will be added in future update"
                # TODO: Implement macOS ARM installer
                $failedBuilds += $platform
            }
            default {
                Write-Warning "Unknown platform: $platform"
                $failedBuilds += $platform
            }
        }
    }
    catch {
        Write-Error "Failed to build installer for $platform`: $($_.Exception.Message)"
        $failedBuilds += $platform
    }
}

# Summary
Write-Host "`n" + "="*60 -ForegroundColor Yellow
Write-Host "BUILD SUMMARY" -ForegroundColor Yellow
Write-Host "="*60 -ForegroundColor Yellow

if ($successfulBuilds.Count -gt 0) {
    Write-Host "✓ Successful builds:" -ForegroundColor Green
    foreach ($platform in $successfulBuilds) {
        Write-Host "  - $platform" -ForegroundColor Green
    }
}

if ($failedBuilds.Count -gt 0) {
    Write-Host "✗ Failed builds:" -ForegroundColor Red
    foreach ($platform in $failedBuilds) {
        Write-Host "  - $platform" -ForegroundColor Red
    }
}

Write-Host "`nInstaller files are located in: $OutputPath" -ForegroundColor Cyan

# List generated installer files
if (Test-Path $OutputPath) {
    $installerFiles = Get-ChildItem -Path $OutputPath -Recurse -Filter "*Installer*.exe"
    if ($installerFiles.Count -gt 0) {
        Write-Host "`nGenerated installer files:" -ForegroundColor Cyan
        foreach ($file in $installerFiles) {
            $size = [math]::Round($file.Length / 1MB, 2)
            Write-Host "  - $($file.FullName) ($size MB)" -ForegroundColor Gray
        }
    }
}

if ($failedBuilds.Count -gt 0) {
    exit 1
}

Write-Host "`nAll requested installers built successfully! 🎉" -ForegroundColor Green
