#!/usr/bin/env sh
set -eu
ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
echo "[1/4] Restaurando backend..."
dotnet restore "$ROOT/DeskFlow.sln"
echo "[2/4] Compilando e testando backend..."
dotnet build "$ROOT/DeskFlow.sln" -c Release --no-restore
dotnet test "$ROOT/DeskFlow.sln" -c Release --no-build
echo "[3/4] Instalando dependencias do frontend..."
cd "$ROOT/frontend/deskflow-web"
npm install --no-audit --no-fund
echo "[4/4] Compilando frontend..."
npm run build
echo "DeskFlow verificado com sucesso."
