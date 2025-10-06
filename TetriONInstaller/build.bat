@echo off
echo Building TetriON Installer...
powershell -ExecutionPolicy Bypass -File "build.ps1"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
    echo Check the 'dist' folder for your installer.
    pause
) else (
    echo.
    echo Build failed!
    pause
)