@echo off
cd /d "%~dp0ProductionPlanning"
set ASPNETCORE_ENVIRONMENT=Development
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        "C:\Program Files\dotnet\dotnet.exe" run
    ) else (
        echo SDK .NET не найден. Установите .NET 8: https://dotnet.microsoft.com/download/dotnet/8.0
        exit /b 1
    )
) else (
    dotnet run
)
