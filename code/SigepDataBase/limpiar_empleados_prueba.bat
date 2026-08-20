@echo off
:: ============================================================
:: SIGEP - Limpieza de empleados de prueba + Liquidaciones
:: SQL Server Express - Windows Authentication
:: ============================================================

title SIGEP - Limpieza de datos de prueba

set DB_SERVER=LAPTOP-56772AJK\SQLEXPRESS
set SQL_SCRIPT=%~dp0limpiar_empleados_prueba.sql

echo.
echo ============================================================
echo  SIGEP - Limpieza de empleados de prueba y liquidaciones
echo  Servidor: %DB_SERVER%
echo  Script  : %SQL_SCRIPT%
echo ============================================================
echo.
echo  Esto va a borrar permanentemente:
echo    - Los 4 empleados de prueba (118630522, 118630529,
echo      118630524, 118635236) y todo lo relacionado a ellos
echo      (usuario, vacaciones, permisos, asistencia, planilla,
echo      evaluaciones, incapacidades, aguinaldo).
echo    - TODAS las liquidaciones registradas actualmente.
echo.
echo  El resto de empleados y datos NO se toca.
echo.
set /p CONFIRM=Desea continuar? (s/N):

if /i "%CONFIRM%" neq "s" (
    echo.
    echo Operacion cancelada.
    pause
    exit /b 0
)

echo.
echo Ejecutando limpiar_empleados_prueba.sql...
sqlcmd -S "%DB_SERVER%" -E -i "%SQL_SCRIPT%" -I -b
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la ejecucion del script SQL.
    echo.
    echo  Posibles causas:
    echo    - sqlcmd no esta instalado
    echo      Instalar: winget install Microsoft.SqlServer.CommandLineUtils
    echo    - El servicio SQLEXPRESS no esta corriendo
    echo      Iniciar:  net start MSSQL$SQLEXPRESS
    echo.
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  Listo. Revisa el detalle arriba para confirmar que salio bien.
echo ============================================================
echo.
pause