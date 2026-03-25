@echo off
:: ============================================================
:: SIGEP - Reset y Seed de Base de Datos Local
:: SQL Server LocalDB (mssqllocaldb)
:: ============================================================

title SIGEP - Reset DB

set DB_SERVER=(localdb)\mssqllocaldb
set SQL_SCRIPT=%~dp0reset.sql

echo.
echo ============================================================
echo  SIGEP - Reset completo de base de datos local
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
echo [1/3] Verificando que LocalDB este corriendo...
SqlLocalDB.exe start mssqllocaldb >nul 2>&1
if %errorlevel% neq 0 (
    echo      LocalDB ya esta corriendo o se inicio correctamente.
) else (
    echo      LocalDB iniciado.
)

echo [2/3] Ejecutando reset.sql (DROP + CREATE + Schema + Seed)...
sqlcmd -S "%DB_SERVER%" -i "%SQL_SCRIPT%" -b
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
sqlcmd -S "%DB_SERVER%" -d SigepDB -Q "SELECT COUNT(*) AS TotalTablas FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" -b
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
echo    admin        - Administrador
echo    rrhh         - Recursos Humanos
echo    supervisor   - Jefatura
echo    juan.perez   - Empleado
echo    ana.rodriguez- Empleado
echo    carlos.martinez - Empleado
echo ============================================================
echo.
pause
