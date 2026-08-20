import { useState, useEffect, useRef } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

/* ============================================================
   Iconos de LÍNEA (stroke) — estilo uniforme en todo el sistema.
   Todos comparten el mismo grosor de trazo y usan currentColor,
   por lo que heredan el color del enlace activo/inactivo.
   ============================================================ */
const stroke = {
  width: 20,
  height: 20,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.8,
  strokeLinecap: 'round',
  strokeLinejoin: 'round',
};

const Icon = {
  dashboard: (p) => (
    <svg {...stroke} {...p}><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/></svg>
  ),
  attendance: (p) => (
    <svg {...stroke} {...p}><circle cx="12" cy="12" r="9"/><path d="M8.5 12.5l2.5 2.5 4.5-5"/></svg>
  ),
  vacations: (p) => (
    <svg {...stroke} {...p}><path d="M2 22h20"/><path d="M6 18c4-6 9-9 14-9 0 0-1 6-7 8-3 1-5 1-7 1z"/><path d="M12 13c-2-3-2-7 1-10"/></svg>
  ),
  permissions: (p) => (
    <svg {...stroke} {...p}><rect x="5" y="4" width="14" height="17" rx="2"/><path d="M9 4h6v3H9z"/><path d="M8.5 11h7M8.5 15h7"/></svg>
  ),
  disabilities: (p) => (
    <svg {...stroke} {...p}><path d="M20.8 8.6a5 5 0 0 0-8.8-2.3A5 5 0 0 0 3.2 8.6c0 3.7 4.5 7 8.8 10.4 4.3-3.4 8.8-6.7 8.8-10.4z"/><path d="M12 9v5M9.5 11.5h5"/></svg>
  ),
  performance: (p) => (
    <svg {...stroke} {...p}><path d="M12 3.5l2.6 5.3 5.9.9-4.2 4.1 1 5.8L12 17l-5.3 2.6 1-5.8-4.2-4.1 5.9-.9z"/></svg>
  ),
  overtime: (p) => (
    <svg {...stroke} {...p}><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>
  ),
  payroll: (p) => (
    <svg {...stroke} {...p}><rect x="2.5" y="5" width="19" height="14" rx="2"/><path d="M2.5 9.5h19"/><path d="M6 14.5h4"/></svg>
  ),
  bonus: (p) => (
    <svg {...stroke} {...p}><rect x="3" y="8" width="18" height="13" rx="1.5"/><path d="M3 12h18M12 8v13"/><path d="M12 8c-1.5-3-5-3-5-1s2.5 1 5 1zM12 8c1.5-3 5-3 5-1s-2.5 1-5 1z"/></svg>
  ),
  settlements: (p) => (
    <svg {...stroke} {...p}><path d="M7 3h7l5 5v13a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z"/><path d="M14 3v5h5"/><path d="M9 13h6M9 17h6"/></svg>
  ),
  reports: (p) => (
    <svg {...stroke} {...p}><path d="M4 4v16h16"/><path d="M8 16v-4M12 16V8M16 16v-6"/></svg>
  ),
  employees: (p) => (
    <svg {...stroke} {...p}><circle cx="9" cy="8" r="3.2"/><path d="M3.5 19c0-3 2.5-5 5.5-5s5.5 2 5.5 5"/><path d="M16 6.5a3 3 0 0 1 0 5.8M17.5 19c0-2.2-1-4-2.7-4.7"/></svg>
  ),
  logout: (p) => (
    <svg {...stroke} {...p}><path d="M14 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3"/><path d="M9 12h11M16.5 8.5L20 12l-3.5 3.5"/></svg>
  ),
};

const NAV_ITEMS = [
  { path: '/dashboard',    label: 'Inicio',        icon: 'dashboard',    roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { path: '/attendance',   label: 'Asistencia',    icon: 'attendance',   roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { path: '/vacations',    label: 'Vacaciones',    icon: 'vacations',    roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { path: '/permissions',  label: 'Permisos',      icon: 'permissions',  roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { path: '/disabilities', label: 'Incapacidades', icon: 'disabilities', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { path: '/performance',  label: 'Desempeño',     icon: 'performance',  roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos', 'Jefatura', 'Empleado'] },
  { divider: true, label: 'Gestión', roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/overtime',     label: 'Horas Extra',   icon: 'overtime',     roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/payroll',      label: 'Planilla',      icon: 'payroll',      roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/annual-bonus', label: 'Aguinaldo',     icon: 'bonus',        roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/settlements',  label: 'Liquidaciones', icon: 'settlements',  roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/reports',      label: 'Reportes',      icon: 'reports',      roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
  { path: '/employees',    label: 'Empleados',     icon: 'employees',    roles: ['Admin', 'Administrador', 'RRHH', 'Recursos Humanos'] },
];

const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(false);

  // Conserva la posición del scroll del menú al navegar (cada página monta su
  // propio Layout, así que sin esto el menú "se devuelve para arriba").
  const navRef = useRef(null);
  useEffect(() => {
    const el = navRef.current;
    if (!el) return;
    const saved = sessionStorage.getItem('sigep_nav_scroll');
    if (saved) el.scrollTop = parseInt(saved, 10) || 0;

    const onScroll = () => sessionStorage.setItem('sigep_nav_scroll', String(el.scrollTop));
    el.addEventListener('scroll', onScroll, { passive: true });
    return () => el.removeEventListener('scroll', onScroll);
  }, []);

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
    Administrador: 'Administrador',
    RRHH: 'Recursos Humanos',
    'Recursos Humanos': 'Recursos Humanos',
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

        <nav className="sidebar-nav" ref={navRef}>
          {visibleItems.map((item, idx) => {
            if (item.divider) {
              return (
                <div key={idx} className="nav-divider">
                  {!collapsed && <span>{item.label}</span>}
                </div>
              );
            }
            const isActive = location.pathname === item.path;
            const IconCmp = Icon[item.icon];
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`nav-item ${isActive ? 'active' : ''}`}
                title={collapsed ? item.label : ''}
              >
                <span className="nav-icon">{IconCmp ? <IconCmp /> : null}</span>
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
            <Icon.logout />
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
