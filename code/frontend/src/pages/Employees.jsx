import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { employeeAPI } from '../api/api';
import './Employees.css';

const API_BASE = 'http://localhost:5017/api/v1';

const EMPTY_FORM = {
  firstName: '', lastName: '', identificationNumber: '',
  email: '', phone: '', hireDate: '', baseSalary: '',
  positionId: '', scheduleId: '', supervisorId: '',
  vacationDaysPerYear: 14,
  // Dirección con dropdowns
  provinceId: '', cantonId: '', districtId: '', exactAddress: '',
  // Acceso
  username: '', password: '', userRole: 'Empleado',
};

const EMPTY_EDIT = {
  firstName: '', lastName: '', email: '', phone: '',
  baseSalary: '', positionId: '', scheduleId: '', supervisorId: '',
  vacationDaysPerYear: 14, status: 'Activo',
  provinceId: '', cantonId: '', districtId: '', exactAddress: '',
};

const validarCedula    = (v) => /^\d{9}$/.test(v.replace(/-/g, ''));
const validarTelefono  = (v) => !v || /^\d{4}-?\d{4}$/.test(v);
const validarEmail     = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);

// Hook para cargar cantones/distritos dependientes
function useGeo(token) {
  const headers = { Authorization: `Bearer ${token}` };

  const getProvinces = () =>
    fetch(`${API_BASE}/employees/provinces`, { headers }).then(r => r.json());
  const getCantons = (pId) =>
    fetch(`${API_BASE}/employees/provinces/${pId}/cantons`, { headers }).then(r => r.json());
  const getDistricts = (cId) =>
    fetch(`${API_BASE}/employees/cantons/${cId}/districts`, { headers }).then(r => r.json());

  return { getProvinces, getCantons, getDistricts };
}

// ── Sección dirección con dropdowns ────────────────────────────────
const AddressSection = ({ formData, onProv, onCant, onDist, onExact,
    cantonsData, districtsData, loadingC, loadingD, errors, provinces }) => (
    <>
      <p style={{ fontWeight:600, color:'#2d5a1b', margin:'16px 0 8px' }}>
        Dirección
      </p>
      <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr 1fr', gap:12 }}>
        <Field label="Provincia" error={errors?.provinceId}>
          <select value={formData.provinceId}
            style={{ borderColor: errors?.provinceId ? '#e74c3c' : '' }}
            onChange={e => onProv(e.target.value)}>
            <option value="">Seleccione...</option>
            {provinces.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
        </Field>

        <Field label="Cantón" error={errors?.cantonId}>
          <select value={formData.cantonId}
            disabled={!formData.provinceId || loadingC}
            style={{ borderColor: errors?.cantonId ? '#e74c3c' : '' }}
            onChange={e => onCant(e.target.value)}>
            <option value="">{loadingC ? 'Cargando...' : 'Seleccione...'}</option>
            {cantonsData.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </Field>

        <Field label="Distrito" error={errors?.districtId}>
          <select value={formData.districtId}
            disabled={!formData.cantonId || loadingD}
            style={{ borderColor: errors?.districtId ? '#e74c3c' : '' }}
            onChange={e => onDist(e.target.value)}>
            <option value="">{loadingD ? 'Cargando...' : 'Seleccione...'}</option>
            {districtsData.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
        </Field>
      </div>

      <Field label="Señas exactas (opcional)">
        <input type="text" value={formData.exactAddress}
          placeholder="Casa, barrio, referencias..."
          onChange={e => onExact(e.target.value)} />
      </Field>
    </>
  );

// Componente Field definido FUERA de Employees para evitar re-renders
const Field = ({ label, error, required, children }) => (
  <div className="form-group">
    <label>{label}{required && ' *'}</label>
    {children}
    {error && <small style={{ color:'#e74c3c', fontSize:'0.8rem' }}>{error}</small>}
  </div>
);

function Employees() {
  const [employees, setEmployees]   = useState([]);
  const [positions, setPositions]   = useState([]);
  const [schedules, setSchedules]   = useState([]);
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [search, setSearch]         = useState('');
  const [submitting, setSubmitting] = useState(false);

  // Modales
  const [showAdd, setShowAdd]       = useState(false);
  const [showEdit, setShowEdit]     = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [showDeact, setShowDeact]   = useState(false);
  const [selectedEmp, setSelected]  = useState(null);

  // Formularios
  const [form, setForm]         = useState(EMPTY_FORM);
  const [editForm, setEditForm] = useState(EMPTY_EDIT);
  const [fieldErrors, setFieldErrors] = useState({});

  // Geo — create
  const [provinces, setProvinces]       = useState([]);
  const [cantons, setCantons]           = useState([]);
  const [districts, setDistricts]       = useState([]);
  const [loadingCantons, setLoadingC]   = useState(false);
  const [loadingDistricts, setLoadingD] = useState(false);

  // Geo — edit
  const [editCantons, setEditCantons]     = useState([]);
  const [editDistricts, setEditDistricts] = useState([]);

  const token = localStorage.getItem('token');
  const geo   = useGeo(token);

  useEffect(() => {
    loadAll();
    geo.getProvinces().then(data => setProvinces(Array.isArray(data) ? data : []));
  }, []);

  // ── Geo handlers — Create ──────────────────────────────────────────
  const onProvinceChange = async (pId) => {
    setForm(f => ({ ...f, provinceId: pId, cantonId: '', districtId: '' }));
    setCantons([]); setDistricts([]);
    if (!pId) return;
    setLoadingC(true);
    const data = await geo.getCantons(pId);
    setCantons(Array.isArray(data) ? data : []);
    setLoadingC(false);
  };

  const onCantonChange = async (cId) => {
    setForm(f => ({ ...f, cantonId: cId, districtId: '' }));
    setDistricts([]);
    if (!cId) return;
    setLoadingD(true);
    const data = await geo.getDistricts(cId);
    setDistricts(Array.isArray(data) ? data : []);
    setLoadingD(false);
  };

  // ── Geo handlers — Edit ────────────────────────────────────────────
  const onEditProvinceChange = async (pId) => {
    setEditForm(f => ({ ...f, provinceId: pId, cantonId: '', districtId: '' }));
    setEditCantons([]); setEditDistricts([]);
    if (!pId) return;
    const data = await geo.getCantons(pId);
    setEditCantons(Array.isArray(data) ? data : []);
  };

  const onEditCantonChange = async (cId) => {
    setEditForm(f => ({ ...f, cantonId: cId, districtId: '' }));
    setEditDistricts([]);
    if (!cId) return;
    const data = await geo.getDistricts(cId);
    setEditDistricts(Array.isArray(data) ? data : []);
  };

  // ── Carga ──────────────────────────────────────────────────────────
  const loadAll = async () => {
    try {
      setLoading(true);
      const [empRes, posRes, schRes] = await Promise.all([
        employeeAPI.getAll(),
        employeeAPI.getPositions(),
        employeeAPI.getSchedules(),
      ]);
      setEmployees(empRes.data || []);
      setPositions(posRes.data || []);
      setSchedules(schRes.data || []);
      setError('');
    } catch { setError('Error al cargar los datos'); }
    finally { setLoading(false); }
  };

  // ── Helpers ────────────────────────────────────────────────────────
  const showSuccess = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 4000);
  };

  const fmt = (d) => {
    if (!d) return '-';
    const date = new Date(d);
    return isNaN(date) ? '-' : date.toLocaleDateString('es-CR');
  };

  const fmtCurrency = (v) =>
    new Intl.NumberFormat('es-CR', { style:'currency', currency:'CRC', minimumFractionDigits:0 }).format(v || 0);

  const filtered = employees.filter(e =>
    e.fullName?.toLowerCase().includes(search.toLowerCase()) ||
    e.identificationNumber?.includes(search) ||
    e.email?.toLowerCase().includes(search.toLowerCase())
  );

  // ── Validaciones ───────────────────────────────────────────────────
  const validateCreate = () => {
    const errs = {};
    if (!form.firstName.trim())            errs.firstName = 'Requerido';
    if (!form.lastName.trim())             errs.lastName  = 'Requerido';
    if (!form.identificationNumber.trim()) errs.identificationNumber = 'Requerido';
    else if (!validarCedula(form.identificationNumber))
                                           errs.identificationNumber = 'Cédula inválida (9 dígitos)';
    if (!form.email.trim())                errs.email = 'Requerido';
    else if (!validarEmail(form.email))    errs.email = 'Correo inválido';
    if (form.phone && !validarTelefono(form.phone)) errs.phone = 'Formato: 8888-8888';
    if (!form.hireDate)                    errs.hireDate   = 'Requerido';
    if (!form.baseSalary || parseFloat(form.baseSalary) <= 0) errs.baseSalary = 'Mayor a 0';
    // Dirección es opcional — no validar provincia/cantón/distrito
    if (form.username && !form.password)   errs.password   = 'Ingrese contraseña';
    if (form.password && !form.username)   errs.username   = 'Ingrese usuario';
    if (form.password && form.password.length < 6) errs.password = 'Mínimo 6 caracteres';
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const validateEdit = () => {
    const errs = {};
    if (!editForm.firstName.trim()) errs.firstName = 'Requerido';
    if (!editForm.lastName.trim())  errs.lastName  = 'Requerido';
    if (!editForm.email.trim())     errs.email     = 'Requerido';
    else if (!validarEmail(editForm.email)) errs.email = 'Correo inválido';
    if (editForm.phone && !validarTelefono(editForm.phone)) errs.phone = 'Formato: 8888-8888';
    if (!editForm.baseSalary || parseFloat(editForm.baseSalary) <= 0) errs.baseSalary = 'Mayor a 0';
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  // ── Crear ──────────────────────────────────────────────────────────
  const handleCreate = async (e) => {
    e.preventDefault();
    if (!validateCreate()) return;
    try {
      setSubmitting(true);
      setError('');
      // Construir dirección textual desde los dropdowns
      const prov = provinces.find(p => p.id === parseInt(form.provinceId))?.name || '';
      const cant = cantons.find(c => c.id === parseInt(form.cantonId))?.name || '';
      const dist = districts.find(d => d.id === parseInt(form.districtId))?.name || '';
      const address = `${dist}, ${cant}, ${prov}${form.exactAddress ? '. ' + form.exactAddress : ''}`;

      await employeeAPI.create({
        firstName:            form.firstName.trim(),
        lastName:             form.lastName.trim(),
        identificationNumber: form.identificationNumber.trim(),
        email:                form.email.trim(),
        phone:                form.phone || null,
        address,
        hireDate:             form.hireDate,
        baseSalary:           parseFloat(form.baseSalary),
        positionId:           form.positionId   ? parseInt(form.positionId)   : null,
        scheduleId:           form.scheduleId   ? parseInt(form.scheduleId)   : null,
        supervisorId:         form.supervisorId ? parseInt(form.supervisorId) : null,
        vacationDaysPerYear:  parseInt(form.vacationDaysPerYear) || 14,
        username:             form.username || null,
        password:             form.password || null,
        userRole:             form.userRole || 'Empleado',
      });
      setShowAdd(false);
      setForm(EMPTY_FORM);
      setCantons([]); setDistricts([]);
      setFieldErrors({});
      showSuccess('Empleado creado exitosamente');
      loadAll();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al crear el empleado');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Editar ─────────────────────────────────────────────────────────
  const openEdit = (emp) => {
    setSelected(emp);
    setEditForm({
      firstName:           emp.firstName,
      lastName:            emp.lastName,
      email:               emp.email,
      phone:               emp.phone || '',
      baseSalary:          emp.baseSalary,
      positionId:          emp.positionId   || '',
      scheduleId:          emp.scheduleId   || '',
      supervisorId:        emp.supervisorId || '',
      vacationDaysPerYear: emp.vacationDaysPerYear || 14,
      status:              emp.status || 'Activo',
      provinceId: '', cantonId: '', districtId: '',
      exactAddress: emp.address || '',
    });
    setEditCantons([]); setEditDistricts([]);
    setFieldErrors({});
    setError('');
    setShowEdit(true);
  };

  const handleEdit = async (e) => {
    e.preventDefault();
    if (!validateEdit()) return;
    try {
      setSubmitting(true);
      setError('');

      // Si se seleccionó nueva dirección, construirla; si no, mantener la existente
      let address = editForm.exactAddress;
      if (editForm.provinceId && editForm.cantonId && editForm.districtId) {
        const prov = provinces.find(p => p.id === parseInt(editForm.provinceId))?.name || '';
        const cant = editCantons.find(c => c.id === parseInt(editForm.cantonId))?.name || '';
        const dist = editDistricts.find(d => d.id === parseInt(editForm.districtId))?.name || '';
        const extra = editForm.exactAddress ? '. ' + editForm.exactAddress : '';
        address = `${dist}, ${cant}, ${prov}${extra}`;
      }

      await employeeAPI.update(selectedEmp.id, {
        firstName:           editForm.firstName.trim(),
        lastName:            editForm.lastName.trim(),
        email:               editForm.email.trim(),
        phone:               editForm.phone || null,
        address,
        baseSalary:          parseFloat(editForm.baseSalary),
        positionId:          editForm.positionId   ? parseInt(editForm.positionId)   : null,
        scheduleId:          editForm.scheduleId   ? parseInt(editForm.scheduleId)   : null,
        supervisorId:        editForm.supervisorId ? parseInt(editForm.supervisorId) : null,
        vacationDaysPerYear: parseInt(editForm.vacationDaysPerYear) || 14,
        status:              editForm.status,
      });
      setShowEdit(false);
      setFieldErrors({});
      showSuccess(`Empleado ${selectedEmp.fullName} actualizado`);
      loadAll();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al actualizar');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Desactivar ─────────────────────────────────────────────────────
  const handleDeactivate = async () => {
    try {
      await employeeAPI.deactivate(selectedEmp.id);
      setShowDeact(false);
      showSuccess(`Empleado ${selectedEmp.fullName} desactivado`);
      loadAll();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al desactivar');
    }
  };

  // Field definido fuera del componente (ver arriba)

  // AddressSection definido fuera del componente


  // ── Render ─────────────────────────────────────────────────────────
  return (
    <Layout>
      <div className="employees">

        <div className="page-header">
          <div>
            <h1>Gestión de Empleados</h1>
            <p className="page-subtitle">CRUD completo del personal</p>
          </div>
          <button className="btn btn-primary"
            onClick={() => {
              setForm(EMPTY_FORM);
              setCantons([]); setDistricts([]);
              setFieldErrors({}); setError('');
              setShowAdd(true);
            }}>
            + Agregar Empleado
          </button>
        </div>

        {error      && <div className="alert alert-error"   onClick={() => setError('')}>{error}</div>}
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

        <div style={{ marginBottom:16 }}>
          <input type="text" value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Buscar por nombre, cédula o correo..."
            style={{ width:'100%', padding:'10px 14px', borderRadius:8, border:'1px solid #ddd', fontSize:14 }}
          />
        </div>

        {loading ? (
          <div className="loading">Cargando...</div>
        ) : (
          <div className="card">
            <table className="table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Cédula</th>
                  <th>Correo</th>
                  <th>Puesto</th>
                  <th>Salario Base</th>
                  <th>Ingreso</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">No hay empleados</td></tr>
                ) : (
                  filtered.map(emp => (
                    <tr key={emp.id}>
                      <td><strong>{emp.fullName}</strong></td>
                      <td>{emp.identificationNumber}</td>
                      <td style={{ fontSize:'0.88rem' }}>{emp.email}</td>
                      <td style={{ fontSize:'0.88rem' }}>{emp.positionName || '-'}</td>
                      <td>{fmtCurrency(emp.baseSalary)}</td>
                      <td>{fmt(emp.hireDate)}</td>
                      <td>
                        <span className={`badge ${emp.status === 'Activo' ? 'badge-success' : 'badge-secondary'}`}>
                          {emp.status}
                        </span>
                      </td>
                      <td style={{ display:'flex', gap:4, flexWrap:'wrap' }}>
                        <button className="btn btn-sm btn-secondary"
                          onClick={() => { setSelected(emp); setShowDetail(true); }}>Ver</button>
                        <button className="btn btn-sm btn-primary"
                          onClick={() => openEdit(emp)}>Editar</button>
                        {emp.status === 'Activo' && (
                          <button className="btn btn-sm btn-danger"
                            onClick={() => { setSelected(emp); setShowDeact(true); }}>
                            Desactivar
                          </button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* ══════════ MODAL: Agregar ══════════ */}
        {showAdd && (
          <div className="modal-overlay" onClick={() => setShowAdd(false)}>
            <div className="modal" style={{ maxWidth:660, maxHeight:'90vh', overflowY:'auto' }}
              onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Agregar Nuevo Empleado</h2>
                <button className="close-btn" onClick={() => setShowAdd(false)}>×</button>
              </div>

              <form onSubmit={handleCreate} noValidate>

                <p style={{ fontWeight:600, color:'#2d5a1b', marginBottom:8 }}>Datos personales</p>
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Nombre" error={fieldErrors.firstName} required>
                    <input type="text" value={form.firstName}
                      style={{ borderColor: fieldErrors.firstName ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, firstName: e.target.value})} />
                  </Field>
                  <Field label="Apellidos" error={fieldErrors.lastName} required>
                    <input type="text" value={form.lastName}
                      style={{ borderColor: fieldErrors.lastName ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, lastName: e.target.value})} />
                  </Field>
                </div>

                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Cédula (9 dígitos)" error={fieldErrors.identificationNumber} required>
                    <input type="text" value={form.identificationNumber}
                      placeholder="101234567"
                      style={{ borderColor: fieldErrors.identificationNumber ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, identificationNumber: e.target.value})} />
                  </Field>
                  <Field label="Teléfono" error={fieldErrors.phone}>
                    <input type="text" value={form.phone} placeholder="8888-8888"
                      style={{ borderColor: fieldErrors.phone ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, phone: e.target.value})} />
                  </Field>
                </div>

                <Field label="Correo electrónico" error={fieldErrors.email} required>
                  <input type="email" value={form.email} placeholder="correo@ejemplo.com"
                    style={{ borderColor: fieldErrors.email ? '#e74c3c':'' }}
                    onChange={e => setForm({...form, email: e.target.value})} />
                </Field>

                {/* Dirección con dropdowns */}
                <AddressSection
                  formData={form}
                  onProv={onProvinceChange}
                  onCant={onCantonChange}
                  onDist={v => setForm(f => ({...f, districtId: v}))}
                  onExact={v => setForm(f => ({...f, exactAddress: v}))}
                  cantonsData={cantons}
                  districtsData={districts}
                  loadingC={loadingCantons}
                  loadingD={loadingDistricts}
                  errors={fieldErrors}
                  provinces={provinces}
                />

                <p style={{ fontWeight:600, color:'#2d5a1b', margin:'16px 0 8px' }}>Datos laborales</p>
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Fecha de ingreso" error={fieldErrors.hireDate} required>
                    <input type="date" value={form.hireDate}
                      style={{ borderColor: fieldErrors.hireDate ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, hireDate: e.target.value})} />
                  </Field>
                  <Field label="Salario base (₡)" error={fieldErrors.baseSalary} required>
                    <input type="number" value={form.baseSalary} placeholder="500000" min="0"
                      style={{ borderColor: fieldErrors.baseSalary ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, baseSalary: e.target.value})} />
                  </Field>
                </div>

                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Puesto">
                    <select value={form.positionId}
                      onChange={e => setForm({...form, positionId: e.target.value})}>
                      <option value="">Sin puesto</option>
                      {positions.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                  </Field>
                  <Field label="Horario">
                    <select value={form.scheduleId}
                      onChange={e => setForm({...form, scheduleId: e.target.value})}>
                      <option value="">Sin horario</option>
                      {schedules.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                    </select>
                  </Field>
                </div>

                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Supervisor">
                    <select value={form.supervisorId}
                      onChange={e => setForm({...form, supervisorId: e.target.value})}>
                      <option value="">Sin supervisor</option>
                      {employees.filter(e => e.status === 'Activo')
                        .map(e => <option key={e.id} value={e.id}>{e.fullName}</option>)}
                    </select>
                  </Field>
                  <Field label="Días de vacaciones al año">
                    <input type="number" value={form.vacationDaysPerYear} min="14"
                      onChange={e => setForm({...form, vacationDaysPerYear: e.target.value})} />
                  </Field>
                </div>

                <p style={{ fontWeight:600, color:'#2d5a1b', margin:'16px 0 4px' }}>Acceso al sistema</p>
                <p style={{ fontSize:'0.83rem', color:'#666', marginBottom:10 }}>
                  Opcional. Si no se asignan credenciales, el empleado no podrá iniciar sesión.
                </p>
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Usuario" error={fieldErrors.username}>
                    <input type="text" value={form.username} placeholder="juan.perez"
                      style={{ borderColor: fieldErrors.username ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, username: e.target.value})} />
                  </Field>
                  <Field label="Contraseña" error={fieldErrors.password}>
                    <input type="password" value={form.password} placeholder="Mínimo 6 caracteres"
                      style={{ borderColor: fieldErrors.password ? '#e74c3c':'' }}
                      onChange={e => setForm({...form, password: e.target.value})} />
                  </Field>
                </div>
                <Field label="Rol en el sistema">
                  <select value={form.userRole}
                    onChange={e => setForm({...form, userRole: e.target.value})}>
                    <option value="Empleado">Empleado</option>
                    <option value="Jefatura">Jefatura</option>
                    <option value="RRHH">Recursos Humanos</option>
                    <option value="Admin">Administrador</option>
                  </select>
                </Field>

                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary"
                    onClick={() => setShowAdd(false)}>Cancelar</button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Guardando...' : 'Guardar Empleado'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ══════════ MODAL: Editar ══════════ */}
        {showEdit && selectedEmp && (
          <div className="modal-overlay" onClick={() => setShowEdit(false)}>
            <div className="modal" style={{ maxWidth:640, maxHeight:'90vh', overflowY:'auto' }}
              onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Editar — {selectedEmp.fullName}</h2>
                <button className="close-btn" onClick={() => setShowEdit(false)}>×</button>
              </div>

              <form onSubmit={handleEdit} noValidate>
                <p style={{ fontWeight:600, color:'#2d5a1b', marginBottom:8 }}>Datos personales</p>
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Nombre" error={fieldErrors.firstName} required>
                    <input type="text" value={editForm.firstName}
                      style={{ borderColor: fieldErrors.firstName ? '#e74c3c':'' }}
                      onChange={e => setEditForm({...editForm, firstName: e.target.value})} />
                  </Field>
                  <Field label="Apellidos" error={fieldErrors.lastName} required>
                    <input type="text" value={editForm.lastName}
                      style={{ borderColor: fieldErrors.lastName ? '#e74c3c':'' }}
                      onChange={e => setEditForm({...editForm, lastName: e.target.value})} />
                  </Field>
                </div>

                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Correo" error={fieldErrors.email} required>
                    <input type="email" value={editForm.email}
                      style={{ borderColor: fieldErrors.email ? '#e74c3c':'' }}
                      onChange={e => setEditForm({...editForm, email: e.target.value})} />
                  </Field>
                  <Field label="Teléfono" error={fieldErrors.phone}>
                    <input type="text" value={editForm.phone} placeholder="8888-8888"
                      style={{ borderColor: fieldErrors.phone ? '#e74c3c':'' }}
                      onChange={e => setEditForm({...editForm, phone: e.target.value})} />
                  </Field>
                </div>

                {/* Dirección editable */}
                <p style={{ fontWeight:600, color:'#2d5a1b', margin:'12px 0 4px' }}>
                  Actualizar dirección <span style={{ fontWeight:400, color:'#888', fontSize:'0.83rem' }}>(opcional)</span>
                </p>
                {selectedEmp.address && (
                  <p style={{ fontSize:'0.83rem', color:'#666', marginBottom:8 }}>
                    Actual: <em>{selectedEmp.address}</em>
                  </p>
                )}
                <AddressSection
                  formData={editForm}
                  onProv={onEditProvinceChange}
                  onCant={onEditCantonChange}
                  onDist={v => setEditForm(f => ({...f, districtId: v}))}
                  onExact={v => setEditForm(f => ({...f, exactAddress: v}))}
                  cantonsData={editCantons}
                  districtsData={editDistricts}
                  loadingC={false}
                  loadingD={false}
                  errors={{}}
                  provinces={provinces}
                />

                <p style={{ fontWeight:600, color:'#2d5a1b', margin:'16px 0 8px' }}>Datos laborales</p>
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <Field label="Salario base (₡)" error={fieldErrors.baseSalary} required>
                    <input type="number" value={editForm.baseSalary} min="0"
                      style={{ borderColor: fieldErrors.baseSalary ? '#e74c3c':'' }}
                      onChange={e => setEditForm({...editForm, baseSalary: e.target.value})} />
                  </Field>
                  <Field label="Días de vacaciones al año">
                    <input type="number" value={editForm.vacationDaysPerYear} min="14"
                      onChange={e => setEditForm({...editForm, vacationDaysPerYear: e.target.value})} />
                  </Field>
                </div>

                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr 1fr', gap:12 }}>
                  <Field label="Puesto">
                    <select value={editForm.positionId}
                      onChange={e => setEditForm({...editForm, positionId: e.target.value})}>
                      <option value="">Sin puesto</option>
                      {positions.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                  </Field>
                  <Field label="Horario">
                    <select value={editForm.scheduleId}
                      onChange={e => setEditForm({...editForm, scheduleId: e.target.value})}>
                      <option value="">Sin horario</option>
                      {schedules.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                    </select>
                  </Field>
                  <Field label="Estado">
                    <select value={editForm.status}
                      onChange={e => setEditForm({...editForm, status: e.target.value})}>
                      <option value="Activo">Activo</option>
                      <option value="Inactivo">Inactivo</option>
                    </select>
                  </Field>
                </div>

                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary"
                    onClick={() => setShowEdit(false)}>Cancelar</button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Guardando...' : 'Guardar Cambios'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ══════════ MODAL: Detalle ══════════ */}
        {showDetail && selectedEmp && (
          <div className="modal-overlay" onClick={() => setShowDetail(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>{selectedEmp.fullName}</h2>
                <button className="close-btn" onClick={() => setShowDetail(false)}>×</button>
              </div>
              <div className="info-box">
                <p><strong>Cédula:</strong> {selectedEmp.identificationNumber}</p>
                <p><strong>Correo:</strong> {selectedEmp.email}</p>
                <p><strong>Teléfono:</strong> {selectedEmp.phone || '-'}</p>
                <p><strong>Dirección:</strong> {selectedEmp.address || '-'}</p>
                <p><strong>Puesto:</strong> {selectedEmp.positionName || '-'}</p>
                <p><strong>Horario:</strong> {selectedEmp.scheduleName || '-'}</p>
                <p><strong>Salario base:</strong> {fmtCurrency(selectedEmp.baseSalary)}</p>
                <p><strong>Fecha de ingreso:</strong> {fmt(selectedEmp.hireDate)}</p>
                <p><strong>Días vacaciones/año:</strong> {selectedEmp.vacationDaysPerYear ?? 14}</p>
                <p><strong>Estado:</strong>{' '}
                  <span className={`badge ${selectedEmp.status === 'Activo' ? 'badge-success' : 'badge-secondary'}`}>
                    {selectedEmp.status}
                  </span>
                </p>
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetail(false)}>Cerrar</button>
                <button className="btn btn-primary"
                  onClick={() => { setShowDetail(false); openEdit(selectedEmp); }}>
                  Editar
                </button>
              </div>
            </div>
          </div>
        )}

        {/* ══════════ MODAL: Desactivar ══════════ */}
        {showDeact && selectedEmp && (
          <div className="modal-overlay" onClick={() => setShowDeact(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Desactivar Empleado</h2>
                <button className="close-btn" onClick={() => setShowDeact(false)}>×</button>
              </div>
              <p>¿Está seguro que desea desactivar a <strong>{selectedEmp.fullName}</strong>?</p>
              <p style={{ color:'#e67e22', marginTop:8, fontSize:'0.88rem' }}>
                ⚠ El empleado no podrá acceder al sistema. Su historial se conserva.
              </p>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDeact(false)}>Cancelar</button>
                <button className="btn btn-danger" onClick={handleDeactivate}>Confirmar</button>
              </div>
            </div>
          </div>
        )}

      </div>
    </Layout>
  );
}

export default Employees;
