import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { attendanceAPI, employeeAPI } from '../api/api';
import { USE_MOCK, getTodayAttendance, getMyAttendanceRecords, getAllAttendanceRecords, mockEmployeesDetailed, mockAttendanceRecords } from '../api/mockData';
import Layout from '../components/Layout';
import './Attendance.css';

// Variable local para simular cambios en mock
let mockTodayRecord = null;

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
  const [filters, setFilters] = useState({
    employeeId: '',
    dateFrom: '',
    dateTo: ''
  });

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH' || user?.role === 'Jefatura';

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
        const existingTodayRecord = getTodayAttendance(employeeId);
        setTodayRecord(mockTodayRecord || existingTodayRecord);
        setMyRecords(getMyAttendanceRecords(employeeId));
        if (isManager) {
          setAllRecords(getAllAttendanceRecords());
          setEmployees(mockEmployeesDetailed);
        }
        setLoading(false);
        return;
      }
      // ============ FIN MOCK DATA ============

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
    
    // ============ MOCK CHECK IN ============
    if (USE_MOCK) {
      const now = new Date();
      mockTodayRecord = {
        id: Date.now(),
        employeeId: user?.employeeId || 4,
        employeeName: user?.fullName || 'Usuario',
        date: now.toISOString().split('T')[0],
        checkInTime: now.toISOString(),
        checkOutTime: null,
        workedHours: null,
        status: 'Parcial'
      };
      setTodayRecord(mockTodayRecord);
      setSuccessMessage('Entrada registrada exitosamente');
      setActionLoading(false);
      return;
    }
    // ============ FIN MOCK CHECK IN ============
    
    try {
      const res = await attendanceAPI.checkIn();
      setTodayRecord(res.data);
      setSuccessMessage('Entrada registrada exitosamente');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al registrar entrada');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCheckOut = async () => {
    setActionLoading(true);
    setError('');
    setSuccessMessage('');
    
    // ============ MOCK CHECK OUT ============
    if (USE_MOCK) {
      const now = new Date();
      const checkIn = new Date(mockTodayRecord.checkInTime);
      const workedHours = (now - checkIn) / (1000 * 60 * 60);
      mockTodayRecord = {
        ...mockTodayRecord,
        checkOutTime: now.toISOString(),
        workedHours: workedHours,
        status: 'Completo'
      };
      setTodayRecord(mockTodayRecord);
      setSuccessMessage('Salida registrada exitosamente');
      setActionLoading(false);
      return;
    }
    // ============ FIN MOCK CHECK OUT ============
    
    try {
      const res = await attendanceAPI.checkOut();
      setTodayRecord(res.data);
      setSuccessMessage('Salida registrada exitosamente');
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
    return new Date(dateStr).toLocaleTimeString('es-CR', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('es-CR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
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

  const canCheckIn = !todayRecord;
  const canCheckOut = todayRecord && todayRecord.checkInTime && !todayRecord.checkOutTime;

  if (loading) {
    return (
      <Layout>
        <div className="loading">Cargando...</div>
      </Layout>
    );
  }

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
          <button
            className={`tab ${activeTab === 'today' ? 'active' : ''}`}
            onClick={() => setActiveTab('today')}
          >
            Hoy
          </button>
          <button
            className={`tab ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            Mi Historial
          </button>
          {isManager && (
            <button
              className={`tab ${activeTab === 'all' ? 'active' : ''}`}
              onClick={() => setActiveTab('all')}
            >
              Todos los Registros
            </button>
          )}
        </div>

        {/* Tab: Hoy */}
        {activeTab === 'today' && (
          <div className="today-panel">
            <div className="today-card">
              <h2>Registro del Día</h2>
              <p className="today-date">{new Date().toLocaleDateString('es-CR', {
                weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
              })}</p>

              <div className="clock-display">
                {new Date().toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}
              </div>

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
                  </div>
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
                  onClick={handleCheckOut}
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
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {myRecords.length === 0 ? (
                  <tr><td colSpan="5" className="no-data">No hay registros de asistencia</td></tr>
                ) : (
                  myRecords.map(rec => (
                    <tr key={rec.id}>
                      <td>{formatDate(rec.date)}</td>
                      <td>{formatTime(rec.checkInTime)}</td>
                      <td>{formatTime(rec.checkOutTime)}</td>
                      <td>{rec.workedHours ? `${parseFloat(rec.workedHours).toFixed(2)}h` : '-'}</td>
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
                placeholder="Desde"
              />
              <input
                type="date"
                value={filters.dateTo}
                onChange={e => setFilters({ ...filters, dateTo: e.target.value })}
                placeholder="Hasta"
              />
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
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {allRecords.length === 0 ? (
                  <tr><td colSpan="6" className="no-data">No hay registros</td></tr>
                ) : (
                  allRecords.map(rec => (
                    <tr key={rec.id}>
                      <td>{rec.employeeName}</td>
                      <td>{formatDate(rec.date)}</td>
                      <td>{formatTime(rec.checkInTime)}</td>
                      <td>{formatTime(rec.checkOutTime)}</td>
                      <td>{rec.workedHours ? `${parseFloat(rec.workedHours).toFixed(2)}h` : '-'}</td>
                      <td>{getStatusBadge(rec.status)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Attendance;
