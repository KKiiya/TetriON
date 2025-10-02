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
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o "publish"

# Create game files archive
Write-Host "Creating game files archive..." -ForegroundColor Yellow
$publishPath = ".\publish"
$archivePath = "..\TetriONInstaller\game_files.zip"

if (Test-Path $archivePath) {
    Remove-Item $archivePath
}

Compress-Archive -Path "$publishPath\*" -DestinationPath $archivePath -CompressionLevel Optimal

# Copy icon
Copy-Item "Icon.ico" "..\TetriONInstaller\installer_icon.ico" -Force

# Build installer
Write-Host "Building installer..." -ForegroundColor Yellow
Set-Location "..\TetriONInstaller"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $OutputPath

# Build uninstaller
Write-Host "Building uninstaller..." -ForegroundColor Yellow
$uninstallerCode = @"
using System;
using System.Windows.Forms;

namespace TetriONInstaller
{
    internal static class UninstallProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string installPath = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
            Application.Run(new UninstallerForm(installPath));
        }
    }
}
"@

$uninstallerCode | Out-File -FilePath "UninstallProgram.cs" -Encoding UTF8

# Create uninstaller project
$uninstallerProject = @"
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net8.0-windows</TargetFramework>
        <UseWindowsForms>true</UseWindowsForms>
        <AssemblyName>uninstall</AssemblyName>
        <PublishSingleFile>true</PublishSingleFile>
        <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.Win32.Registry" Version="5.0.0" />
    </ItemGroup>
    <ItemGroup>
        <Compile Include="UninstallerForm.cs" />
        <Compile Include="UninstallProgram.cs" />
    </ItemGroup>
</Project>
"@

$uninstallerProject | Out-File -FilePath "Uninstaller.csproj" -Encoding UTF8

$buildResult = dotnet publish Uninstaller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$OutputPath\uninstaller"

# Copy uninstaller to main output if build succeeded
if (Test-Path "$OutputPath\uninstaller\uninstall.exe") {
    Copy-Item "$OutputPath\uninstaller\uninstall.exe" "$OutputPath\" -Force
    Remove-Item -Recurse -Force "$OutputPath\uninstaller"
} else {
    Write-Warning "Uninstaller build failed, skipping..."
}

# Clean up temporary files
if (Test-Path "UninstallProgram.cs") { Remove-Item "UninstallProgram.cs" -Force }
if (Test-Path "Uninstaller.csproj") { Remove-Item "Uninstaller.csproj" -Force }
Remove-Item "game_files.zip" -Force
Remove-Item "installer_icon.ico" -Force

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Installer: $OutputPath\TetriONInstaller.exe" -ForegroundColor Cyan
Write-Host "File size: $((Get-Item "$OutputPath\TetriONInstaller.exe").Length / 1MB) MB" -ForegroundColor Cyan