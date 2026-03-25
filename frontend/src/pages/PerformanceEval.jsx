import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { evaluationAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './PerformanceEval.css';

const STATUS_COLORS = {
  Borrador: 'badge-secondary',
  Completada: 'badge-success',
  RevisadaPorEmpleado: 'badge-info'
};

const STATUS_LABELS = {
  Borrador: 'Borrador',
  Completada: 'Completada',
  RevisadaPorEmpleado: 'Revisada'
};

const SCORE_COLORS = [, , , 'badge-danger', 'badge-danger', 'badge-warning', 'badge-warning', 'badge-info', 'badge-info', 'badge-success', 'badge-success'];

function PerformanceEval() {
  const { user } = useAuth();
  const [evaluations, setEvaluations] = useState([]);
  const [myEvaluations, setMyEvaluations] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [selected, setSelected] = useState(null);
  const [activeTab, setActiveTab] = useState('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [saving, setSaving] = useState(false);
  const [filters, setFilters] = useState({ employeeId: '', year: '', status: '' });
  const [form, setForm] = useState({
    employeeId: '',
    evaluationDate: new Date().toISOString().split('T')[0],
    periodStartDate: '',
    periodEndDate: '',
    score: 7,
    comments: '',
    strengths: '',
    areasToImprove: '',
    goals: ''
  });

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH' || user?.role === 'Jefatura';

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const promises = [evaluationAPI.getMy()];
      if (isManager) {
        promises.push(evaluationAPI.getAll(), employeeAPI.getAll());
      }
      const results = await Promise.all(promises);
      setMyEvaluations(results[0].data);
      if (isManager) {
        setEvaluations(results[1].data);
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
      const res = await evaluationAPI.getAll({
        employeeId: filters.employeeId || undefined,
        year: filters.year || undefined,
        status: filters.status || undefined
      });
      setEvaluations(res.data);
    } catch (err) {
      setError('Error al filtrar');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    if (!form.employeeId) { setError('Seleccione un empleado'); return; }
    if (form.score < 3 || form.score > 10) { setError('La puntuación debe ser entre 3 y 10'); return; }
    try {
      setSaving(true);
      setError('');
      await evaluationAPI.create({
        ...form,
        employeeId: parseInt(form.employeeId),
        score: parseInt(form.score),
        periodStartDate: form.periodStartDate || null,
        periodEndDate: form.periodEndDate || null
      });
      setSuccess('Evaluación registrada exitosamente');
      setShowCreate(false);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al guardar');
    } finally {
      setSaving(false);
    }
  };

  const handleAcknowledge = async (id) => {
    try {
      await evaluationAPI.acknowledge(id);
      setSuccess('Evaluación confirmada');
      loadData();
      setShowDetail(false);
    } catch (err) {
      setError(err.response?.data?.message || 'Error al confirmar');
    }
  };

  const openDetail = async (id) => {
    try {
      const res = await evaluationAPI.getById(id);
      setSelected(res.data);
      setShowDetail(true);
    } catch (err) {
      setError('Error al cargar detalle');
    }
  };

  const formatDate = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  const renderTable = (evals, showEmployee = true) => (
    <div className="table-card">
      <table className="table">
        <thead>
          <tr>
            {showEmployee && <th>Empleado</th>}
            {showEmployee && <th>Puesto</th>}
            <th>Fecha</th>
            <th>Puntuación</th>
            <th>Calificación</th>
            <th>Evaluador</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {evals.length === 0 ? (
            <tr><td colSpan={showEmployee ? 8 : 6} className="no-data">No hay evaluaciones</td></tr>
          ) : (
            evals.map(ev => (
              <tr key={ev.id}>
                {showEmployee && <td><strong>{ev.employeeName}</strong></td>}
                {showEmployee && <td>{ev.positionName || '-'}</td>}
                <td>{formatDate(ev.evaluationDate)}</td>
                <td>
                  <span className={`badge ${SCORE_COLORS[ev.score] || 'badge-secondary'}`}>
                    {ev.score}/10
                  </span>
                </td>
                <td>{ev.scoreLabel}</td>
                <td>{ev.evaluatorName}</td>
                <td><span className={`badge ${STATUS_COLORS[ev.status] || 'badge-secondary'}`}>{STATUS_LABELS[ev.status] || ev.status}</span></td>
                <td>
                  <button className="btn btn-sm btn-ghost" onClick={() => openDetail(ev.id)}>Ver</button>
                  {!showEmployee && ev.status === 'Completada' && !ev.employeeAcknowledged && (
                    <button className="btn btn-sm btn-primary" onClick={() => handleAcknowledge(ev.id)}>Confirmar</button>
                  )}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="eval-container">
        <div className="page-header">
          <div>
            <h1>Evaluación de Desempeño</h1>
            <p className="page-subtitle">Registro y seguimiento de evaluaciones</p>
          </div>
          {isManager && (
            <button className="btn btn-primary" onClick={() => { setShowCreate(true); setError(''); }}>
              + Nueva Evaluación
            </button>
          )}
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="tabs">
          {isManager && (
            <button className={`tab ${activeTab === 'all' ? 'active' : ''}`} onClick={() => setActiveTab('all')}>
              Todas las evaluaciones ({evaluations.length})
            </button>
          )}
          <button className={`tab ${activeTab === 'my' ? 'active' : ''}`} onClick={() => setActiveTab('my')}>
            Mis evaluaciones ({myEvaluations.length})
          </button>
        </div>

        {/* Tab: Todas */}
        {activeTab === 'all' && isManager && (
          <>
            <div className="filters-bar">
              <select value={filters.employeeId} onChange={e => setFilters({ ...filters, employeeId: e.target.value })}>
                <option value="">Todos los empleados</option>
                {employees.map(e => <option key={e.id} value={e.id}>{e.fullName || `${e.firstName} ${e.lastName}`}</option>)}
              </select>
              <input type="number" placeholder="Año" value={filters.year}
                onChange={e => setFilters({ ...filters, year: e.target.value })}
                min="2020" max="2030" style={{ width: '100px' }} />
              <select value={filters.status} onChange={e => setFilters({ ...filters, status: e.target.value })}>
                <option value="">Todos los estados</option>
                <option value="Completada">Completada</option>
                <option value="RevisadaPorEmpleado">Revisada por empleado</option>
              </select>
              <button className="btn btn-primary" onClick={handleFilter}>Filtrar</button>
            </div>
            {renderTable(evaluations, true)}
          </>
        )}

        {/* Tab: Mis evaluaciones */}
        {activeTab === 'my' && renderTable(myEvaluations, false)}

        {/* Modal Crear */}
        {showCreate && (
          <div className="modal-overlay" onClick={() => setShowCreate(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Nueva Evaluación de Desempeño</h2>
                <button className="close-btn" onClick={() => setShowCreate(false)}>&times;</button>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Empleado *</label>
                  <select value={form.employeeId} onChange={e => setForm({ ...form, employeeId: e.target.value })}>
                    <option value="">Seleccione...</option>
                    {employees.map(emp => (
                      <option key={emp.id} value={emp.id}>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Fecha de Evaluación *</label>
                  <input type="date" value={form.evaluationDate}
                    onChange={e => setForm({ ...form, evaluationDate: e.target.value })} />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Inicio Período</label>
                  <input type="date" value={form.periodStartDate}
                    onChange={e => setForm({ ...form, periodStartDate: e.target.value })} />
                </div>
                <div className="form-group">
                  <label>Fin Período</label>
                  <input type="date" value={form.periodEndDate}
                    onChange={e => setForm({ ...form, periodEndDate: e.target.value })} />
                </div>
              </div>
              <div className="form-group">
                <label>Puntuación (3-10) *</label>
                <div className="score-input">
                  <input type="range" min="3" max="10" value={form.score}
                    onChange={e => setForm({ ...form, score: parseInt(e.target.value) })} />
                  <span className={`badge ${SCORE_COLORS[form.score] || 'badge-secondary'} score-badge`}>
                    {form.score}/10
                  </span>
                </div>
              </div>
              <div className="form-group">
                <label>Comentarios generales</label>
                <textarea rows="3" value={form.comments}
                  onChange={e => setForm({ ...form, comments: e.target.value })}
                  placeholder="Observaciones sobre el desempeño..." />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Fortalezas</label>
                  <textarea rows="2" value={form.strengths}
                    onChange={e => setForm({ ...form, strengths: e.target.value })}
                    placeholder="Puntos fuertes del empleado..." />
                </div>
                <div className="form-group">
                  <label>Áreas a mejorar</label>
                  <textarea rows="2" value={form.areasToImprove}
                    onChange={e => setForm({ ...form, areasToImprove: e.target.value })}
                    placeholder="Oportunidades de mejora..." />
                </div>
              </div>
              <div className="form-group">
                <label>Objetivos para el siguiente período</label>
                <textarea rows="2" value={form.goals}
                  onChange={e => setForm({ ...form, goals: e.target.value })}
                  placeholder="Metas y objetivos..." />
              </div>
              {error && <div className="alert alert-error">{error}</div>}
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowCreate(false)}>Cancelar</button>
                <button className="btn btn-primary" onClick={handleCreate} disabled={saving}>
                  {saving ? 'Guardando...' : 'Guardar Evaluación'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Modal Detalle */}
        {showDetail && selected && (
          <div className="modal-overlay" onClick={() => setShowDetail(false)}>
            <div className="modal modal-lg" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Evaluación — {selected.employeeName}</h2>
                <button className="close-btn" onClick={() => setShowDetail(false)}>&times;</button>
              </div>
              <div className="eval-detail-header">
                <div className="eval-score-circle">
                  <span className="score-number">{selected.score}</span>
                  <span className="score-max">/10</span>
                </div>
                <div>
                  <p className="score-label-text">{selected.scoreLabel}</p>
                  <p className="eval-meta">Evaluado por: <strong>{selected.evaluatorName}</strong> el {formatDate(selected.evaluationDate)}</p>
                  {selected.periodStartDate && (
                    <p className="eval-meta">Período: {formatDate(selected.periodStartDate)} — {formatDate(selected.periodEndDate)}</p>
                  )}
                </div>
              </div>
              {selected.comments && (
                <div className="eval-section">
                  <h4>Comentarios generales</h4>
                  <p>{selected.comments}</p>
                </div>
              )}
              {selected.strengths && (
                <div className="eval-section strengths">
                  <h4>Fortalezas</h4>
                  <p>{selected.strengths}</p>
                </div>
              )}
              {selected.areasToImprove && (
                <div className="eval-section improve">
                  <h4>Áreas a mejorar</h4>
                  <p>{selected.areasToImprove}</p>
                </div>
              )}
              {selected.goals && (
                <div className="eval-section goals">
                  <h4>Objetivos</h4>
                  <p>{selected.goals}</p>
                </div>
              )}
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetail(false)}>Cerrar</button>
                {selected.status === 'Completada' && !selected.employeeAcknowledged &&
                  user?.employeeId === selected.employeeId && (
                  <button className="btn btn-primary" onClick={() => handleAcknowledge(selected.id)}>
                    Confirmar lectura
                  </button>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default PerformanceEval;
