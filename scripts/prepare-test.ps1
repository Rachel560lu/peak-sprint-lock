param([string]$CoreSource = 'C:\Users\midor\AppData\Roaming\r2modmanPlus-local\PEAK\profiles\PeakItemInsight-Test\BepInEx\core')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$target = Join-Path $root 'test-profile\BepInEx'
if (Test-Path (Join-Path $target 'SprintLockDiagnostics')) {
    $running = @(Get-Process PEAK -ErrorAction SilentlyContinue | Where-Object { $_.Threads.Count -gt 0 -or $_.HandleCount -gt 0 })
    if ($running.Count) { throw 'Exit PEAK before refreshing an existing test profile.' }
}
New-Item -ItemType Directory -Force -Path (Join-Path $target 'core'),(Join-Path $target 'plugins\PeakSprintLock'),(Join-Path $target 'config') | Out-Null
Copy-Item -Path (Join-Path $CoreSource '*') -Destination (Join-Path $target 'core') -Force
Copy-Item -LiteralPath (Join-Path $root 'bin\Release\netstandard2.1\PeakSprintLock.dll') -Destination (Join-Path $target 'plugins\PeakSprintLock\PeakSprintLock.dll') -Force
$cfg = Join-Path $target 'config\dev.midor.peaksprintlock.cfg'
if (!(Test-Path $cfg)) { "[Diagnostics]`r`nEnabled = true`r`n" | Set-Content -LiteralPath $cfg -Encoding UTF8 }
Get-FileHash -LiteralPath (Join-Path $target 'plugins\PeakSprintLock\PeakSprintLock.dll')
