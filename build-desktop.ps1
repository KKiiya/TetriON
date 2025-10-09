# TetriON Desktop-Only Build Script (PowerShell)
# Builds only for Windows, Linux, and macOS (no mobile platforms)

Write-Host "Building TetriON for desktop platforms only..." -ForegroundColor Green

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
    dotnet build TetriON.Linux/TetriON.Linux.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r linux-x64 -o builds/linux-x64
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

# Build Summary
Write-Host "`n" + "="*50 -ForegroundColor Cyan
Write-Host "DESKTOP BUILD SUMMARY" -ForegroundColor Cyan
Write-Host "="*50 -ForegroundColor Cyan

foreach ($result in $buildResults) {
    if ($result -match "SUCCESS") {
        Write-Host $result -ForegroundColor Green
    } else {
        Write-Host $result -ForegroundColor Red
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

Write-Host "`nDesktop build complete! Mobile platforms skipped." -ForegroundColor Green
Write-Host "Use './build-all.ps1' if you want to include Android/iOS builds." -ForegroundColor Cyan