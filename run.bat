@echo off
cd /d "%~dp0ProductionPlanning"
set ASPNETCORE_ENVIRONMENT=Development

set DOTNET=
where dotnet >nul 2>&1 && set DOTNET=dotnet
if not defined DOTNET if exist "C:\Program Files\dotnet\dotnet.exe" set "DOTNET=C:\Program Files\dotnet\dotnet.exe"
if not defined DOTNET if exist "C:\Program Files (x86)\dotnet\dotnet.exe" set "DOTNET=C:\Program Files (x86)\dotnet\dotnet.exe"
if not defined DOTNET if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" set "DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe"
if not defined DOTNET if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"

if not defined DOTNET (
    echo.
    echo .NET SDK not found. Installing...
    winget install --silent --exact --accept-source-agreements --accept-package-agreements Microsoft.DotNet.SDK.8 2>nul
    if exist "C:\Program Files\dotnet\dotnet.exe" set "DOTNET=C:\Program Files\dotnet\dotnet.exe"
)

if not defined DOTNET (
    echo.
    echo ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo Starting ProductionPlanning...
echo.
call "%DOTNET%" run
pause
