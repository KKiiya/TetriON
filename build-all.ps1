# TetriON Multi-Platform Build Script (PowerShell)

Write-Host "Building TetriON for multiple platforms..." -ForegroundColor Green

# Function to check if a workload is installed
function Test-WorkloadInstalled {
    param($workloadId)
    try {
        $workloads = dotnet workload list 2>$null | Out-String
        return $workloads -match $workloadId
    } catch {
        return $false
    }
}

# Function to install workload if needed
function Install-WorkloadIfNeeded {
    param($workloadId, $displayName)

    Write-Host "Checking $displayName workload..." -ForegroundColor Cyan

    if (Test-WorkloadInstalled $workloadId) {
        Write-Host "$displayName workload already installed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "ERROR! $displayName workload not installed!" -ForegroundColor Red
        return $false
    }
}

# Create output directory
New-Item -ItemType Directory -Path "builds" -Force | Out-Null

$buildResults = @()

# Build Windows (x64)
Write-Host "`nBuilding for Windows x64..." -ForegroundColor Yellow
try {
    dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r win-x64 -o builds/windows-x64
    if ($LASTEXITCODE -eq 0) {
        $buildResults += "✓ Windows x64: SUCCESS"
        Write-Host "Windows x64 build completed" -ForegroundColor Green
    } else {
        $buildResults += "✗ Windows x64: FAILED"
        Write-Host "Windows x64 build failed" -ForegroundColor Red
    }
} catch {
    $buildResults += "✗ Windows x64: ERROR"
    Write-Host "Windows x64 build error: $($_.Exception.Message)" -ForegroundColor Red
}

# Build Linux (x64)
Write-Host "`nBuilding for Linux x64..." -ForegroundColor Yellow
try {
    dotnet publish TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r linux-x64 -o builds/linux-x64
    if ($LASTEXITCODE -eq 0) {
        $buildResults += "✓ Linux x64: SUCCESS"
        Write-Host "Linux x64 build completed" -ForegroundColor Green
    } else {
        $buildResults += "✗ Linux x64: FAILED"
        Write-Host "Linux x64 build failed" -ForegroundColor Red
    }
} catch {
    $buildResults += "✗ Linux x64: ERROR"
    Write-Host "Linux x64 build error: $($_.Exception.Message)" -ForegroundColor Red
}

# Build macOS (x64)
Write-Host "`nBuilding for macOS x64..." -ForegroundColor Yellow
try {
    dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r osx-x64 -o builds/macos-x64
    if ($LASTEXITCODE -eq 0) {
        $buildResults += "✓ macOS x64: SUCCESS"
        Write-Host "macOS x64 build completed" -ForegroundColor Green
    } else {
        $buildResults += "✗ macOS x64: FAILED"
        Write-Host "macOS x64 build failed" -ForegroundColor Red
    }
} catch {
    $buildResults += "✗ macOS x64: ERROR"
    Write-Host "macOS x64 build error: $($_.Exception.Message)" -ForegroundColor Red
}

# Build macOS (ARM64 - Apple Silicon)
Write-Host "`nBuilding for macOS ARM64..." -ForegroundColor Yellow
try {
    dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r osx-arm64 -o builds/macos-arm64
    if ($LASTEXITCODE -eq 0) {
        $buildResults += "✓ macOS ARM64: SUCCESS"
        Write-Host "macOS ARM64 build completed" -ForegroundColor Green
    } else {
        $buildResults += "✗ macOS ARM64: FAILED"
        Write-Host "macOS ARM64 build failed" -ForegroundColor Red
    }
} catch {
    $buildResults += "✗ macOS ARM64: ERROR"
    Write-Host "macOS ARM64 build error: $($_.Exception.Message)" -ForegroundColor Red
}

# Build Android (requires Android workload)
Write-Host "`nAttempting Android build..." -ForegroundColor Yellow
if (Install-WorkloadIfNeeded "android" "Android") {
    try {
        Write-Host "Building for Android..." -ForegroundColor Yellow
        dotnet build TetriON.Android/TetriON.Android.csproj -c Release -f net8.0-android -o builds/android
        if ($LASTEXITCODE -eq 0) {
            $buildResults += "✓ Android: SUCCESS"
            Write-Host "Android build completed" -ForegroundColor Green
        } else {
            $buildResults += "✗ Android: FAILED"
            Write-Host "Android build failed" -ForegroundColor Red
        }
    } catch {
        $buildResults += "✗ Android: ERROR"
        Write-Host "Android build error: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    $buildResults += "⚠ Android: SKIPPED (workload installation failed)"
    Write-Host "Skipping Android build - workload installation failed" -ForegroundColor Yellow
}

# iOS builds require macOS
$isWindowsPlatform = $IsWindows -or ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT)
if ($isWindowsPlatform) {
    $buildResults += "⚠ iOS: SKIPPED (requires macOS)"
    Write-Host "`niOS builds require macOS with Xcode - skipping on Windows" -ForegroundColor Cyan
} else {
    Write-Host "`nAttempting iOS build..." -ForegroundColor Yellow
    if (Install-WorkloadIfNeeded "ios" "iOS") {
        try {
            Write-Host "Building for iOS..." -ForegroundColor Yellow
            dotnet build TetriON.iOS/TetriON.iOS.csproj -c Release -f net8.0-ios -o builds/ios
            if ($LASTEXITCODE -eq 0) {
                $buildResults += "✓ iOS: SUCCESS"
                Write-Host "iOS build completed" -ForegroundColor Green
            } else {
                $buildResults += "✗ iOS: FAILED"
                Write-Host "iOS build failed" -ForegroundColor Red
            }
        } catch {
            $buildResults += "✗ iOS: ERROR"
            Write-Host "iOS build error: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        $buildResults += "⚠ iOS: SKIPPED (workload installation failed)"
        Write-Host "Skipping iOS build - workload installation failed" -ForegroundColor Yellow
    }
}

# Build Summary
Write-Host "`n" + "="*50 -ForegroundColor Cyan
Write-Host "BUILD SUMMARY" -ForegroundColor Cyan
Write-Host "="*50 -ForegroundColor Cyan

foreach ($result in $buildResults) {
    if ($result -match "SUCCESS") {
        Write-Host $result -ForegroundColor Green
    } elseif ($result -match "FAILED|ERROR") {
        Write-Host $result -ForegroundColor Red
    } else {
        Write-Host $result -ForegroundColor Yellow
    }
}

# Show build sizes
if (Test-Path "builds") {
    Write-Host "`nBuild Output Sizes:" -ForegroundColor Cyan
    Get-ChildItem "builds" -Directory | Where-Object { (Get-ChildItem $_.FullName -File -Recurse).Count -gt 0 } | ForEach-Object {
        $files = Get-ChildItem $_.FullName -Recurse -File
        $size = ($files | Measure-Object -Property Length -Sum).Sum
        $fileCount = $files.Count
        $sizeStr = if ($size -gt 1GB) { "{0:N1} GB" -f ($size/1GB) }
                   elseif ($size -gt 1MB) { "{0:N1} MB" -f ($size/1MB) }
                   else { "{0:N0} KB" -f ($size/1KB) }
        Write-Host "  $($_.Name): $sizeStr ($fileCount files)" -ForegroundColor White
    }
}

Write-Host "`nBuild complete! Check the 'builds' directory for output." -ForegroundColor Green
