-- ============================================================
-- SIGEP - Limpieza de empleados de prueba + Liquidaciones
-- ============================================================
-- Qué hace este script:
--   1) Borra los 4 empleados de prueba (y TODO lo relacionado a
--      ellos: usuario/login, direcciones, teléfonos, vacaciones,
--      permisos, asistencia, horas extra, planilla, evaluaciones,
--      incapacidades, aguinaldo y liquidaciones) para que no
--      aparezcan más en el sistema.
--   2) Vacía por completo el módulo de Liquidaciones (todas las
--      liquidaciones de prueba, no solo las de estos 4 empleados),
--      tal como se pidió.
--
-- Empleados que se van a borrar (identificados por cédula):
--   118630522 - Arianna Quiros Alpizaar
--   118630529 - Arianna Quiros Alpizar
--   118630524 - Giannina Quiros Alpizar
--   118635236 - Brayan Quiros Alpizar
--
-- Es seguro volver a correr este script: si esos empleados ya
-- fueron borrados, simplemente no encuentra nada que borrar.
--
-- Todo corre dentro de una sola transacción: si algo falla, no
-- se borra nada (rollback automático) y se imprime el error.
-- ============================================================

USE SigepDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- ------------------------------------------------------------
    -- 0) Ubicar los empleados y usuarios objetivo
    -- ------------------------------------------------------------
    IF OBJECT_ID('tempdb..#TargetEmployees') IS NOT NULL DROP TABLE #TargetEmployees;
    IF OBJECT_ID('tempdb..#TargetUsers') IS NOT NULL DROP TABLE #TargetUsers;

    SELECT Id
    INTO #TargetEmployees
    FROM dbo.Employees
    WHERE IdentificationNumber IN ('118630522', '118630529', '118630524', '118635236');

    SELECT Id
    INTO #TargetUsers
    FROM dbo.Users
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DECLARE @TotalEmpleados INT = (SELECT COUNT(*) FROM #TargetEmployees);
    PRINT 'Empleados de prueba encontrados: ' + CAST(@TotalEmpleados AS NVARCHAR(10));

    IF @TotalEmpleados = 0
    BEGIN
        PRINT 'No hay nada que borrar (ya se limpiaron antes, o las cédulas no existen).';
    END

    -- ------------------------------------------------------------
    -- 1) Quitar auto-referencias (por si alguno es "jefe" de otro
    --    empleado en el sistema)
    -- ------------------------------------------------------------
    UPDATE dbo.Employees
    SET SupervisorId = NULL
    WHERE SupervisorId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 2) Vacaciones
    -- ------------------------------------------------------------
    DELETE h
    FROM dbo.VacationRequestHistory h
    INNER JOIN dbo.VacationRequests v ON v.Id = h.VacationRequestId
    WHERE v.EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.VacationRequests
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.VacationBalances
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 3) Permisos
    -- ------------------------------------------------------------
    DELETE FROM dbo.PermissionRequests
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 4) Horas extra y Asistencia
    -- ------------------------------------------------------------
    DELETE FROM dbo.OvertimeRecords
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.AttendanceRecords
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 5) Planilla
    -- ------------------------------------------------------------
    DELETE pd
    FROM dbo.PayrollDeductions pd
    INNER JOIN dbo.PayrollDetails d ON d.Id = pd.PayrollDetailId
    WHERE d.EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE pb
    FROM dbo.PayrollBenefits pb
    INNER JOIN dbo.PayrollDetails d ON d.Id = pb.PayrollDetailId
    WHERE d.EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.PayrollDetails
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 6) Aguinaldo (solo el detalle de estos empleados; si el
    --    aguinaldo del año ya estaba calculado, usa "Recalcular"
    --    desde la pantalla de Aguinaldo para que el total se
    --    actualice sin estos empleados)
    -- ------------------------------------------------------------
    DELETE FROM dbo.AnnualBonusDetails
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 7) Evaluaciones de desempeño e Incapacidades
    -- ------------------------------------------------------------
    DELETE FROM dbo.PerformanceEvaluations
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.DisabilityRequests
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 8) Datos de contacto del empleado
    -- ------------------------------------------------------------
    DELETE FROM dbo.EmployeeAddresses
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.EmployeePhones
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 9) LIQUIDACIONES: se vacía el módulo completo (todas las
    --    liquidaciones de prueba, de cualquier empleado), tal como
    --    se pidió. Si en el futuro solo quieres borrar las de
    --    estos 4 empleados, comenta este bloque "TODAS" y descomenta
    --    el bloque "SOLO estos empleados" de abajo.
    -- ------------------------------------------------------------

    -- === TODAS las liquidaciones ===
    DELETE FROM dbo.SettlementDeductions;
    DELETE FROM dbo.Settlements;

    -- === (Alternativa) SOLO las de estos empleados ===
    -- DELETE sd
    -- FROM dbo.SettlementDeductions sd
    -- INNER JOIN dbo.Settlements s ON s.Id = sd.SettlementId
    -- WHERE s.EmployeeId IN (SELECT Id FROM #TargetEmployees);
    --
    -- DELETE FROM dbo.Settlements
    -- WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    -- ------------------------------------------------------------
    -- 10) Bitácora y notificaciones del usuario/login de estos
    --     empleados (no se toca la bitácora de nadie más)
    -- ------------------------------------------------------------
    DELETE FROM dbo.AuditLogs
    WHERE UserId IN (SELECT Id FROM #TargetUsers);

    DELETE FROM dbo.Notifications
    WHERE UserId IN (SELECT Id FROM #TargetUsers);

    -- ------------------------------------------------------------
    -- 11) Usuario/login y por último el empleado
    -- ------------------------------------------------------------
    DELETE FROM dbo.Users
    WHERE EmployeeId IN (SELECT Id FROM #TargetEmployees);

    DELETE FROM dbo.Employees
    WHERE Id IN (SELECT Id FROM #TargetEmployees);

    DROP TABLE #TargetEmployees;
    DROP TABLE #TargetUsers;

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '✔ Limpieza completada: empleados de prueba y liquidaciones borrados.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '✘ Ocurrió un error, NO se borró nada (se deshizo todo):';
    PRINT ERROR_MESSAGE();
END CATCH
GO