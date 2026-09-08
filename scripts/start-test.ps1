param([string]$GameDir = 'D:\SteamLibrary\steamapps\common\PEAK')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$peakProcesses = @(Get-Process PEAK -ErrorAction SilentlyContinue)
foreach ($existing in $peakProcesses) {
    # Windows may retain a terminated process object while another process owns
    # its handle. A zero-thread, zero-handle record cannot execute game code.
    if ($existing.Threads.Count -eq 0 -and $existing.HandleCount -eq 0) {
        Write-Output "Ignoring terminated PEAK process record PID=$($existing.Id)."
        continue
    }
    throw 'PEAK is running. Exit normally before starting this test.'
}
$preloader = Join-Path $root 'test-profile\BepInEx\core\BepInEx.Preloader.dll'
if (!(Test-Path $preloader)) { throw 'Run prepare-test.ps1 first.' }
$plugins = @(Get-ChildItem (Join-Path $root 'test-profile\BepInEx\plugins') -Recurse -Filter '*.dll')
if ($plugins.Count -ne 1 -or $plugins[0].Name -ne 'PeakSprintLock.dll') { throw 'Unexpected test plugins.' }
$appid = Join-Path $GameDir 'steam_appid.txt'
if (Test-Path -LiteralPath $appid) { throw 'Existing steam_appid.txt found; not overwriting it.' }
# Temporary development app ID avoids Steam relaunching without this profile.
# The existing game BepInEx junction and all existing plugins stay untouched.
try {
    '3527290' | Set-Content -LiteralPath $appid -Encoding ASCII
    $arguments = @('--doorstop-enabled', 'true', '--doorstop-target-assembly', ('"' + $preloader + '"'))
    $process = Start-Process -FilePath (Join-Path $GameDir 'PEAK.exe') -WorkingDirectory $GameDir -ArgumentList $arguments -WindowStyle Normal -PassThru
    Write-Output "TEST_PID=$($process.Id)"
    Write-Output 'Enter an offline airport and test Forward + CapsLock on clear ground. Do not continue a saved expedition for the smoke test.'
    $process.WaitForExit()
}
finally {
    if ((Test-Path -LiteralPath $appid) -and (Get-Content -LiteralPath $appid -Raw).Trim() -eq '3527290') {
        Remove-Item -LiteralPath $appid
    }
}
