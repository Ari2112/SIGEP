import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { employeeAPI } from '../api/api';
import './Employees.css';

const EMPTY_FORM = {
  firstName: '',
  lastName: '',
  identificationNumber: '',
  email: '',
  phone: '',
  address: '',
  hireDate: '',
  baseSalary: '',
  positionId: '',
  scheduleId: '',
  supervisorId: '',
  vacationDaysPerYear: 14,
  username: '',
  password: '',
  userRole: 'Empleado',
};

const Employees = () => {
  const [employees, setEmployees] = useState([]);
  const [positions, setPositions] = useState([]);
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  const [showAddModal, setShowAddModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [formData, setFormData] = useState(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [search, setSearch] = useState('');

  useEffect(() => { loadAll(); }, []);

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
    } catch (err) {
      setError('Error al cargar los datos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');
    try {
      await employeeAPI.create({
        firstName: formData.firstName,
        lastName: formData.lastName,
        identificationNumber: formData.identificationNumber,
        email: formData.email,
        phone: formData.phone || null,
        address: formData.address || null,
        hireDate: formData.hireDate,
        baseSalary: parseFloat(formData.baseSalary),
        positionId: formData.positionId ? parseInt(formData.positionId) : null,
        scheduleId: formData.scheduleId ? parseInt(formData.scheduleId) : null,
        supervisorId: formData.supervisorId ? parseInt(formData.supervisorId) : null,
        vacationDaysPerYear: parseInt(formData.vacationDaysPerYear) || 14,
        username: formData.username || null,
        password: formData.password || null,
        userRole: formData.userRole || 'Empleado',
      });
      setSuccessMsg('Empleado agregado exitosamente');
      setShowAddModal(false);
      setFormData(EMPTY_FORM);
      loadAll();
      setTimeout(() => setSuccessMsg(''), 4000);
    } catch (err) {
      setError(err.response?.data?.message || 'Error al agregar el empleado');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeactivate = async () => {
    try {
      setError('');
      await employeeAPI.deactivate(selectedEmployee.id);
      setSuccessMsg(`Empleado ${selectedEmployee.fullName} desactivado`);
      setShowDeleteModal(false);
      setSelectedEmployee(null);
      loadAll();
      setTimeout(() => setSuccessMsg(''), 4000);
    } catch (err) {
      setError(err.response?.data?.message || 'Error al desactivar el empleado');
    }
  };

  const formatCurrency = (amount) =>
    new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC' }).format(amount);

  const formatDate = (d) => {
    if (!d) return '-';
    const date = new Date(d);
    return isNaN(date) ? '-' : date.toLocaleDateString('es-CR');
  };

  const filtered = employees.filter(e =>
    e.fullName?.toLowerCase().includes(search.toLowerCase()) ||
    e.identificationNumber?.includes(search) ||
    e.email?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <Layout>
      <div className="employees">
        <div className="page-header">
          <h1>Gestión de Empleados</h1>
          <button className="btn btn-primary" onClick={() => { setFormData(EMPTY_FORM); setShowAddModal(true); }}>
            + Agregar Empleado
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

        <div style={{ marginBottom: '16px' }}>
          <input
            type="text"
            placeholder="Buscar por nombre, cédula o correo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', border: '1px solid #ddd', fontSize: '14px' }}
          />
        </div>

        {loading ? (
          <div className="card"><p>Cargando empleados...</p></div>
        ) : (
          <div className="card">
            <div className="table-responsive">
              <table className="table">
                <thead>
                  <tr>
                    <th>Nombre Completo</th>
                    <th>Cédula</th>
                    <th>Correo</th>
                    <th>Puesto</th>
                    <th>Salario Base</th>
                    <th>Fecha Ingreso</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.length === 0 ? (
                    <tr><td colSpan="8" style={{ textAlign: 'center' }}>No hay empleados registrados</td></tr>
                  ) : (
                    filtered.map((emp) => (
                      <tr key={emp.id}>
                        <td>{emp.fullName}</td>
                        <td>{emp.identificationNumber}</td>
                        <td>{emp.email}</td>
                        <td>{emp.positionName || '-'}</td>
                        <td>{formatCurrency(emp.baseSalary)}</td>
                        <td>{formatDate(emp.hireDate)}</td>
                        <td>
                          <span className={`status-badge status-${emp.status?.toLowerCase()}`}>
                            {emp.status}
                          </span>
                        </td>
                        <td>
                          <button
                            className="btn btn-sm btn-secondary"
                            style={{ marginRight: '6px' }}
                            onClick={() => { setSelectedEmployee(emp); setShowDetailModal(true); }}
                          >
                            Ver
                          </button>
                          {emp.status === 'Activo' && (
                            <button
                              className="btn btn-sm btn-danger"
                              onClick={() => { setSelectedEmployee(emp); setShowDeleteModal(true); }}
                            >
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
          </div>
        )}

        {/* Modal agregar empleado */}
        {showAddModal && (
          <div className="modal-overlay">
            <div className="modal" style={{ maxWidth: '620px', maxHeight: '90vh', overflowY: 'auto' }}>
              <div className="modal-header">
                <h2>Agregar Nuevo Empleado</h2>
                <button className="close-btn" onClick={() => setShowAddModal(false)}>×</button>
              </div>
              <form onSubmit={handleSubmit}>

                <p style={{ fontWeight: '500', marginBottom: '8px', color: '#555' }}>Datos personales</p>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Nombre *</label>
                    <input type="text" value={formData.firstName}
                      onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                      placeholder="Ej: Juan" required />
                  </div>
                  <div className="form-group">
                    <label>Apellido *</label>
                    <input type="text" value={formData.lastName}
                      onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                      placeholder="Ej: Pérez" required />
                  </div>
                </div>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Cédula *</label>
                    <input type="text" value={formData.identificationNumber}
                      onChange={(e) => setFormData({ ...formData, identificationNumber: e.target.value })}
                      placeholder="Ej: 101234567" required />
                  </div>
                  <div className="form-group">
                    <label>Teléfono</label>
                    <input type="text" value={formData.phone}
                      onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                      placeholder="Ej: 8888-8888" />
                  </div>
                </div>

                <div className="form-group">
                  <label>Correo electrónico *</label>
                  <input type="email" value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    placeholder="correo@ejemplo.com" required />
                </div>

                <div className="form-group">
                  <label>Dirección</label>
                  <input type="text" value={formData.address}
                    onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                    placeholder="Provincia, Cantón, Distrito..." />
                </div>

                <p style={{ fontWeight: '500', margin: '12px 0 8px', color: '#555' }}>Datos laborales</p>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Fecha de Ingreso *</label>
                    <input type="date" value={formData.hireDate}
                      onChange={(e) => setFormData({ ...formData, hireDate: e.target.value })}
                      required />
                  </div>
                  <div className="form-group">
                    <label>Salario Base (₡) *</label>
                    <input type="number" value={formData.baseSalary}
                      onChange={(e) => setFormData({ ...formData, baseSalary: e.target.value })}
                      placeholder="Ej: 500000" min="0" required />
                  </div>
                </div>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Puesto</label>
                    <select value={formData.positionId}
                      onChange={(e) => setFormData({ ...formData, positionId: e.target.value })}>
                      <option value="">Sin puesto asignado</option>
                      {positions.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Horario</label>
                    <select value={formData.scheduleId}
                      onChange={(e) => setFormData({ ...formData, scheduleId: e.target.value })}>
                      <option value="">Sin horario asignado</option>
                      {schedules.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                    </select>
                  </div>
                </div>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Supervisor</label>
                    <select value={formData.supervisorId}
                      onChange={(e) => setFormData({ ...formData, supervisorId: e.target.value })}>
                      <option value="">Sin supervisor</option>
                      {employees.filter(e => e.status === 'Activo').map(e =>
                        <option key={e.id} value={e.id}>{e.fullName}</option>
                      )}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Días de vacaciones al año</label>
                    <input type="number" value={formData.vacationDaysPerYear}
                      onChange={(e) => setFormData({ ...formData, vacationDaysPerYear: e.target.value })}
                      min="14" />
                  </div>
                </div>

                <p style={{ fontWeight: '500', margin: '12px 0 8px', color: '#555' }}>Acceso al sistema</p>

                <div className="time-inputs">
                  <div className="form-group">
                    <label>Usuario</label>
                    <input type="text" value={formData.username}
                      onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                      placeholder="Ej: juan.perez" />
                  </div>
                  <div className="form-group">
                    <label>Contraseña</label>
                    <input type="password" value={formData.password}
                      onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                      placeholder="Mínimo 6 caracteres" />
                  </div>
                </div>

                <div className="form-group">
                  <label>Rol en el sistema</label>
                  <select value={formData.userRole}
                    onChange={(e) => setFormData({ ...formData, userRole: e.target.value })}>
                    <option value="Empleado">Empleado</option>
                    <option value="Jefatura">Jefatura</option>
                    <option value="RRHH">Recursos Humanos</option>
                    <option value="Admin">Administrador</option>
                  </select>
                </div>

                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowAddModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Guardando...' : 'Guardar Empleado'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal detalle empleado */}
        {showDetailModal && selectedEmployee && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>{selectedEmployee.fullName}</h2>
                <button className="close-btn" onClick={() => setShowDetailModal(false)}>×</button>
              </div>
              <div className="info-box">
                <p><strong>Cédula:</strong> {selectedEmployee.identificationNumber}</p>
                <p><strong>Correo:</strong> {selectedEmployee.email}</p>
                <p><strong>Puesto:</strong> {selectedEmployee.positionName || '-'}</p>
                <p><strong>Horario:</strong> {selectedEmployee.scheduleName || '-'}</p>
                <p><strong>Salario Base:</strong> {formatCurrency(selectedEmployee.baseSalary)}</p>
                <p><strong>Fecha Ingreso:</strong> {formatDate(selectedEmployee.hireDate)}</p>
                <p><strong>Estado:</strong> {selectedEmployee.status}</p>
                <p><strong>Días vacaciones/año:</strong> {selectedEmployee.vacationDaysPerYear ?? 14}</p>
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetailModal(false)}>Cerrar</button>
              </div>
            </div>
          </div>
        )}

        {/* Modal desactivar empleado */}
        {showDeleteModal && selectedEmployee && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Desactivar Empleado</h2>
                <button className="close-btn" onClick={() => setShowDeleteModal(false)}>×</button>
              </div>
              <p>¿Está seguro que desea desactivar a <strong>{selectedEmployee.fullName}</strong>?</p>
              <p style={{ color: '#e67e22', marginTop: '8px', fontSize: '14px' }}>
                ⚠ El empleado no podrá acceder al sistema ni aparecer en planillas futuras.
                Esta acción no elimina el historial.
              </p>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDeleteModal(false)}>Cancelar</button>
                <button className="btn btn-danger" onClick={handleDeactivate}>Desactivar</button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default Employees;