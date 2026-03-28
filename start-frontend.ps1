# Script para iniciar el Frontend del SIGEP
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SIGEP - Iniciando Frontend (React)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$frontendPath = "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\frontend"

# Verificar si existe la carpeta
if (Test-Path $frontendPath) {
    Set-Location $frontendPath
    
    # Verificar si existe node_modules
    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 Instalando dependencias..." -ForegroundColor Yellow
        npm install
        Write-Host ""
    } else {
        Write-Host "✅ Dependencias ya instaladas" -ForegroundColor Green
        Write-Host ""
    }
    
    Write-Host "🚀 Iniciando aplicación React..." -ForegroundColor Green
    Write-Host ""
    Write-Host "La aplicación estará disponible en:" -ForegroundColor White
    Write-Host "  - http://localhost:3000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usuarios de prueba:" -ForegroundColor Yellow
    Write-Host "  - admin / admin123 (Admin)" -ForegroundColor White
    Write-Host "  - rrhh / admin123 (RRHH)" -ForegroundColor White
    Write-Host "  - juan.perez / admin123 (Empleado)" -ForegroundColor White
    Write-Host ""
    Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Gray
    Write-Host ""
    
    npm run dev
} else {
    Write-Host "❌ Error: No se encontró la carpeta del frontend" -ForegroundColor Red
    Write-Host "Ruta esperada: $frontendPath" -ForegroundColor Red
    pause
}
