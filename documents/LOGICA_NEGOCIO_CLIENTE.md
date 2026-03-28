# SIGEP - Sistema Integral de Gestión de Personal
## Resumen Ejecutivo para el Cliente

---

## 1. ¿Qué es SIGEP?

SIGEP es una **plataforma web centralizada** diseñada para digitalizar y automatizar todos los procesos de gestión de recursos humanos de *Alquileres Segura*.

El sistema reemplaza:
- Formularios físicos
- Hojas de cálculo manuales
- Cálculos propensos a errores

Por una solución que ofrece:
- **Control de acceso por roles** (quién puede ver y hacer qué)
- **Trazabilidad completa** (registro de todas las acciones)
- **Validaciones automáticas** (el sistema verifica reglas de negocio)
- **Generación de documentos oficiales** (PDF)

---

## 2. Roles del Sistema

| Rol | Descripción | Permisos principales |
|-----|-------------|---------------------|
| **Empleado** | Personal de la empresa | Registrar asistencia, solicitar vacaciones/permisos, consultar sus datos |
| **Jefatura** | Supervisores de área | Todo lo del empleado + aprobar solicitudes de su equipo |
| **RRHH** | Recursos Humanos | Gestión completa del personal, planillas, liquidaciones, reportes |
| **Admin** | Administrador del sistema | Acceso total + configuración del sistema |

---

## 3. Módulos Implementados

### 3.1 Autenticación y Seguridad
- Inicio de sesión con usuario y contraseña
- Control de acceso basado en roles
- Registro automático de todas las acciones (auditoría)
- Notificaciones en tiempo real

### 3.2 Gestión de Empleados
- Registro completo de información personal y laboral
- Asignación de cargo, horario y supervisor
- Estados: Activo, Inactivo, Suspendido, Liquidado
- Historial de cambios

### 3.3 Vacaciones
**Flujo:**
1. El empleado solicita vacaciones indicando fechas
2. El sistema valida automáticamente el saldo disponible
3. RRHH/Jefatura aprueba o rechaza con comentarios
4. Se actualiza el saldo y se notifica al empleado

**Reglas de negocio:**
- Cada empleado tiene un saldo anual de días
- Se descuentan los días pendientes (solicitudes en proceso)
- Se permite arrastrar días no utilizados (con fecha de vencimiento)
- No se pueden solicitar más días de los disponibles

### 3.4 Permisos
**Tipos de permiso configurables:**
- Cita médica (hasta 12 días/año, requiere documento)
- Asunto personal (hasta 6 días/año)
- Duelo (hasta 5 días, requiere documento)
- Matrimonio (3 días, requiere documento)
- Nacimiento de hijo (3 días, requiere documento)
- Emergencias (sin límite, justificado)

**Flujo similar a vacaciones:** solicitud → validación → aprobación/rechazo → notificación

### 3.5 Asistencia
- El empleado registra entrada y salida con un clic
- El sistema captura automáticamente la hora exacta
- Solo se permiten 2 marcas por día (entrada y salida)
- Los registros son **inmutables** (no se pueden modificar ni eliminar)

**Estados de asistencia:**
- Parcial (solo entrada)
- Completo (entrada y salida)
- Ausente
- Permiso
- Vacaciones
- Incapacidad

### 3.6 Horas Extra
- El sistema detecta **automáticamente** cuando un empleado trabaja más de su jornada
- RRHH revisa las horas detectadas
- Aprueba o rechaza con justificación
- Las horas aprobadas se integran a la planilla

**Fórmula:** Horas × Tarifa por hora × Multiplicador (1.5x por defecto)

---

## 4. Módulos en Desarrollo (Próximas Fases)

### 4.1 Planilla
- Generación quincenal o mensual
- Integra: salario base + horas extra - deducciones + beneficios
- Comprobantes individuales por empleado
- Exportación a PDF

### 4.2 Liquidaciones
- Cálculo automático al terminar relación laboral
- Incluye: vacaciones pendientes, aguinaldo proporcional, cesantía
- Diferentes tipos de terminación (renuncia, despido, mutuo acuerdo)

### 4.3 Aguinaldo
- Cálculo anual basado en salarios del período
- Fórmula configurable
- Detalle por empleado

### 4.4 Evaluaciones de Desempeño
- Calificación numérica (3 a 10)
- Registro de fortalezas y áreas de mejora
- Historial por empleado

### 4.5 Incapacidades
- Registro con documentación adjunta
- Flujo de aprobación
- Afecta asistencia y planilla

---

## 5. Estructura de Datos (Base de Datos)

### 5.1 Tablas de Catálogos (Configuración)

| Tabla | Propósito |
|-------|-----------|
| `Schedules` | Horarios de trabajo (ej: 8am-5pm, lunes a viernes) |
| `Positions` | Cargos/puestos de la empresa |
| `PermissionTypes` | Tipos de permisos disponibles |
| `DeductionTypes` | Tipos de deducciones (CCSS, impuestos, etc.) |
| `BenefitTypes` | Tipos de beneficios (bonificaciones, etc.) |
| `SystemSettings` | Configuraciones del sistema |

### 5.2 Tablas de Personal

| Tabla | Propósito |
|-------|-----------|
| `Employees` | Información de empleados (nombre, ID, salario, cargo, supervisor) |
| `Users` | Credenciales de acceso al sistema |

### 5.3 Tablas de Módulos Operativos

| Tabla | Propósito |
|-------|-----------|
| `VacationBalances` | Saldo de vacaciones por empleado por año |
| `VacationRequests` | Solicitudes de vacaciones |
| `VacationRequestHistory` | Historial de cambios en solicitudes |
| `PermissionRequests` | Solicitudes de permisos |
| `AttendanceRecords` | Registros diarios de entrada/salida |
| `OvertimeRecords` | Horas extra detectadas y su estado |

### 5.4 Tablas de Nómina

| Tabla | Propósito |
|-------|-----------|
| `Payrolls` | Planillas generadas (cabecera) |
| `PayrollDetails` | Detalle por empleado en cada planilla |
| `PayrollDeductions` | Deducciones aplicadas |
| `PayrollBenefits` | Beneficios aplicados |
| `Settlements` | Liquidaciones laborales |
| `AnnualBonuses` | Aguinaldos calculados |
| `AnnualBonusDetails` | Detalle de aguinaldo por empleado |

### 5.5 Tablas de Gestión

| Tabla | Propósito |
|-------|-----------|
| `PerformanceEvaluations` | Evaluaciones de desempeño |
| `DisabilityRequests` | Solicitudes de incapacidad |
| `Notifications` | Notificaciones para usuarios |
| `AuditLogs` | Bitácora de auditoría (trazabilidad) |

---

## 6. Relaciones Clave entre Módulos

```
┌─────────────┐     ┌──────────────┐     ┌────────────┐
│  Empleados  │────▶│  Asistencia  │────▶│ Horas Extra│
└─────────────┘     └──────────────┘     └──────┬─────┘
       │                                        │
       │            ┌──────────────┐            │
       ├───────────▶│  Vacaciones  │            │
       │            └──────────────┘            │
       │                                        ▼
       │            ┌──────────────┐     ┌────────────┐
       ├───────────▶│   Permisos   │     │  Planilla  │
       │            └──────────────┘     └──────┬─────┘
       │                                        │
       │            ┌──────────────┐            │
       └───────────▶│ Liquidación  │◀───────────┘
                    └──────────────┘
```

**Dependencias:**
- La **asistencia** alimenta las **horas extra**
- Las **horas extra aprobadas** se incluyen en la **planilla**
- Las **vacaciones pendientes** se incluyen en la **liquidación**
- El **aguinaldo** se calcula con el historial de **planillas**

---

## 7. Validaciones Automáticas del Sistema

| Módulo | Validaciones |
|--------|--------------|
| Vacaciones | Saldo suficiente, fechas no superpuestas, fecha fin ≥ fecha inicio |
| Permisos | Límite anual por tipo, documento adjunto si es requerido |
| Asistencia | Una entrada por día, no salida sin entrada, registros inmutables |
| Horas Extra | Solo sobre asistencia completa, no duplicar períodos |
| Planilla | Un registro por empleado por período, no generar duplicados |

---

## 8. Beneficios para la Empresa

1. **Reducción de errores** en cálculos de planilla y liquidaciones
2. **Ahorro de tiempo** en procesos que antes eran manuales
3. **Trazabilidad total** para auditorías
4. **Información centralizada** y accesible
5. **Cumplimiento normativo** mediante validaciones automáticas
6. **Histórico completo** de toda la información laboral

---

## 9. Estado Actual del Proyecto

| Módulo | Estado |
|--------|--------|
| Autenticación | ✅ Implementado |
| Gestión de Empleados | ✅ Implementado |
| Vacaciones | ✅ Implementado |
| Permisos | ✅ Implementado |
| Asistencia | ✅ Implementado |
| Horas Extra | ✅ Implementado |
| Notificaciones | ✅ Implementado |
| Bitácora de Auditoría | ✅ Implementado |
| Planilla | 🔄 En desarrollo |
| Liquidaciones | 🔄 Pendiente |
| Aguinaldo | 🔄 Pendiente |
| Evaluaciones | 🔄 Pendiente |
| Incapacidades | 🔄 Pendiente |
| Reportes PDF | 🔄 Pendiente |

---

*Documento generado para Alquileres Segura - SIGEP v1.0*
