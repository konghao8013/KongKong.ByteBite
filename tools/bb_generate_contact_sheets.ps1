param(
  [Parameter(Mandatory = $true)]
  [string]$InputDir,

  [Parameter(Mandatory = $true)]
  [string]$OutputDir,

  [int]$PerPage = 25,
  [int]$Columns = 5,
  [int]$ThumbWidth = 360,
  [int]$ThumbHeight = 270,
  [int]$Padding = 14,
  [int]$HeaderHeight = 44
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

function New-Dir([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Force -Path $Path | Out-Null
  }
}

function Get-FitRect([int]$srcW, [int]$srcH, [int]$dstW, [int]$dstH) {
  if ($srcW -le 0 -or $srcH -le 0) {
    return [pscustomobject]@{ X = 0; Y = 0; W = $dstW; H = $dstH }
  }
  $scale = [Math]::Min($dstW / [double]$srcW, $dstH / [double]$srcH)
  $w = [int][Math]::Round($srcW * $scale)
  $h = [int][Math]::Round($srcH * $scale)
  $x = [int][Math]::Round(($dstW - $w) / 2.0)
  $y = [int][Math]::Round(($dstH - $h) / 2.0)
  return [pscustomobject]@{ X = $x; Y = $y; W = $w; H = $h }
}

New-Dir -Path $OutputDir

$files = Get-ChildItem -LiteralPath $InputDir -File | Sort-Object Name
if ($files.Count -eq 0) {
  throw "No files found in input dir: $InputDir"
}

$rows = [int][Math]::Ceiling($PerPage / [double]$Columns)

$pageInnerW = ($Columns * $ThumbWidth) + (($Columns - 1) * $Padding)
$pageInnerH = ($rows * $ThumbHeight) + (($rows - 1) * $Padding)
$pageW = $pageInnerW + ($Padding * 2)
$pageH = $pageInnerH + ($Padding * 2) + $HeaderHeight

$font = New-Object System.Drawing.Font("Segoe UI", 14, [System.Drawing.FontStyle]::Bold)
$smallFont = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Regular)
$brushWhite = [System.Drawing.Brushes]::White
$brushBlack = [System.Drawing.Brushes]::Black
$brushDim = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(180, 0, 0, 0))
$penGrid = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(40, 255, 255, 255), 1)

$indexCsv = Join-Path $OutputDir "index.csv"
$indexRows = New-Object System.Collections.Generic.List[object]

$total = $files.Count
$pageCount = [int][Math]::Ceiling($total / [double]$PerPage)

for ($page = 1; $page -le $pageCount; $page++) {
  $bmp = New-Object System.Drawing.Bitmap $pageW, $pageH
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
  $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
  $g.Clear([System.Drawing.Color]::FromArgb(18, 18, 18))

  $startIndex0 = ($page - 1) * $PerPage
  $endIndex0 = [Math]::Min($startIndex0 + $PerPage - 1, $total - 1)
  $header = "BBQ Contact Sheet  Page $page/$pageCount   Items " + ($startIndex0 + 1) + "-" + ($endIndex0 + 1) + " / $total"
  $g.DrawString($header, $font, $brushWhite, $Padding, 10)

  for ($i0 = $startIndex0; $i0 -le $endIndex0; $i0++) {
    $globalIndex = $i0 + 1
    $within = $i0 - $startIndex0
    $r = [int][Math]::Floor($within / [double]$Columns)
    $c = [int]($within % $Columns)

    $cellX = $Padding + ($c * ($ThumbWidth + $Padding))
    $cellY = $HeaderHeight + $Padding + ($r * ($ThumbHeight + $Padding))

    $path = $files[$i0].FullName
    $name = $files[$i0].Name

    $indexRows.Add([pscustomobject]@{
      index = $globalIndex
      page = $page
      row = ($r + 1)
      col = ($c + 1)
      name = $name
      full_path = $path
    }) | Out-Null

    try {
      $img = [System.Drawing.Image]::FromFile($path)
      $fit = Get-FitRect -srcW $img.Width -srcH $img.Height -dstW $ThumbWidth -dstH $ThumbHeight
      $dstRect = New-Object System.Drawing.Rectangle ($cellX + $fit.X), ($cellY + $fit.Y), $fit.W, $fit.H
      $g.DrawImage($img, $dstRect)
      $img.Dispose()
    } catch {
      # Draw placeholder on error
      $g.FillRectangle([System.Drawing.Brushes]::DarkRed, $cellX, $cellY, $ThumbWidth, $ThumbHeight)
      $g.DrawString("ERR", $font, $brushWhite, $cellX + 12, $cellY + 12)
    }

    # Index overlay
    $tag = "#$globalIndex"
    $tagSize = $g.MeasureString($tag, $smallFont)
    $tagW = [int][Math]::Ceiling($tagSize.Width) + 14
    $tagH = [int][Math]::Ceiling($tagSize.Height) + 8
    $g.FillRectangle($brushDim, $cellX + 6, $cellY + 6, $tagW, $tagH)
    $g.DrawString($tag, $smallFont, $brushWhite, $cellX + 12, $cellY + 9)

    # Subtle border
    $g.DrawRectangle($penGrid, $cellX, $cellY, $ThumbWidth, $ThumbHeight)
  }

  $outPath = Join-Path $OutputDir ("sheet_{0:00}.jpg" -f $page)
  $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
  $g.Dispose()
  $bmp.Dispose()
}

$indexRows | Export-Csv -NoTypeInformation -Encoding UTF8 -LiteralPath $indexCsv

$font.Dispose()
$smallFont.Dispose()
$brushDim.Dispose()
$penGrid.Dispose()

Write-Host "Wrote contact sheets to: $OutputDir"
Write-Host "Wrote index CSV to: $indexCsv"

