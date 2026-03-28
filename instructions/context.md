# Contexto del Proyecto – Sistema de Gestión de Recursos Humanos (SIGEP)

## 1. Visión general del sistema

La aplicación es un **Sistema Integral de Gestión de Recursos Humanos (RRHH)** diseñado para una empresa privada (caso de estudio:  *Alquileres Segura* ), cuyo objetivo es **digitalizar, centralizar y automatizar** los procesos administrativos relacionados con la gestión del personal.

El sistema reemplaza procesos manuales y dispersos (formularios físicos, hojas de cálculo, cálculos manuales) por una  **plataforma web centralizada** , con control de acceso por roles, trazabilidad de acciones, validaciones automáticas y generación de documentos oficiales.

La aplicación está pensada como un  **sistema interno corporativo** , no como una app pública, y es utilizada principalmente por:

* **Empleados**
* **Administradores de Recursos Humanos (RRHH)**

---

## 2. Objetivos principales

* Centralizar toda la información laboral del personal.
* Reducir errores humanos en cálculos de planilla, aguinaldo y liquidaciones.
* Automatizar validaciones (fechas, saldos, duplicados, políticas internas).
* Proveer trazabilidad completa de solicitudes, aprobaciones y cambios.
* Facilitar la generación de documentos oficiales (PDF).
* Servir como fuente única de verdad para asistencia, pagos y evaluaciones.

---

## 3. Roles del sistema

### 3.1 Empleado

Puede:

* Iniciar sesión en el sistema.
* Registrar su asistencia diaria (entrada y salida).
* Solicitar vacaciones, permisos e incapacidades.
* Consultar el estado de sus solicitudes.
* Recibir notificaciones de aprobaciones o rechazos.
* Consultar comprobantes de pago (cuando estén disponibles).

No puede:

* Modificar registros históricos.
* Aprobar solicitudes.
* Generar planillas o cálculos globales.

### 3.2 Administrador de RRHH

Puede:

* Acceder a todos los módulos del sistema.
* Aprobar o rechazar solicitudes (vacaciones, permisos, horas extra, incapacidades).
* Consultar y auditar registros históricos.
* Generar planillas, liquidaciones y aguinaldos.
* Evaluar el desempeño de empleados.
* Generar reportes y documentos PDF.
* Acceder a bitácoras de auditoría.

---

## 4. Módulos funcionales del sistema

El sistema está organizado en  **módulos independientes pero interconectados** , accesibles desde un tablero principal según el rol del usuario.

---

### 4.1 Autenticación, roles y bitácora (módulo transversal)

* Login con usuario y contraseña.
* Validación de rol (Empleado / RRHH).
* Redirección a un tablero con los módulos habilitados.
* Registro de acciones relevantes en una  **bitácora de auditoría** :
  * Usuario
  * Acción
  * Módulo
  * Fecha y hora
* Cierre de sesión seguro.

Este módulo es transversal y soporta a todos los demás.

---

### 4.2 Módulo de Vacaciones

Permite la gestión completa del ciclo de vacaciones.

**Empleado:**

* Solicita vacaciones indicando rango de fechas.
* El sistema valida automáticamente:
  * Saldo disponible.
  * Conflictos de fechas.
* Puede modificar y reenviar solicitudes observadas.

**RRHH:**

* Revisa solicitudes pendientes.
* Aprueba o rechaza con observaciones.
* El sistema actualiza estados y notifica al empleado.

Todo cambio queda registrado con trazabilidad.

---

### 4.3 Módulo de Permisos

Gestiona permisos especiales (personales, médicos, otros).

**Empleado:**

* Registra solicitudes indicando tipo de permiso y fechas.
* El sistema evita solicitudes duplicadas.
* Consulta el estado de sus permisos.

**RRHH:**

* Evalúa y aprueba o rechaza según políticas internas.
* El sistema valida fechas y reglas predefinidas.
* Se notifican los cambios de estado.

---

### 4.4 Módulo de Asistencia

Controla la marcación diaria del personal.

**Empleado:**

* Registra entrada y salida.
* El sistema usa la hora del servidor.
* Solo se permiten dos marcas por día.
* No se permite salida sin entrada previa.

**RRHH:**

* Consulta registros por empleado.
* Los registros son inmutables (no editables ni eliminables).

Este módulo es base para  **horas extra y planilla** .

---

### 4.5 Módulo de Horas Extra

Se apoya en los registros de asistencia.

**Sistema:**

* Detecta automáticamente horas fuera de la jornada laboral.

**RRHH:**

* Revisa horas extra detectadas.
* Aprueba o rechaza con observaciones.
* Las horas aprobadas se marcan para planilla.
* Se registran en bitácora.

---

### 4.6 Módulo de Planilla

Genera la planilla quincenal de salarios.

**RRHH:**

* Selecciona periodo (quincena).
* El sistema:
  * Obtiene empleados activos.
  * Integra asistencia, horas extra aprobadas, deducciones y beneficios.
  * Calcula salario bruto, deducciones y salario neto.
* Se evita generar planillas duplicadas.
* Se alertan empleados con datos incompletos.
* Se pueden generar:
  * Planilla consolidada.
  * Comprobantes individuales por empleado.
* Exportación a PDF.

---

### 4.7 Módulo de Liquidaciones

Gestiona liquidaciones laborales (salida de empleados).

**RRHH:**

* Ingresa datos mínimos:
  * Fechas de ingreso y salida.
  * Salario promedio.
  * Vacaciones pendientes.
  * Deducciones.
* El sistema calcula:
  * Monto total.
  * Desglose detallado.
* Se valida que todos los datos obligatorios estén presentes.
* Se guarda la liquidación para consulta futura.

---

### 4.8 Módulo de Aguinaldo

Calcula el aguinaldo anual.

**RRHH:**

* Selecciona el año.
* El sistema consolida salarios del periodo definido.
* Aplica fórmula de cálculo parametrizada.
* Genera montos por empleado.
* Evita duplicar cálculos del mismo año.
* Permite:
  * Ver desglose por empleado.
  * Recalcular si hay cambios.

---

### 4.9 Módulo de Evaluación de Desempeño

Registra evaluaciones internas.

**RRHH:**

* Selecciona empleado.
* Asigna calificación numérica (3 a 10).
* Observación opcional.
* Se guarda con:
  * Fecha
  * Evaluador
* Se puede consultar el historial de evaluaciones.

---

### 4.10 Módulo de Incapacidades

Gestiona incapacidades médicas u otras.

**Empleado:**

* Registra incapacidad.
* Adjunta boleta/documento.

**RRHH:**

* Evalúa solicitud.
* Aprueba o rechaza con justificación.
* El sistema registra y notifica el resultado.

---

### 4.11 Módulo de Reportes

Provee información consolidada para control y auditoría.

Incluye:

* Reportes de asistencia.
* Reportes de horas extra.
* Reportes de planilla.
* Exportación en PDF.

---

## 5. Características no funcionales clave

* **Trazabilidad completa** de acciones.
* **Integridad de datos** (evitar duplicados, registros inválidos).
* **Validaciones automáticas** en frontend y backend.
* **Separación clara de responsabilidades por rol** .
* **Persistencia histórica** (no se eliminan datos críticos).
* **Generación de documentos oficiales (PDF)** .
* **Base de datos como fuente única de verdad** .

---

## 6. Naturaleza del sistema para desarrollo

* Sistema  **modular** , orientado a casos de uso.
* Backend con lógica de negocio fuerte (reglas laborales).
* Frontend orientado a flujos administrativos claros.
* Dependencias claras entre módulos:
  * Asistencia → Horas Extra → Planilla → Aguinaldo
* Ideal para arquitectura en capas o hexagonal.
