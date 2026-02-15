import { useState, useEffect } from 'react';
import { permissionAPI } from '../api/api';
import { USE_MOCK, mockPermissionTypes, mockPermissionRequests, getMyPermissionRequests, getPendingPermissionRequests, getPermissionUsage } from '../api/mockData';
import { useAuth } from '../context/AuthContext';
import Layout from '../components/Layout';
import './Permissions.css';

const Permissions = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('my-requests');
  const [requests, setRequests] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [permissionTypes, setPermissionTypes] = useState([]);
  const [usageSummary, setUsageSummary] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Modal states
  const [showNewModal, setShowNewModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  
  // Form state
  const [formData, setFormData] = useState({
    permissionTypeId: '',
    date: '',
    isPartialDay: false,
    startTime: '',
    endTime: '',
    reason: ''
  });
  const [approverComments, setApproverComments] = useState('');

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH' || user?.role === 'Jefatura';

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // ============ MOCK DATA - REMOVER EN PRODUCCIÓN ============
      if (USE_MOCK) {
        setPermissionTypes(mockPermissionTypes);
        setRequests(getMyPermissionRequests(user?.employeeId || 4));
        setUsageSummary(getPermissionUsage(user?.employeeId || 4));
        if (isManager) {
          setPendingRequests(getPendingPermissionRequests());
        }
        setLoading(false);
        return;
      }
      // ============ FIN MOCK DATA ============

      const [typesRes, requestsRes, usageRes] = await Promise.all([
        permissionAPI.getTypes(),
        permissionAPI.getMyRequests(),
        permissionAPI.getUsageSummary()
      ]);
      
      setPermissionTypes(typesRes.data || []);
      setRequests(requestsRes.data || []);
      setUsageSummary(usageRes.data || []);
      
      if (isManager) {
        const pendingRes = await permissionAPI.getPendingApproval();
        setPendingRequests(pendingRes.data || []);
      }
    } catch (err) {
      setError('Error al cargar los datos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const type = mockPermissionTypes.find(t => t.id === parseInt(formData.permissionTypeId));
      mockPermissionRequests.push({
        id: mockPermissionRequests.length + 1,
        employeeId: user?.employeeId || 4,
        employeeName: user?.fullName || 'Usuario',
        permissionTypeId: parseInt(formData.permissionTypeId),
        permissionTypeName: type?.name || 'Permiso',
        date: formData.date,
        isPartialDay: formData.isPartialDay,
        startTime: formData.startTime || null,
        endTime: formData.endTime || null,
        reason: formData.reason,
        status: 'Pendiente',
        createdAt: new Date().toISOString().split('T')[0]
      });
      setShowNewModal(false);
      setFormData({
        permissionTypeId: '',
        date: '',
        isPartialDay: false,
        startTime: '',
        endTime: '',
        reason: ''
      });
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      setError(null);
      await permissionAPI.create(formData);
      setShowNewModal(false);
      setFormData({
        permissionTypeId: '',
        date: '',
        isPartialDay: false,
        startTime: '',
        endTime: '',
        reason: ''
      });
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al crear la solicitud');
    }
  };

  const handleApprove = async () => {
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const req = mockPermissionRequests.find(r => r.id === selectedRequest.id);
      if (req) {
        req.status = 'Aprobado';
        req.approvedAt = new Date().toISOString();
      }
      setShowApproveModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      setError(null);
      await permissionAPI.approve(selectedRequest.id, { comments: approverComments });
      setShowApproveModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al aprobar la solicitud');
    }
  };

  const handleReject = async () => {
    // ============ MOCK - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const req = mockPermissionRequests.find(r => r.id === selectedRequest.id);
      if (req) {
        req.status = 'Rechazado';
      }
      setShowRejectModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
      return;
    }
    // ============ FIN MOCK ============

    try {
      setError(null);
      await permissionAPI.reject(selectedRequest.id, { comments: approverComments });
      setShowRejectModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al rechazar la solicitud');
    }
  };

  const getStatusBadge = (status) => {
    const badges = {
      'Pendiente': 'badge-warning',
      'Aprobado': 'badge-success',
      'Rechazado': 'badge-danger',
      'Cancelado': 'badge-secondary'
    };
    return badges[status] || 'badge-info';
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('es-CR');
  };

  if (loading) {
    return <Layout><div className="loading">Cargando...</div></Layout>;
  }

  return (
    <Layout>
    <div className="permissions-container">
      <div className="page-header">
        <h1>Gestión de Permisos</h1>
        <button className="btn btn-primary" onClick={() => setShowNewModal(true)}>
          + Solicitar Permiso
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {/* Usage Summary Cards */}
      <div className="usage-summary">
        <h3>Resumen de Uso Anual</h3>
        <div className="usage-cards">
          {usageSummary.map((item) => (
            <div key={item.permissionTypeId} className="usage-card">
              <div className="usage-type">{item.typeName}</div>
              <div className="usage-stats">
                <span className="used">{item.usedDays} usados</span>
                <span className="limit">/ {item.maxDaysPerYear || '∞'} máx</span>
              </div>
              <div className="usage-bar">
                <div 
                  className="usage-progress" 
                  style={{ 
                    width: item.maxDaysPerYear 
                      ? `${Math.min((item.usedDays / item.maxDaysPerYear) * 100, 100)}%` 
                      : '0%' 
                  }}
                ></div>
              </div>
            </div>
          ))}
          {usageSummary.length === 0 && (
            <div className="no-usage">No hay uso registrado este año</div>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="tabs">
        <button 
          className={`tab ${activeTab === 'my-requests' ? 'active' : ''}`}
          onClick={() => setActiveTab('my-requests')}
        >
          Mis Solicitudes
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

      {/* My Requests Table */}
      {activeTab === 'my-requests' && (
        <div className="requests-table">
          <table>
            <thead>
              <tr>
                <th>Tipo</th>
                <th>Fecha</th>
                <th>Horario</th>
                <th>Motivo</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {requests.map((request) => (
                <tr key={request.id}>
                  <td>{request.permissionTypeName}</td>
                  <td>{formatDate(request.date)}</td>
                  <td>
                    {request.isPartialDay 
                      ? `${request.startTime} - ${request.endTime}` 
                      : 'Día completo'}
                  </td>
                  <td>{request.reason}</td>
                  <td>
                    <span className={`badge ${getStatusBadge(request.status)}`}>
                      {request.status}
                    </span>
                  </td>
                  <td>
                    {request.status === 'Pendiente' && (
                      <button className="btn btn-sm btn-danger">
                        Cancelar
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {requests.length === 0 && (
                <tr>
                  <td colSpan="6" className="no-data">
                    No tienes solicitudes de permiso
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Pending Approval Table */}
      {activeTab === 'pending' && isManager && (
        <div className="requests-table">
          <table>
            <thead>
              <tr>
                <th>Empleado</th>
                <th>Tipo</th>
                <th>Fecha</th>
                <th>Horario</th>
                <th>Motivo</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {pendingRequests.map((request) => (
                <tr key={request.id}>
                  <td>{request.employeeName}</td>
                  <td>{request.permissionTypeName}</td>
                  <td>{formatDate(request.date)}</td>
                  <td>
                    {request.isPartialDay 
                      ? `${request.startTime} - ${request.endTime}` 
                      : 'Día completo'}
                  </td>
                  <td>{request.reason}</td>
                  <td>
                    <button 
                      className="btn btn-sm btn-success"
                      onClick={() => {
                        setSelectedRequest(request);
                        setShowApproveModal(true);
                      }}
                    >
                      Aprobar
                    </button>
                    <button 
                      className="btn btn-sm btn-danger"
                      onClick={() => {
                        setSelectedRequest(request);
                        setShowRejectModal(true);
                      }}
                    >
                      Rechazar
                    </button>
                  </td>
                </tr>
              ))}
              {pendingRequests.length === 0 && (
                <tr>
                  <td colSpan="6" className="no-data">
                    No hay solicitudes pendientes de aprobación
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* New Request Modal */}
      {showNewModal && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h2>Nueva Solicitud de Permiso</h2>
              <button className="close-btn" onClick={() => setShowNewModal(false)}>×</button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Tipo de Permiso *</label>
                <select
                  value={formData.permissionTypeId}
                  onChange={(e) => setFormData({...formData, permissionTypeId: e.target.value})}
                  required
                >
                  <option value="">Seleccionar tipo</option>
                  {permissionTypes.map((type) => (
                    <option key={type.id} value={type.id}>
                      {type.name} {type.maxDaysPerYear ? `(máx ${type.maxDaysPerYear} días/año)` : ''}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Fecha *</label>
                <input
                  type="date"
                  value={formData.date}
                  onChange={(e) => setFormData({...formData, date: e.target.value})}
                  min={new Date().toISOString().split('T')[0]}
                  required
                />
              </div>
              <div className="form-group checkbox-group">
                <label>
                  <input
                    type="checkbox"
                    checked={formData.isPartialDay}
                    onChange={(e) => setFormData({...formData, isPartialDay: e.target.checked})}
                  />
                  Es permiso parcial (horas)
                </label>
              </div>
              {formData.isPartialDay && (
                <div className="time-inputs">
                  <div className="form-group">
                    <label>Hora Inicio *</label>
                    <input
                      type="time"
                      value={formData.startTime}
                      onChange={(e) => setFormData({...formData, startTime: e.target.value})}
                      required={formData.isPartialDay}
                    />
                  </div>
                  <div className="form-group">
                    <label>Hora Fin *</label>
                    <input
                      type="time"
                      value={formData.endTime}
                      onChange={(e) => setFormData({...formData, endTime: e.target.value})}
                      required={formData.isPartialDay}
                    />
                  </div>
                </div>
              )}
              <div className="form-group">
                <label>Motivo *</label>
                <textarea
                  value={formData.reason}
                  onChange={(e) => setFormData({...formData, reason: e.target.value})}
                  rows="3"
                  placeholder="Describa el motivo de su solicitud..."
                  required
                ></textarea>
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowNewModal(false)}>
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
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h2>Aprobar Solicitud</h2>
              <button className="close-btn" onClick={() => setShowApproveModal(false)}>×</button>
            </div>
            <p>¿Está seguro que desea aprobar esta solicitud de permiso?</p>
            <div className="request-summary">
              <p><strong>Empleado:</strong> {selectedRequest.employeeName}</p>
              <p><strong>Tipo:</strong> {selectedRequest.permissionTypeName}</p>
              <p><strong>Fecha:</strong> {formatDate(selectedRequest.date)}</p>
              <p><strong>Motivo:</strong> {selectedRequest.reason}</p>
            </div>
            <div className="form-group">
              <label>Comentarios (opcional)</label>
              <textarea
                value={approverComments}
                onChange={(e) => setApproverComments(e.target.value)}
                rows="2"
                placeholder="Agregar comentarios..."
              ></textarea>
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
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h2>Rechazar Solicitud</h2>
              <button className="close-btn" onClick={() => setShowRejectModal(false)}>×</button>
            </div>
            <p>¿Está seguro que desea rechazar esta solicitud de permiso?</p>
            <div className="request-summary">
              <p><strong>Empleado:</strong> {selectedRequest.employeeName}</p>
              <p><strong>Tipo:</strong> {selectedRequest.permissionTypeName}</p>
              <p><strong>Fecha:</strong> {formatDate(selectedRequest.date)}</p>
            </div>
            <div className="form-group">
              <label>Motivo del Rechazo *</label>
              <textarea
                value={approverComments}
                onChange={(e) => setApproverComments(e.target.value)}
                rows="3"
                placeholder="Indique el motivo del rechazo..."
                required
              ></textarea>
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
};

export default Permissions;
