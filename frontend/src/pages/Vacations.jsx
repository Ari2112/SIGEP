import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { vacationAPI } from '../api/api';
import { USE_MOCK, getVacationBalance, getMyVacationRequests, getPendingVacationRequests, mockVacationRequests } from '../api/mockData';
import Layout from '../components/Layout';
import './Vacations.css';

function Vacations() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('my-requests');
  const [balance, setBalance] = useState(null);
  const [myRequests, setMyRequests] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showNewRequestModal, setShowNewRequestModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [newRequest, setNewRequest] = useState({
    startDate: '',
    endDate: '',
    reason: ''
  });
  const [rejectReason, setRejectReason] = useState('');
  const [approveComments, setApproveComments] = useState('');

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH' || user?.role === 'Jefatura';

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');

      // ============ MOCK DATA - REMOVER EN PRODUCCIÓN ============
      if (USE_MOCK) {
        setBalance(getVacationBalance(user?.employeeId || 4));
        setMyRequests(getMyVacationRequests(user?.employeeId || 4));
        if (isManager) {
          setPendingRequests(getPendingVacationRequests());
        }
        setLoading(false);
        return;
      }
      // ============ FIN MOCK DATA ============

      const [balanceRes, requestsRes] = await Promise.all([
        vacationAPI.getMyBalance(),
        vacationAPI.getMyRequests()
      ]);

      setBalance(balanceRes.data);
      setMyRequests(requestsRes.data);

      if (isManager) {
        const pendingRes = await vacationAPI.getPendingRequests();
        setPendingRequests(pendingRes.data);
      }
    } catch (err) {
      setError('Error al cargar datos: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleCreateRequest = async (e) => {
    e.preventDefault();
    
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const start = new Date(newRequest.startDate);
      const end = new Date(newRequest.endDate);
      const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1;
      
      mockVacationRequests.push({
        id: mockVacationRequests.length + 1,
        employeeId: user?.employeeId || 4,
        employeeName: user?.fullName || 'Usuario',
        startDate: newRequest.startDate,
        endDate: newRequest.endDate,
        requestedDays: days,
        reason: newRequest.reason,
        status: 'Pendiente',
        createdAt: new Date().toISOString().split('T')[0]
      });
      setShowNewRequestModal(false);
      setNewRequest({ startDate: '', endDate: '', reason: '' });
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      await vacationAPI.createRequest(newRequest);
      setShowNewRequestModal(false);
      setNewRequest({ startDate: '', endDate: '', reason: '' });
      loadData();
    } catch (err) {
      alert('Error al crear solicitud: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleApprove = async () => {
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const req = mockVacationRequests.find(r => r.id === selectedRequest.id);
      if (req) {
        req.status = 'Aprobada';
        req.approvedAt = new Date().toISOString();
        req.approverComments = approveComments;
      }
      setShowApproveModal(false);
      setApproveComments('');
      setSelectedRequest(null);
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      await vacationAPI.approveRequest(selectedRequest.id, approveComments);
      setShowApproveModal(false);
      setApproveComments('');
      setSelectedRequest(null);
      loadData();
    } catch (err) {
      alert('Error al aprobar: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      alert('Debe proporcionar un motivo de rechazo');
      return;
    }
    
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const req = mockVacationRequests.find(r => r.id === selectedRequest.id);
      if (req) {
        req.status = 'Rechazada';
        req.approverComments = rejectReason;
      }
      setShowRejectModal(false);
      setRejectReason('');
      setSelectedRequest(null);
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      await vacationAPI.rejectRequest(selectedRequest.id, rejectReason);
      setShowRejectModal(false);
      setRejectReason('');
      setSelectedRequest(null);
      loadData();
    } catch (err) {
      alert('Error al rechazar: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleCancel = async (request) => {
    if (window.confirm('¿Está seguro de cancelar esta solicitud?')) {
      // ============ MOCK - REMOVER EN PRODUCCIÓN ============
      if (USE_MOCK) {
        const req = mockVacationRequests.find(r => r.id === request.id);
        if (req) req.status = 'Cancelada';
        loadData();
        return;
      }
      // ============ FIN MOCK ============

      try {
        await vacationAPI.cancelRequest(request.id);
        loadData();
      } catch (err) {
        alert('Error al cancelar: ' + (err.response?.data?.message || err.message));
      }
    }
  };

  const getStatusBadge = (status) => {
    const statusMap = {
      'Pendiente': 'badge-warning',
      'Aprobada': 'badge-success',
      'Rechazada': 'badge-danger',
      'Cancelada': 'badge-secondary',
      'EnRevision': 'badge-info'
    };
    return <span className={`badge ${statusMap[status] || 'badge-secondary'}`}>{status}</span>;
  };

  const formatDate = (dateStr) => {
    return new Date(dateStr).toLocaleDateString('es-CR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  };

  if (loading) {
    return (
      <Layout>
        <div className="loading">Cargando...</div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="vacations-container">
        <div className="page-header">
          <h1>Gestión de Vacaciones</h1>
          <button 
            className="btn btn-primary"
            onClick={() => setShowNewRequestModal(true)}
          >
            Nueva Solicitud
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        {/* Balance Card */}
        {balance && (
          <div className="balance-card">
            <h3>Mi Saldo de Vacaciones {balance.year}</h3>
            <div className="balance-stats">
              <div className="stat">
                <span className="stat-value">{balance.totalDays}</span>
                <span className="stat-label">Total</span>
              </div>
              <div className="stat">
                <span className="stat-value">{balance.usedDays}</span>
                <span className="stat-label">Usados</span>
              </div>
              <div className="stat">
                <span className="stat-value">{balance.pendingDays}</span>
                <span className="stat-label">Pendientes</span>
              </div>
              <div className="stat stat-highlight">
                <span className="stat-value">{balance.availableDays}</span>
                <span className="stat-label">Disponibles</span>
              </div>
              {balance.carriedOverDays > 0 && (
                <div className="stat">
                  <span className="stat-value">{balance.carriedOverDays}</span>
                  <span className="stat-label">Acarreados</span>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Tabs */}
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
              Pendientes de Aprobar ({pendingRequests.length})
            </button>
          )}
        </div>

        {/* Tab Content */}
        {activeTab === 'my-requests' && (
          <div className="requests-table">
            {myRequests.length === 0 ? (
              <p className="no-data">No tiene solicitudes de vacaciones</p>
            ) : (
              <table>
                <thead>
                  <tr>
                    <th>Fecha Inicio</th>
                    <th>Fecha Fin</th>
                    <th>Días</th>
                    <th>Motivo</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {myRequests.map(req => (
                    <tr key={req.id}>
                      <td>{formatDate(req.startDate)}</td>
                      <td>{formatDate(req.endDate)}</td>
                      <td>{req.requestedDays}</td>
                      <td>{req.reason || '-'}</td>
                      <td>{getStatusBadge(req.status)}</td>
                      <td>
                        {req.status === 'Pendiente' && (
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

        {activeTab === 'pending' && isManager && (
          <div className="requests-table">
            {pendingRequests.length === 0 ? (
              <p className="no-data">No hay solicitudes pendientes de aprobar</p>
            ) : (
              <table>
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
                      <td>{req.employeeName}</td>
                      <td>{formatDate(req.startDate)}</td>
                      <td>{formatDate(req.endDate)}</td>
                      <td>{req.requestedDays}</td>
                      <td>{req.reason || '-'}</td>
                      <td>{getStatusBadge(req.status)}</td>
                      <td>
                        <button 
                          className="btn btn-sm btn-success"
                          onClick={() => {
                            setSelectedRequest(req);
                            setShowApproveModal(true);
                          }}
                        >
                          Aprobar
                        </button>
                        <button 
                          className="btn btn-sm btn-danger"
                          onClick={() => {
                            setSelectedRequest(req);
                            setShowRejectModal(true);
                          }}
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

        {/* New Request Modal */}
        {showNewRequestModal && (
          <div className="modal-overlay" onClick={() => setShowNewRequestModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Nueva Solicitud de Vacaciones</h2>
                <button className="close-btn" onClick={() => setShowNewRequestModal(false)}>&times;</button>
              </div>
              <form onSubmit={handleCreateRequest}>
                <div className="form-group">
                  <label>Fecha de Inicio</label>
                  <input
                    type="date"
                    value={newRequest.startDate}
                    onChange={e => setNewRequest({...newRequest, startDate: e.target.value})}
                    min={new Date().toISOString().split('T')[0]}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Fecha de Fin</label>
                  <input
                    type="date"
                    value={newRequest.endDate}
                    onChange={e => setNewRequest({...newRequest, endDate: e.target.value})}
                    min={newRequest.startDate || new Date().toISOString().split('T')[0]}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Motivo (opcional)</label>
                  <textarea
                    value={newRequest.reason}
                    onChange={e => setNewRequest({...newRequest, reason: e.target.value})}
                    rows="3"
                  />
                </div>
                <div className="modal-actions">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowNewRequestModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Enviar Solicitud
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Approve Modal */}
        {showApproveModal && selectedRequest && (
          <div className="modal-overlay" onClick={() => setShowApproveModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Aprobar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowApproveModal(false)}>&times;</button>
              </div>
              <p>¿Aprobar solicitud de <strong>{selectedRequest.employeeName}</strong>?</p>
              <p>{formatDate(selectedRequest.startDate)} - {formatDate(selectedRequest.endDate)} ({selectedRequest.requestedDays} días)</p>
              <div className="form-group">
                <label>Comentarios (opcional)</label>
                <textarea
                  value={approveComments}
                  onChange={e => setApproveComments(e.target.value)}
                  rows="2"
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowApproveModal(false)}>
                  Cancelar
                </button>
                <button className="btn btn-success" onClick={handleApprove}>
                  Aprobar
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Reject Modal */}
        {showRejectModal && selectedRequest && (
          <div className="modal-overlay" onClick={() => setShowRejectModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Rechazar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowRejectModal(false)}>&times;</button>
              </div>
              <p>¿Rechazar solicitud de <strong>{selectedRequest.employeeName}</strong>?</p>
              <div className="form-group">
                <label>Motivo del Rechazo *</label>
                <textarea
                  value={rejectReason}
                  onChange={e => setRejectReason(e.target.value)}
                  rows="3"
                  required
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowRejectModal(false)}>
                  Cancelar
                </button>
                <button className="btn btn-danger" onClick={handleReject}>
                  Rechazar
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
