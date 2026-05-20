import { useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

const NAV_ITEMS = [
  { path: '/dashboard', label: 'Inicio', icon: '⊞', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
{ path: '/attendance', label: 'Asistencia', icon: '✓', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
{ path: '/vacations', label: 'Vacaciones', icon: '✈', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
{ path: '/permissions', label: 'Permisos', icon: '📋', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
{ path: '/disabilities', label: 'Incapacidades', icon: '🏥', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
{ path: '/performance', label: 'Desempeño', icon: '⭐', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { divider: true, label: 'Gestión', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/overtime', label: 'Horas Extra', icon: '⏱', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/payroll', label: 'Planilla', icon: '💳', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/annual-bonus', label: 'Aguinaldo', icon: '🎁', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/settlements', label: 'Liquidaciones', icon: '📄', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/reports', label: 'Reportes', icon: '📊', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
{ path: '/employees', label: 'Empleados', icon: '👥', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
];
const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(false);

  const avatarKey = `sigep_avatar_${user?.id}`;
  const [avatar, setAvatar] = useState(() => localStorage.getItem(avatarKey) || null);

  useEffect(() => {
    const sync = () => setAvatar(localStorage.getItem(avatarKey) || null);
    window.addEventListener('sigep_avatar_updated', sync);
    return () => window.removeEventListener('sigep_avatar_updated', sync);
  }, [avatarKey]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const visibleItems = NAV_ITEMS.filter(item => {
    if (!item.roles) return false;
    return item.roles.includes(user?.role);
  });

  const initials = user?.fullName
    ? user.fullName.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
    : user?.username?.slice(0, 2).toUpperCase() || '??';

  const roleLabel = {
    Admin: 'Administrador',
    RRHH: 'Recursos Humanos',
    Jefatura: 'Jefatura',
    Empleado: 'Empleado',
  }[user?.role] || user?.role;

  const currentLabel = [
    ...visibleItems,
    { path: '/profile', label: 'Mi Perfil' },
  ].find(i => i.path === location.pathname)?.label || 'SIGEP';

  return (
    <div className={`layout ${collapsed ? 'sidebar-collapsed' : ''}`}>
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div className="sidebar-logo">
            {collapsed ? 'S' : 'SIGEP'}
          </div>
          <button className="collapse-btn" onClick={() => setCollapsed(!collapsed)}>
            {collapsed ? '›' : '‹'}
          </button>
        </div>

        <nav className="sidebar-nav">
          {visibleItems.map((item, idx) => {
            if (item.divider) {
              return (
                <div key={idx} className="nav-divider">
                  {!collapsed && <span>{item.label}</span>}
                </div>
              );
            }
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`nav-item ${isActive ? 'active' : ''}`}
                title={collapsed ? item.label : ''}
              >
                <span className="nav-icon">{item.icon}</span>
                {!collapsed && <span className="nav-label">{item.label}</span>}
              </Link>
            );
          })}
        </nav>

        <div className="sidebar-footer">
          <div className="user-avatar-sm">
            {avatar
              ? <img src={avatar} alt="avatar" />
              : initials
            }
          </div>
          {!collapsed && (
            <div className="user-info">
              <span className="user-name">{user?.username}</span>
              <span className="user-role">{roleLabel}</span>
            </div>
          )}
          <button onClick={handleLogout} className="logout-btn" title="Cerrar sesión">
            ⏻
          </button>
        </div>
      </aside>

      {/* Contenido principal */}
      <div className="main-wrapper">
        <header className="topbar">
          <div className="topbar-left">
            <h2 className="page-title">{currentLabel}</h2>
          </div>
          <div className="topbar-right">
            <Link to="/profile" className="topbar-avatar" title="Mi Perfil">
              {avatar
                ? <img src={avatar} alt="avatar" />
                : <span>{initials}</span>
              }
            </Link>
          </div>
        </header>
        <main className="main-content">
          <div className="container">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
};

export default Layout;
