/**
 * SIGEP - Script de Testing de API
 * Prueba todos los endpoints del backend con verificaciones de lógica de negocio.
 * Uso: node test-api.js
 * Requiere: backend corriendo en http://localhost:5017
 */

const BASE = 'http://localhost:5017/api/v1';

// ============================================================
// Helpers
// ============================================================
let passed = 0, failed = 0, warned = 0;
const tokens = {};  // { admin, rrhh, supervisor, empleado }
const ids = {};     // IDs creados durante las pruebas

function color(code, text) { return `\x1b[${code}m${text}\x1b[0m`; }
const green = t => color(32, t);
const red   = t => color(31, t);
const yellow= t => color(33, t);
const cyan  = t => color(36, t);
const bold  = t => color(1, t);

function log(symbol, label, msg) {
  console.log(`  ${symbol} ${label.padEnd(55)} ${msg}`);
}

async function req(method, path, body, token) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  try {
    const res = await fetch(`${BASE}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
    let data = null;
    try { data = await res.json(); } catch {}
    return { status: res.status, data };
  } catch (e) {
    return { status: 0, data: null, error: e.message };
  }
}

function expect(label, condition, detail = '') {
  if (condition) {
    passed++;
    log(green('✓'), label, detail ? color(90, detail) : '');
  } else {
    failed++;
    log(red('✗'), label, red(detail || 'FALLÓ'));
  }
}

function warn(label, detail) {
  warned++;
  log(yellow('⚠'), label, yellow(detail));
}

function section(title) {
  console.log('\n' + bold(cyan(`▶ ${title}`)));
}

// ============================================================
// Tests
// ============================================================

async function testAuth() {
  section('AUTH');

  // Login válido - admin
  let r = await req('POST', '/auth/login', { username: 'admin', password: 'admin123' });
  expect('Login admin (credenciales correctas) → 200', r.status === 200, `status=${r.status}`);
  expect('Login admin → token presente', !!r.data?.token, `token=${r.data?.token?.slice(0,20)}...`);
  if (r.data?.token) tokens.admin = r.data.token;

  // Login válido - rrhh
  r = await req('POST', '/auth/login', { username: 'rrhh', password: 'admin123' });
  expect('Login rrhh (credenciales correctas) → 200', r.status === 200);
  if (r.data?.token) tokens.rrhh = r.data.token;

  // Login válido - supervisor
  r = await req('POST', '/auth/login', { username: 'supervisor', password: 'admin123' });
  expect('Login supervisor (credenciales correctas) → 200', r.status === 200);
  if (r.data?.token) tokens.supervisor = r.data.token;

  // Login válido - empleado
  r = await req('POST', '/auth/login', { username: 'juan.perez', password: 'admin123' });
  expect('Login juan.perez (credenciales correctas) → 200', r.status === 200);
  if (r.data?.token) tokens.empleado = r.data.token;

  // Login inválido
  r = await req('POST', '/auth/login', { username: 'admin', password: 'wrong' });
  expect('Login contraseña incorrecta → 401', r.status === 401, `status=${r.status}`);

  r = await req('POST', '/auth/login', { username: 'noexiste', password: 'admin123' });
  expect('Login usuario inexistente → 401', r.status === 401, `status=${r.status}`);

  // GET /me
  r = await req('GET', '/auth/me', null, tokens.admin);
  expect('GET /me con token válido → 200', r.status === 200, `user=${r.data?.username}`);

  r = await req('GET', '/auth/me', null, null);
  expect('GET /me sin token → 401', r.status === 401, `status=${r.status}`);
}

async function testEmployees() {
  section('EMPLEADOS');

  // Listar empleados
  let r = await req('GET', '/employees', null, tokens.admin);
  expect('GET /employees → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Empleado sin auth
  r = await req('GET', '/employees', null, null);
  expect('GET /employees sin auth → 401', r.status === 401);

  // Crear empleado (como RRHH) — ID único por ejecución para evitar conflictos
  const uniqueId = 'T' + Date.now().toString().slice(-8);
  const nuevoEmp = {
    firstName: 'Test',
    lastName: 'Usuario',
    identificationNumber: uniqueId,
    email: `test.${uniqueId}@sigep.com`,
    hireDate: '2025-01-01',
    baseSalary: 600000,
    positionId: 1,
    scheduleId: 1,
  };
  r = await req('POST', '/employees', nuevoEmp, tokens.rrhh);
  expect('POST /employees (como RRHH) → 200/201', r.status === 200 || r.status === 201, `status=${r.status}`);
  if (r.data?.id) {
    ids.newEmployee = r.data.id;
    log(color(90, ' '), 'ID empleado creado', `#${ids.newEmployee}`);
  }

  // Empleado intenta crear → 403
  r = await req('POST', '/employees', nuevoEmp, tokens.empleado);
  expect('POST /employees (empleado sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // GET por ID
  r = await req('GET', '/employees/1', null, tokens.admin);
  expect('GET /employees/1 → 200', r.status === 200, `name=${r.data?.firstName}`);

  // Actualizar empleado
  if (ids.newEmployee) {
    r = await req('PUT', `/employees/${ids.newEmployee}`, { ...nuevoEmp, baseSalary: 650000 }, tokens.rrhh);
    expect('PUT /employees/{id} (actualizar salario) → 200', r.status === 200, `status=${r.status}`);
  }

  // Posiciones y horarios
  r = await req('GET', '/employees/positions', null, tokens.admin);
  expect('GET /employees/positions → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/employees/schedules', null, tokens.admin);
  expect('GET /employees/schedules → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);
}

async function testVacations() {
  section('VACACIONES');

  // Balance del empleado
  let r = await req('GET', '/vacations/balance', null, tokens.empleado);
  expect('GET /vacations/balance (empleado) → 200', r.status === 200,
    r.status === 200 ? `available=${r.data?.availableDays}` : `status=${r.status} msg=${r.data?.message}`);

  // Balance por ID (admin)
  r = await req('GET', '/vacations/balance/3', null, tokens.admin);
  expect('GET /vacations/balance/3 (admin) → 200', r.status === 200,
    r.status === 200 ? `available=${r.data?.availableDays}` : `status=${r.status} msg=${r.data?.message}`);

  // Balance por ID (empleado sin permiso)
  r = await req('GET', '/vacations/balance/1', null, tokens.empleado);
  expect('GET /vacations/balance/1 (empleado → otro) → 403', r.status === 403, `status=${r.status}`);

  // Mis solicitudes
  r = await req('GET', '/vacations/requests/my', null, tokens.empleado);
  expect('GET /vacations/requests/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Crear solicitud de vacaciones
  const hoy = new Date();
  const startDate = new Date(hoy); startDate.setDate(hoy.getDate() + 30);
  const endDate = new Date(startDate); endDate.setDate(startDate.getDate() + 4);
  const vacReq = {
    startDate: startDate.toISOString().split('T')[0],
    endDate: endDate.toISOString().split('T')[0],
    reason: 'Vacaciones de prueba - testing',
  };
  r = await req('POST', '/vacations/requests', vacReq, tokens.empleado);
  if (r.status === 200 || r.status === 201) {
    expect('POST /vacations/requests (crear solicitud) → 200/201', true, `status=${r.status}`);
    if (r.data?.id) ids.vacRequest = r.data.id;
  } else if (r.status === 409) {
    warn('POST /vacations/requests (crear solicitud)', `Conflicto: ${r.data?.message}`);
    const myVacs = await req('GET', '/vacations/requests/my', null, tokens.empleado);
    if (Array.isArray(myVacs.data)) {
      const pending = myVacs.data.find(v => v.status === 'Pendiente');
      if (pending) ids.vacRequest = pending.id;
    }
  } else {
    expect('POST /vacations/requests (crear solicitud) → 200/201', false, `status=${r.status} msg=${r.data?.message}`);
  }

  // Solicitudes pendientes (admin)
  r = await req('GET', '/vacations/requests/pending', null, tokens.admin);
  expect('GET /vacations/requests/pending (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Aprobar solicitud
  if (ids.vacRequest) {
    r = await req('POST', `/vacations/requests/${ids.vacRequest}/approve`, { comments: 'Aprobado en test' }, tokens.admin);
    expect(`POST /vacations/requests/${ids.vacRequest}/approve → 200`, r.status === 200, `status=${r.status}`);
  }
}

async function testPermissions() {
  section('PERMISOS');

  let r = await req('GET', '/permissions/types', null, tokens.admin);
  expect('GET /permissions/types → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/permissions/requests/my', null, tokens.empleado);
  expect('GET /permissions/requests/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Crear solicitud de permiso
  // Tipo 9 (Otro): sin límite anual, requiere documento → se provee documentUrl
  const hoy2 = new Date();
  const permStart = new Date(hoy2); permStart.setDate(hoy2.getDate() + 15);
  const permEnd = new Date(hoy2); permEnd.setDate(hoy2.getDate() + 15);
  const permReq = {
    permissionTypeId: 9,
    startDate: permStart.toISOString().split('T')[0],
    endDate: permEnd.toISOString().split('T')[0],
    reason: 'Permiso de prueba - testing',
    startTime: '08:00:00',
    endTime: '12:00:00',
    isPartialDay: true,
    documentUrl: 'https://test.com/permiso-doc.pdf',
  };
  r = await req('POST', '/permissions/requests', permReq, tokens.empleado);
  if (r.status === 200 || r.status === 201) {
    expect('POST /permissions/requests (crear) → 200/201', true, `status=${r.status}`);
    if (r.data?.id) ids.permRequest = r.data.id;
  } else if (r.status === 409) {
    warn('POST /permissions/requests (crear)', `Conflicto (ya existe): ${r.data?.message}`);
    // Intentar usar una solicitud pendiente existente para el test de aprobación
    const myPerms = await req('GET', '/permissions/requests/my', null, tokens.empleado);
    if (Array.isArray(myPerms.data)) {
      const pending = myPerms.data.find(p => p.status === 'Pendiente');
      if (pending) ids.permRequest = pending.id;
    }
  } else {
    expect('POST /permissions/requests (crear) → 200/201', false, `status=${r.status} msg=${r.data?.message}`);
  }

  // Pendientes
  r = await req('GET', '/permissions/requests/pending', null, tokens.supervisor);
  expect('GET /permissions/requests/pending (supervisor) → 200', r.status === 200);

  // Aprobar
  if (ids.permRequest) {
    r = await req('POST', `/permissions/requests/${ids.permRequest}/approve`, { comments: 'Aprobado en test' }, tokens.supervisor);
    if (r.status === 200) {
      expect(`POST /permissions/requests/${ids.permRequest}/approve → 200`, true, `status=${r.status}`);
    } else if (r.status === 409) {
      warn(`POST /permissions/requests/${ids.permRequest}/approve`, 'Permiso ya procesado');
    } else {
      expect(`POST /permissions/requests/${ids.permRequest}/approve → 200`, false, `status=${r.status}`);
    }
  }
}

async function testAttendance() {
  section('ASISTENCIA');

  let r = await req('GET', '/attendance/today', null, tokens.empleado);
  expect('GET /attendance/today → 200 ó 204', r.status === 200 || r.status === 204, `status=${r.status}`);

  r = await req('GET', '/attendance/my', null, tokens.empleado);
  expect('GET /attendance/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/attendance', null, tokens.admin);
  expect('GET /attendance (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/attendance', null, tokens.empleado);
  expect('GET /attendance (empleado sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // Check-in (puede fallar si ya hay uno hoy)
  r = await req('POST', '/attendance/check-in', {}, tokens.empleado);
  if (r.status === 200 || r.status === 201) {
    expect('POST /attendance/check-in → 200/201', true, 'check-in registrado');
    ids.attendanceCheckedIn = true;
  } else if (r.status === 409 || (r.status === 400 && r.data?.message?.toLowerCase().includes('ya'))) {
    warn('POST /attendance/check-in', `Ya existe check-in hoy: ${r.data?.message}`);
    ids.attendanceCheckedIn = false;
  } else {
    expect('POST /attendance/check-in → 200/201', false, `status=${r.status} msg=${r.data?.message}`);
  }

  // Check-out
  if (ids.attendanceCheckedIn) {
    r = await req('POST', '/attendance/check-out', {}, tokens.empleado);
    expect('POST /attendance/check-out → 200/201', r.status === 200 || r.status === 201, `status=${r.status}`);
  }
}

async function testOvertime() {
  section('HORAS EXTRA');

  let r = await req('GET', '/overtime', null, tokens.admin);
  expect('GET /overtime (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/overtime/my', null, tokens.empleado);
  expect('GET /overtime/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/overtime', null, tokens.empleado);
  expect('GET /overtime (empleado → sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // Revisar primera hora extra si existe
  r = await req('GET', '/overtime', null, tokens.admin);
  if (Array.isArray(r.data) && r.data.length > 0) {
    const first = r.data.find(x => x.status === 1 || x.status === 2 || x.statusName === 'Detectada' || x.statusName === 'Pendiente');
    if (first) {
      const rev = await req('POST', `/overtime/${first.id}/review`, { approve: true, comments: 'Aprobado en test' }, tokens.admin);
      expect(`POST /overtime/${first.id}/review (aprobar) → 200`, rev.status === 200, `status=${rev.status}`);
    } else {
      warn('POST /overtime/review', 'No hay registros pendientes de revisión');
    }
  }
}

async function testPayroll() {
  section('PLANILLA');

  let r = await req('GET', '/payroll', null, tokens.admin);
  expect('GET /payroll (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/payroll', null, tokens.empleado);
  expect('GET /payroll (empleado → sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // Generar planilla
  const now = new Date();
  const payrollReq = {
    periodYear: now.getFullYear(),
    periodMonth: now.getMonth() + 1,
    periodType: 1, // 1=Quincenal primera, 2=Quincenal segunda, 3=Mensual
  };
  r = await req('POST', '/payroll/generate', payrollReq, tokens.admin);
  if (r.status === 200 || r.status === 201) {
    expect('POST /payroll/generate → 200/201', true, `id=${r.data?.id}`);
    ids.payroll = r.data?.id;
  } else if (r.status === 409 || (r.status === 400 && r.data?.message?.includes('ya existe'))) {
    warn('POST /payroll/generate', `Planilla ya existe: ${r.data?.message}`);
    // Buscar la existente
    const list = await req('GET', '/payroll', null, tokens.admin);
    if (Array.isArray(list.data)) {
      const existing = list.data.find(p => p.periodYear === payrollReq.periodYear && p.periodMonth === payrollReq.periodMonth);
      if (existing) ids.payroll = existing.id;
    }
  } else {
    expect('POST /payroll/generate → 200/201', false, `status=${r.status} msg=${r.data?.message}`);
  }

  // Detalle planilla
  if (ids.payroll) {
    r = await req('GET', `/payroll/${ids.payroll}`, null, tokens.admin);
    expect(`GET /payroll/${ids.payroll} → 200`, r.status === 200, `employees=${r.data?.details?.length ?? '?'}`);

    // Aprobar planilla
    r = await req('POST', `/payroll/${ids.payroll}/approve`, {}, tokens.admin);
    if (r.status === 200) {
      expect(`POST /payroll/${ids.payroll}/approve → 200`, true, `status=${r.status}`);
    } else if (r.status === 409) {
      warn(`POST /payroll/${ids.payroll}/approve`, 'Planilla ya aprobada');
    } else {
      expect(`POST /payroll/${ids.payroll}/approve → 200`, false, `status=${r.status}`);
    }
  }

  r = await req('GET', '/payroll/deduction-types', null, tokens.admin);
  expect('GET /payroll/deduction-types → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/payroll/benefit-types', null, tokens.admin);
  expect('GET /payroll/benefit-types → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);
}

async function testSettlement() {
  section('LIQUIDACIONES');

  let r = await req('GET', '/settlement', null, tokens.admin);
  expect('GET /settlement (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/settlement', null, tokens.empleado);
  expect('GET /settlement (empleado → sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // Calcular liquidación para el empleado creado en test (si existe)
  if (ids.newEmployee) {
    const settleReq = {
      employeeId: ids.newEmployee,
      terminationDate: new Date().toISOString().split('T')[0],
      terminationType: 1, // 1=Renuncia
      reason: 'Test liquidación',
    };
    r = await req('POST', '/settlement/calculate', settleReq, tokens.admin);
    expect('POST /settlement/calculate → 200/201', r.status === 200 || r.status === 201, `status=${r.status} msg=${r.data?.message}`);
    if (r.data?.id) ids.settlement = r.data.id;
  } else {
    warn('POST /settlement/calculate', 'Omitido: no se creó empleado de test');
  }
}

async function testAnnualBonus() {
  section('AGUINALDO');

  let r = await req('GET', '/annualbonus', null, tokens.admin);
  expect('GET /annualbonus (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/annualbonus', null, tokens.empleado);
  expect('GET /annualbonus (empleado → sin permiso) → 403', r.status === 403, `status=${r.status}`);

  // Calcular aguinaldo
  const year = new Date().getFullYear();
  r = await req('POST', '/annualbonus/calculate', { year }, tokens.admin);
  if (r.status === 200 || r.status === 201) {
    expect('POST /annualbonus/calculate → 200/201', true, `id=${r.data?.id}`);
    ids.annualBonus = r.data?.id;
  } else if (r.status === 409 || r.status === 400) {
    warn('POST /annualbonus/calculate', `${r.data?.message}`);
    r = await req('GET', `/annualbonus/year/${year}`, null, tokens.admin);
    if (r.status === 200 && r.data?.id) ids.annualBonus = r.data.id;
  } else {
    expect('POST /annualbonus/calculate → 200/201', false, `status=${r.status}`);
  }

  if (ids.annualBonus) {
    r = await req('GET', `/annualbonus/${ids.annualBonus}`, null, tokens.admin);
    expect(`GET /annualbonus/${ids.annualBonus} → 200`, r.status === 200, `employees=${r.data?.details?.length ?? '?'}`);
  }
}

async function testPerformance() {
  section('EVALUACIÓN DE DESEMPEÑO');

  let r = await req('GET', '/performanceevaluation', null, tokens.supervisor);
  expect('GET /performanceevaluation (supervisor) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/performanceevaluation/my', null, tokens.empleado);
  expect('GET /performanceevaluation/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Crear evaluación
  const evalYear = new Date().getFullYear();
  const evalReq = {
    employeeId: 3,
    evaluationDate: new Date().toISOString().split('T')[0],
    periodStartDate: `${evalYear}-01-01`,
    periodEndDate: `${evalYear}-03-31`,
    score: 8,
    comments: 'Buen desempeño - evaluación de test',
    strengths: 'Puntualidad, trabajo en equipo',
    areasToImprove: 'Comunicación',
  };
  r = await req('POST', '/performanceevaluation', evalReq, tokens.supervisor);
  expect('POST /performanceevaluation (crear) → 200/201', r.status === 200 || r.status === 201, `status=${r.status}`);
  if (r.data?.id) ids.perfEval = r.data.id;

  // Empleado reconoce evaluación
  if (ids.perfEval) {
    r = await req('POST', `/performanceevaluation/${ids.perfEval}/acknowledge`, {}, tokens.empleado);
    expect(`POST /performanceevaluation/${ids.perfEval}/acknowledge → 200`, r.status === 200, `status=${r.status}`);
  }
}

async function testDisability() {
  section('INCAPACIDADES');

  let r = await req('GET', '/disability', null, tokens.admin);
  expect('GET /disability (admin) → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/disability/my', null, tokens.empleado);
  expect('GET /disability/my → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  // Crear incapacidad
  const now = new Date();
  const disReq = {
    type: 1,
    startDate: new Date(now.setDate(now.getDate() - 5)).toISOString().split('T')[0],
    endDate: new Date(now.setDate(now.getDate() + 2)).toISOString().split('T')[0],
    daysCount: 7,
    diagnosis: 'Gripe - test',
    institutionName: 'CCSS',
    voucherNumber: 'TEST-001',
  };
  r = await req('POST', '/disability', disReq, tokens.empleado);
  expect('POST /disability (crear) → 200/201', r.status === 200 || r.status === 201, `status=${r.status}`);
  if (r.data?.id) ids.disability = r.data.id;

  // Revisar incapacidad
  if (ids.disability) {
    r = await req('POST', `/disability/${ids.disability}/review`, { approve: true, comments: 'Aprobado en test' }, tokens.admin);
    expect(`POST /disability/${ids.disability}/review → 200`, r.status === 200, `status=${r.status}`);
  }
}

async function testReports() {
  section('REPORTES');

  let r = await req('GET', '/report/dashboard', null, tokens.admin);
  expect('GET /report/dashboard → 200', r.status === 200, `keys=${r.data ? Object.keys(r.data).join(',') : '?'}`);

  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
  const to = now.toISOString().split('T')[0];

  r = await req('GET', `/report/attendance?dateFrom=${from}&dateTo=${to}`, null, tokens.admin);
  expect('GET /report/attendance → 200', r.status === 200, `status=${r.status}`);

  r = await req('GET', `/report/overtime?dateFrom=${from}&dateTo=${to}`, null, tokens.admin);
  expect('GET /report/overtime → 200', r.status === 200, `status=${r.status}`);

  if (ids.payroll) {
    r = await req('GET', `/report/payroll/${ids.payroll}`, null, tokens.admin);
    expect(`GET /report/payroll/${ids.payroll} → 200`, r.status === 200, `status=${r.status}`);
  }
}

async function testNotifications() {
  section('NOTIFICACIONES');

  let r = await req('GET', '/notifications', null, tokens.admin);
  expect('GET /notifications → 200', r.status === 200, `count=${Array.isArray(r.data) ? r.data.length : '?'}`);

  r = await req('GET', '/notifications/unread-count', null, tokens.admin);
  expect('GET /notifications/unread-count → 200', r.status === 200, `count=${r.data?.count ?? r.data}`);

  if (Array.isArray(r.data) && r.data.length > 0) {
    const first = r.data[0];
    const mark = await req('POST', `/notifications/${first.id}/read`, {}, tokens.admin);
    expect(`POST /notifications/${first.id}/read → 200`, mark.status === 200);
  }

  r = await req('POST', '/notifications/read-all', {}, tokens.admin);
  expect('POST /notifications/read-all → 200 ó 204', r.status === 200 || r.status === 204, `status=${r.status}`);
}

async function testCleanup() {
  section('LIMPIEZA');

  if (ids.newEmployee) {
    const r = await req('DELETE', `/employees/${ids.newEmployee}`, null, tokens.admin);
    expect(`DELETE /employees/${ids.newEmployee} (limpiar test) → 200/204`, r.status === 200 || r.status === 204, `status=${r.status}`);
  } else {
    warn('DELETE /employees', 'No hay empleado de test para limpiar');
  }
}

// ============================================================
// Main
// ============================================================
async function main() {
  console.log(bold('\n╔══════════════════════════════════════════════════╗'));
  console.log(bold('║        SIGEP - API Test Suite                   ║'));
  console.log(bold('╚══════════════════════════════════════════════════╝'));
  console.log(color(90, `  Backend: ${BASE}`));
  console.log(color(90, `  Fecha: ${new Date().toLocaleString()}\n`));

  // Verificar que el backend esté activo
  const health = await req('GET', '/../health', null, null);
  if (health.status === 0) {
    console.log(red('\n✗ No se puede conectar al backend en http://localhost:5017'));
    console.log(red('  Inicia el backend con start-backend.bat y vuelve a ejecutar.\n'));
    process.exit(1);
  }

  try {
    await testAuth();
    await testEmployees();
    await testVacations();
    await testPermissions();
    await testAttendance();
    await testOvertime();
    await testPayroll();
    await testSettlement();
    await testAnnualBonus();
    await testPerformance();
    await testDisability();
    await testReports();
    await testNotifications();
    await testCleanup();
  } catch (e) {
    console.log(red(`\n✗ Error inesperado: ${e.message}`));
    console.log(e.stack);
  }

  // Resumen
  const total = passed + failed;
  console.log('\n' + bold('═'.repeat(55)));
  console.log(bold('  RESULTADOS FINALES'));
  console.log('═'.repeat(55));
  console.log(`  ${green(`✓ Pasaron: ${passed}`)}  ${red(`✗ Fallaron: ${failed}`)}  ${yellow(`⚠ Advertencias: ${warned}`)}`);
  console.log(`  Total: ${total} pruebas`);
  const pct = total > 0 ? Math.round((passed / total) * 100) : 0;
  const bar = '█'.repeat(Math.round(pct / 5)) + '░'.repeat(20 - Math.round(pct / 5));
  console.log(`  [${pct >= 80 ? green(bar) : pct >= 60 ? yellow(bar) : red(bar)}] ${pct}%`);
  console.log('═'.repeat(55) + '\n');

  process.exit(failed > 0 ? 1 : 0);
}

main();
