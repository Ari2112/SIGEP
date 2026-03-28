# Script para iniciar el Backend del SIGEP
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SIGEP - Iniciando Backend (API)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$apiPath = "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\SigepAPI"

# Verificar si existe la carpeta
if (Test-Path $apiPath) {
    Set-Location $apiPath
    
    Write-Host "🚀 Iniciando API..." -ForegroundColor Green
    Write-Host ""
    Write-Host "El API estará disponible en:" -ForegroundColor White
    Write-Host "  - HTTP:    http://localhost:5017" -ForegroundColor Cyan
    Write-Host "  - Swagger: http://localhost:5017/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Gray
    Write-Host ""

    dotnet run --launch-profile http
} else {
    Write-Host "❌ Error: No se encontró la carpeta del API" -ForegroundColor Red
    Write-Host "Ruta esperada: $apiPath" -ForegroundColor Red
    pause
}
