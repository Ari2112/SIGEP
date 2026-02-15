@echo off
echo ========================================
echo   SIGEP - Solo Seed (Datos Iniciales)
echo ========================================
echo.
echo Esto agregara datos iniciales a la base existente.
echo.

sqlcmd -S "(localdb)\mssqllocaldb" -d SigepDB -i "%~dp0seed.sql" -b

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Seed completado exitosamente!
) else (
    echo.
    echo ERROR: No se pudo ejecutar el script.
)

echo.
pause
