# Convierte un sprite PNG (por ejemplo un objeto del juego) en src\app.ico con varios tamaños.
# Recorta los bordes transparentes, deja el dibujo cuadrado y lo amplía sin suavizar (pixel art nítido).
# Uso: powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1 <sprite.png> [salida.ico]
param(
    [Parameter(Mandatory = $true)][string]$Sprite,
    [string]$Out
)
Add-Type -AssemblyName System.Drawing
# En PowerShell 5.1 $PSScriptRoot no está disponible en los valores por defecto de param().
if (-not $Out) { $Out = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\src\app.ico" }

$src = New-Object System.Drawing.Bitmap $Sprite
# Caja que contiene los píxeles no transparentes.
$minX = $src.Width; $minY = $src.Height; $maxX = -1; $maxY = -1
for ($y = 0; $y -lt $src.Height; $y++) {
    for ($x = 0; $x -lt $src.Width; $x++) {
        if ($src.GetPixel($x, $y).A -gt 16) {
            if ($x -lt $minX) { $minX = $x }; if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }; if ($y -gt $maxY) { $maxY = $y }
        }
    }
}
if ($maxX -lt 0) { throw "El sprite está vacío: $Sprite" }
$w = $maxX - $minX + 1; $h = $maxY - $minY + 1; $side = [Math]::Max($w, $h)

$sizes = 16, 24, 32, 48, 64, 128, 256
$pngs = @()
foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    # Escala entera cuando cabe (bordes nítidos); en tamaños menores que el dibujo, reducción suave.
    $scale = [Math]::Floor($size / $side)
    if ($scale -ge 1) {
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $dw = $w * $scale; $dh = $h * $scale
    } else {
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $dw = [int]($w * $size / $side); $dh = [int]($h * $size / $side)
    }
    $dest = New-Object System.Drawing.Rectangle ([int](($size - $dw) / 2)), ([int](($size - $dh) / 2)), $dw, $dh
    $g.DrawImage($src, $dest, $minX, $minY, $w, $h, [System.Drawing.GraphicsUnit]::Pixel)
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngs += , $ms.ToArray()
    $bmp.Dispose()
}
$src.Dispose()

# Formato ICO con imágenes PNG dentro (Windows Vista y posteriores).
$ico = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $ico
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]; $dim = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$dim); $bw.Write([byte]$dim); $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([UInt16]1); $bw.Write([UInt16]32)
    $bw.Write([UInt32]$pngs[$i].Length); $bw.Write([UInt32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $bw.Write($p) }
$bw.Flush()
[System.IO.File]::WriteAllBytes([System.IO.Path]::GetFullPath($Out), $ico.ToArray())
"Icono creado: $([System.IO.Path]::GetFullPath($Out)) (sprite recortado a ${w}x${h})"
