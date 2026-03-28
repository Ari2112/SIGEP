# Script para iniciar Backend y Frontend del SIGEP
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "     SIGEP - Inicio Completo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Este script abrirá dos ventanas:" -ForegroundColor Yellow
Write-Host "  1. Backend (API) en puerto 7087" -ForegroundColor White
Write-Host "  2. Frontend (React) en puerto 3000" -ForegroundColor White
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Iniciar Backend en una nueva ventana
Write-Host "🚀 Iniciando Backend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-File", "$scriptPath\start-backend.ps1"

# Esperar 3 segundos antes de iniciar el frontend
Start-Sleep -Seconds 3

# Iniciar Frontend en una nueva ventana
Write-Host "🚀 Iniciando Frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-File", "$scriptPath\start-frontend.ps1"

Write-Host ""
Write-Host "✅ Ambos servicios se están iniciando..." -ForegroundColor Green
Write-Host ""
Write-Host "URLs:" -ForegroundColor Yellow
Write-Host "  - API: https://localhost:7087/swagger" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Usuarios de prueba:" -ForegroundColor Yellow
Write-Host "  - admin / admin123" -ForegroundColor White
Write-Host "  - rrhh / admin123" -ForegroundColor White
Write-Host "  - juan.perez / admin123" -ForegroundColor White
Write-Host ""
Write-Host "Presiona cualquier tecla para cerrar esta ventana..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
