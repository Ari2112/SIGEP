@echo off
:: ============================================================
:: SIGEP - Reset y Seed de Base de Datos
:: SQL Server Express - Windows Authentication
:: ============================================================

title SIGEP - Reset DB

set DB_SERVER=LAPTOP-56772AJK\SQLEXPRESS
set SQL_SCRIPT=%~dp0reset.sql

echo.
echo ============================================================
echo  SIGEP - Reset completo de base de datos
echo  Servidor: %DB_SERVER%
echo  Script  : %SQL_SCRIPT%
echo ============================================================
echo.
echo [AVISO] Esto ELIMINARA y recreara la base de datos SigepDB.
echo         Todos los datos actuales seran BORRADOS.
echo.
set /p CONFIRM=Desea continuar? (s/N):

if /i "%CONFIRM%" neq "s" (
    echo.
    echo Operacion cancelada.
    pause
    exit /b 0
)

echo.
echo [1/3] Verificando conexion a SQL Server...
sqlcmd -S "%DB_SERVER%" -E -Q "SELECT @@SERVERNAME" -b >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] No se pudo conectar a %DB_SERVER%.
    echo         Verifique que el servicio SQL Server (SQLEXPRESS) este corriendo.
    echo         Puede iniciarlo con: net start MSSQL$SQLEXPRESS
    pause
    exit /b 1
) else (
    echo      Conexion exitosa a %DB_SERVER%.
)

echo [2/3] Ejecutando reset.sql (DROP + CREATE + Schema + Seed)...
sqlcmd -S "%DB_SERVER%" -E -i "%SQL_SCRIPT%" -I -b
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la ejecucion del script SQL.
    echo         Verifique que sqlcmd este instalado y en el PATH.
    echo         Instalar con: winget install Microsoft.SqlServer.CommandLineUtils
    echo.
    pause
    exit /b 1
)

echo [3/3] Verificando tablas creadas...
sqlcmd -S "%DB_SERVER%" -E -d SigepDB -Q "SELECT COUNT(*) AS TotalTablas FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" -b
if %errorlevel% neq 0 (
    echo [ERROR] No se pudo verificar la base de datos.
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  Base de datos SigepDB lista.
echo.
echo  Usuarios de prueba (password: admin123):
echo    admin           - Administrador
echo    rrhh            - Recursos Humanos
echo    supervisor      - Jefatura
echo    juan.perez      - Empleado
echo    ana.rodriguez   - Empleado
echo    carlos.martinez - Empleado
echo ============================================================
echo.
pause
