//  Antes de dejar pasar a alguien, revisa dos cosas:
//    1. ¿La persona inició sesión? Si no, la manda al login.
//    2. ¿Tiene el rol permitido para esta pantalla? Si no, la manda a una
//       página de "no autorizado".
//  Solo si pasa ambos filtros, muestra el contenido protegido.

import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// "children" es el contenido que queremos proteger que es la pantalla en sí.
// "allowedRoles" es la lista de roles que tienen permiso de entrar.
const ProtectedRoute = ({ children, allowedRoles }) => {
  const { user, loading } = useAuth();

  // Mientras todavía estamos revisando si hay sesión, mostramos un aviso de
  // "Cargando..." para no decidir nada antes de tiempo.
  if (loading) {
    return <div>Cargando...</div>;
  }

  // Filtro 1: si no hay usuario conectado, lo enviamos al login.
  // ("replace" evita que pueda volver atrás con el botón del navegador.)
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Filtro 2: si la pantalla exige ciertos roles y el del usuario no está en
  // la lista, lo enviamos a la página de "no autorizado".
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />;
  }

  // Pasó ambos filtros: mostramos el contenido protegido.
  return children;
};

export default ProtectedRoute;