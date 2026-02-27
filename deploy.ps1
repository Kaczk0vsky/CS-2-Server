$ErrorActionPreference = 'Stop'

$root = "E:\CS-2-Server"    # Change this to your server root path
$codmod = Join-Path $root "addons\counterstrikesharp\plugins\CodMod"
$publish = Join-Path $codmod "bin\Release\net8.0\publish"
$addonsSrc = Join-Path $root "addons"
$addonsDst = Join-Path $root "cs2-data\game\csgo\addons"

Set-Location $codmod
Write-Host "[1/3] Publishing CodMod..."
dotnet publish -c Release
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

Write-Host "[2/3] Copying publish output to CodMod folder..."
robocopy $publish $codmod /E /R:2 /W:1 /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -gt 7) { throw "Copy publish -> CodMod failed with exit code $LASTEXITCODE" }

Write-Host "[3/3] Copying addons to cs2-data/game/csgo/addons..."
robocopy $addonsSrc $addonsDst /E /R:2 /W:1 /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -gt 7) { throw "Copy addons -> csgo/addons failed with exit code $LASTEXITCODE" }

Write-Host "Done."
