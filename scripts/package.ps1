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
# Original geometric icon: mountain, forward chevrons and a lock.
$bmp = New-Object Drawing.Bitmap 256,256
$g = [Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([Drawing.ColorTranslator]::FromHtml('#142B32'))
$mint = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#8EE1BA'))
$muted = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#28505A'))
$white = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#F5F2E5'))
$pen = New-Object Drawing.Pen ([Drawing.ColorTranslator]::FromHtml('#F5F2E5')),10
try {
    $g.FillPolygon($muted, [Drawing.Point[]]@([Drawing.Point]::new(8,180),[Drawing.Point]::new(88,46),[Drawing.Point]::new(167,180)))
    $g.FillPolygon($mint, [Drawing.Point[]]@([Drawing.Point]::new(49,180),[Drawing.Point]::new(146,30),[Drawing.Point]::new(242,180)))
    $g.FillPolygon($white, [Drawing.Point[]]@([Drawing.Point]::new(119,72),[Drawing.Point]::new(146,30),[Drawing.Point]::new(174,73),[Drawing.Point]::new(147,61),[Drawing.Point]::new(135,78)))
    $dark = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#142B32'))
    try { $g.FillEllipse($dark,151,126,88,88) } finally { $dark.Dispose() }
    $g.DrawArc($pen,178,146,32,34,180,180)
    $g.FillRectangle($white,172,162,44,34)
    $g.FillRectangle($muted,191,173,6,13)
    foreach ($offset in @(0,36)) {
        $g.FillPolygon($white,[Drawing.Point[]]@([Drawing.Point]::new(30+$offset,205),[Drawing.Point]::new(43+$offset,205),[Drawing.Point]::new(59+$offset,220),[Drawing.Point]::new(43+$offset,235),[Drawing.Point]::new(30+$offset,235),[Drawing.Point]::new(46+$offset,220)))
    }
    $bmp.Save($iconPath,[Drawing.Imaging.ImageFormat]::Png)
} finally { $g.Dispose(); $bmp.Dispose(); $mint.Dispose(); $muted.Dispose(); $white.Dispose(); $pen.Dispose() }
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
