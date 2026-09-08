# Reproduce the README artwork as a PNG using the same geometry as mountain-run.svg.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$bitmap = [Drawing.Bitmap]::new(1024,1024)
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.ScaleTransform(2,2)
function Color([string]$hex) { [Drawing.ColorTranslator]::FromHtml($hex) }
function Polygon([string]$hex, [float[]]$coords) {
    $points = for ($i=0; $i -lt $coords.Length; $i+=2) { [Drawing.PointF]::new($coords[$i],$coords[$i+1]) }
    $brush = [Drawing.SolidBrush]::new((Color $hex))
    try { $g.FillPolygon($brush,[Drawing.PointF[]]$points) } finally { $brush.Dispose() }
}
function Line([string]$hex, [float]$width, [float[]]$coords) {
    $points = for ($i=0; $i -lt $coords.Length; $i+=2) { [Drawing.PointF]::new($coords[$i],$coords[$i+1]) }
    $pen = [Drawing.Pen]::new((Color $hex),$width)
    $pen.StartCap=$pen.EndCap=[Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin=[Drawing.Drawing2D.LineJoin]::Round
    try { $g.DrawLines($pen,[Drawing.PointF[]]$points) } finally { $pen.Dispose() }
}
function Circle([string]$hex,[float]$x,[float]$y,[float]$r) {
    $brush=[Drawing.SolidBrush]::new((Color $hex))
    try { $g.FillEllipse($brush,$x-$r,$y-$r,2*$r,2*$r) } finally { $brush.Dispose() }
}
try {
    $g.Clear([Drawing.Color]::Transparent)
    $path=[Drawing.Drawing2D.GraphicsPath]::new()
    $brush=[Drawing.SolidBrush]::new((Color '#11282f'))
    try {
        $path.AddArc(0,0,88,88,180,90); $path.AddArc(424,0,88,88,270,90)
        $path.AddArc(424,424,88,88,0,90); $path.AddArc(0,424,88,88,90,90)
        $path.CloseFigure(); $g.FillPath($brush,$path)
    } finally { $brush.Dispose(); $path.Dispose() }
    Polygon '#355c66' @(49,382,215,105,379,382)
    Polygon '#284851' @(245,382,355,185,466,382)
    Polygon '#d6e9df' @(171,179,215,105,261,182,230,169,208,189,191,176)
    Polygon '#8eb2b3' @(331,229,355,185,379,230,359,221,347,237)
    foreach ($stroke in @(@('#11282f',33),@('#9be66a',19))) {
        Line $stroke[0] $stroke[1] @(293,238,260,302,302,335,323,388)
        Line $stroke[0] $stroke[1] @(260,302,224,353,163,365)
        Line $stroke[0] $stroke[1] @(284,253,328,286,365,268)
        Line $stroke[0] $stroke[1] @(283,253,236,232,210,263)
    }
    Circle '#11282f' 309 204 27
    Circle '#9be66a' 309 204 21
    Line '#9be66a' 10 @(100,262,166,262)
    Line '#9be66a' 10 @(83,289,145,289)
    Line '#9be66a' 10 @(106,316,158,316)
    Line '#6c9395' 5 @(76,427,436,427)
    foreach ($x in @(216,247,278)) { Line '#9be66a' 7 @($x,415,($x+14),427,$x,439) }
    $bitmap.Save((Join-Path $root 'docs\images\mountain-run.png'),[Drawing.Imaging.ImageFormat]::Png)
} finally { $g.Dispose(); $bitmap.Dispose() }
