
╔══════════════════════════════════════════════════╗
║        SIGEP - API Test Suite                   ║
╚══════════════════════════════════════════════════╝
  Backend: http://localhost:5017/api/v1
  Fecha: 3/27/2026, 4:43:07 PM

▶ AUTH
  ✓ Login admin (credenciales correctas) → 200              status=200
  ✓ Login admin → token presente                            token=eyJhbGciOiJIUzI1NiIs...
  ✓ Login rrhh (credenciales correctas) → 200
  ✓ Login supervisor (credenciales correctas) → 200
  ✓ Login juan.perez (credenciales correctas) → 200
  ✓ Login contraseña incorrecta → 401                       status=401
  ✓ Login usuario inexistente → 401                         status=401
  ✓ GET /me con token válido → 200                          user=admin
  ✓ GET /me sin token → 401                                 status=401

▶ EMPLEADOS
  ✓ GET /employees → 200                                    count=10
  ✓ GET /employees sin auth → 401
  ✓ POST /employees (como RRHH) → 200/201                   status=201
    ID empleado creado                                      #11
  ✓ POST /employees (empleado sin permiso) → 403            status=403
  ✓ GET /employees/1 → 200                                  name=Admin
  ✓ PUT /employees/{id} (actualizar salario) → 200          status=200
  ✓ GET /employees/positions → 200                          count=9
  ✓ GET /employees/schedules → 200                          count=5

▶ VACACIONES
  ✗ GET /vacations/balance (empleado) → 200                 available=undefined
  ✗ GET /vacations/balance/3 (admin) → 200                  status=500
  ✓ GET /vacations/balance/1 (empleado → otro) → 403        status=403
  ✓ GET /vacations/requests/my → 200                        count=1
  ✗ POST /vacations/requests (crear solicitud) → 200/201    status=500
  ✓ GET /vacations/requests/pending (admin) → 200           count=1

▶ PERMISOS
  ✓ GET /permissions/types → 200                            count=9
  ✓ GET /permissions/requests/my → 200                      count=1
  ✓ POST /permissions/requests (crear) → 200/201            status=201
  ✓ GET /permissions/requests/pending (supervisor) → 200
  ✓ POST /permissions/requests/4/approve → 200              status=200

▶ ASISTENCIA
  ✓ GET /attendance/today → 200 ó 204                       status=204
  ✓ GET /attendance/my → 200                                count=0
  ✓ GET /attendance (admin) → 200                           count=0
  ✓ GET /attendance (empleado sin permiso) → 403            status=403
  ✓ POST /attendance/check-in → 200/201                     check-in registrado
  ✓ POST /attendance/check-out → 200/201                    status=200

▶ HORAS EXTRA
  ✓ GET /overtime (admin) → 200                             count=1
  ✓ GET /overtime/my → 200                                  count=1
  ✓ GET /overtime (empleado → sin permiso) → 403            status=403
  ⚠ POST /overtime/review                                   No hay registros pendientes de revisión

▶ PLANILLA
  ✓ GET /payroll (admin) → 200                              count=0
  ✓ GET /payroll (empleado → sin permiso) → 403             status=403
  ✓ POST /payroll/generate → 200/201                        id=1
  ✓ GET /payroll/1 → 200                                    employees=11
  ✓ POST /payroll/1/approve → 200                           status=200
  ✓ GET /payroll/deduction-types → 200                      count=7
  ✓ GET /payroll/benefit-types → 200                        count=5

▶ LIQUIDACIONES
  ✓ GET /settlement (admin) → 200                           count=0
  ✓ GET /settlement (empleado → sin permiso) → 403          status=403
  ✓ POST /settlement/calculate → 200/201                    status=200 msg=undefined

▶ AGUINALDO
  ✓ GET /annualbonus (admin) → 200                          count=0
  ✓ GET /annualbonus (empleado → sin permiso) → 403         status=403
  ✓ POST /annualbonus/calculate → 200/201                   id=1
  ✓ GET /annualbonus/1 → 200                                employees=11

▶ EVALUACIÓN DE DESEMPEÑO
  ✓ GET /performanceevaluation (supervisor) → 200           count=0
  ✓ GET /performanceevaluation/my → 200                     count=0
  ✓ POST /performanceevaluation (crear) → 200/201           status=200
  ✓ POST /performanceevaluation/1/acknowledge → 200         status=200

▶ INCAPACIDADES
  ✓ GET /disability (admin) → 200                           count=0
  ✓ GET /disability/my → 200                                count=0
  ✓ POST /disability (crear) → 200/201                      status=200
  ✓ POST /disability/1/review → 200                         status=200

▶ REPORTES
  ✓ GET /report/dashboard → 200                             keys=totalEmployees,activeEmployees,pendingVacations,pendingPermissions,pendingOvertimes,pendingDisabilities,monthlyPayrollTotal,attendanceTodayCount,checkedInToday
  ✓ GET /report/attendance → 200                            status=200
  ✓ GET /report/overtime → 200                              status=200
  ✓ GET /report/payroll/1 → 200                             status=200

▶ NOTIFICACIONES
  ✓ GET /notifications → 200                                count=1
  ✓ GET /notifications/unread-count → 200                   count=1
  ✓ POST /notifications/read-all → 200 ó 204                status=204

▶ LIMPIEZA
  ✓ DELETE /employees/11 (limpiar test) → 200/204           status=204

═══════════════════════════════════════════════════════
  RESULTADOS FINALES
═══════════════════════════════════════════════════════
  ✓ Pasaron: 64  ✗ Fallaron: 3  ⚠ Advertencias: 1
  Total: 67 pruebas
  [███████████████████░] 96%
═══════════════════════════════════════════════════════
