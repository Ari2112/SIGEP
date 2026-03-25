import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { reportAPI, attendanceAPI } from '../api/api';
import Layout from '../components/Layout';
import './Dashboard.css';

const Dashboard = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [stats, setStats] = useState(null);
  const [todayAttendance, setTodayAttendance] = useState(null);

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH';

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      const promises = [];
      if (isManager) promises.push(reportAPI.getDashboardStats());
      promises.push(attendanceAPI.getToday());
      const results = await Promise.all(promises);
      if (isManager) setStats(results[0].data);
      setTodayAttendance(isManager ? results[1].data : results[0].data);
    } catch (_) {
      // Silently fail — dashboard funciona sin stats
    }
  };

  const formatCurrency = (v) => new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v || 0);

  const quickLinks = [
    { icon: '✓', label: 'Asistencia', path: '/attendance', color: 'primary', desc: 'Marcar entrada/salida' },
    { icon: '✈', label: 'Vacaciones', path: '/vacations', color: 'success', desc: 'Solicitar días libres' },
    { icon: '📋', label: 'Permisos', path: '/permissions', color: 'info', desc: 'Pedir permiso especial' },
    { icon: '🏥', label: 'Incapacidades', path: '/disabilities', color: 'warning', desc: 'Registrar incapacidad' },
    { icon: '⭐', label: 'Desempeño', path: '/performance', color: 'secondary', desc: 'Ver mis evaluaciones' },
    ...(isManager ? [
      { icon: '⏱', label: 'Horas Extra', path: '/overtime', color: 'warning', desc: 'Aprobar horas extra' },
      { icon: '💳', label: 'Planilla', path: '/payroll', color: 'success', desc: 'Gestionar planillas' },
      { icon: '🎁', label: 'Aguinaldo', path: '/annual-bonus', color: 'primary', desc: 'Calcular aguinaldo' },
      { icon: '📄', label: 'Liquidaciones', path: '/settlements', color: 'danger', desc: 'Procesar liquidaciones' },
      { icon: '📊', label: 'Reportes', path: '/reports', color: 'info', desc: 'Generar informes' },
      { icon: '👥', label: 'Empleados', path: '/employees', color: 'secondary', desc: 'Gestionar personal' },
    ] : [])
  ];

  return (
    <Layout>
      <div className="dashboard">

        {/* Bienvenida */}
        <div className="welcome-banner">
          <div className="welcome-text">
            <h1>Bienvenido, <span>{user?.fullName || user?.username}</span></h1>
            <p>{new Date().toLocaleDateString('es-CR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</p>
          </div>
        </div>

        {/* Estado de asistencia hoy */}
        {todayAttendance !== null && (
          <div className={`attendance-banner ${todayAttendance?.checkInTime ? (todayAttendance?.checkOutTime ? 'done' : 'checked-in') : 'not-checked'}`}>
            <span className="attendance-icon">
              {!todayAttendance?.checkInTime ? '⚪' : todayAttendance?.checkOutTime ? '✅' : '🟢'}
            </span>
            <span className="attendance-text">
              {!todayAttendance?.checkInTime
                ? 'No has marcado entrada hoy'
                : todayAttendance?.checkOutTime
                  ? `Jornada completa — ${todayAttendance.workedHours}h trabajadas`
                  : `Entrada marcada — ${new Date(todayAttendance.checkInTime).toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' })}`
              }
            </span>
            <button className="btn btn-sm btn-ghost" onClick={() => navigate('/attendance')}>
              Ir a Asistencia →
            </button>
          </div>
        )}

        {/* Estadísticas rápidas (solo managers) */}
        {isManager && stats && (
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon employees">👥</div>
              <div className="stat-info">
                <span className="stat-value">{stats.activeEmployees}</span>
                <span className="stat-label">Empleados activos</span>
              </div>
            </div>
            <div className="stat-card" onClick={() => navigate('/vacations')} style={{ cursor: 'pointer' }}>
              <div className="stat-icon vacations">✈</div>
              <div className="stat-info">
                <span className="stat-value">{stats.pendingVacations}</span>
                <span className="stat-label">Vacaciones pendientes</span>
              </div>
            </div>
            <div className="stat-card" onClick={() => navigate('/overtime')} style={{ cursor: 'pointer' }}>
              <div className="stat-icon overtime">⏱</div>
              <div className="stat-info">
                <span className="stat-value">{stats.pendingOvertimes}</span>
                <span className="stat-label">H. Extra por revisar</span>
              </div>
            </div>
            <div className="stat-card" onClick={() => navigate('/disabilities')} style={{ cursor: 'pointer' }}>
              <div className="stat-icon disabilities">🏥</div>
              <div className="stat-info">
                <span className="stat-value">{stats.pendingDisabilities}</span>
                <span className="stat-label">Incapacidades pendientes</span>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon attendance">✓</div>
              <div className="stat-info">
                <span className="stat-value">{stats.checkedInToday}/{stats.activeEmployees}</span>
                <span className="stat-label">Asistencia hoy</span>
              </div>
            </div>
            {stats.monthlyPayrollTotal > 0 && (
              <div className="stat-card" onClick={() => navigate('/payroll')} style={{ cursor: 'pointer' }}>
                <div className="stat-icon payroll">💳</div>
                <div className="stat-info">
                  <span className="stat-value stat-value-sm">{formatCurrency(stats.monthlyPayrollTotal)}</span>
                  <span className="stat-label">Última planilla neta</span>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Accesos rápidos */}
        <div className="section-header">
          <h2>Accesos rápidos</h2>
        </div>
        <div className="quick-links-grid">
          {quickLinks.map(link => (
            <div key={link.path} className={`quick-card quick-card-${link.color}`} onClick={() => navigate(link.path)}>
              <span className="quick-icon">{link.icon}</span>
              <div className="quick-info">
                <span className="quick-label">{link.label}</span>
                <span className="quick-desc">{link.desc}</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </Layout>
  );
};

export default Dashboard;
