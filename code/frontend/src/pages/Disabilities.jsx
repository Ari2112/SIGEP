import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { disabilityAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './Disabilities.css';

const API_BASE = 'http://localhost:5017';

const STATUS_BADGE = {
  'Pendiente': 'badge-warning',
  'Aprobada':  'badge-success',
  'Rechazada': 'badge-danger',
};

function Disabilities() {
  const { user } = useAuth();

  const [myRequests, setMyRequests]     = useState([]);
  const [pendingRequests, setPending]   = useState([]);
  const [allRequests, setAllRequests]   = useState([]);
  const [employees, setEmployees]       = useState([]);
  const [disabilityTypes, setTypes]     = useState([]);

  const [activeTab, setActiveTab]   = useState('my');
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState('');
  const [successMsg, setSuccess]    = useState('');
  const [submitting, setSubmitting] = useState(false);

  const [showCreate, setShowCreate] = useState(false);
  const [showReview, setShowReview] = useState(false);
  const [selected, setSelected]     = useState(null);
  const [reviewApprove, setReviewApprove]   = useState(true);
  const [reviewComments, setReviewComments] = useState('');

  const [filters, setFilters] = useState({ employeeId: '', status: '', dateFrom: '', dateTo: '' });

  // Formulario simplificado
  const [form, setForm] = useState({
    startDate: '', endDate: '', type: 1, reason: '', attachmentPath: ''
  });
  const [uploadFile, setUploadFile]       = useState(null);
  const [uploadPreview, setUploadPreview] = useState('');
  const [uploading, setUploading]         = useState(false);

  const isManager = ['Admin', 'Administrador', 'RRHH'].includes(user?.role);

  useEffect(() => { loadData(); }, []);

  // ── Carga ──────────────────────────────────────────────────────────
  const loadData = async () => {
    try {
      setLoading(true);
      setError('');

      const token = localStorage.getItem('token');
      
      // Cargar tipos (tolerante a fallos — usa fallback si el endpoint no existe)
      let typesData = [];
      try {
        const typesRes = await fetch(`${API_BASE}/api/v1/disability/types`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        if (typesRes.ok) {
          const json = await typesRes.json();
          typesData = Array.isArray(json) ? json : [];
        }
      } catch (_) {}
      setTypes(typesData);

      const myRes = await disabilityAPI.getMy();
      setMyRequests(myRes.data || []);

      if (isManager) {
        const [pendRes, empRes] = await Promise.all([
          disabilityAPI.getAll({ status: 'Pendiente' }),
          employeeAPI.getAll()
        ]);
        setPending(pendRes.data || []);
        setEmployees(empRes.data || []);
      }
    } catch (err) {
      setError('Error al cargar datos');
    } finally {
      setLoading(false);
    }
  };

  const loadAll = async () => {
    try {
      setLoading(true);
      const params = {};
      if (filters.employeeId) params.employeeId = filters.employeeId;
      if (filters.status)     params.status     = filters.status;
      if (filters.dateFrom)   params.dateFrom   = filters.dateFrom;
      if (filters.dateTo)     params.dateTo     = filters.dateTo;
      const res = await disabilityAPI.getAll(params);
      setAllRequests(res.data || []);
    } catch { setError('Error al filtrar'); }
    finally { setLoading(false); }
  };

  // ── Helpers ────────────────────────────────────────────────────────
  const showSuccessMsg = (msg) => {
    setSuccess(msg);
    setTimeout(() => setSuccess(''), 4000);
  };

  const fmt = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  const calcDays = (start, end) => {
    if (!start || !end) return 0;
    return Math.max(1, Math.round((new Date(end) - new Date(start)) / 86400000) + 1);
  };

  const resetForm = () => {
    setForm({ startDate:'', endDate:'', type:1, reason:'', attachmentPath:'' });
    setUploadFile(null);
    setUploadPreview('');
  };

  // ── Subida de archivo ──────────────────────────────────────────────
  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const allowed = ['application/pdf', 'image/jpeg', 'image/jpg', 'image/png'];
    if (!allowed.includes(file.type)) { setError('Solo se permiten PDF, JPG o PNG'); return; }
    if (file.size > 5 * 1024 * 1024) { setError('El archivo no puede superar 5MB'); return; }

    setUploadFile(file);
    setError('');

    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (ev) => setUploadPreview(ev.target.result);
      reader.readAsDataURL(file);
    } else {
      setUploadPreview('pdf');
    }
  };

  const uploadDocument = async () => {
    if (!uploadFile) return null;
    try {
      setUploading(true);
      const fd = new FormData();
      fd.append('file', uploadFile);
      const token = localStorage.getItem('token');
      const res = await fetch(`${API_BASE}/api/v1/disability/upload`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        body: fd
      });
      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || 'Error al subir archivo');
      }
      const data = await res.json();
      return data.path;
    } finally {
      setUploading(false);
    }
  };

  // ── Registrar ──────────────────────────────────────────────────────
  const handleCreate = async (e) => {
    e.preventDefault();
    if (!form.startDate || !form.endDate) { setError('Las fechas son requeridas'); return; }
    if (new Date(form.endDate) < new Date(form.startDate)) {
      setError('La fecha de fin no puede ser anterior a la fecha de inicio');
      return;
    }
    if (!form.reason.trim()) { setError('Debe describir el motivo de la incapacidad'); return; }

    try {
      setSubmitting(true);
      setError('');

      let attachmentPath = null;
      if (uploadFile) {
        attachmentPath = await uploadDocument();
      }

      await disabilityAPI.create({
        startDate:      form.startDate,
        endDate:        form.endDate,
        type:           parseInt(form.type),
        diagnosis:      form.reason,   // usamos reason como diagnosis internamente
        doctorName:     null,
        medicalCenter:  null,
        documentNumber: null,
        attachmentPath: attachmentPath
      });

      setShowCreate(false);
      resetForm();
      showSuccessMsg('Incapacidad registrada correctamente. RRHH será notificado.');
      loadData();
    } catch (err) {
      setError(err.message || err.response?.data?.message || 'Error al registrar');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Revisar ────────────────────────────────────────────────────────
  const openReview = (req, approve) => {
    setSelected(req);
    setReviewApprove(approve);
    setReviewComments('');
    setShowReview(true);
  };

  const handleReview = async () => {
    if (!reviewApprove && !reviewComments.trim()) {
      setError('Debe proporcionar un motivo de rechazo');
      return;
    }
    try {
      setSubmitting(true);
      setError('');
      await disabilityAPI.review(selected.id, reviewApprove, reviewComments);
      setShowReview(false);
      showSuccessMsg(`Incapacidad ${reviewApprove ? 'aprobada' : 'rechazada'} correctamente`);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al procesar');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Render ─────────────────────────────────────────────────────────
  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="disabilities-container">

        <div className="page-header">
          <div>
            <h1>Incapacidades</h1>
            <p className="page-subtitle">Registro y gestión de incapacidades médicas</p>
          </div>
          <button className="btn btn-primary"
            onClick={() => { resetForm(); setError(''); setShowCreate(true); }}>
            + Registrar Incapacidad
          </button>
        </div>

        {error      && <div className="alert alert-error"   onClick={() => setError('')}>{error}</div>}
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

        {/* Tabs */}
        <div className="tabs">
          <button className={`tab ${activeTab === 'my' ? 'active' : ''}`}
            onClick={() => setActiveTab('my')}>
            Mis Incapacidades ({myRequests.length})
          </button>
          {isManager && (
            <>
              <button className={`tab ${activeTab === 'pending' ? 'active' : ''}`}
                onClick={() => setActiveTab('pending')}>
                Pendientes
                {pendingRequests.length > 0 && (
                  <span className="badge badge-warning" style={{ marginLeft:6 }}>
                    {pendingRequests.length}
                  </span>
                )}
              </button>
              <button className={`tab ${activeTab === 'all' ? 'active' : ''}`}
                onClick={() => { setActiveTab('all'); loadAll(); }}>
                Historial
              </button>
            </>
          )}
        </div>

        {/* ── Mis incapacidades ── */}
        {activeTab === 'my' && (
          <div className="table-card">
            {myRequests.length === 0 ? (
              <p className="no-data">No tiene incapacidades registradas</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Tipo</th>
                    <th>Desde</th>
                    <th>Hasta</th>
                    <th>Días</th>
                    <th>Motivo</th>
                    <th>Documento</th>
                    <th>Estado</th>
                    <th>Observación RRHH</th>
                  </tr>
                </thead>
                <tbody>
                  {myRequests.map(r => (
                    <tr key={r.id}>
                      <td><span className="badge badge-info">{r.type}</span></td>
                      <td>{fmt(r.startDate)}</td>
                      <td>{fmt(r.endDate)}</td>
                      <td><strong>{r.totalDays}</strong></td>
                      <td>{r.diagnosis || '-'}</td>
                      <td>
                        {r.attachmentPath
                          ? <a href={`${API_BASE}${r.attachmentPath}`} target="_blank" rel="noreferrer"
                              className="btn btn-sm btn-secondary" style={{ fontSize:'0.78rem' }}>
                              📎 Ver doc.
                            </a>
                          : <span style={{ color:'#aaa', fontSize:'0.82rem' }}>Sin adjunto</span>
                        }
                      </td>
                      <td>
                        <span className={`badge ${STATUS_BADGE[r.status] || 'badge-secondary'}`}>
                          {r.status}
                        </span>
                      </td>
                      <td style={{ fontSize:'0.82rem', color:'#555' }}>
                        {r.reviewComments || '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* ── Pendientes ── */}
        {activeTab === 'pending' && isManager && (
          <div className="table-card">
            {pendingRequests.length === 0 ? (
              <p className="no-data">No hay incapacidades pendientes de revisión</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Empleado</th>
                    <th>Tipo</th>
                    <th>Desde</th>
                    <th>Hasta</th>
                    <th>Días</th>
                    <th>Motivo</th>
                    <th>Documento</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {pendingRequests.map(r => (
                    <tr key={r.id}>
                      <td><strong>{r.employeeName}</strong></td>
                      <td><span className="badge badge-info">{r.type}</span></td>
                      <td>{fmt(r.startDate)}</td>
                      <td>{fmt(r.endDate)}</td>
                      <td><strong>{r.totalDays}</strong></td>
                      <td>{r.diagnosis || '-'}</td>
                      <td>
                        {r.attachmentPath
                          ? <a href={`${API_BASE}${r.attachmentPath}`} target="_blank" rel="noreferrer"
                              className="btn btn-sm btn-secondary" style={{ fontSize:'0.78rem' }}>
                              📎 Ver doc.
                            </a>
                          : <span style={{ color:'#aaa', fontSize:'0.82rem' }}>Sin adjunto</span>
                        }
                      </td>
                      <td style={{ display:'flex', gap:6 }}>
                        <button className="btn btn-sm btn-success"
                          onClick={() => openReview(r, true)}>Aprobar</button>
                        <button className="btn btn-sm btn-danger"
                          onClick={() => openReview(r, false)}>Rechazar</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* ── Historial ── */}
        {activeTab === 'all' && isManager && (
          <div className="table-card">
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
              <select value={filters.status}
                onChange={e => setFilters({...filters, status: e.target.value})}>
                <option value="">Todos los estados</option>
                <option value="Pendiente">Pendiente</option>
                <option value="Aprobada">Aprobada</option>
                <option value="Rechazada">Rechazada</option>
              </select>
              <input type="date" value={filters.dateFrom}
                onChange={e => setFilters({...filters, dateFrom: e.target.value})} />
              <input type="date" value={filters.dateTo}
                onChange={e => setFilters({...filters, dateTo: e.target.value})} />
              <button className="btn btn-primary" onClick={loadAll}>Buscar</button>
            </div>

            {allRequests.length === 0 ? (
              <p className="no-data">Aplique filtros y presione Buscar</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Empleado</th>
                    <th>Tipo</th>
                    <th>Desde</th>
                    <th>Hasta</th>
                    <th>Días</th>
                    <th>Estado</th>
                    <th>Documento</th>
                    <th>Revisado por</th>
                  </tr>
                </thead>
                <tbody>
                  {allRequests.map(r => (
                    <tr key={r.id}>
                      <td><strong>{r.employeeName}</strong></td>
                      <td><span className="badge badge-info">{r.type}</span></td>
                      <td>{fmt(r.startDate)}</td>
                      <td>{fmt(r.endDate)}</td>
                      <td>{r.totalDays}</td>
                      <td>
                        <span className={`badge ${STATUS_BADGE[r.status] || 'badge-secondary'}`}>
                          {r.status}
                        </span>
                      </td>
                      <td>
                        {r.attachmentPath
                          ? <a href={`${API_BASE}${r.attachmentPath}`} target="_blank" rel="noreferrer"
                              className="btn btn-sm btn-secondary" style={{ fontSize:'0.78rem' }}>
                              📎 Ver
                            </a>
                          : '-'
                        }
                      </td>
                      <td style={{ fontSize:'0.85rem' }}>{r.reviewedByName || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* ══════════ MODAL: Registrar ══════════ */}
        {showCreate && (
          <div className="modal-overlay" onClick={() => setShowCreate(false)}>
            <div className="modal" style={{ maxWidth:520 }} onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Registrar Incapacidad</h2>
                <button className="close-btn" onClick={() => setShowCreate(false)}>&times;</button>
              </div>



              <form onSubmit={handleCreate}>

                {/* Tipo */}
                <div className="form-group">
                  <label>Tipo de incapacidad *</label>
                  <select value={form.type}
                    onChange={e => setForm({...form, type: e.target.value})} required>
                    {disabilityTypes.length > 0
                      ? disabilityTypes.map(t => (
                          <option key={t.id} value={t.id}>{t.name}</option>
                        ))
                      : <>
                          <option value={1}>Enfermedad común</option>
                          <option value={2}>Accidente laboral</option>
                          <option value={3}>Maternidad / Lactancia</option>
                          <option value={4}>Incapacidad CCSS</option>
                          <option value={5}>Centro médico privado</option>
                          <option value={6}>Otro</option>
                        </>
                    }
                  </select>
                </div>

                {/* Fechas */}
                <div style={{ display:'grid', gridTemplateColumns:'1fr 1fr', gap:12 }}>
                  <div className="form-group">
                    <label>Fecha inicio *</label>
                    <input type="date" value={form.startDate} required
                      onChange={e => setForm({...form, startDate: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label>Fecha fin *</label>
                    <input type="date" value={form.endDate} required
                      min={form.startDate}
                      onChange={e => setForm({...form, endDate: e.target.value})} />
                  </div>
                </div>

                {form.startDate && form.endDate && (
                  <p style={{ fontSize:'0.88rem', color:'#2d5a1b', marginBottom:12 }}>
                    Duración: <strong>{calcDays(form.startDate, form.endDate)} días naturales</strong>
                  </p>
                )}

                {/* Motivo */}
                <div className="form-group">
                  <label>Motivo *</label>
                  <textarea
                    value={form.reason}
                    onChange={e => setForm({...form, reason: e.target.value})}
                    rows={3}
                    placeholder="Describa brevemente el motivo de la incapacidad..."
                    required
                  />
                </div>

                {/* Documento adjunto */}
                <div className="form-group">
                  <label>
                    Documento de incapacidad *
                    <span style={{ color:'#888', fontWeight:'normal', fontSize:'0.82rem' }}>
                      {' '}(PDF, JPG o PNG — máx. 5MB)
                    </span>
                  </label>
                  <input type="file" accept=".pdf,.jpg,.jpeg,.png"
                    onChange={handleFileChange} />

                  {uploadPreview && uploadPreview !== 'pdf' && (
                    <div style={{ marginTop:10 }}>
                      <img src={uploadPreview} alt="Vista previa"
                        style={{
                          maxWidth:'100%', maxHeight:180,
                          borderRadius:8, border:'1px solid #ddd'
                        }} />
                    </div>
                  )}
                  {uploadPreview === 'pdf' && (
                    <div style={{
                      marginTop:10, padding:'10px 14px', background:'#fff8e1',
                      borderRadius:8, border:'1px solid #ffc107', fontSize:'0.88rem'
                    }}>
                      📄 PDF seleccionado: <strong>{uploadFile?.name}</strong>
                    </div>
                  )}
                </div>

                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary"
                    onClick={() => setShowCreate(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn btn-primary"
                    disabled={submitting || uploading}>
                    {uploading ? 'Subiendo documento...'
                      : submitting ? 'Registrando...'
                      : 'Registrar Incapacidad'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ══════════ MODAL: Aprobar / Rechazar ══════════ */}
        {showReview && selected && (
          <div className="modal-overlay" onClick={() => setShowReview(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>{reviewApprove ? '✅ Aprobar' : '❌ Rechazar'} Incapacidad</h2>
                <button className="close-btn" onClick={() => setShowReview(false)}>&times;</button>
              </div>

              <div className="info-box">
                <p><strong>Empleado:</strong> {selected.employeeName}</p>
                <p><strong>Tipo:</strong> {selected.type}</p>
                <p>
                  <strong>Período:</strong> {fmt(selected.startDate)} — {fmt(selected.endDate)}{' '}
                  (<strong>{selected.totalDays} días</strong>)
                </p>
                {selected.diagnosis && (
                  <p><strong>Motivo:</strong> {selected.diagnosis}</p>
                )}
                {selected.attachmentPath && (
                  <p>
                    <strong>Documento:</strong>{' '}
                    <a href={`${API_BASE}${selected.attachmentPath}`}
                      target="_blank" rel="noreferrer">
                      📎 Ver documento adjunto
                    </a>
                  </p>
                )}
              </div>

              {reviewApprove && (
                <div style={{
                  background:'#e8f4fd', border:'1px solid #3498db',
                  borderRadius:8, padding:'10px 14px', marginBottom:12, fontSize:'0.85rem'
                }}>
                  Al aprobar, esta incapacidad se integrará a la planilla del empleado en el próximo período.
                </div>
              )}

              <div className="form-group">
                <label>
                  {reviewApprove ? 'Observaciones (opcional)' : 'Motivo del rechazo *'}
                </label>
                <textarea
                  value={reviewComments}
                  onChange={e => setReviewComments(e.target.value)}
                  rows={3}
                  placeholder={reviewApprove
                    ? 'Comentarios adicionales...'
                    : 'Indique el motivo del rechazo...'}
                  autoFocus={!reviewApprove}
                />
                {!reviewApprove && !reviewComments.trim() && (
                  <small style={{ color:'#e74c3c' }}>
                    Este campo es obligatorio para rechazar
                  </small>
                )}
              </div>

              <div className="modal-actions">
                <button className="btn btn-secondary"
                  onClick={() => setShowReview(false)}>Cancelar</button>
                <button
                  className={`btn ${reviewApprove ? 'btn-success' : 'btn-danger'}`}
                  onClick={handleReview}
                  disabled={submitting || (!reviewApprove && !reviewComments.trim())}
                >
                  {submitting ? 'Procesando...'
                    : reviewApprove ? 'Confirmar Aprobación' : 'Confirmar Rechazo'}
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