import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { attendanceAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './Attendance.css';

function Attendance() {
  const { user } = useAuth();
  const [todayRecord, setTodayRecord] = useState(null);
  const [myRecords, setMyRecords] = useState([]);
  const [allRecords, setAllRecords] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [activeTab, setActiveTab] = useState('today');
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [currentTime, setCurrentTime] = useState(new Date());
  const [filters, setFilters] = useState({ employeeId: '', dateFrom: '', dateTo: '' });

  // Modal para motivo de horas extra
  const [showOvertimeModal, setShowOvertimeModal] = useState(false);
  const [overtimeReason, setOvertimeReason] = useState('');

  const isManager = ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura'].includes(user?.role);

  // Reloj en tiempo real
  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');

      const [todayRes, myRes] = await Promise.all([
        attendanceAPI.getToday(),
        attendanceAPI.getMyRecords()
      ]);

      setTodayRecord(todayRes.data);
      setMyRecords(myRes.data);

      if (isManager) {
        const [allRes, empRes] = await Promise.all([
          attendanceAPI.getAll(),
          employeeAPI.getAll()
        ]);
        setAllRecords(allRes.data);
        setEmployees(empRes.data);
      }
    } catch (err) {
      setError('Error al cargar datos: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleCheckIn = async () => {
    setActionLoading(true);
    setError('');
    setSuccessMessage('');
    try {
      const res = await attendanceAPI.checkIn();
      setTodayRecord(res.data);
      // Mostrar aviso de tardía si aplica
      if (res.data.isLate) {
        setSuccessMessage(`Entrada registrada — TARDÍA: ${res.data.lateMinutes} minutos después del horario`);
      } else {
        setSuccessMessage('Entrada registrada exitosamente');
      }
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al registrar entrada');
    } finally {
      setActionLoading(false);
    }
  };

  // Verificar si es posible que tenga horas extra antes de mostrar modal
  const handleCheckOutClick = () => {
    if (todayRecord?.scheduledEndTime) {
      const now = new Date();
      const [h, m] = todayRecord.scheduledEndTime.split(':');
      const scheduled = new Date();
      scheduled.setHours(parseInt(h), parseInt(m), 0);
      // Si son más de 10 minutos después del horario, pedir motivo
      if ((now - scheduled) > 10 * 60 * 1000) {
        setShowOvertimeModal(true);
        return;
      }
    }
    executeCheckOut(null);
  };

  const handleOvertimeConfirm = () => {
    if (!overtimeReason.trim()) {
      setError('Debe ingresar el motivo de las horas extra');
      return;
    }
    setShowOvertimeModal(false);
    executeCheckOut(overtimeReason);
    setOvertimeReason('');
  };

  const executeCheckOut = async (reason) => {
    setActionLoading(true);
    setError('');
    setSuccessMessage('');
    try {
      const res = await attendanceAPI.checkOut({ overtimeReason: reason });
      setTodayRecord(res.data);
      const horasExtra = res.data.overtimeHours;
      if (horasExtra && horasExtra > 0) {
        setSuccessMessage(`Salida registrada — ${horasExtra.toFixed(2)} horas extra detectadas y enviadas para aprobación`);
      } else {
        setSuccessMessage('Salida registrada exitosamente');
      }
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al registrar salida');
    } finally {
      setActionLoading(false);
    }
  };

  const handleFilter = async () => {
    try {
      setLoading(true);
      const res = await attendanceAPI.getAll({
        employeeId: filters.employeeId || undefined,
        dateFrom: filters.dateFrom || undefined,
        dateTo: filters.dateTo || undefined
      });
      setAllRecords(res.data);
    } catch (err) {
      setError('Error al filtrar: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

const formatTime = (dateStr) => {
  if (!dateStr) return '-';
  // El backend devuelve hora local de Costa Rica (sin Z)
  // No agregar 'Z' para no hacer conversión de zona horaria incorrecta
  const d = new Date(dateStr);
  if (isNaN(d)) return '-';
  // Si el string no trae zona horaria, JS lo trata como local — mostrarlo directamente
  return d.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' });
};

const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  const d = dateStr.endsWith('Z') ? dateStr : dateStr + 'Z';
  return new Date(d).toLocaleDateString('es-CR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    timeZone: 'America/Costa_Rica'
  });
};

  const getStatusBadge = (status) => {
    const map = {
      'Completo': 'badge-success',
      'Parcial': 'badge-warning',
      'Ausente': 'badge-danger',
      'Permiso': 'badge-info',
      'Vacaciones': 'badge-info',
      'Incapacidad': 'badge-secondary'
    };
    return <span className={`badge ${map[status] || 'badge-secondary'}`}>{status}</span>;
  };

  const canCheckIn  = !todayRecord;
  const canCheckOut = todayRecord && todayRecord.checkInTime && !todayRecord.checkOutTime;

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="attendance-container">
        <div className="page-header">
          <h1>Asistencia</h1>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {successMessage && <div className="alert alert-success">{successMessage}</div>}

        {/* Tabs */}
        <div className="tabs">
          <button className={`tab ${activeTab === 'today' ? 'active' : ''}`} onClick={() => setActiveTab('today')}>Hoy</button>
          <button className={`tab ${activeTab === 'history' ? 'active' : ''}`} onClick={() => setActiveTab('history')}>Mi Historial</button>
          {isManager && (
            <button className={`tab ${activeTab === 'all' ? 'active' : ''}`} onClick={() => setActiveTab('all')}>Todos los Registros</button>
          )}
        </div>

        {/* Tab: Hoy */}
        {activeTab === 'today' && (
          <div className="today-panel">
            <div className="today-card">
              <h2>Registro del Día</h2>
              <p className="today-date">
                {currentTime.toLocaleDateString('es-CR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
              </p>

              <div className="clock-display">
                {currentTime.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
              </div>

              {/* Horario del empleado */}
              {todayRecord?.scheduledStartTime && (
                <div className="schedule-info">
                  <span>Horario: {todayRecord.scheduledStartTime.substring(0,5)} — {todayRecord.scheduledEndTime?.substring(0,5)}</span>
                  {todayRecord.scheduleName && <span className="schedule-name"> ({todayRecord.scheduleName})</span>}
                </div>
              )}

              {todayRecord ? (
                <div className="today-status">
                  <div className="time-info">
                    <div className="time-block">
                      <span className="time-label">Entrada</span>
                      <span className="time-value">{formatTime(todayRecord.checkInTime)}</span>
                    </div>
                    <div className="time-block">
                      <span className="time-label">Salida</span>
                      <span className="time-value">{formatTime(todayRecord.checkOutTime)}</span>
                    </div>
                    {todayRecord.workedHours && (
                      <div className="time-block">
                        <span className="time-label">Horas Trabajadas</span>
                        <span className="time-value">{parseFloat(todayRecord.workedHours).toFixed(2)}h</span>
                      </div>
                    )}
                    {todayRecord.overtimeHours > 0 && (
                      <div className="time-block">
                        <span className="time-label">Horas Extra</span>
                        <span className="time-value" style={{color:'#e67e22'}}>{parseFloat(todayRecord.overtimeHours).toFixed(2)}h</span>
                      </div>
                    )}
                  </div>

                  {/* Alerta de tardía */}
                  {todayRecord.isLate && (
                    <div className="alert alert-warning" style={{marginTop:'10px'}}>
                      ⚠ Tardía detectada: {todayRecord.lateMinutes} minutos después del horario
                    </div>
                  )}

                  <div className="status-display">
                    {getStatusBadge(todayRecord.status)}
                  </div>
                </div>
              ) : (
                <p className="no-record">No ha registrado asistencia hoy</p>
              )}

              <div className="action-buttons">
                <button
                  className="btn btn-primary btn-lg"
                  onClick={handleCheckIn}
                  disabled={!canCheckIn || actionLoading}
                >
                  {actionLoading ? 'Procesando...' : 'Registrar Entrada'}
                </button>
                <button
                  className="btn btn-secondary btn-lg"
                  onClick={handleCheckOutClick}
                  disabled={!canCheckOut || actionLoading}
                >
                  {actionLoading ? 'Procesando...' : 'Registrar Salida'}
                </button>
              </div>

              {!canCheckIn && !canCheckOut && todayRecord && (
                <p className="completed-msg">Asistencia completada para hoy</p>
              )}
            </div>
          </div>
        )}

        {/* Tab: Mi Historial */}
        {activeTab === 'history' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Entrada</th>
                  <th>Salida</th>
                  <th>Horas</th>
                  <th>Tardía</th>
                  <th>H. Extra</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {myRecords.length === 0 ? (
                  <tr><td colSpan="7" className="no-data">No hay registros de asistencia</td></tr>
                ) : (
                  myRecords.map(rec => (
                    <tr key={rec.id}>
                      <td>{formatDate(rec.date)}</td>
                      <td>{formatTime(rec.checkInTime)}</td>
                      <td>{formatTime(rec.checkOutTime)}</td>
                      <td>{rec.workedHours ? `${parseFloat(rec.workedHours).toFixed(2)}h` : '-'}</td>
                      <td>
                        {rec.isLate
                          ? <span style={{color:'#e74c3c'}}>⚠ {rec.lateMinutes} min</span>
                          : <span style={{color:'#27ae60'}}>✓</span>}
                      </td>
                      <td>{rec.overtimeHours > 0 ? `${parseFloat(rec.overtimeHours).toFixed(2)}h` : '-'}</td>
                      <td>{getStatusBadge(rec.status)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Tab: Todos los Registros (Admin/RRHH) */}
        {activeTab === 'all' && isManager && (
          <div className="table-card">
            <div className="filters-bar">
              <select value={filters.employeeId} onChange={e => setFilters({ ...filters, employeeId: e.target.value })}>
                <option value="">Todos los empleados</option>
                {employees.map(emp => (
                  <option key={emp.id} value={emp.id}>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</option>
                ))}
              </select>
              <input type="date" value={filters.dateFrom} onChange={e => setFilters({ ...filters, dateFrom: e.target.value })} />
              <input type="date" value={filters.dateTo} onChange={e => setFilters({ ...filters, dateTo: e.target.value })} />
              <button className="btn btn-primary" onClick={handleFilter}>Filtrar</button>
            </div>

            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Fecha</th>
                  <th>Entrada</th>
                  <th>Salida</th>
                  <th>Horas</th>
                  <th>Tardía</th>
                  <th>H. Extra</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {allRecords.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">No hay registros</td></tr>
                ) : (
                  allRecords.map(rec => (
                    <tr key={rec.id}>
                      <td>{rec.employeeName}</td>
                      <td>{formatDate(rec.date)}</td>
                      <td>{formatTime(rec.checkInTime)}</td>
                      <td>{formatTime(rec.checkOutTime)}</td>
                      <td>{rec.workedHours ? `${parseFloat(rec.workedHours).toFixed(2)}h` : '-'}</td>
                      <td>
                        {rec.isLate
                          ? <span style={{color:'#e74c3c'}}>⚠ {rec.lateMinutes} min</span>
                          : <span style={{color:'#27ae60'}}>✓</span>}
                      </td>
                      <td>{rec.overtimeHours > 0 ? `${parseFloat(rec.overtimeHours).toFixed(2)}h` : '-'}</td>
                      <td>{getStatusBadge(rec.status)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Modal motivo horas extra */}
        {showOvertimeModal && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Motivo de Horas Extra</h2>
              </div>
              <p style={{marginBottom:'12px', color:'#555'}}>
                Está registrando salida después del horario laboral. Debe indicar el motivo de las horas extra.
              </p>
              <div className="form-group">
                <label>Motivo *</label>
                <textarea
                  value={overtimeReason}
                  onChange={e => setOvertimeReason(e.target.value)}
                  rows="3"
                  placeholder="Ej: Inventario, Atención de emergencia, Carga de trabajo..."
                  style={{width:'100%', padding:'8px', borderRadius:'6px', border:'1px solid #ddd'}}
                />
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => { setShowOvertimeModal(false); setOvertimeReason(''); }}>
                  Cancelar
                </button>
                <button className="btn btn-primary" onClick={handleOvertimeConfirm}>
                  Registrar Salida
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Attendance;