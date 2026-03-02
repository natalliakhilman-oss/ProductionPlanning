# Локальный запуск ProductionPlanning (Development + SQLite)
$ErrorActionPreference = "Stop"
$projDir = Join-Path $PSScriptRoot "ProductionPlanning"
$env:ASPNETCORE_ENVIRONMENT = "Development"

$dotnet = $null
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnet = "dotnet"
} elseif (Test-Path "C:\Program Files\dotnet\dotnet.exe") {
    $dotnet = "C:\Program Files\dotnet\dotnet.exe"
} else {
    Write-Host "SDK .NET не найден. Установите .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Set-Location $projDir
& $dotnet run
