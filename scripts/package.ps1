$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Path (Join-Path $root 'dist') -Force | Out-Null
$manifestPath = Join-Path $root 'packaging\manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$dll = Join-Path $root 'bin\Release\netstandard2.1\PeakSprintLock.dll'
$assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($dll).Version
if ($assemblyVersion.ToString(3) -ne $manifest.version_number) { throw 'DLL/manifest version mismatch.' }
$out = Join-Path $root "dist\PeakSprintLock-$($manifest.version_number).zip"
$iconPath = Join-Path $root 'packaging\icon.png'
$files = [ordered]@{
    'manifest.json' = $manifestPath
    'README.md' = (Join-Path $root 'packaging\README.md')
    'CHANGELOG.md' = (Join-Path $root 'CHANGELOG.md')
    'icon.png' = $iconPath
    'plugins/PeakSprintLock/PeakSprintLock.dll' = $dll
}
# Explicit allowlist: no test config, game assemblies, logs, SDK or launch scripts.
$stream = [IO.File]::Open($out,[IO.FileMode]::Create)
$zip = [IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $files.GetEnumerator()) { [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$entry.Value,$entry.Key) | Out-Null }
} finally { $zip.Dispose(); $stream.Dispose() }
$archive = [IO.Compression.ZipFile]::OpenRead($out)
try {
    if ($archive.Entries.Count -ne 5) { throw 'Unexpected package contents.' }
    foreach ($entry in $archive.Entries) {
        if (!$files.Contains($entry.FullName)) { throw "Unexpected ZIP entry: $($entry.FullName)" }
        $sourceHash = (Get-FileHash -LiteralPath $files[$entry.FullName]).Hash
        $reader = $entry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $zipHash = [BitConverter]::ToString($sha.ComputeHash($reader)).Replace('-','') } finally { $reader.Dispose(); $sha.Dispose() }
        if ($sourceHash -ne $zipHash) { throw "Hash mismatch: $($entry.FullName)" }
        Write-Output "VERIFIED $($entry.FullName) ($($entry.Length) bytes)"
    }
} finally { $archive.Dispose() }
$image = [Drawing.Image]::FromFile($iconPath)
try { if ($image.Width -ne 256 -or $image.Height -ne 256) { throw 'Icon must be 256x256.' } } finally { $image.Dispose() }
Get-FileHash -LiteralPath $out
