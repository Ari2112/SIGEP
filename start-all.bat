@echo off
echo ========================================
echo      SIGEP - Inicio Completo
echo ========================================
echo.
echo Este script abrira dos ventanas:
echo   1. Backend (API) en puerto 7087
echo   2. Frontend (React) en puerto 3000
echo.

:: Iniciar Backend en una nueva ventana
echo Iniciando Backend...
start "SIGEP Backend" cmd /k "cd /d c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\SigepAPI && dotnet run"

:: Esperar 3 segundos antes de iniciar el frontend
timeout /t 3 /nobreak > nul

:: Iniciar Frontend en una nueva ventana
echo Iniciando Frontend...
start "SIGEP Frontend" cmd /k "cd /d c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\frontend && npm run dev"

echo.
echo Ambos servicios se estan iniciando...
echo.
echo URLs:
echo   - API: https://localhost:7087/swagger
echo   - Frontend: http://localhost:3000
echo.
echo Usuarios de prueba:
echo   - admin / admin123
echo   - rrhh / admin123
echo   - juan.perez / admin123
echo.
pause
