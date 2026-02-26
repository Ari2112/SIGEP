import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Layout from '../components/Layout';
import './Dashboard.css';

const Dashboard = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const isManager = user?.role === 'Admin' || user?.role === 'RRHH';

  const modules = [
    {
      icon: '🕐',
      title: 'Asistencia',
      description: 'Registra tu entrada y salida diaria',
      path: '/attendance',
      roles: ['Admin', 'RRHH', 'Jefatura', 'Empleado']
    },
    {
      icon: '🌴',
      title: 'Vacaciones',
      description: 'Solicita y gestiona tus vacaciones',
      path: '/vacations',
      roles: ['Admin', 'RRHH', 'Jefatura', 'Empleado']
    },
    {
      icon: '📋',
      title: 'Permisos',
      description: 'Solicita permisos especiales',
      path: '/permissions',
      roles: ['Admin', 'RRHH', 'Jefatura', 'Empleado']
    },
    {
      icon: '⏰',
      title: 'Horas Extra',
      description: 'Revisa y aprueba horas extra detectadas',
      path: '/overtime',
      roles: ['Admin', 'RRHH']
    },
    {
      icon: '👥',
      title: 'Empleados',
      description: 'Gestiona el personal de la empresa',
      path: '/employees',
      roles: ['Admin', 'RRHH']
    },
  ];

  const visibleModules = modules.filter(m => m.roles.includes(user?.role));

  return (
    <Layout>
      <div className="dashboard">
        <div className="dashboard-welcome">
          <h1>Bienvenido, {user?.username}</h1>
          <p className="welcome-subtitle">Panel de Control — {user?.role}</p>
        </div>

        <div className="modules-grid">
          {visibleModules.map(mod => (
            <div
              key={mod.path}
              className="module-card"
              onClick={() => navigate(mod.path)}
            >
              <span className="module-icon">{mod.icon}</span>
              <h3>{mod.title}</h3>
              <p>{mod.description}</p>
            </div>
          ))}
        </div>

        <div className="card status-card">
          <h2>Estado del Sistema</h2>
          <ul className="status-list">
            <li><span className="status-dot green"></span> Autenticación JWT</li>
            <li><span className="status-dot green"></span> Gestión de Empleados</li>
            <li><span className="status-dot green"></span> Módulo de Asistencia</li>
            <li><span className="status-dot green"></span> Módulo de Vacaciones</li>
            <li><span className="status-dot green"></span> Módulo de Permisos</li>
            <li><span className="status-dot green"></span> Módulo de Horas Extra</li>
            <li><span className="status-dot yellow"></span> Planilla (próximamente)</li>
            <li><span className="status-dot yellow"></span> Aguinaldo (próximamente)</li>
            <li><span className="status-dot yellow"></span> Liquidaciones (próximamente)</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
};

export default Dashboard;
