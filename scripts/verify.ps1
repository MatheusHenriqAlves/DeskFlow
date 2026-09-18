$ErrorActionPreference = "Stop"

Write-Host "[1/4] Restaurando backend..." -ForegroundColor Cyan
dotnet restore "$PSScriptRoot\..\DeskFlow.sln"

Write-Host "[2/4] Compilando e testando backend..." -ForegroundColor Cyan
dotnet build "$PSScriptRoot\..\DeskFlow.sln" -c Release --no-restore
dotnet test "$PSScriptRoot\..\DeskFlow.sln" -c Release --no-build

Write-Host "[3/4] Instalando dependencias do frontend..." -ForegroundColor Cyan
Push-Location "$PSScriptRoot\..\frontend\deskflow-web"
npm install --no-audit --no-fund

Write-Host "[4/4] Compilando frontend..." -ForegroundColor Cyan
npm run build
Pop-Location

Write-Host "DeskFlow verificado com sucesso." -ForegroundColor Green
