#!/bin/bash

# TetriON Multi-Platform Build Script

echo "Building TetriON for multiple platforms..."

# Create output directory
mkdir -p builds

# Build Windows (x64)
echo "Building for Windows x64..."
dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r win-x64 -o builds/windows-x64

# Build Linux (x64)
echo "Building for Linux x64..."
dotnet build TetriON.Linux/TetriON.Linux.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r linux-x64 -o builds/linux-x64

# Build macOS (x64)
echo "Building for macOS x64..."
dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r osx-x64 -o builds/macos-x64

# Build macOS (ARM64 - Apple Silicon)
echo "Building for macOS ARM64..."
dotnet build TetriON/TetriON.csproj -c Release -p:PublishTrimmed=true -p:PublishReadyToRun=true -r osx-arm64 -o builds/macos-arm64

# Build Android (requires Android SDK)
if command -v android-sdk &> /dev/null; then
    echo "Building for Android..."
    dotnet build TetriON.Android/TetriON.Android.csproj -c Release -f net8.0-android -o builds/android
else
    echo "Android SDK not found, skipping Android build"
fi

# Build iOS (requires Xcode - Mac only)
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Building for iOS..."
    dotnet build TetriON.iOS/TetriON.iOS.csproj -c Release -f net8.0-ios -o builds/ios
else
    echo "iOS build requires macOS with Xcode, skipping"
fi

echo "Build complete! Check the 'builds' directory for output."