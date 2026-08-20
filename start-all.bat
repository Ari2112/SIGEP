@echo off
echo ========================================
echo      SIGEP - Inicio Completo
echo ========================================
echo.
echo Este script abrira dos ventanas:
echo   1. Backend (API) en puerto 5017
echo   2. Frontend (React) en puerto 3000
echo.

:: Iniciar Backend en una nueva ventana
echo Iniciando Backend...
start "SIGEP Backend" cmd /k "cd /d "%~dp0code\SigepAPI" && dotnet run --launch-profile http"

:: Esperar 5 segundos antes de iniciar el frontend
timeout /t 5 /nobreak > nul

:: Iniciar Frontend en una nueva ventana
echo Iniciando Frontend...
start "SIGEP Frontend" cmd /k "cd /d "%~dp0code\frontend" && npm run dev"

echo.
echo Ambos servicios se estan iniciando...
echo.
echo URLs:
echo   - API:      http://localhost:5017
echo   - Frontend: http://localhost:3000
echo.
echo Usuarios de prueba (password: admin123):
echo   - admin / admin123
echo   - rrhh / admin123
echo   - supervisor / admin123
echo   - juan.perez / admin123
echo.
pause
