
---

# Plan de implementación para Claude (C#/.NET + React + SQL Server)

## 0) Decisiones base (para no desviarse)

* **Backend:** ASP.NET Core Web API (.NET 8 ideal)
* **ORM:** Entity Framework Core + Migrations
* **Auth:** JWT + Roles (Empleado / Jefatura / RRHH / Admin)
* **DB:** SQL Server (Express o Standard según hosting)
* **Frontend:** React 18 + React Router + (MUI/PrimeReact/Tailwind según prefieran)
* **Arquitectura:** Clean-ish por capas: API → Application (servicios) → Domain (reglas) → Infrastructure (DB)

> Regla clave:  **la lógica legal (planilla/aguinaldo/liquidación) vive en servicios de dominio** , nunca pegada a controllers.

---

## 1) Estructura de repositorio (monorepo recomendado)

```txt
/HRSystem
  /backend
    /src
      HRSystem.Api
      HRSystem.Application
      HRSystem.Domain
      HRSystem.Infrastructure
    /tests
      HRSystem.Tests
  /frontend
  /docs
  docker-compose.yml (opcional)
  README.md
```

### Backend projects (Visual Studio Solution)

* `HRSystem.Api` (Controllers, Middlewares, Auth, DI)
* `HRSystem.Application` (UseCases/Services, DTOs, Validators)
* `HRSystem.Domain` (Entidades, ValueObjects, reglas de negocio)
* `HRSystem.Infrastructure` (DbContext, Repos, EF configs)

---

## 2) Paso 1: Crear la base del backend (.NET)

**Objetivo:** dejar “esqueleto” listo para crecer por módulos.

Checklist:

* Crear Solution + proyectos por capa
* Configurar:
  * `appsettings.json` + variables de entorno
  * Logging
  * CORS (para React)
  * Swagger/OpenAPI
* Health endpoint: `/health`

---

## 3) Paso 2: Base de datos + EF Core (antes de módulos)

### Modelo mínimo inicial (tablas base)

**Seguridad**

* Users
* Roles
* UserRoles (si aplica)
* RefreshTokens (opcional)
* AuditLog (muy recomendado)

**Organización**

* Employees
* Positions (Puestos)
* Schedules (Horarios)

**Catálogos RRHH**

* PermissionTypes
* IncapacityTypes
* TaxBrackets (tramos renta)
* LiquidationTypes

> Luego se agregan las tablas operativas por módulo.

### Migraciones

* EF Core Migrations desde `Infrastructure`
* Comando típico: `Add-Migration InitialCreate` / `Update-Database` (o CLI)

---

## 4) Paso 3: Autenticación + roles (fundación del sistema)

Implementar primero:

* Registro/Seed de Admin
* Login (JWT)
* Authorization por rol (policies)
* Guardar claims: `userId`, `role`

Endpoints mínimos:

* `POST /auth/login`
* `GET /auth/me`
* `POST /auth/refresh` (opcional)

Frontend:

* Login page
* Guard de rutas por rol
* Layout protegido

---

## 5) Paso 4: Orden correcto de módulos (NO cambiar el orden)

Este orden evita bloquearte con dependencias:

### (A) Catálogos + Empleados

1. **Puestos / Horarios (CRUD)**
2. **Empleados (CRUD)**
   * salario base, tipo jornada, fecha ingreso, estado

### (B) Tiempo y asistencia

3. **Asistencia**
   * marcas entrada/salida
   * reportes básicos por rango
4. **Horas extra**
   * solicitud → aprobación → registro
   * cálculo (se usa después en planilla)

### (C) Ausencias

5. **Vacaciones**
   * saldo/acumulación
   * solicitud → aprobación → historial
6. **Permisos**
   * con goce / sin goce
   * solicitud → aprobación → estados
7. **Incapacidades**
   * registro de incapacidades
   * impacto en asistencia/planilla (según reglas del negocio del proyecto)

### (D) Dinero (cuando ya hay asistencia/ausencias)

8. **Planilla**
   * genera corrida por periodo
   * calcula: salario + extras – deducciones
9. **Aguinaldo**
   * cálculo anual/proporcional
   * reglas según legislación del proyecto
10. **Liquidaciones**

* preaviso, cesantía, vacaciones pendientes, aguinaldo proporcional

### (E) Salidas

11. **Reportes + Consultas**

* export Excel/PDF si se requiere

---

## 6) Diseño por módulo (patrón repetible)

Cada módulo debe seguir el mismo patrón para velocidad y consistencia:

**Domain**

* Entidad + reglas (ej: `VacationRequest`, `PayrollRun`, etc.)

**Application**

* DTOs
* Commands/Handlers o Services
* Validaciones (FluentValidation si quieren)

**Infrastructure**

* Repos (EF queries)
* Configurations (fluent API)

**Api**

* Controller endpoints limpios
* Responses consistentes

---

## 7) Flujos de aprobación (estándar para vacaciones/permisos/horas extra)

Estados:

* `Pending`
* `Approved`
* `Rejected`
* `Cancelled` (opcional)

Campos recomendados:

* `RequestedBy`
* `ApprovedBy`
* `DecisionAt`
* `DecisionComment`
* `CreatedAt`, `UpdatedAt`

Regla:  **nunca borrar** , solo cambiar estado (trazabilidad).

---

## 8) Planilla: cómo evitar el caos (regla de oro)

En `Domain/Application` crear calculadoras:

* `PayrollCalculator`
* `DeductionsCalculator` (CCSS + renta por tramos)
* `OvertimeCalculator`
* `AguinaldoCalculator`
* `SettlementCalculator`

**Planilla debe ser reproducible**

* Guardar en tabla:
  * bruto
  * deducciones detalladas
  * neto
  * parámetros usados (tramos de renta de ese momento)

Esto es importante para auditoría y correcciones posteriores.

---

## 9) Frontend (React) en paralelo, por entregas

Estructura recomendada:

```txt
/src
  /api (axios client + interceptors)
  /auth
  /components
  /pages
  /routes
  /state (zustand/redux opcional)
```

Implementar UI en el mismo orden que backend:

1. Login + Layout
2. Empleados
3. Asistencia
4. Horas extra
5. Vacaciones
6. Permisos
7. Incapacidades
8. Planilla
9. Aguinaldo
10. Liquidaciones
11. Reportes

---

## 10) Pruebas (mínimo profesional)

* Unit tests para:
  * calculadoras (planilla/aguinaldo/liquidación)
* Integration tests para:
  * auth
  * generación de planilla
* Datos semilla para escenarios (empleado con/ sin horas extra, con incapacidad, etc.)

---

# Prompt listo para pegarle a Claude (si querés)

Puedes copiar y pegar esto tal cual:

**“Quiero que construyas un sistema web de RRHH usando: Backend ASP.NET Core Web API (.NET 8, C#), Frontend React 18, Base de datos SQL Server, siguiendo arquitectura por capas (Api, Application, Domain, Infrastructure) con EF Core migrations. Implementa primero Auth JWT y roles (Empleado/Jefatura/RRHH/Admin). Luego módulos en este orden: Empleados/Puestos/Horarios → Asistencia → Horas Extra (solicitud/aprobación) → Vacaciones (saldo+solicitud/aprobación) → Permisos → Incapacidades → Planilla (con deducciones y cálculo reproducible) → Aguinaldo → Liquidaciones → Reportes. Toda lógica legal y cálculos deben estar en servicios/calculadoras en Application/Domain, nunca en Controllers. Mantén trazabilidad (estados, audit log opcional, no borrar registros). Genera endpoints REST versionados /api/v1 y UI en React con guard de rutas por rol.”**
