import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { evaluationAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './PerformanceEval.css';

// Escala de puntuación 1-10
const SCORE_INFO = (score) => {
  if (score >= 9)  return { label: 'Excelente',    color: '#1a6b1a', bg: '#e8f5e9', badge: 'badge-success' };
  if (score >= 7)  return { label: 'Bueno',         color: '#2d6a4f', bg: '#d8f3dc', badge: 'badge-success' };
  if (score >= 5)  return { label: 'Aceptable',     color: '#e67e22', bg: '#fff3e0', badge: 'badge-warning' };
  if (score >= 3)  return { label: 'Por mejorar',   color: '#c0392b', bg: '#fdecea', badge: 'badge-danger'  };
  return               { label: 'Insatisfactorio', color: '#922b21', bg: '#fdecea', badge: 'badge-danger'  };
};

const STATUS_BADGE = {
  'Borrador':            'badge-secondary',
  'Completada':          'badge-success',
  'RevisadaPorEmpleado': 'badge-info',
};
const STATUS_LABEL = {
  'Borrador':            'Borrador',
  'Completada':          'Completada',
  'RevisadaPorEmpleado': 'Revisada por empleado',
};

const EMPTY_FORM = {
  employeeId: '', evaluationDate: new Date().toISOString().split('T')[0],
  periodStartDate: '', periodEndDate: '',
  score: 7, comments: '', strengths: '', areasToImprove: '', goals: ''
};

function PerformanceEval() {
  const { user } = useAuth();

  const [evaluations, setEvaluations]   = useState([]);
  const [myEvaluations, setMyEvals]     = useState([]);
  const [employees, setEmployees]       = useState([]);
  const [selected, setSelected]         = useState(null);

  const [activeTab, setActiveTab] = useState('all');
  const [loading, setLoading]     = useState(true);
  const [error, setError]         = useState('');
  const [successMsg, setSuccess]  = useState('');
  const [submitting, setSubmitting] = useState(false);

  const [showCreate, setShowCreate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);

  const [filters, setFilters] = useState({ employeeId: '', year: new Date().getFullYear(), status: '' });
  const [form, setForm]       = useState(EMPTY_FORM);

  const isManager = ['Admin', 'Administrador', 'RRHH', 'Jefatura'].includes(user?.role);

  useEffect(() => {
    loadData();
    if (!isManager) setActiveTab('my');
  }, []);

  // ── Carga ──────────────────────────────────────────────────────────
  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      const myRes = await evaluationAPI.getMy();
      setMyEvals(myRes.data || []);

      if (isManager) {
        const [allRes, empRes] = await Promise.all([
          evaluationAPI.getAll(),
          employeeAPI.getAll()
        ]);
        setEvaluations(allRes.data || []);
        setEmployees(empRes.data || []);
      }
    } catch (err) {
      setError('Error al cargar datos');
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = async () => {
    try {
      setLoading(true);
      const params = {};
      if (filters.employeeId) params.employeeId = filters.employeeId;
      if (filters.year)       params.year       = filters.year;
      if (filters.status)     params.status     = filters.status;
      const res = await evaluationAPI.getAll(params);
      setEvaluations(res.data || []);
    } catch { setError('Error al filtrar'); }
    finally { setLoading(false); }
  };

  // ── Helpers ────────────────────────────────────────────────────────
  const showSuccessMsg = (msg) => {
    setSuccess(msg);
    setTimeout(() => setSuccess(''), 4000);
  };

  const fmt = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  // Resumen estadístico
  const getStats = (evals) => {
    if (!evals.length) return null;
    const avg    = (evals.reduce((s, e) => s + e.score, 0) / evals.length).toFixed(1);
    const excellent = evals.filter(e => e.score >= 9).length;
    const good      = evals.filter(e => e.score >= 7 && e.score < 9).length;
    const needsWork = evals.filter(e => e.score < 5).length;
    return { avg, excellent, good, needsWork, total: evals.length };
  };

  // ── Crear evaluación ───────────────────────────────────────────────
  const handleCreate = async (e) => {
    e.preventDefault();
    if (!form.employeeId) { setError('Seleccione un empleado'); return; }
    try {
      setSubmitting(true);
      setError('');
      await evaluationAPI.create({
        ...form,
        employeeId:      parseInt(form.employeeId),
        score:           parseInt(form.score),
        periodStartDate: form.periodStartDate || null,
        periodEndDate:   form.periodEndDate   || null,
      });
      setShowCreate(false);
      setForm(EMPTY_FORM);
      showSuccessMsg('Evaluación registrada. RRHH fue notificado.');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al guardar');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Ver detalle ────────────────────────────────────────────────────
  const openDetail = async (id) => {
    try {
      const res = await evaluationAPI.getById(id);
      setSelected(res.data);
      setShowDetail(true);
    } catch { setError('Error al cargar detalle'); }
  };

  // ── Confirmar lectura (empleado) ───────────────────────────────────
  const handleAcknowledge = async (id) => {
    try {
      await evaluationAPI.acknowledge(id);
      showSuccessMsg('Evaluación confirmada correctamente');
      setShowDetail(false);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al confirmar');
    }
  };

  // ── Render tabla ───────────────────────────────────────────────────
  const renderTable = (evals, showEmployee = true) => {
    const stats = getStats(evals);
    return (
      <>
        {/* Resumen estadístico */}
        {stats && isManager && (
          <div style={{ display:'grid', gridTemplateColumns:'repeat(4,1fr)', gap:12, marginBottom:16 }}>
            <div style={{ background:'#f8f9fa', borderRadius:10, padding:'14px 18px', textAlign:'center', border:'1px solid #e0e0e0' }}>
              <div style={{ fontSize:'1.8rem', fontWeight:700, color:'#2d5a1b' }}>{stats.avg}</div>
              <div style={{ fontSize:'0.82rem', color:'#666' }}>Promedio general</div>
            </div>
            <div style={{ background:'#e8f5e9', borderRadius:10, padding:'14px 18px', textAlign:'center', border:'1px solid #a5d6a7' }}>
              <div style={{ fontSize:'1.8rem', fontWeight:700, color:'#1a6b1a' }}>{stats.excellent}</div>
              <div style={{ fontSize:'0.82rem', color:'#2d6a4f' }}>Excelentes (9-10)</div>
            </div>
            <div style={{ background:'#fff8e1', borderRadius:10, padding:'14px 18px', textAlign:'center', border:'1px solid #ffe082' }}>
              <div style={{ fontSize:'1.8rem', fontWeight:700, color:'#e67e22' }}>{stats.good}</div>
              <div style={{ fontSize:'0.82rem', color:'#7d6608' }}>Buenos (7-8)</div>
            </div>
            <div style={{ background:'#fdecea', borderRadius:10, padding:'14px 18px', textAlign:'center', border:'1px solid #ef9a9a' }}>
              <div style={{ fontSize:'1.8rem', fontWeight:700, color:'#c0392b' }}>{stats.needsWork}</div>
              <div style={{ fontSize:'0.82rem', color:'#922b21' }}>Por mejorar (&lt;5)</div>
            </div>
          </div>
        )}

        <div className="table-card">
          {evals.length === 0 ? (
            <p className="no-data">No hay evaluaciones registradas</p>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  {showEmployee && <th>Empleado</th>}
                  {showEmployee && <th>Puesto</th>}
                  <th>Fecha</th>
                  <th>Puntuación</th>
                  <th>Calificación</th>
                  {showEmployee && <th>Evaluador</th>}
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {evals.map(ev => {
                  const info = SCORE_INFO(ev.score);
                  return (
                    <tr key={ev.id}>
                      {showEmployee && <td><strong>{ev.employeeName}</strong></td>}
                      {showEmployee && <td style={{ color:'#666', fontSize:'0.88rem' }}>{ev.positionName || '-'}</td>}
                      <td>{fmt(ev.evaluationDate)}</td>
                      <td>
                        <span style={{
                          background: info.bg, color: info.color,
                          padding:'3px 10px', borderRadius:20,
                          fontWeight:700, fontSize:'0.88rem'
                        }}>
                          {ev.score}/10
                        </span>
                      </td>
                      <td style={{ color: info.color, fontWeight:500, fontSize:'0.88rem' }}>
                        {ev.scoreLabel || info.label}
                      </td>
                      {showEmployee && <td style={{ fontSize:'0.88rem' }}>{ev.evaluatorName}</td>}
                      <td>
                        <span className={`badge ${STATUS_BADGE[ev.status] || 'badge-secondary'}`}>
                          {STATUS_LABEL[ev.status] || ev.status}
                        </span>
                      </td>
                      <td>
                        <button className="btn btn-sm btn-secondary"
                          onClick={() => openDetail(ev.id)}>
                          Ver detalle
                        </button>
                        {!showEmployee && ev.status === 'Completada' && !ev.employeeAcknowledged && (
                          <button className="btn btn-sm btn-primary" style={{ marginLeft:6 }}
                            onClick={() => handleAcknowledge(ev.id)}>
                            Confirmar
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      </>
    );
  };

  // ── Render principal ───────────────────────────────────────────────
  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="eval-container">

        <div className="page-header">
          <div>
            <h1>Evaluación de Desempeño</h1>
            <p className="page-subtitle">Registro, seguimiento y análisis del desempeño del personal</p>
          </div>
          {isManager && (
            <button className="btn btn-primary"
              onClick={() => { setForm(EMPTY_FORM); setError(''); setShowCreate(true); }}>
              + Nueva Evaluación
            </button>
          )}
        </div>

        {error      && <div className="alert alert-error"   onClick={() => setError('')}>{error}</div>}
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

        {/* Tabs */}
        <div className="tabs">
          {isManager && (
            <button className={`tab ${activeTab === 'all' ? 'active' : ''}`}
              onClick={() => setActiveTab('all')}>
              Todas las evaluaciones ({evaluations.length})
            </button>
          )}
          <button className={`tab ${activeTab === 'my' ? 'active' : ''}`}
            onClick={() => setActiveTab('my')}>
            Mis evaluaciones ({myEvaluations.length})
            {myEvaluations.some(e => e.status === 'Completada' && !e.employeeAcknowledged) && (
              <span className="badge badge-warning" style={{ marginLeft:6 }}>!</span>
            )}
          </button>
        </div>

        {/* Tab: Todas */}
        {activeTab === 'all' && isManager && (
          <>
            <div className="filters-bar" style={{ flexWrap:'wrap', gap:8, marginBottom:16 }}>
              <select value={filters.employeeId}
                onChange={e => setFilters({...filters, employeeId: e.target.value})}>
                <option value="">Todos los empleados</option>
                {employees.map(e => (
                  <option key={e.id} value={e.id}>
                    {e.fullName || `${e.firstName} ${e.lastName}`}
                  </option>
                ))}
              </select>
              <input type="number" placeholder="Año" value={filters.year}
                min="2020" max="2030" style={{ width:90 }}
                onChange={e => setFilters({...filters, year: e.target.value})} />
              <select value={filters.status}
                onChange={e => setFilters({...filters, status: e.target.value})}>
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
        {activeTab === 'my' && (
          <>
            {myEvaluations.some(e => e.status === 'Completada' && !e.employeeAcknowledged) && (
              <div style={{
                background:'#fff8e1', border:'1px solid #ffc107',
                borderRadius:8, padding:'10px 16px', marginBottom:16, fontSize:'0.9rem'
              }}>
                ⚠ Tiene evaluaciones pendientes de confirmar. Por favor revíselas y presione <strong>Confirmar</strong>.
              </div>
            )}
            {renderTable(myEvaluations, false)}
          </>
        )}

        {/* ══════════ MODAL: Nueva evaluación ══════════ */}
        {showCreate && (
          <div className="modal-overlay" onClick={() => setShowCreate(false)}>
            <div className="modal modal-lg" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Nueva Evaluación de Desempeño</h2>
                <button className="close-btn" onClick={() => setShowCreate(false)}>&times;</button>
              </div>

              <form onSubmit={handleCreate}>
                {/* Empleado y fecha */}
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <div className="form-group">
                    <label>Empleado *</label>
                    <select value={form.employeeId} required
                      onChange={e => setForm({...form, employeeId: e.target.value})}>
                      <option value="">Seleccione un empleado...</option>
                      {employees.map(emp => (
                        <option key={emp.id} value={emp.id}>
                          {emp.fullName || `${emp.firstName} ${emp.lastName}`}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Fecha de evaluación *</label>
                    <input type="date" value={form.evaluationDate} required
                      onChange={e => setForm({...form, evaluationDate: e.target.value})} />
                  </div>
                </div>

                {/* Período */}
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <div className="form-group">
                    <label>Inicio del período evaluado</label>
                    <input type="date" value={form.periodStartDate}
                      onChange={e => setForm({...form, periodStartDate: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label>Fin del período evaluado</label>
                    <input type="date" value={form.periodEndDate}
                      min={form.periodStartDate}
                      onChange={e => setForm({...form, periodEndDate: e.target.value})} />
                  </div>
                </div>

                {/* Puntuación */}
                <div className="form-group">
                  <label>Puntuación general (1 - 10) *</label>
                  <div className="score-input">
                    <input type="range" min="1" max="10" value={form.score}
                      onChange={e => setForm({...form, score: parseInt(e.target.value)})} />
                    <span style={{
                      background: SCORE_INFO(form.score).bg,
                      color: SCORE_INFO(form.score).color,
                      padding:'4px 14px', borderRadius:20,
                      fontWeight:700, fontSize:'1rem', minWidth:60, textAlign:'center'
                    }}>
                      {form.score}/10
                    </span>
                    <span style={{
                      color: SCORE_INFO(form.score).color,
                      fontWeight:500, fontSize:'0.9rem'
                    }}>
                      {SCORE_INFO(form.score).label}
                    </span>
                  </div>
                </div>

                {/* Comentarios */}
                <div className="form-group">
                  <label>Comentarios generales *</label>
                  <textarea rows={3} value={form.comments} required
                    placeholder="Observaciones sobre el desempeño general del empleado..."
                    onChange={e => setForm({...form, comments: e.target.value})} />
                </div>

                {/* Fortalezas y áreas */}
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <div className="form-group">
                    <label>Fortalezas destacadas</label>
                    <textarea rows={3} value={form.strengths}
                      placeholder="¿En qué aspectos destaca este empleado?"
                      onChange={e => setForm({...form, strengths: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label>Áreas a mejorar</label>
                    <textarea rows={3} value={form.areasToImprove}
                      placeholder="¿Qué aspectos debe trabajar para mejorar?"
                      onChange={e => setForm({...form, areasToImprove: e.target.value})} />
                  </div>
                </div>

                {/* Objetivos */}
                <div className="form-group">
                  <label>Objetivos para el próximo período</label>
                  <textarea rows={2} value={form.goals}
                    placeholder="Metas y compromisos para el siguiente período de evaluación..."
                    onChange={e => setForm({...form, goals: e.target.value})} />
                </div>

                {/* Nota bono */}
                {form.score >= 9 && (
                  <div style={{
                    background:'#e8f5e9', border:'1px solid #a5d6a7',
                    borderRadius:8, padding:'10px 14px', marginBottom:12, fontSize:'0.88rem', color:'#1a6b1a'
                  }}>
                    ⭐ Puntuación excelente — este empleado podría ser candidato para reconocimiento o bono de desempeño.
                  </div>
                )}
                {form.score < 5 && (
                  <div style={{
                    background:'#fdecea', border:'1px solid #ef9a9a',
                    borderRadius:8, padding:'10px 14px', marginBottom:12, fontSize:'0.88rem', color:'#c0392b'
                  }}>
                    ⚠ Puntuación baja — se recomienda plan de mejora y seguimiento con el empleado.
                  </div>
                )}

                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary"
                    onClick={() => setShowCreate(false)}>Cancelar</button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Guardando...' : 'Guardar Evaluación'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ══════════ MODAL: Detalle ══════════ */}
        {showDetail && selected && (() => {
          const info = SCORE_INFO(selected.score);
          return (
            <div className="modal-overlay" onClick={() => setShowDetail(false)}>
              <div className="modal modal-lg" onClick={e => e.stopPropagation()}>
                <div className="modal-header">
                  <h2>Evaluación de Desempeño</h2>
                  <button className="close-btn" onClick={() => setShowDetail(false)}>&times;</button>
                </div>

                {/* Cabecera con score */}
                <div className="eval-detail-header" style={{ background: info.bg }}>
                  <div className="eval-score-circle" style={{ background: info.color }}>
                    <span className="score-number">{selected.score}</span>
                    <span className="score-max">/10</span>
                  </div>
                  <div>
                    <p className="score-label-text" style={{ color: info.color }}>
                      {selected.scoreLabel || info.label}
                    </p>
                    <p className="eval-meta">
                      <strong>{selected.employeeName}</strong>
                      {selected.positionName && ` — ${selected.positionName}`}
                    </p>
                    <p className="eval-meta">
                      Evaluado por: <strong>{selected.evaluatorName}</strong> · {fmt(selected.evaluationDate)}
                    </p>
                    {selected.periodStartDate && (
                      <p className="eval-meta">
                        Período: {fmt(selected.periodStartDate)} — {fmt(selected.periodEndDate)}
                      </p>
                    )}
                    <span className={`badge ${STATUS_BADGE[selected.status] || 'badge-secondary'}`}
                      style={{ marginTop:6, display:'inline-block' }}>
                      {STATUS_LABEL[selected.status] || selected.status}
                    </span>
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
                    <h4>✅ Fortalezas destacadas</h4>
                    <p>{selected.strengths}</p>
                  </div>
                )}

                {selected.areasToImprove && (
                  <div className="eval-section improve">
                    <h4>📈 Áreas a mejorar</h4>
                    <p>{selected.areasToImprove}</p>
                  </div>
                )}

                {selected.goals && (
                  <div className="eval-section goals">
                    <h4>🎯 Objetivos para el próximo período</h4>
                    <p>{selected.goals}</p>
                  </div>
                )}

                {selected.employeeAcknowledged && (
                  <p style={{ fontSize:'0.85rem', color:'#666', marginTop:12 }}>
                    ✓ Confirmada por el empleado el {fmt(selected.acknowledgedAt)}
                  </p>
                )}

                <div className="modal-actions">
                  <button className="btn btn-secondary"
                    onClick={() => setShowDetail(false)}>Cerrar</button>
                  {selected.status === 'Completada' &&
                   !selected.employeeAcknowledged &&
                   user?.employeeId === selected.employeeId && (
                    <button className="btn btn-primary"
                      onClick={() => handleAcknowledge(selected.id)}>
                      ✓ Confirmar que he leído esta evaluación
                    </button>
                  )}
                </div>
              </div>
            </div>
          );
        })()}

      </div>
    </Layout>
  );
}

export default PerformanceEval;
