import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { overtimeAPI, employeeAPI } from '../api/api';
import { USE_MOCK, getMyOvertimeRecords, getPendingOvertimeRecords, getAllOvertimeRecords, mockEmployeesDetailed, mockOvertimeRecords } from '../api/mockData';
import Layout from '../components/Layout';
import './OvertimeHours.css';

function OvertimeHours() {
  const { user } = useAuth();
  const [records, setRecords] = useState([]);
  const [myRecords, setMyRecords] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [activeTab, setActiveTab] = useState('pending');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [showReviewModal, setShowReviewModal] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState(null);
  const [reviewAction, setReviewAction] = useState(null);
  const [reviewComments, setReviewComments] = useState('');
  const [showJustifyModal, setShowJustifyModal] = useState(false);
  const [justifyRecord, setJustifyRecord] = useState(null);
  const [justifyText, setJustifyText] = useState('');
  const [justifyError, setJustifyError] = useState('');
  const [filters, setFilters] = useState({
    employeeId: '',
    dateFrom: '',
    dateTo: '',
    status: ''
  });

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH';

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');

      // ============ MOCK DATA ============
      if (USE_MOCK) {
        const employeeId = user?.employeeId || 4;
        setMyRecords(getMyOvertimeRecords(employeeId));
        if (isManager) {
          setRecords(getPendingOvertimeRecords());
          setEmployees(mockEmployeesDetailed);
        }
        setLoading(false);
        return;
      }
      // ============ FIN MOCK DATA ============

      const myRes = await overtimeAPI.getMy();
      setMyRecords(myRes.data);

      if (isManager) {
        const [allRes, empRes] = await Promise.all([
          overtimeAPI.getAll(),
          employeeAPI.getAll()
        ]);
        // Pendientes de revisión: detectadas automáticamente o ya justificadas por el empleado
        const reviewable = allRes.data.filter(r => r.status === 'Detectada' || r.status === 'Pendiente');
        setRecords(reviewable);
        setEmployees(empRes.data);
      }
    } catch (err) {
      setError('Error al cargar datos: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = async () => {
    // ============ MOCK FILTER ============
    if (USE_MOCK) {
      let filtered = getAllOvertimeRecords();
      if (filters.employeeId) {
        filtered = filtered.filter(r => r.employeeId === parseInt(filters.employeeId));
      }
      if (filters.status) {
        filtered = filtered.filter(r => r.status === filters.status);
      }
      setRecords(filtered);
      return;
    }
    // ============ FIN MOCK FILTER ============
    
    try {
      setLoading(true);
      const res = await overtimeAPI.getAll({
        employeeId: filters.employeeId || undefined,
        dateFrom: filters.dateFrom || undefined,
        dateTo: filters.dateTo || undefined,
        status: filters.status || undefined
      });
      setRecords(res.data);
    } catch (err) {
      setError('Error al filtrar: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const openReviewModal = (record, approve) => {
    setSelectedRecord(record);
    setReviewAction(approve);
    setReviewComments('');
    setShowReviewModal(true);
  };

  const handleReview = async () => {
    // ============ MOCK REVIEW ============
    if (USE_MOCK) {
      setShowReviewModal(false);
      setSelectedRecord(null);
      setSuccessMessage(`Horas extra ${reviewAction ? 'aprobadas' : 'rechazadas'} exitosamente (modo demo)`);
      // Actualizar el registro localmente
      const updatedRecords = records.filter(r => r.id !== selectedRecord.id);
      setRecords(updatedRecords);
      return;
    }
    // ============ FIN MOCK REVIEW ============
    
    try {
      await overtimeAPI.review(selectedRecord.id, reviewAction, reviewComments);
      setShowReviewModal(false);
      setSelectedRecord(null);
      setSuccessMessage(`Horas extra ${reviewAction ? 'aprobadas' : 'rechazadas'} exitosamente`);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al procesar');
    }
  };

  const openJustifyModal = (record) => {
    setJustifyRecord(record);
    setJustifyText(record.justification || '');
    setJustifyError('');
    setShowJustifyModal(true);
  };

  const handleJustify = async () => {
    if (!justifyText.trim()) {
      setJustifyError('Por favor indique el motivo de la hora extra');
      return;
    }

    // ============ MOCK JUSTIFY ============
    if (USE_MOCK) {
      setShowJustifyModal(false);
      const updated = myRecords.map(r =>
        r.id === justifyRecord.id
          ? { ...r, justification: justifyText.trim(), status: r.status === 'Detectada' ? 'Pendiente' : r.status }
          : r
      );
      setMyRecords(updated);
      setSuccessMessage('Justificación registrada exitosamente (modo demo)');
      return;
    }
    // ============ FIN MOCK JUSTIFY ============

    try {
      await overtimeAPI.justify(justifyRecord.id, justifyText.trim());
      setShowJustifyModal(false);
      setSuccessMessage('Justificación registrada exitosamente');
      loadData();
    } catch (err) {
      setJustifyError(err.response?.data?.message || 'Error al guardar la justificación');
    }
  };

  const formatDate = (dateStr) => {
    return new Date(dateStr).toLocaleDateString('es-CR', {
      day: '2-digit', month: '2-digit', year: 'numeric'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency',
      currency: 'CRC',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const getStatusBadge = (status) => {
    const map = {
      'Detectada': 'badge-info',
      'Pendiente': 'badge-warning',
      'Aprobada': 'badge-success',
      'Rechazada': 'badge-danger',
      'Pagada': 'badge-secondary'
    };
    return <span className={`badge ${map[status] || 'badge-secondary'}`}>{status}</span>;
  };

  if (loading) {
    return <Layout><div className="loading">Cargando...</div></Layout>;
  }

  return (
    <Layout>
      <div className="overtime-container">
        <div className="page-header">
          <h1>Horas Extra</h1>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {successMessage && <div className="alert alert-success">{successMessage}</div>}

        <div className="tabs">
          {isManager && (
            <button
              className={`tab ${activeTab === 'pending' ? 'active' : ''}`}
              onClick={() => setActiveTab('pending')}
            >
              Pendientes de Revisión ({records.length})
            </button>
          )}
          <button
            className={`tab ${activeTab === 'my' ? 'active' : ''}`}
            onClick={() => setActiveTab('my')}
          >
            Mis Horas Extra ({myRecords.length})
          </button>
          {isManager && (
            <button
              className={`tab ${activeTab === 'all' ? 'active' : ''}`}
              onClick={() => setActiveTab('all')}
            >
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
                  <th>Fecha</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Horas</th>
                  <th>Monto</th>
                  <th>Justificación</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {records.length === 0 ? (
                  <tr><td colSpan="9" className="no-data">No hay horas extra pendientes de revisión</td></tr>
                ) : (
                  records.map(rec => (
                    <tr key={rec.id}>
                      <td>{rec.employeeName}</td>
                      <td>{formatDate(rec.date)}</td>
                      <td>{rec.startTime}</td>
                      <td>{rec.endTime}</td>
                      <td>{rec.totalHours}h</td>
                      <td>{formatCurrency(rec.totalAmount)}</td>
                      <td className="justification-cell">
                        {rec.justification
                          ? <span title={rec.justification}>{rec.justification}</span>
                          : <span className="badge badge-warning">Sin justificar</span>}
                      </td>
                      <td>{getStatusBadge(rec.status)}</td>
                      <td>
                        <button
                          className="btn btn-sm btn-success"
                          onClick={() => openReviewModal(rec, true)}
                        >
                          Aprobar
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => openReviewModal(rec, false)}
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

        {/* Tab: Mis horas extra */}
        {activeTab === 'my' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Horas</th>
                  <th>Monto</th>
                  <th>Estado</th>
                  <th>Mi Justificación</th>
                  <th>Observación de RRHH</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {myRecords.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">No tiene registros de horas extra</td></tr>
                ) : (
                  myRecords.map(rec => {
                    const canJustify = !rec.justification && (rec.status === 'Detectada' || rec.status === 'Pendiente');
                    return (
                      <tr key={rec.id}>
                        <td>{formatDate(rec.date)}</td>
                        <td>{rec.startTime}</td>
                        <td>{rec.endTime}</td>
                        <td>{rec.totalHours}h</td>
                        <td>{formatCurrency(rec.totalAmount)}</td>
                        <td>{getStatusBadge(rec.status)}</td>
                        <td className="justification-cell">
                          {rec.justification
                            ? <span title={rec.justification}>{rec.justification}</span>
                            : <span className="badge badge-warning">Falta justificar</span>}
                        </td>
                        <td>{rec.reviewComments || '-'}</td>
                        <td>
                          {canJustify && (
                            <button
                              className="btn btn-sm btn-secondary"
                              onClick={() => openJustifyModal(rec)}
                            >
                              Justificar
                            </button>
                          )}
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Tab: Buscar/Historial */}
        {activeTab === 'all' && isManager && (
          <div className="table-card">
            <div className="filters-bar">
              <select
                value={filters.employeeId}
                onChange={e => setFilters({ ...filters, employeeId: e.target.value })}
              >
                <option value="">Todos los empleados</option>
                {employees.map(emp => (
                  <option key={emp.id} value={emp.id}>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</option>
                ))}
              </select>
              <input
                type="date"
                value={filters.dateFrom}
                onChange={e => setFilters({ ...filters, dateFrom: e.target.value })}
              />
              <input
                type="date"
                value={filters.dateTo}
                onChange={e => setFilters({ ...filters, dateTo: e.target.value })}
              />
              <select
                value={filters.status}
                onChange={e => setFilters({ ...filters, status: e.target.value })}
              >
                <option value="">Todos los estados</option>
                <option value="Detectada">Detectada</option>
                <option value="Aprobada">Aprobada</option>
                <option value="Rechazada">Rechazada</option>
                <option value="Pagada">Pagada</option>
              </select>
              <button className="btn btn-primary" onClick={handleFilter}>Filtrar</button>
            </div>

            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Fecha</th>
                  <th>Horas</th>
                  <th>Monto</th>
                  <th>Justificación</th>
                  <th>Estado</th>
                  <th>Revisado por</th>
                  <th>Observación</th>
                </tr>
              </thead>
              <tbody>
                {records.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">No hay registros</td></tr>
                ) : (
                  records.map(rec => (
                    <tr key={rec.id}>
                      <td>{rec.employeeName}</td>
                      <td>{formatDate(rec.date)}</td>
                      <td>{rec.totalHours}h</td>
                      <td>{formatCurrency(rec.totalAmount)}</td>
                      <td className="justification-cell">
                        {rec.justification
                          ? <span title={rec.justification}>{rec.justification}</span>
                          : '-'}
                      </td>
                      <td>{getStatusBadge(rec.status)}</td>
                      <td>{rec.reviewedByName || '-'}</td>
                      <td>{rec.reviewComments || '-'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Review Modal */}
        {showReviewModal && selectedRecord && (
          <div className="modal-overlay" onClick={() => setShowReviewModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>{reviewAction ? 'Aprobar' : 'Rechazar'} Horas Extra</h2>
                <button className="close-btn" onClick={() => setShowReviewModal(false)}>&times;</button>
              </div>
              <div className="info-box">
                <p><strong>Empleado:</strong> {selectedRecord.employeeName}</p>
                <p><strong>Fecha:</strong> {formatDate(selectedRecord.date)}</p>
                <p><strong>Horas:</strong> {selectedRecord.totalHours}h ({selectedRecord.startTime} - {selectedRecord.endTime})</p>
                <p><strong>Monto:</strong> {formatCurrency(selectedRecord.totalAmount)}</p>
              </div>
              <div className={`justification-box ${!selectedRecord.justification ? 'justification-missing' : ''}`}>
                <strong>Justificación del empleado:</strong>
                <p>{selectedRecord.justification || 'El empleado aún no ha justificado esta hora extra.'}</p>
              </div>
              <div className="form-group">
                <label>Observación {!reviewAction ? '(requerida)' : '(opcional)'}</label>
                <textarea
                  value={reviewComments}
                  onChange={e => setReviewComments(e.target.value)}
                  rows="3"
                  required={!reviewAction}
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowReviewModal(false)}>
                  Cancelar
                </button>
                <button
                  className={`btn ${reviewAction ? 'btn-success' : 'btn-danger'}`}
                  onClick={handleReview}
                >
                  {reviewAction ? 'Aprobar' : 'Rechazar'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Justify Modal */}
        {showJustifyModal && justifyRecord && (
          <div className="modal-overlay" onClick={() => setShowJustifyModal(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Justificar Hora Extra</h2>
                <button className="close-btn" onClick={() => setShowJustifyModal(false)}>&times;</button>
              </div>
              <div className="info-box">
                <p><strong>Fecha:</strong> {formatDate(justifyRecord.date)}</p>
                <p><strong>Horas:</strong> {justifyRecord.totalHours}h ({justifyRecord.startTime} - {justifyRecord.endTime})</p>
                <p><strong>Monto:</strong> {formatCurrency(justifyRecord.totalAmount)}</p>
              </div>
              {justifyError && <div className="alert alert-error">{justifyError}</div>}
              <div className="form-group">
                <label>Motivo de la hora extra (requerido)</label>
                <textarea
                  value={justifyText}
                  onChange={e => setJustifyText(e.target.value)}
                  rows="3"
                  placeholder="Ej: Cierre de proyecto solicitado por mi supervisor..."
                  required
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowJustifyModal(false)}>
                  Cancelar
                </button>
                <button className="btn btn-primary" onClick={handleJustify}>
                  Guardar Justificación
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default OvertimeHours;