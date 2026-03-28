## 0) Módulo transversal: Acceso, roles, navegación y bitácora

**Contexto:** login, validación de rol, tablero con módulos habilitados y bitácora.

### HU-0.1 Login con rol

**Como** usuario (Empleado o Admin RRHH) **quiero** iniciar sesión **para** acceder al sistema según mi rol.

**Aceptación:**

* Dado usuario/contraseña válidos, el sistema valida autenticidad y rol.
* Muestra tablero con módulos habilitados por rol.

### HU-0.2 Navegación a módulos desde tablero/menú

**Como** usuario **quiero** navegar a cualquier módulo habilitado **para** realizar mis gestiones.

**Aceptación:**

* Desde tablero puedo abrir vacaciones, permisos, asistencia, horas extra, planilla, liquidaciones, aguinaldo, reportes, evaluación.

### HU-0.3 Bitácora de acciones relevantes

**Como** Administrador **quiero** que el sistema registre acciones relevantes **para** auditoría.

**Aceptación:**

* Se registra usuario, módulo y fecha en bitácora.

### HU-0.4 Cierre de sesión

**Como** usuario **quiero** cerrar sesión **para** finalizar de forma segura.

**Aceptación:**

* Al cerrar sesión, queda cerrada correctamente.
* Acciones quedan registradas en auditoría.

---

## 1) Módulo: Vacaciones

**Contexto:** registrar/aprobar/modificar/consultar; validar saldo y conflictos; notificaciones; trazabilidad.

### HU-1.1 Crear solicitud de vacaciones

**Como** empleado **quiero** solicitar vacaciones indicando fechas **para** gestionar mi tiempo libre.

**Aceptación:**

* El sistema valida saldo y conflicto de fechas automáticamente.
* La solicitud queda registrada con estado y trazabilidad.

### HU-1.2 Revisar y aprobar/rechazar solicitud

**Como** Admin RRHH **quiero** aprobar o desaprobar solicitudes **para** controlar disponibilidad.

**Aceptación:**

* Admin abre solicitud en estado pendiente y decide aprobar/desaprobar.
* El sistema notifica resultado al empleado y guarda en BD.

### HU-1.3 Reenviar solicitud modificada (revisión)

**Como** empleado **quiero** modificar una solicitud observada **para** reenviarla a revisión.

**Aceptación:**

* Usuario cambia fechas/observación y guarda.
* El sistema versiona y reenvía a revisión.

### HU-1.4 Notificaciones por cambio de estado

**Como** empleado/admin **quiero** recibir notificaciones cuando cambie el estado **para** dar seguimiento.

**Aceptación:**

* Se emiten notificaciones internas al cambiar de estado.

---

## 2) Módulo: Permisos

**Contexto:** empleado solicita, admin revisa y aprueba/rechaza; catálogo de tipos; validaciones y notificaciones.

### HU-2.1 Registrar solicitud de permiso

**Como** empleado **quiero** solicitar un permiso (tipo y fechas) **para** justificar ausencias/tiempos.

**Aceptación:**

* Puedo crear uno nuevo desde el módulo.
* Selecciono tipo de permiso e ingreso fechas.

### HU-2.2 Prevenir duplicados de permiso

**Como** sistema **quiero** evitar solicitudes duplicadas **para** mantener integridad del historial.

**Aceptación:**

* Se valida que el empleado no haya pedido 2 veces el mismo permiso.

### HU-2.3 Aprobación/Rechazo con trazabilidad

**Como** Admin RRHH **quiero** aprobar o rechazar permisos **para** aplicar políticas internas.

**Aceptación:**

* Admin revisa, evalúa justificación y decide.
* Se actualiza estado y el empleado consulta el resultado.

### HU-2.4 Validación de fechas por política

**Como** sistema **quiero** validar fechas según políticas internas **para** evitar permisos inválidos.

**Aceptación:**

* Si no cumple política, muestra error y solicita ajuste.

### HU-2.5 Notificación por cambio de estado

**Como** empleado **quiero** ser notificado si cambió el estado del permiso **para** actuar en consecuencia.

**Aceptación:**

* Se notifica internamente o por correo al cambiar estado.

---

## 3) Módulo: Asistencia

**Contexto:** marcas entrada/salida con hora del servidor; 2 marcas/día; restricciones y consulta por admin.

### HU-3.1 Registrar entrada/salida

**Como** empleado **quiero** registrar mi entrada o salida **para** que quede constancia digital.

**Aceptación:**

* El sistema guarda automáticamente fecha y hora del servidor.
* Solo se permiten 2 marcas por día (entrada/salida).

### HU-3.2 Bloquear marcas inválidas

**Como** sistema **quiero** bloquear acciones inválidas **para** evitar inconsistencias.

**Aceptación:**

* Si intenta marcar más de una vez en el día, el sistema no lo permite.
* Si intenta salida sin entrada previa, se bloquea el botón de salida.

### HU-3.3 Consulta de asistencia por Admin

**Como** Admin RRHH **quiero** consultar registros por empleado **para** control y reportes.

**Aceptación:**

* Admin puede consultar registros por empleado.
* Registros no se pueden editar ni eliminar.

---

## 4) Módulo: Horas Extra

**Contexto:** detección a partir de asistencia fuera de jornada; RRHH aprueba/rechaza; pasa a planilla; bitácora.

### HU-4.1 Listar horas extra detectadas

**Como** Admin RRHH **quiero** ver horas extra pendientes detectadas **para** revisarlas.

**Aceptación:**

* Se muestran solicitudes detectadas (empleado, fecha, tramo, total).
* Detección automática desde asistencia fuera de jornada.

### HU-4.2 Aprobar/Rechazar con observación

**Como** Admin RRHH **quiero** aprobar o rechazar horas extra con observación **para** dejar evidencia.

**Aceptación:**

* Admin aprueba/rechaza e ingresa observación.
* El sistema actualiza estado, registra en bitácora y recalcula totales del periodo.

### HU-4.3 Marcar horas aprobadas para Planilla

**Como** sistema **quiero** enviar horas aprobadas a planilla **para** incluir pago en el periodo.

**Aceptación:**

* Horas aprobadas quedan marcadas para planilla.
* Quedan trazables y disponibles para Planilla.

### HU-4.4 Consulta/filtrado de horas extra

**Como** Admin RRHH **quiero** filtrar por empleado y rango de fechas **para** auditar rápidamente.

**Aceptación:**

* Se filtra por empleado y rango; se observan horas del último mes.

---

## 5) Módulo: Planilla

**Contexto:** genera planilla quincenal con asistencia, horas extra aprobadas, deducciones/beneficios; exporta PDF; comprobantes.

### HU-5.1 Generar nueva planilla quincenal

**Como** Admin RRHH **quiero** generar planilla por quincena **para** calcular salarios automáticamente.

**Aceptación:**

* Selecciono quincena y el sistema valida empleados activos.
* El sistema obtiene asistencia, horas extra, deducciones y beneficios.
* Calcula salario bruto, deducciones y neto por empleado.

### HU-5.2 Confirmar y guardar planilla

**Como** Admin RRHH **quiero** revisar y confirmar la planilla **para** dejarla registrada oficialmente.

**Aceptación:**

* Admin revisa resumen y confirma generación final.
* Se guarda y queda disponible para consulta/exportación.

### HU-5.3 Exportar planilla a PDF

**Como** Admin RRHH **quiero** exportar planilla **para** compartirla con contabilidad.

**Aceptación:**

* El sistema permite exportar en formato PDF.

### HU-5.4 Generar comprobante de planilla por empleado

**Como** Admin RRHH **quiero** generar comprobantes **para** entregar detalle de pago al colaborador.

**Aceptación:**

* “Previsualizar” muestra detalle de cálculo y permite “Generar comprobante”.
* Se genera documento PDF con detalle del pago.

### HU-5.5 Evitar duplicados / manejar faltantes

**Como** sistema **quiero** prevenir planillas duplicadas y advertir faltantes **para** mantener consistencia.

**Aceptación:**

* Si periodo ya fue procesado, bloquea duplicado.
* Si hay empleados sin asistencia, avisa que no serán incluidos.

---

## 6) Módulo: Liquidaciones

**Contexto:** cálculo básico con datos mínimos; muestra total + desglose; guardar.

### HU-6.1 Calcular liquidación

**Como** Admin RRHH **quiero** calcular una liquidación ingresando datos mínimos **para** obtener monto a pagar.

**Aceptación:**

* Se ingresa info mínima (ingreso/salida, salario prom., vacaciones pendientes, deducciones).
* El sistema muestra total y desglose.

### HU-6.2 Guardar liquidación

**Como** Admin RRHH **quiero** guardar la liquidación calculada **para** consultarla después.

**Aceptación:**

* “Guardar liquidación” registra en base de datos.
* Queda disponible para consulta e impresión.

### HU-6.3 Validar datos obligatorios

**Como** sistema **quiero** bloquear el cálculo si faltan datos **para** evitar errores.

**Aceptación:**

* Si falta un dato obligatorio, muestra mensaje y bloquea cálculo.

---

## 7) Módulo: Aguinaldos

**Contexto:** parametrizable, consolida salarios por año, calcula, guarda, detalla por empleado, recalcular. content

content

content

### HU-7.1 Calcular aguinaldo anual por año

**Como** Admin RRHH **quiero** calcular aguinaldo por año **para** generar listado por empleado.

**Aceptación:**

* Selecciono año y el sistema consolida salarios del periodo parametrizado. content
* Aplica fórmula configurada y calcula monto por empleado. content
* Muestra resultados y guarda en BD. content

### HU-7.2 Guardar cálculo anual evitando duplicados

**Como** sistema **quiero** evitar guardar dos veces el mismo año **para** prevenir duplicados.

**Aceptación:**

* Si ya fue guardado, muestra alerta y evita duplicados. content

### HU-7.3 Ver detalle por empleado

**Como** Admin RRHH **quiero** ver el desglose por empleado **para** auditar el cálculo.

**Aceptación:**

* Al seleccionar empleado, muestra desglose de montos considerados. content

### HU-7.4 Recalcular aguinaldo

**Como** Admin RRHH **quiero** recalcular si hubo cambios/errores **para** corregir montos.

**Aceptación:**

* “Recalcular” vuelve a consolidar y actualiza montos. content

---

## 8) Módulo: Evaluación de desempeño

**Contexto:** RRHH asigna calificación 3–10, observación opcional, guarda con fecha/evaluador; consulta historial.

### HU-8.1 Registrar evaluación a un empleado

**Como** Admin RRHH **quiero** evaluar a un empleado **para** registrar su desempeño.

**Aceptación:**

* Sistema muestra empleados activos y permite seleccionar uno.
* Permite calificación 3–10 y observación opcional.
* Guarda evaluación con fecha, evaluador y nota.

### HU-8.2 Validar calificación obligatoria

**Como** sistema **quiero** evitar guardar sin calificación **para** asegurar consistencia.

**Aceptación:**

* Si intenta guardar sin calificación, muestra mensaje de error.

### HU-8.3 Consultar historial de evaluaciones

**Como** Admin RRHH **quiero** consultar evaluaciones registradas **para** comparar y dar seguimiento.

**Aceptación:**

* “Gestión de evaluaciones” muestra lista con calificaciones, fecha y observaciones.

---

## 9) Módulo: Incapacidades

**Contexto:** empleado registra incapacidad con motivo y boleta; RRHH aprueba/rechaza; notifica; PDF; queda registrada.

### HU-9.1 Registrar incapacidad con documento

**Como** empleado **quiero** registrar una incapacidad con motivo y boleta **para** justificar ausencia.

**Aceptación:**

* Se adjunta boleta/documento correspondiente (PDF según el proceso descrito).

### HU-9.2 Evaluar y aprobar/rechazar incapacidad

**Como** RRHH **quiero** evaluar la solicitud **para** aprobarla o rechazarla con justificación.

**Aceptación:**

* Si motivo es válido, RRHH evalúa y aprueba; si no, rechaza con justificación.

### HU-9.3 Registrar y notificar resultado

**Como** sistema **quiero** registrar la incapacidad y notificar al empleado **para** cerrar el trámite.

**Aceptación:**

* Si se aprueba, registra en BD y notifica al empleado.
* Si se rechaza, notifica el motivo.

---

## 10) Módulo: Reportes (transversal)

En los casos de uso se menciona “reportes” como módulo disponible por rol.

Como no aparece una tabla de caso de uso específica en los fragmentos recuperados, te dejo **historias mínimas** coherentes con lo ya definido:

### HU-10.1 Reporte de asistencia por empleado/rango

**Como** Admin RRHH **quiero** generar reportes de asistencia **para** control y soporte a planilla/evaluación.

**Aceptación (mínima):**

* Permite filtrar por empleado y fechas usando registros de asistencia (no editables).

### HU-10.2 Reporte de horas extra aprobadas por periodo

**Como** Admin RRHH **quiero** un reporte de horas extra aprobadas **para** auditar pagos en planilla.

**Aceptación (mínima):**

* Incluye estado final y trazabilidad; alineado al periodo de planilla.

### HU-10.3 Reporte consolidado de planilla

**Como** Admin RRHH/Contabilidad **quiero** exportar planillas y comprobantes **para** respaldo y distribución.

**Aceptación (mínima):**

* Exportación en PDF disponible.
