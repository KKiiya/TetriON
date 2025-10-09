# Setup script to install required .NET workloads for TetriON multi-platform builds

param(
    [switch]$SkipAndroid,
    [switch]$SkipIOS,
    [switch]$Force
)

Write-Host "Setting up .NET workloads for TetriON multi-platform builds..." -ForegroundColor Green

# Check current platform (use built-in variables in PowerShell 6+)
$IsWindowsPlatformDetected = $IsWindows -or ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT)
$IsMacPlatformDetected = $IsMacOS -or (-not $IsWindowsPlatformDetected -and (Get-Command "uname" -ErrorAction SilentlyContinue) -and ((uname) -eq "Darwin"))

Write-Host "Detected platform: $(if ($IsWindowsPlatformDetected) { 'Windows' } elseif ($IsMacPlatformDetected) { 'macOS' } else { 'Linux' })" -ForegroundColor Cyan

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

# List of workloads to potentially install
$workloads = @()

if (-not $SkipAndroid) {
    $workloads += @{ Id = "android"; Name = "Android"; Description = "For building Android APKs"; Required = $true }
}

if ($IsMacPlatformDetected -and -not $SkipIOS) {
    $workloads += @{ Id = "ios"; Name = "iOS"; Description = "For building iOS apps (macOS only)"; Required = $true }
    $workloads += @{ Id = "maccatalyst"; Name = "macOS Catalyst"; Description = "For building macOS Catalyst apps"; Required = $false }
}

if ($workloads.Count -eq 0) {
    Write-Host "No mobile workloads to install. Desktop builds don't require additional workloads." -ForegroundColor Green
    Write-Host "You can run './build-desktop.ps1' for desktop-only builds." -ForegroundColor Cyan
    exit 0
}

Write-Host "`nWorkloads to install:" -ForegroundColor Yellow
foreach ($workload in $workloads) {
    $status = if (Test-WorkloadInstalled $workload.Id) { 
        if ($Force) { "INSTALLED (will reinstall due to -Force)" } else { "ALREADY INSTALLED" }
    } else { 
        "NOT INSTALLED" 
    }
    Write-Host "  $($workload.Name): $status" -ForegroundColor $(if ($status -match "NOT INSTALLED") { "Red" } elseif ($status -match "ALREADY") { "Green" } else { "Yellow" })
}

Write-Host "`nStarting workload installation..." -ForegroundColor Cyan

$success = 0
$failed = 0

foreach ($workload in $workloads) {
    Write-Host "`nProcessing $($workload.Name) workload..." -ForegroundColor Yellow
    Write-Host "  Description: $($workload.Description)" -ForegroundColor Gray
    
    # Skip if already installed and not forcing
    if ((Test-WorkloadInstalled $workload.Id) -and -not $Force) {
        Write-Host "  * $($workload.Name) workload already installed (use -Force to reinstall)" -ForegroundColor Green
        $success++
        continue
    }
    
    try {
        if ($Force -and (Test-WorkloadInstalled $workload.Id)) {
            Write-Host "  Uninstalling existing $($workload.Name) workload..." -ForegroundColor Yellow
            dotnet workload uninstall $workload.Id --verbosity quiet
        }
        
        Write-Host "  Installing $($workload.Name) workload..." -ForegroundColor Yellow
        dotnet workload install $workload.Id --verbosity quiet
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  * $($workload.Name) workload installed successfully" -ForegroundColor Green
            $success++
        } else {
            Write-Host "  X Failed to install $($workload.Name) workload (exit code: $LASTEXITCODE)" -ForegroundColor Red
            if ($workload.Required) {
                Write-Host "    This is a required workload for mobile builds" -ForegroundColor Red
            }
            $failed++
        }
    } catch {
        Write-Host "  X Exception installing $($workload.Name) workload: $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }
}

# Summary
Write-Host "`n" + "="*50 -ForegroundColor Cyan
Write-Host "WORKLOAD SETUP SUMMARY" -ForegroundColor Cyan
Write-Host "="*50 -ForegroundColor Cyan
Write-Host "Successfully installed: $success" -ForegroundColor Green
Write-Host "Failed installations: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })

if ($failed -eq 0) {
    Write-Host "`n* All workloads installed successfully!" -ForegroundColor Green
    Write-Host "You can now run './build-all.ps1' to build for all platforms." -ForegroundColor Cyan
} else {
    Write-Host "`n! Some workloads failed to install" -ForegroundColor Yellow
    Write-Host "You can still build for desktop platforms using './build-desktop.ps1'" -ForegroundColor Cyan
    Write-Host "Or retry this setup with elevated permissions" -ForegroundColor Yellow
}

# Show all currently installed workloads
Write-Host "`nCurrently installed workloads:" -ForegroundColor Yellow
try {
    dotnet workload list
} catch {
    Write-Host "Could not list workloads" -ForegroundColor Red
}

Write-Host "`nSetup complete!" -ForegroundColor Green