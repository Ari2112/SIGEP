RELACIONES SIMPLIFICADAS

## Catálogos base

* Positions
* Schedules
* PermissionTypes
* DeductionTypes
* BenefitTypes
* SystemSettings (independiente)

---

## Núcleo: Empleados y Usuarios

* Employees
  * PositionId → Positions
  * ScheduleId → Schedules
  * SupervisorId → Employees (self relation)
* Users
  * EmployeeId → Employees

---

## Auditoría y Notificaciones

* AuditLogs
  * UserId → Users
* Notifications
  * UserId → Users

---

## Vacaciones

* VacationBalances
  * EmployeeId → Employees
* VacationRequests
  * EmployeeId → Employees
  * ApprovedByUserId → Users
* VacationRequestHistory
  * VacationRequestId → VacationRequests
  * ChangedByUserId → Users

---

## Permisos

* PermissionRequests
  * EmployeeId → Employees
  * PermissionTypeId → PermissionTypes
  * ApprovedByUserId → Users

---

## Asistencia y tiempo

* AttendanceRecords
  * EmployeeId → Employees
* OvertimeRecords
  * EmployeeId → Employees
  * AttendanceId → AttendanceRecords
  * ReviewedById → Users

---

## Planilla (Payroll)

* Payrolls
  * ProcessedById → Users
  * ApprovedById → Users
* PayrollDetails
  * PayrollId → Payrolls
  * EmployeeId → Employees
* PayrollDeductions
  * PayrollDetailId → PayrollDetails
  * DeductionTypeId → DeductionTypes
* PayrollBenefits
  * PayrollDetailId → PayrollDetails
  * BenefitTypeId → BenefitTypes

---

## Liquidaciones

* Settlements
  * EmployeeId → Employees
  * CalculatedById → Users
  * ApprovedById → Users
* SettlementDeductions
  * SettlementId → Settlements

---

## Aguinaldo

* AnnualBonuses
  * CalculatedById → Users
  * ApprovedById → Users
* AnnualBonusDetails
  * AnnualBonusId → AnnualBonuses
  * EmployeeId → Employees

---

## Evaluación de desempeño

* PerformanceEvaluations
  * EmployeeId → Employees
  * EvaluatorId → Users

---

## Incapacidades

* DisabilityRequests
  * EmployeeId → Employees
  * ReviewedById → Users

---

# 🧠 MICRO RESUMEN (modelo mental rápido)

## 🧍 Centro del modelo

**Employees es el hub principal**

* Todo lo operativo gira alrededor del empleado:
  * asistencia
  * vacaciones
  * permisos
  * planilla
  * evaluaciones
  * incapacidades
  * liquidaciones
  * aguinaldo

---

## 👤 Users = capa de sistema

* Representan cuentas del sistema
* Siempre ligados a Employees (1:1 opcional)
* Se usan para:
  * aprobaciones
  * auditoría
  * notificaciones
  * evaluaciones
  * procesamiento de planillas

👉 patrón:
**Users = actor del sistema**
**Employees = entidad de negocio**

---

## 🧾 Módulos tipo workflow

Todos siguen patrón similar:

**Entidad base → historial/aprobaciones**

* VacationRequests → History
* PermissionRequests → approvals via Users
* Overtime → revisión
* DisabilityRequests → revisión

👉 patrón:
Employee crea → User aprueba

---

## 💰 Dominio financiero (más normalizado)

### Payroll (más complejo)

Jerarquía clara:

```
Payroll
 └── PayrollDetails (por empleado)
      ├── PayrollDeductions (tipos catálogo)
      └── PayrollBenefits (tipos catálogo)
```

👉 diseño contable clásico: header → detail → breakdown

---

## 🧮 Cálculos especiales

Mismo patrón jerárquico:

* Settlements → SettlementDeductions
* AnnualBonuses → AnnualBonusDetails

👉 todos los cálculos grandes usan:
**Cabecera + Detalle**

---

## ⏱ Tiempo laboral

Cadena lógica:

```
Schedules → Employees
Employees → Attendance
Attendance → Overtime
```

---

## 📚 Catálogos reutilizables

Se reutilizan como dimensiones:

* PermissionTypes
* DeductionTypes
* BenefitTypes
* Positions
* Schedules

👉 diseño tipo data warehouse friendly

---

## 🔁 Relaciones clave que debes dibujar sí o sí

Si solo diagramas lo esencial:

1. Employees ↔ Users
2. Employees self (Supervisor)
3. Payroll jerárquico
4. Requests (Vacation, Permission, Disability) → Employees + Users
5. Attendance → Overtime
6. Catálogos → entidades financieras
