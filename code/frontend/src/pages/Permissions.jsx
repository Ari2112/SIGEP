import { useState, useEffect } from 'react';
import { permissionAPI } from '../api/api';
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
  const [successMsg, setSuccessMsg] = useState(null);

  const [showNewModal, setShowNewModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);

  const [formData, setFormData] = useState({
    startDate: '',
    endDate: '',
    isPartialDay: false,
    startTime: '',
    endTime: '',
    reason: '',
    documentUrl: '',
  });
  const [selectedFile, setSelectedFile] = useState(null);
  const [approverComments, setApproverComments] = useState('');

  const isManager = ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura'].includes(user?.role);

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      const [typesRes, requestsRes, usageRes] = await Promise.all([
        permissionAPI.getTypes(),
        permissionAPI.getMyRequests(),
        permissionAPI.getUsageSummary(),
      ]);

      setPermissionTypes(typesRes.data || []);
      setRequests(requestsRes.data || []);
      const usageData = usageRes.data;
      setUsageSummary(Array.isArray(usageData) ? usageData : (usageData ? [usageData] : []));

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

  const calcWorkDays = (start, end) => {
    if (!start || !end) return 0;
    let count = 0;
    const cur = new Date(start);
    const last = new Date(end);
    while (cur <= last) {
      const dow = cur.getDay();
      if (dow !== 0 && dow !== 6) count++;
      cur.setDate(cur.getDate() + 1);
    }
    return count;
  };

  const previewDays = formData.isPartialDay ? 0.5 : calcWorkDays(formData.startDate, formData.endDate);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setError(null);

      const otroType = permissionTypes.find(t => t.name === 'Otro');
      const defaultTypeId = otroType ? otroType.id : 9;

      await permissionAPI.create({
        employeeId: user?.employeeId,
        permissionTypeId: defaultTypeId,
        startDate: formData.startDate,
        endDate: formData.isPartialDay
          ? formData.startDate
          : (formData.endDate || formData.startDate),
        isPartialDay: formData.isPartialDay,
        startTime: formData.isPartialDay ? formData.startTime : null,
        endTime: formData.isPartialDay ? formData.endTime : null,
        reason: formData.reason,
        documentUrl: formData.documentUrl || null,
      });

      setSuccessMsg('Solicitud enviada exitosamente');
      setShowNewModal(false);
      setSelectedFile(null);
      setFormData({
        startDate: '',
        endDate: '',
        isPartialDay: false,
        startTime: '',
        endTime: '',
        reason: '',
        documentUrl: '',
      });
      loadData();
      setTimeout(() => setSuccessMsg(null), 4000);
    } catch (err) {
      setError(err.response?.data?.message || 'Error al crear la solicitud');
    }
  };

  const handleApprove = async () => {
    try {
      setError(null);
      await permissionAPI.approve(selectedRequest.id, { comments: approverComments });
      setShowApproveModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al aprobar');
    }
  };

  const handleReject = async () => {
    try {
      setError(null);
      await permissionAPI.reject(selectedRequest.id, { reason: approverComments });
      setShowRejectModal(false);
      setSelectedRequest(null);
      setApproverComments('');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al rechazar');
    }
  };

  const getStatusBadge = (status) => {
    const badges = {
      'Pendiente': 'badge-warning',
      'Aprobada': 'badge-success',
      'Aprobado': 'badge-success',
      'Rechazada': 'badge-danger',
      'Rechazado': 'badge-danger',
      'Cancelada': 'badge-secondary',
      'Cancelado': 'badge-secondary',
    };
    return badges[status] || 'badge-info';
  };

  const formatDate = (d) => {
    if (!d) return '-';
    const date = new Date(d);
    if (isNaN(date)) return '-';
    return date.toLocaleDateString('es-CR');
  };

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

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
        {successMsg && <div className="alert alert-success">{successMsg}</div>}

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

        {activeTab === 'my-requests' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Descripción</th>
                  <th>Fecha Inicio</th>
                  <th>Fecha Fin</th>
                  <th>Días</th>
                  <th>Horario</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {requests.length === 0 ? (
                  <tr>
                    <td colSpan="7" className="no-data">No tienes solicitudes de permiso</td>
                  </tr>
                ) : (
                  requests.map((r) => (
                    <tr key={r.id}>
                      <td title={r.reason}>
                        {r.reason?.substring(0, 40)}{r.reason?.length > 40 ? '...' : ''}
                      </td>
                      <td>{formatDate(r.startDate)}</td>
                      <td>{formatDate(r.endDate)}</td>
                      <td>{r.durationDays}</td>
                      <td>
                        {r.isPartialDay
                          ? `${r.startTime} - ${r.endTime}`
                          : 'Día completo'}
                      </td>
                      <td>
                        <span className={`badge ${getStatusBadge(r.requestStatusName)}`}>
                          {r.requestStatusName}
                        </span>
                      </td>
                      <td>
                        {r.requestStatusName === 'Pendiente' && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={() => permissionAPI.cancel(r.id).then(loadData)}
                          >
                            Cancelar
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

        {activeTab === 'pending' && isManager && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Descripción</th>
                  <th>Fecha Inicio</th>
                  <th>Fecha Fin</th>
                  <th>Días</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {pendingRequests.length === 0 ? (
                  <tr>
                    <td colSpan="6" className="no-data">No hay solicitudes pendientes</td>
                  </tr>
                ) : (
                  pendingRequests.map((r) => (
                    <tr key={r.id}>
                      <td>{r.employeeName}</td>
                      <td title={r.reason}>
                        {r.reason?.substring(0, 40)}{r.reason?.length > 40 ? '...' : ''}
                      </td>
                      <td>{formatDate(r.startDate)}</td>
                      <td>{formatDate(r.endDate)}</td>
                      <td>{r.durationDays}</td>
                      <td>
                        <button
                          className="btn btn-sm btn-success"
                          style={{ marginRight: '6px' }}
                          onClick={() => { setSelectedRequest(r); setShowApproveModal(true); }}
                        >
                          Aprobar
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => { setSelectedRequest(r); setShowRejectModal(true); }}
                        >
                          Rechazar
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {showNewModal && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Nueva Solicitud de Permiso</h2>
                <button className="close-btn" onClick={() => setShowNewModal(false)}>×</button>
              </div>
              <form onSubmit={handleSubmit}>

                <div className="form-group">
                  <label>Descripción del permiso *</label>
                  <input
                    type="text"
                    value={formData.reason}
                    onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                    placeholder="Ej: Trámite en el banco, reunión escolar, visita al médico..."
                    required
                  />
                  <small style={{ color: '#888', fontSize: '12px' }}>
                    ⚠ Para incapacidades médicas o lactancia, use el módulo de <strong>Incapacidades</strong>.
                  </small>
                </div>

                <div className="form-group checkbox-group">
                  <label>
                    <input
                      type="checkbox"
                      checked={formData.isPartialDay}
                      onChange={(e) => setFormData({ ...formData, isPartialDay: e.target.checked })}
                    />
                    Es permiso parcial (solo horas)
                  </label>
                </div>

                {formData.isPartialDay ? (
                  <>
                    <div className="form-group">
                      <label>Fecha *</label>
                      <input
                        type="date"
                        value={formData.startDate}
                        onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                        min={new Date().toISOString().split('T')[0]}
                        required
                      />
                    </div>
                    <div className="time-inputs">
                      <div className="form-group">
                        <label>Hora Inicio *</label>
                        <input
                          type="time"
                          value={formData.startTime}
                          onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                          required
                        />
                      </div>
                      <div className="form-group">
                        <label>Hora Fin *</label>
                        <input
                          type="time"
                          value={formData.endTime}
                          onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                          required
                        />
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="time-inputs">
                    <div className="form-group">
                      <label>Fecha Inicio *</label>
                      <input
                        type="date"
                        value={formData.startDate}
                        onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                        min={new Date().toISOString().split('T')[0]}
                        required
                      />
                    </div>
                    <div className="form-group">
                      <label>Fecha Fin *</label>
                      <input
                        type="date"
                        value={formData.endDate}
                        onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                        min={formData.startDate || new Date().toISOString().split('T')[0]}
                        required
                      />
                    </div>
                  </div>
                )}

                {!formData.isPartialDay && formData.startDate && formData.endDate && (
                  <div className="alert alert-info" style={{ marginBottom: '12px' }}>
                    📅 Días hábiles solicitados: <strong>{previewDays}</strong>
                  </div>
                )}

                <div className="form-group">
                  <label>Comprobante <span style={{ color: '#888' }}>(opcional)</span></label>
                  <input
                    type="file"
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={(e) => {
                      const file = e.target.files[0];
                      if (file) {
                        setSelectedFile(file);
                        setFormData({ ...formData, documentUrl: file.name });
                      }
                    }}
                  />
                  {selectedFile && (
                    <small style={{ color: '#27ae60' }}>
                      ✓ Archivo seleccionado: {selectedFile.name}
                    </small>
                  )}
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

        {showApproveModal && selectedRequest && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Aprobar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowApproveModal(false)}>×</button>
              </div>
              <div className="info-box">
                <p><strong>Empleado:</strong> {selectedRequest.employeeName}</p>
                <p><strong>Descripción:</strong> {selectedRequest.reason}</p>
                <p><strong>Fechas:</strong> {formatDate(selectedRequest.startDate)} → {formatDate(selectedRequest.endDate)}</p>
                <p><strong>Días:</strong> {selectedRequest.durationDays}</p>
              </div>
              <div className="form-group">
                <label>Comentarios (opcional)</label>
                <textarea
                  value={approverComments}
                  onChange={(e) => setApproverComments(e.target.value)}
                  rows="2"
                  placeholder="Agregar comentarios..."
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowApproveModal(false)}>Cancelar</button>
                <button className="btn btn-success" onClick={handleApprove}>Aprobar</button>
              </div>
            </div>
          </div>
        )}

        {showRejectModal && selectedRequest && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Rechazar Solicitud</h2>
                <button className="close-btn" onClick={() => setShowRejectModal(false)}>×</button>
              </div>
              <div className="info-box">
                <p><strong>Empleado:</strong> {selectedRequest.employeeName}</p>
                <p><strong>Descripción:</strong> {selectedRequest.reason}</p>
                <p><strong>Fechas:</strong> {formatDate(selectedRequest.startDate)} → {formatDate(selectedRequest.endDate)}</p>
              </div>
              <div className="form-group">
                <label>Motivo del Rechazo *</label>
                <textarea
                  value={approverComments}
                  onChange={(e) => setApproverComments(e.target.value)}
                  rows="3"
                  placeholder="Indique el motivo del rechazo..."
                  required
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowRejectModal(false)}>Cancelar</button>
                <button className="btn btn-danger" onClick={handleReject}>Rechazar</button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default Permissions;