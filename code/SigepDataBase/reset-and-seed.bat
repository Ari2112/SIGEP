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
echo [1/2] Ejecutando reset.sql (DROP + CREATE + Schema + Seed)...
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
    echo    - El servidor no coincide. Verifique en SSMS el nombre exacto.
    echo.
    pause
    exit /b 1
)

echo.
echo [2/2] Verificando tablas creadas...
sqlcmd -S "%DB_SERVER%" -E -d SigepDB -Q "SELECT COUNT(*) AS TotalTablas FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" -b

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
