import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { vacationAPI } from '../api/api';
import Layout from '../components/Layout';
import './Vacations.css';

function Vacations() {
  const { user } = useAuth();

  // Datos
  const [balance, setBalance]               = useState(null);
  const [myRequests, setMyRequests]         = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);

  // UI
  const [activeTab, setActiveTab]           = useState('my-requests');
  const [loading, setLoading]               = useState(true);
  const [error, setError]                   = useState('');
  const [successMsg, setSuccessMsg]         = useState('');

  // Modales
  const [showNewModal, setShowNewModal]     = useState(false);
  const [showApproveModal, setShowApprove]  = useState(false);
  const [showRejectModal, setShowReject]    = useState(false);
  const [selectedRequest, setSelected]      = useState(null);

  // Formularios
  const [newRequest, setNewRequest]         = useState({ startDate: '', endDate: '', reason: '' });
  const [approveComments, setApproveComments] = useState('');
  const [rejectReason, setRejectReason]     = useState('');
  const [submitting, setSubmitting]         = useState(false);

  const isManager = ['Admin', 'Administrador', 'RRHH', 'Jefatura'].includes(user?.role);

  // ── Carga inicial ──────────────────────────────────────────────────
  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');

      const [balRes, reqRes] = await Promise.all([
        vacationAPI.getMyBalance(),
        vacationAPI.getMyRequests()
      ]);

      setBalance(balRes.data);
      setMyRequests(reqRes.data);

      if (isManager) {
        const pendRes = await vacationAPI.getPendingRequests();
        setPendingRequests(pendRes.data);
      }
    } catch (err) {
      setError('Error al cargar datos: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  // ── Helpers ────────────────────────────────────────────────────────
  const showSuccess = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 4000);
  };

  // El backend devuelve requestStatusName (no status)
  const getStatus = (req) => req.requestStatusName || req.status || '';

  const getStatusBadge = (statusName) => {
    const map = {
      'Pendiente':   'badge-warning',
      'Aprobada':    'badge-success',
      'Rechazada':   'badge-danger',
      'Cancelada':   'badge-secondary',
      'En Revision': 'badge-info',
      'En Revisión': 'badge-info',
    };
    return <span className={`badge ${map[statusName] || 'badge-secondary'}`}>{statusName}</span>;
  };

  const fmt = (dateStr) =>
    new Date(dateStr).toLocaleDateString('es-CR', {
      day: '2-digit', month: '2-digit', year: 'numeric'
    });

  const calcDays = (start, end) => {
    if (!start || !end) return 0;
    const diff = new Date(end) - new Date(start);
    return Math.max(1, Math.round(diff / 86400000) + 1);
  };

  // ── Acciones ───────────────────────────────────────────────────────
  const handleCreate = async (e) => {
    e.preventDefault();
    if (!newRequest.startDate || !newRequest.endDate) return;
    try {
      setSubmitting(true);
      await vacationAPI.createRequest({
        employeeId: 0,           // el backend usa el EmployeeId del token
        startDate: newRequest.startDate,
        endDate: newRequest.endDate,
        reason: newRequest.reason
      });
      setShowNewModal(false);
      setNewRequest({ startDate: '', endDate: '', reason: '' });
      showSuccess('Solicitud enviada correctamente');
      loadData();
    } catch (err) {
      setError('Error al crear solicitud: ' + (err.response?.data?.message || err.message));
    } finally {
      setSubmitting(false);
    }
  };

  const handleApprove = async () => {
    try {
      setSubmitting(true);
      await vacationAPI.approveRequest(selectedRequest.id, approveComments);
      setShowApprove(false);
      setApproveComments('');
      setSelected(null);
      showSuccess('Solicitud aprobada. El saldo fue descontado automáticamente.');
      loadData();
    } catch (err) {
      setError('Error al aprobar: ' + (err.response?.data?.message || err.message));
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      setError('Debe proporcionar un motivo de rechazo');
      return;
    }
    try {
      setSubmitting(true);
      await vacationAPI.rejectRequest(selectedRequest.id, rejectReason);
      setShowReject(false);
      setRejectReason('');
      setSelected(null);
      showSuccess('Solicitud rechazada');
      loadData();
    } catch (err) {
      setError('Error al rechazar: ' + (err.response?.data?.message || err.message));
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = async (req) => {
    if (!window.confirm('¿Está seguro de cancelar esta solicitud?')) return;
    try {
      await vacationAPI.cancelRequest(req.id, 'Cancelada por el empleado');
      showSuccess('Solicitud cancelada');
      loadData();
    } catch (err) {
      setError('Error al cancelar: ' + (err.response?.data?.message || err.message));
    }
  };

  // ── Render ─────────────────────────────────────────────────────────
  if (loading) {
    return <Layout><div className="loading">Cargando...</div></Layout>;
  }

  return (
    <Layout>
      <div className="vacations-container">

        {/* Encabezado */}
        <div className="page-header">
          <div>
            <h1>Gestión de Vacaciones</h1>
            <p className="page-subtitle">Solicitudes y saldo de días disponibles</p>
          </div>
          <button className="btn btn-primary" onClick={() => setShowNewModal(true)}>
            + Nueva Solicitud
          </button>
        </div>

        {error      && <div className="alert alert-error"   onClick={() => setError('')}>{error}</div>}
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

        {/* ── Tarjeta de saldo ── */}
        {balance && (
          <div className="balance-card">
            <h3>Mi Saldo de Vacaciones {balance.year}</h3>
            <div className="balance-stats">
              <div className="stat">
                <span className="stat-value">{balance.totalDays}</span>
                <span className="stat-label">Total del período</span>
              </div>
              {balance.carriedOverDays > 0 && (
                <div className="stat">
                  <span className="stat-value">{balance.carriedOverDays}</span>
                  <span className="stat-label">Acarreados del año anterior</span>
                </div>
              )}
              <div className="stat">
                <span className="stat-value">{balance.usedDays}</span>
                <span className="stat-label">Utilizados</span>
              </div>
              <div className="stat">
                <span className="stat-value">{balance.pendingDays}</span>
                <span className="stat-label">Pendientes de aprobación</span>
              </div>
              <div className="stat stat-highlight">
                <span className="stat-value">{balance.availableDays}</span>
                <span className="stat-label">Disponibles</span>
              </div>
            </div>
          </div>
        )}

        {/* ── Tabs ── */}
        <div className="tabs">
          <button
            className={`tab ${activeTab === 'my-requests' ? 'active' : ''}`}
            onClick={() => setActiveTab('my-requests')}
          >
            Mis Solicitudes ({myRequests.length})
          </button>
          {isManager && (
            <button
              className={`tab ${activeTab === 'pending' ? 'active' : ''}`}
              onClick={() => setActiveTab('pending')}
            >
              Pendientes de Aprobar
              {pendingRequests.length > 0 && (
                <span className="badge badge-warning" style={{ marginLeft: 6 }}>
                  {pendingRequests.length}
                </span>
              )}
            </button>
          )}
        </div>

        {/* ── Mis solicitudes ── */}
        {activeTab === 'my-requests' && (
          <div className="table-card">
            {myRequests.length === 0 ? (
              <p className="no-data">No tiene solicitudes de vacaciones registradas</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Fecha Inicio</th>
                    <th>Fecha Fin</th>
                    <th>Días</th>
                    <th>Motivo</th>
                    <th>Estado</th>
                    <th>Comentario del aprobador</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {myRequests.map(req => (
                    <tr key={req.id}>
                      <td>{fmt(req.startDate)}</td>
                      <td>{fmt(req.endDate)}</td>
                      <td><strong>{req.requestedDays}</strong></td>
                      <td>{req.reason || '-'}</td>
                      <td>{getStatusBadge(getStatus(req))}</td>
                      <td style={{ fontSize: '0.85rem', color: '#555' }}>
                        {req.approverComments || '-'}
                      </td>
                      <td>
                        {getStatus(req) === 'Pendiente' && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={() => handleCancel(req)}
                          >
                            Cancelar
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* ── Pendientes de aprobar (managers) ── */}
        {activeTab === 'pending' && isManager && (
          <div className="table-card">
            {pendingRequests.length === 0 ? (
              <p className="no-data">No hay solicitudes pendientes de aprobación</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Empleado</th>
                    <th>Fecha Inicio</th>
                    <th>Fecha Fin</th>
                    <th>Días</th>
                    <th>Motivo</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {pendingRequests.map(req => (
                    <tr key={req.id}>
                      <td><strong>{req.employeeName}</strong></td>
                      <td>{fmt(req.startDate)}</td>
                      <td>{fmt(req.endDate)}</td>
                      <td><strong>{req.requestedDays}</strong></td>
                      <td>{req.reason || '-'}</td>
                      <td>{getStatusBadge(getStatus(req))}</td>
                      <td style={{ display: 'flex', gap: 6 }}>
                        <button
                          className="btn btn-sm btn-success"
                          onClick={() => { setSelected(req); setShowApprove(true); }}
                        >
                          Aprobar
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => { setSelected(req); setShowReject(true); }}
                        >
                          Rechazar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* ══════════ MODALES ══════════ */}

        {/* Nueva solicitud */}
        {showNewModal && (
          <div className="modal-overlay" onClick={() => setShowNewModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Nueva Solicitud de Vacaciones</h2>
                <button className="close-btn" onClick={() => setShowNewModal(false)}>&times;</button>
              </div>

              {balance && (
                <div style={{
                  background: '#f0faf0', border: '1px solid #2d5a1b',
                  borderRadius: 8, padding: '10px 14px', marginBottom: 16,
                  fontSize: '0.9rem'
                }}>
                  <strong>Días disponibles: {balance.availableDays}</strong>
                  {balance.carriedOverDays > 0 && (
                    <span style={{ marginLeft: 12, color: '#555' }}>
                      (incluye {balance.carriedOverDays} acarreados)
                    </span>
                  )}
                </div>
              )}

              <form onSubmit={handleCreate}>
                <div className="form-group">
                  <label>Fecha de Inicio *</label>
                  <input
                    type="date"
                    value={newRequest.startDate}
                    min={new Date().toISOString().split('T')[0]}
                    onChange={e => setNewRequest({ ...newRequest, startDate: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Fecha de Fin *</label>
                  <input
                    type="date"
                    value={newRequest.endDate}
                    min={newRequest.startDate || new Date().toISOString().split('T')[0]}
                    onChange={e => setNewRequest({ ...newRequest, endDate: e.target.value })}
                    required
                  />
                </div>
                {newRequest.startDate && newRequest.endDate && (
                  <p style={{ fontSize: '0.88rem', color: '#2d5a1b', marginBottom: 12 }}>
                    Se solicitarán aproximadamente <strong>{calcDays(newRequest.startDate, newRequest.endDate)} días</strong>
                  </p>
                )}
                <div className="form-group">
                  <label>Motivo (opcional)</label>
                  <textarea
                    value={newRequest.reason}
                    onChange={e => setNewRequest({ ...newRequest, reason: e.target.value })}
                    rows={3}
                    placeholder="Describa el motivo de su solicitud..."
                  />
                </div>
                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary"
                    onClick={() => setShowNewModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? 'Enviando...' : 'Enviar Solicitud'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Aprobar */}
        {showApproveModal && selectedRequest && (
          <div className="modal-overlay" onClick={() => setShowApprove(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Aprobar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowApprove(false)}>&times;</button>
              </div>
              <p>
                ¿Aprobar solicitud de <strong>{selectedRequest.employeeName}</strong>?
              </p>
              <p style={{ color: '#555', fontSize: '0.9rem' }}>
                {fmt(selectedRequest.startDate)} — {fmt(selectedRequest.endDate)}&nbsp;
                (<strong>{selectedRequest.requestedDays} días</strong>)
              </p>
              <p style={{ fontSize: '0.85rem', color: '#c0392b' }}>
                Al aprobar, los días serán descontados automáticamente del saldo del empleado.
              </p>
              <div className="form-group">
                <label>Comentarios (opcional)</label>
                <textarea
                  value={approveComments}
                  onChange={e => setApproveComments(e.target.value)}
                  rows={2}
                  placeholder="Comentarios para el empleado..."
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowApprove(false)}>
                  Cancelar
                </button>
                <button className="btn btn-success" onClick={handleApprove} disabled={submitting}>
                  {submitting ? 'Aprobando...' : 'Confirmar Aprobación'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Rechazar */}
        {showRejectModal && selectedRequest && (
          <div className="modal-overlay" onClick={() => setShowReject(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Rechazar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowReject(false)}>&times;</button>
              </div>
              <p>
                ¿Rechazar solicitud de <strong>{selectedRequest.employeeName}</strong>?
              </p>
              <p style={{ color: '#555', fontSize: '0.9rem' }}>
                {fmt(selectedRequest.startDate)} — {fmt(selectedRequest.endDate)}&nbsp;
                (<strong>{selectedRequest.requestedDays} días</strong>)
              </p>
              <div className="form-group">
                <label>Motivo del Rechazo *</label>
                <textarea
                  value={rejectReason}
                  onChange={e => setRejectReason(e.target.value)}
                  rows={3}
                  required
                  placeholder="Indique el motivo del rechazo..."
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowReject(false)}>
                  Cancelar
                </button>
                <button className="btn btn-danger" onClick={handleReject} disabled={submitting}>
                  {submitting ? 'Rechazando...' : 'Confirmar Rechazo'}
                </button>
              </div>
            </div>
          </div>
        )}

      </div>
    </Layout>
  );
}

export default Vacations;