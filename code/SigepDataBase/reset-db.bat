@echo off
echo ========================================
echo   SIGEP - Reset Base de Datos
echo ========================================
echo.
echo ADVERTENCIA: Esto eliminara TODOS los datos
echo y recreara la base de datos desde cero.
echo.
set /p confirm="¿Desea continuar? (S/N): "
if /i "%confirm%" neq "S" (
    echo Operacion cancelada.
    pause
    exit /b
)

echo.
echo Ejecutando reset.sql en SQL Server LocalDB...
echo.

sqlcmd -S "(localdb)\mssqllocaldb" -i "%~dp0reset.sql" -b

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Reset completado exitosamente!
    echo ========================================
    echo.
    echo Usuarios disponibles:
    echo   - admin / admin123
    echo   - rrhh / admin123
    echo   - juan.perez / admin123
    echo   - maria.gonzalez / admin123
) else (
    echo.
    echo ERROR: No se pudo ejecutar el script.
    echo Asegurate de tener SQL Server LocalDB instalado.
)

echo.
pause
