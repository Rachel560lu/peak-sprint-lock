$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$folder = Join-Path $root 'test-profile\BepInEx\SprintLockDiagnostics'
$processes = @(Get-Process PEAK -ErrorAction SilentlyContinue)
if (!$processes.Count) { Write-Output 'PEAK is not running; no live-session result.'; return }
foreach ($process in $processes) {
    $log = Get-ChildItem -LiteralPath $folder -Filter "session-$($process.Id)-*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (!$log) { Write-Output "PID $($process.Id): no Sprint Lock startup evidence."; continue }
    Write-Output "PID $($process.Id): $($log.FullName)"
    Get-Content -LiteralPath $log.FullName -Tail 40
}
