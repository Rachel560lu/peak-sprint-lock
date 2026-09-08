param(
    [string]$Dotnet = 'C:\Users\midor\Desktop\peak-item-insight\.dotnet-sdk\dotnet.exe',
    [string]$PeakManagedDir = 'D:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed',
    [string]$BepInExCoreDir = 'C:\Users\midor\AppData\Roaming\r2modmanPlus-local\PEAK\profiles\PeakItemInsight-Test\BepInEx\core'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$env:DOTNET_CLI_HOME = Join-Path $root '.dotnet-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& $Dotnet build (Join-Path $root 'PeakSprintLock.csproj') -c Release "-p:PeakManagedDir=$PeakManagedDir" "-p:BepInExCoreDir=$BepInExCoreDir"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
& $Dotnet run --project (Join-Path $root 'tests\Tests.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
