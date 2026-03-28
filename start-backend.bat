@echo off
echo ========================================
echo      SIGEP - Backend API
echo ========================================
echo.
cd /d "%~dp0code\SigepAPI"
echo Iniciando API en http://localhost:5017
echo.
dotnet run --launch-profile http
