import { useAuth } from '../context/AuthContext';
import Layout from '../components/Layout';
import './Dashboard.css';

const Dashboard = () => {
  const { user } = useAuth();

  return (
    <Layout>
      <div className="dashboard">
        <h1>Bienvenido, {user?.username}!</h1>
        
        <div className="dashboard-grid">
          <div className="card stat-card">
            <h3>👥 Empleados</h3>
            <p className="stat-number">-</p>
            <p className="stat-label">Total de empleados activos</p>
          </div>

          <div className="card stat-card">
            <h3>📋 Puestos</h3>
            <p className="stat-number">-</p>
            <p className="stat-label">Posiciones disponibles</p>
          </div>

          <div className="card stat-card">
            <h3>⏰ Horarios</h3>
            <p className="stat-number">-</p>
            <p className="stat-label">Horarios configurados</p>
          </div>

          <div className="card stat-card">
            <h3>✅ Asistencias Hoy</h3>
            <p className="stat-number">-</p>
            <p className="stat-label">Registros del día</p>
          </div>
        </div>

        <div className="card">
          <h2>Sistema Inicial</h2>
          <p>
            Este es el MVP (Producto Mínimo Viable) del Sistema Integral de Gestión de Personal.
          </p>
          <p>
            <strong>Funcionalidades disponibles:</strong>
          </p>
          <ul>
            <li>✅ Autenticación con JWT</li>
            <li>✅ Gestión de roles (Admin, RRHH, Jefatura, Empleado)</li>
            <li>✅ Visualización de empleados</li>
            <li>⏳ Más módulos próximamente...</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
};

export default Dashboard;
