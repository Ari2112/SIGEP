import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { disabilityAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './Disabilities.css';

const TYPES = { EnfermedadComun: 'Enfermedad común', AccidenteLaboral: 'Accidente laboral', Maternidad: 'Maternidad', Otro: 'Otro' };
const TYPE_OPTIONS = [{ value: 1, label: 'Enfermedad común' }, { value: 2, label: 'Accidente laboral' }, { value: 3, label: 'Maternidad' }, { value: 4, label: 'Otro' }];
const STATUS_COLORS = { Pendiente: 'badge-warning', Aprobada: 'badge-success', Rechazada: 'badge-danger' };

function Disabilities() {
  const { user } = useAuth();
  const [requests, setRequests] = useState([]);
  const [myRequests, setMyRequests] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [selected, setSelected] = useState(null);
  const [activeTab, setActiveTab] = useState('pending');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [showReview, setShowReview] = useState(false);
  const [reviewAction, setReviewAction] = useState(null);
  const [reviewComments, setReviewComments] = useState('');
  const [saving, setSaving] = useState(false);
  const [filters, setFilters] = useState({ employeeId: '', status: '', dateFrom: '', dateTo: '' });
  const [form, setForm] = useState({
    startDate: '',
    endDate: '',
    type: 1,
    diagnosis: '',
    doctorName: '',
    medicalCenter: '',
    documentNumber: ''
  });

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH';

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const promises = [disabilityAPI.getMy()];
      if (isManager) {
        promises.push(
          disabilityAPI.getAll({ status: 'Pendiente' }),
          employeeAPI.getAll()
        );
      }
      const results = await Promise.all(promises);
      setMyRequests(results[0].data);
      if (isManager) {
        setRequests(results[1].data);
        setEmployees(results[2].data);
      }
    } catch (err) {
      setError('Error al cargar: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = async () => {
    try {
      setLoading(true);
      const res = await disabilityAPI.getAll({
        employeeId: filters.employeeId || undefined,
        status: filters.status || undefined,
        dateFrom: filters.dateFrom || undefined,
        dateTo: filters.dateTo || undefined
      });
      setRequests(res.data);
    } catch (err) {
      setError('Error al filtrar');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    if (!form.startDate || !form.endDate) { setError('Las fechas son requeridas'); return; }
    try {
      setSaving(true);
      setError('');
      await disabilityAPI.create({ ...form, type: parseInt(form.type) });
      setSuccess('Incapacidad registrada. RRHH será notificado.');
      setShowCreate(false);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al registrar');
    } finally {
      setSaving(false);
    }
  };

  const openReview = (req, approve) => {
    setSelected(req);
    setReviewAction(approve);
    setReviewComments('');
    setShowReview(true);
  };

  const handleReview = async () => {
    try {
      await disabilityAPI.review(selected.id, reviewAction, reviewComments);
      setShowReview(false);
      setSuccess(`Incapacidad ${reviewAction ? 'aprobada' : 'rechazada'}`);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al procesar');
    }
  };

  const formatDate = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="disabilities-container">
        <div className="page-header">
          <div>
            <h1>Incapacidades</h1>
            <p className="page-subtitle">Registro y gestión de incapacidades médicas</p>
          </div>
          <button className="btn btn-primary" onClick={() => { setShowCreate(true); setError(''); }}>
            + Registrar Incapacidad
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="tabs">
          {isManager && (
            <button className={`tab ${activeTab === 'pending' ? 'active' : ''}`} onClick={() => setActiveTab('pending')}>
              Pendientes ({requests.length})
            </button>
          )}
          <button className={`tab ${activeTab === 'my' ? 'active' : ''}`} onClick={() => setActiveTab('my')}>
            Mis Incapacidades ({myRequests.length})
          </button>
          {isManager && (
            <button className={`tab ${activeTab === 'all' ? 'active' : ''}`} onClick={() => { setActiveTab('all'); }}>
              Buscar / Historial
            </button>
          )}
        </div>

        {/* Tab: Pendientes */}
        {activeTab === 'pending' && isManager && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Tipo</th>
                  <th>Desde</th>
                  <th>Hasta</th>
                  <th>Días</th>
                  <th>Diagnóstico</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {requests.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">No hay incapacidades pendientes</td></tr>
                ) : (
                  requests.map(r => (
                    <tr key={r.id}>
                      <td><strong>{r.employeeName}</strong></td>
                      <td><span className="badge badge-info">{TYPES[r.type] || r.type}</span></td>
                      <td>{formatDate(r.startDate)}</td>
                      <td>{formatDate(r.endDate)}</td>
                      <td>{r.totalDays}</td>
                      <td>{r.diagnosis || '-'}</td>
                      <td><span className={`badge ${STATUS_COLORS[r.status] || 'badge-secondary'}`}>{r.status}</span></td>
                      <td>
                        <button className="btn btn-sm btn-success" onClick={() => openReview(r, true)}>Aprobar</button>
                        <button className="btn btn-sm btn-danger" onClick={() => openReview(r, false)}>Rechazar</button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Tab: Mis incapacidades */}
        {activeTab === 'my' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Tipo</th>
                  <th>Desde</th>
                  <th>Hasta</th>
                  <th>Días</th>
                  <th>Diagnóstico</th>
                  <th>Estado</th>
                  <th>Observación</th>
                </tr>
              </thead>
              <tbody>
                {myRequests.length === 0 ? (
                  <tr><td colSpan="7" className="no-data">No tiene incapacidades registradas</td></tr>
                ) : (
                  myRequests.map(r => (
                    <tr key={r.id}>
                      <td><span className="badge badge-info">{TYPES[r.type] || r.type}</span></td>
                      <td>{formatDate(r.startDate)}</td>
                      <td>{formatDate(r.endDate)}</td>
                      <td>{r.totalDays}</td>
                      <td>{r.diagnosis || '-'}</td>
                      <td><span className={`badge ${STATUS_COLORS[r.status] || 'badge-secondary'}`}>{r.status}</span></td>
                      <td>{r.reviewComments || '-'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Tab: Buscar/Historial */}
        {activeTab === 'all' && isManager && (
          <div className="table-card">
            <div className="filters-bar">
              <select value={filters.employeeId} onChange={e => setFilters({ ...filters, employeeId: e.target.value })}>
                <option value="">Todos los empleados</option>
                {employees.map(emp => (
                  <option key={emp.id} value={emp.id}>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</option>
                ))}
              </select>
              <select value={filters.status} onChange={e => setFilters({ ...filters, status: e.target.value })}>
                <option value="">Todos los estados</option>
                <option value="Pendiente">Pendiente</option>
                <option value="Aprobada">Aprobada</option>
                <option value="Rechazada">Rechazada</option>
              </select>
              <input type="date" value={filters.dateFrom} onChange={e => setFilters({ ...filters, dateFrom: e.target.value })} />
              <input type="date" value={filters.dateTo} onChange={e => setFilters({ ...filters, dateTo: e.target.value })} />
              <button className="btn btn-primary" onClick={handleFilter}>Filtrar</button>
            </div>
            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Tipo</th>
                  <th>Desde</th>
                  <th>Hasta</th>
                  <th>Días</th>
                  <th>Estado</th>
                  <th>Revisado por</th>
                </tr>
              </thead>
              <tbody>
                {requests.length === 0 ? (
                  <tr><td colSpan="7" className="no-data">No hay registros</td></tr>
                ) : (
                  requests.map(r => (
                    <tr key={r.id}>
                      <td>{r.employeeName}</td>
                      <td><span className="badge badge-info">{TYPES[r.type] || r.type}</span></td>
                      <td>{formatDate(r.startDate)}</td>
                      <td>{formatDate(r.endDate)}</td>
                      <td>{r.totalDays}</td>
                      <td><span className={`badge ${STATUS_COLORS[r.status] || 'badge-secondary'}`}>{r.status}</span></td>
                      <td>{r.reviewedByName || '-'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Modal Registrar */}
        {showCreate && (
          <div className="modal-overlay" onClick={() => setShowCreate(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Registrar Incapacidad</h2>
                <button className="close-btn" onClick={() => setShowCreate(false)}>&times;</button>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Fecha inicio *</label>
                  <input type="date" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} />
                </div>
                <div className="form-group">
                  <label>Fecha fin *</label>
                  <input type="date" value={form.endDate} onChange={e => setForm({ ...form, endDate: e.target.value })} />
                </div>
              </div>
              <div className="form-group">
                <label>Tipo *</label>
                <select value={form.type} onChange={e => setForm({ ...form, type: e.target.value })}>
                  {TYPE_OPTIONS.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Médico</label>
                  <input type="text" value={form.doctorName}
                    onChange={e => setForm({ ...form, doctorName: e.target.value })}
                    placeholder="Nombre del médico" />
                </div>
                <div className="form-group">
                  <label>Centro médico</label>
                  <input type="text" value={form.medicalCenter}
                    onChange={e => setForm({ ...form, medicalCenter: e.target.value })}
                    placeholder="Hospital / Clínica" />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>N° Documento</label>
                  <input type="text" value={form.documentNumber}
                    onChange={e => setForm({ ...form, documentNumber: e.target.value })}
                    placeholder="Número de orden médica" />
                </div>
                <div className="form-group">
                  <label>Diagnóstico</label>
                  <input type="text" value={form.diagnosis}
                    onChange={e => setForm({ ...form, diagnosis: e.target.value })}
                    placeholder="Diagnóstico médico" />
                </div>
              </div>
              {error && <div className="alert alert-error">{error}</div>}
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowCreate(false)}>Cancelar</button>
                <button className="btn btn-primary" onClick={handleCreate} disabled={saving}>
                  {saving ? 'Registrando...' : 'Registrar'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Modal Revisar */}
        {showReview && selected && (
          <div className="modal-overlay" onClick={() => setShowReview(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>{reviewAction ? 'Aprobar' : 'Rechazar'} Incapacidad</h2>
                <button className="close-btn" onClick={() => setShowReview(false)}>&times;</button>
              </div>
              <div className="info-box">
                <p><strong>Empleado:</strong> {selected.employeeName}</p>
                <p><strong>Tipo:</strong> {TYPES[selected.type] || selected.type}</p>
                <p><strong>Período:</strong> {formatDate(selected.startDate)} — {formatDate(selected.endDate)} ({selected.totalDays} días)</p>
                {selected.diagnosis && <p><strong>Diagnóstico:</strong> {selected.diagnosis}</p>}
                {selected.documentNumber && <p><strong>Documento:</strong> {selected.documentNumber}</p>}
              </div>
              <div className="form-group">
                <label>Observación {!reviewAction ? '(requerida)' : '(opcional)'}</label>
                <textarea value={reviewComments} onChange={e => setReviewComments(e.target.value)} rows="3" />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowReview(false)}>Cancelar</button>
                <button className={`btn ${reviewAction ? 'btn-success' : 'btn-danger'}`} onClick={handleReview}>
                  {reviewAction ? 'Aprobar' : 'Rechazar'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Disabilities;
