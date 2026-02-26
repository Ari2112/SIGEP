// ============ MOCK DATA - REMOVER EN PRODUCCIÓN ============
// Este archivo contiene datos de ejemplo para probar la interfaz sin backend

export const USE_MOCK = true; // Cambiar a false cuando el backend esté listo

// Empleados
export const mockEmployees = [
  { id: 1, firstName: 'Admin', lastName: 'Sistema', email: 'admin@alquileres-segura.com', department: 'TI', position: 'Administrador', hireDate: '2020-01-15', status: 'Activo' },
  { id: 2, firstName: 'María', lastName: 'González', email: 'maria.gonzalez@alquileres-segura.com', department: 'Recursos Humanos', position: 'Jefe de RRHH', hireDate: '2021-03-01', status: 'Activo' },
  { id: 3, firstName: 'Carlos', lastName: 'Ramírez', email: 'carlos.ramirez@alquileres-segura.com', department: 'Operaciones', position: 'Supervisor', hireDate: '2022-06-15', status: 'Activo' },
  { id: 4, firstName: 'Juan', lastName: 'Pérez', email: 'juan.perez@alquileres-segura.com', department: 'Operaciones', position: 'Técnico', hireDate: '2023-01-10', status: 'Activo' },
  { id: 5, firstName: 'Ana', lastName: 'Martínez', email: 'ana.martinez@alquileres-segura.com', department: 'Ventas', position: 'Ejecutiva', hireDate: '2023-05-20', status: 'Activo' },
  { id: 6, firstName: 'Luis', lastName: 'Hernández', email: 'luis.hernandez@alquileres-segura.com', department: 'Operaciones', position: 'Técnico', hireDate: '2024-02-01', status: 'Activo' },
];

// Saldo de vacaciones por empleado
export const mockVacationBalances = {
  1: { employeeId: 1, year: 2026, totalDays: 15, usedDays: 3, pendingDays: 2, availableDays: 10, carriedOverDays: 0 },
  2: { employeeId: 2, year: 2026, totalDays: 18, usedDays: 5, pendingDays: 0, availableDays: 13, carriedOverDays: 3 },
  3: { employeeId: 3, year: 2026, totalDays: 15, usedDays: 2, pendingDays: 5, availableDays: 8, carriedOverDays: 0 },
  4: { employeeId: 4, year: 2026, totalDays: 12, usedDays: 0, pendingDays: 3, availableDays: 9, carriedOverDays: 0 },
};

// Solicitudes de vacaciones
export const mockVacationRequests = [
  { id: 1, employeeId: 4, employeeName: 'Juan Pérez', startDate: '2026-03-01', endDate: '2026-03-05', requestedDays: 5, reason: 'Vacaciones familiares', status: 'Pendiente', createdAt: '2026-02-01' },
  { id: 2, employeeId: 4, employeeName: 'Juan Pérez', startDate: '2026-01-15', endDate: '2026-01-17', requestedDays: 3, reason: 'Asuntos personales', status: 'Aprobado', createdAt: '2026-01-05', approvedAt: '2026-01-06', approverComments: 'Aprobado' },
  { id: 3, employeeId: 5, employeeName: 'Ana Martínez', startDate: '2026-02-20', endDate: '2026-02-25', requestedDays: 6, reason: 'Viaje al exterior', status: 'Pendiente', createdAt: '2026-02-03' },
  { id: 4, employeeId: 6, employeeName: 'Luis Hernández', startDate: '2026-04-10', endDate: '2026-04-12', requestedDays: 3, reason: 'Mudanza', status: 'Pendiente', createdAt: '2026-02-04' },
];

// Tipos de permiso
export const mockPermissionTypes = [
  { id: 1, name: 'Cita Médica', maxDaysPerYear: 12, requiresDocument: true, description: 'Permiso para citas médicas' },
  { id: 2, name: 'Asunto Personal', maxDaysPerYear: 6, requiresDocument: false, description: 'Permisos por asuntos personales' },
  { id: 3, name: 'Duelo', maxDaysPerYear: 5, requiresDocument: true, description: 'Permiso por fallecimiento de familiar' },
  { id: 4, name: 'Matrimonio', maxDaysPerYear: 3, requiresDocument: true, description: 'Permiso por matrimonio' },
  { id: 5, name: 'Nacimiento de hijo', maxDaysPerYear: 3, requiresDocument: true, description: 'Permiso por nacimiento' },
  { id: 6, name: 'Emergencia', maxDaysPerYear: null, requiresDocument: false, description: 'Emergencias justificadas' },
];

// Solicitudes de permisos
export const mockPermissionRequests = [
  { id: 1, employeeId: 4, employeeName: 'Juan Pérez', permissionTypeId: 1, permissionTypeName: 'Cita Médica', date: '2026-02-10', isPartialDay: true, startTime: '08:00', endTime: '12:00', reason: 'Cita con especialista', status: 'Pendiente', createdAt: '2026-02-03' },
  { id: 2, employeeId: 4, employeeName: 'Juan Pérez', permissionTypeId: 2, permissionTypeName: 'Asunto Personal', date: '2026-01-20', isPartialDay: false, startTime: null, endTime: null, reason: 'Trámite bancario', status: 'Aprobado', createdAt: '2026-01-15', approvedAt: '2026-01-16' },
  { id: 3, employeeId: 5, employeeName: 'Ana Martínez', permissionTypeId: 1, permissionTypeName: 'Cita Médica', date: '2026-02-12', isPartialDay: true, startTime: '14:00', endTime: '17:00', reason: 'Control médico', status: 'Pendiente', createdAt: '2026-02-04' },
  { id: 4, employeeId: 6, employeeName: 'Luis Hernández', permissionTypeId: 3, permissionTypeName: 'Duelo', date: '2026-02-08', isPartialDay: false, startTime: null, endTime: null, reason: 'Fallecimiento de familiar', status: 'Aprobado', createdAt: '2026-02-05', approvedAt: '2026-02-05' },
];

// Resumen de uso de permisos por empleado
export const mockPermissionUsage = {
  4: [
    { permissionTypeId: 1, typeName: 'Cita Médica', usedDays: 2, maxDaysPerYear: 12 },
    { permissionTypeId: 2, typeName: 'Asunto Personal', usedDays: 1, maxDaysPerYear: 6 },
  ],
};

// Notificaciones
export const mockNotifications = [
  { id: 1, userId: 4, title: 'Solicitud Aprobada', message: 'Tu solicitud de vacaciones del 15 al 17 de enero ha sido aprobada', type: 'success', isRead: false, createdAt: '2026-01-06T10:30:00' },
  { id: 2, userId: 4, title: 'Recordatorio', message: 'Tienes 9 días de vacaciones disponibles para este año', type: 'info', isRead: true, createdAt: '2026-02-01T08:00:00' },
  { id: 3, userId: 3, title: 'Nueva Solicitud', message: 'Juan Pérez ha solicitado vacaciones del 1 al 5 de marzo', type: 'warning', isRead: false, createdAt: '2026-02-01T14:00:00' },
  { id: 4, userId: 2, title: 'Solicitud Pendiente', message: 'Hay 3 solicitudes de vacaciones pendientes de aprobación', type: 'warning', isRead: false, createdAt: '2026-02-04T09:00:00' },
];

// Dashboard stats
export const mockDashboardStats = {
  totalEmployees: 6,
  activeEmployees: 6,
  pendingVacationRequests: 3,
  pendingPermissionRequests: 2,
  upcomingBirthdays: 2,
  onVacationToday: 0,
};

// Función helper para filtrar por empleado actual
export const getMyVacationRequests = (employeeId) => {
  return mockVacationRequests.filter(r => r.employeeId === employeeId);
};

export const getPendingVacationRequests = () => {
  return mockVacationRequests.filter(r => r.status === 'Pendiente');
};

export const getMyPermissionRequests = (employeeId) => {
  return mockPermissionRequests.filter(r => r.employeeId === employeeId);
};

export const getPendingPermissionRequests = () => {
  return mockPermissionRequests.filter(r => r.status === 'Pendiente');
};

export const getNotificationsForUser = (userId) => {
  return mockNotifications.filter(n => n.userId === userId);
};

export const getVacationBalance = (employeeId) => {
  return mockVacationBalances[employeeId] || { 
    employeeId, year: 2026, totalDays: 12, usedDays: 0, pendingDays: 0, availableDays: 12, carriedOverDays: 0 
  };
};

export const getPermissionUsage = (employeeId) => {
  return mockPermissionUsage[employeeId] || [];
};

// ============ MOCK DATA ADICIONAL ============

// Registros de asistencia
export const mockAttendanceRecords = [
  { id: 1, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-25', checkInTime: '2026-02-25T08:02:00', checkOutTime: null, workedHours: null, status: 'Parcial' },
  { id: 2, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-24', checkInTime: '2026-02-24T07:58:00', checkOutTime: '2026-02-24T17:05:00', workedHours: 9.12, status: 'Completo' },
  { id: 3, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-21', checkInTime: '2026-02-21T08:15:00', checkOutTime: '2026-02-21T17:30:00', workedHours: 9.25, status: 'Completo' },
  { id: 4, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-20', checkInTime: '2026-02-20T08:00:00', checkOutTime: '2026-02-20T12:00:00', workedHours: 4.0, status: 'Parcial' },
  { id: 5, employeeId: 5, employeeName: 'Ana Martínez', date: '2026-02-25', checkInTime: '2026-02-25T08:10:00', checkOutTime: null, workedHours: null, status: 'Parcial' },
  { id: 6, employeeId: 6, employeeName: 'Luis Hernández', date: '2026-02-25', checkInTime: '2026-02-25T07:45:00', checkOutTime: null, workedHours: null, status: 'Parcial' },
  { id: 7, employeeId: 1, employeeName: 'Admin Sistema', date: '2026-02-25', checkInTime: '2026-02-25T07:30:00', checkOutTime: null, workedHours: null, status: 'Parcial' },
];

// Registros de horas extra
export const mockOvertimeRecords = [
  { id: 1, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-24', hours: 1.5, reason: 'Cierre de proyecto urgente', status: 'Detectada', detectionType: 'Automática', amount: 12500 },
  { id: 2, employeeId: 5, employeeName: 'Ana Martínez', date: '2026-02-23', hours: 2.0, reason: 'Atención a cliente', status: 'Detectada', detectionType: 'Automática', amount: 15000 },
  { id: 3, employeeId: 6, employeeName: 'Luis Hernández', date: '2026-02-22', hours: 1.0, reason: 'Mantenimiento emergente', status: 'Aprobada', detectionType: 'Automática', amount: 8500, approvedAt: '2026-02-23', reviewerName: 'María González' },
  { id: 4, employeeId: 4, employeeName: 'Juan Pérez', date: '2026-02-20', hours: 0.75, reason: 'Entrega de informe', status: 'Rechazada', detectionType: 'Automática', amount: 6250, approvedAt: '2026-02-21', reviewerName: 'María González', reviewerComments: 'No autorizada previamente' },
];

// Empleados detallados para gestión
export const mockEmployeesDetailed = [
  { id: 1, firstName: 'Admin', lastName: 'Sistema', fullName: 'Admin Sistema', identificationNumber: '101010101', email: 'admin@alquileres-segura.com', phone: '8888-1111', positionName: 'Administrador de Sistemas', baseSalary: 1200000, hireDate: '2020-01-15', status: 'Activo' },
  { id: 2, firstName: 'María', lastName: 'González', fullName: 'María González', identificationNumber: '202020202', email: 'maria.gonzalez@alquileres-segura.com', phone: '8888-2222', positionName: 'Jefa de Recursos Humanos', baseSalary: 1500000, hireDate: '2021-03-01', status: 'Activo' },
  { id: 3, firstName: 'Carlos', lastName: 'Ramírez', fullName: 'Carlos Ramírez', identificationNumber: '303030303', email: 'carlos.ramirez@alquileres-segura.com', phone: '8888-3333', positionName: 'Supervisor de Operaciones', baseSalary: 1000000, hireDate: '2022-06-15', status: 'Activo' },
  { id: 4, firstName: 'Juan', lastName: 'Pérez', fullName: 'Juan Pérez', identificationNumber: '404040404', email: 'juan.perez@alquileres-segura.com', phone: '8888-4444', positionName: 'Técnico de Mantenimiento', baseSalary: 650000, hireDate: '2023-01-10', status: 'Activo' },
  { id: 5, firstName: 'Ana', lastName: 'Martínez', fullName: 'Ana Martínez', identificationNumber: '505050505', email: 'ana.martinez@alquileres-segura.com', phone: '8888-5555', positionName: 'Ejecutiva de Ventas', baseSalary: 700000, hireDate: '2023-05-20', status: 'Activo' },
  { id: 6, firstName: 'Luis', lastName: 'Hernández', fullName: 'Luis Hernández', identificationNumber: '606060606', email: 'luis.hernandez@alquileres-segura.com', phone: '8888-6666', positionName: 'Técnico de Mantenimiento', baseSalary: 600000, hireDate: '2024-02-01', status: 'Activo' },
];

// Helpers para asistencia
export const getTodayAttendance = (employeeId) => {
  const today = new Date().toISOString().split('T')[0];
  return mockAttendanceRecords.find(r => r.employeeId === employeeId && r.date === today) || null;
};

export const getMyAttendanceRecords = (employeeId) => {
  return mockAttendanceRecords.filter(r => r.employeeId === employeeId).sort((a, b) => new Date(b.date) - new Date(a.date));
};

export const getAllAttendanceRecords = () => {
  return mockAttendanceRecords.sort((a, b) => new Date(b.date) - new Date(a.date));
};

// Helpers para horas extra
export const getMyOvertimeRecords = (employeeId) => {
  return mockOvertimeRecords.filter(r => r.employeeId === employeeId);
};

export const getPendingOvertimeRecords = () => {
  return mockOvertimeRecords.filter(r => r.status === 'Detectada');
};

export const getAllOvertimeRecords = () => {
  return mockOvertimeRecords;
};
