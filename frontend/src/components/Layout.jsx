import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="layout">
      <header className="header">
        <div className="header-content">
          <h1 className="logo">SIGEP</h1>
          <nav className="nav">
            <Link to="/dashboard" className="nav-link">Dashboard</Link>
            <Link to="/employees" className="nav-link">Empleados</Link>
            {(user?.role === 'Admin' || user?.role === 'RRHH') && (
              <>
                <Link to="/positions" className="nav-link">Puestos</Link>
                <Link to="/schedules" className="nav-link">Horarios</Link>
              </>
            )}
          </nav>
          <div className="user-menu">
            <span className="user-name">{user?.username}</span>
            <span className="user-role">({user?.role})</span>
            <button onClick={handleLogout} className="btn btn-secondary btn-sm">
              Cerrar Sesión
            </button>
          </div>
        </div>
      </header>
      <main className="main-content">
        <div className="container">
          {children}
        </div>
      </main>
      <footer className="footer">
        <p>&copy; 2026 SIGEP - Sistema Integral de Gestión de Personal</p>
      </footer>
    </div>
  );
};

export default Layout;
