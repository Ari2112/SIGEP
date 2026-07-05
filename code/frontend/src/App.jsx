
//  MAPA DE RUTAS de la aplicación: define que pantalla se
//  muestra según la dirección URL en la que esté el usuario. Por ejemplo,
//  payroll muestra la pantalla de Planilla, employees la de Empleados, etc.
//
//  Además, envuelve TODO con dos cosas importantes:
//    - AuthProvider: para que toda la app conozca al usuario conectado.
//    - ProtectedRoute: para que las pantallas privadas exijan iniciar sesión.
// ============================================================================

import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

// Importamos todas las pantallas páginas del sistema.
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Employees from './pages/Employees';
import Vacations from './pages/Vacations';
import Permissions from './pages/Permissions';
import Attendance from './pages/Attendance';
import OvertimeHours from './pages/OvertimeHours';
import Payroll from './pages/Payroll';
import Settlements from './pages/Settlements';
import AnnualBonus from './pages/AnnualBonus';
import PerformanceEval from './pages/PerformanceEval';
import Disabilities from './pages/Disabilities';
import Reports from './pages/Reports';
import Profile from './pages/Profile';

function App() {
  return (
    // Todo va dentro de AuthProvider para compartir la sesión en toda la app.
    <AuthProvider>
      {/* El Router es el que vigila la dirección del navegador. */}
      <Router future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <Routes>
          {/* Ruta pública: cualquiera puede ver el login. */}
          <Route path="/login" element={<Login />} />

          {/* Rutas privadas: cada una se envuelve en <ProtectedRoute> para
              exigir que la persona haya iniciado sesión antes de entrar. */}
          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
          <Route path="/employees" element={<ProtectedRoute><Employees /></ProtectedRoute>} />
          <Route path="/vacations" element={<ProtectedRoute><Vacations /></ProtectedRoute>} />
          <Route path="/permissions" element={<ProtectedRoute><Permissions /></ProtectedRoute>} />
          <Route path="/attendance" element={<ProtectedRoute><Attendance /></ProtectedRoute>} />
          <Route path="/overtime" element={<ProtectedRoute><OvertimeHours /></ProtectedRoute>} />
          <Route path="/payroll" element={<ProtectedRoute><Payroll /></ProtectedRoute>} />
          <Route path="/settlements" element={<ProtectedRoute><Settlements /></ProtectedRoute>} />
          <Route path="/annual-bonus" element={<ProtectedRoute><AnnualBonus /></ProtectedRoute>} />
          <Route path="/performance" element={<ProtectedRoute><PerformanceEval /></ProtectedRoute>} />
          <Route path="/disabilities" element={<ProtectedRoute><Disabilities /></ProtectedRoute>} />
          <Route path="/reports" element={<ProtectedRoute><Reports /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

          {/* Si alguien entra a la raíz "/", lo mandamos directo al dashboard. */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          {/* Ruta comodín (*): atrapa cualquier dirección que no exista y
              muestra una página de error 404 amigable. */}
          <Route
            path="*"
            element={
              <div style={{ padding: '50px', textAlign: 'center', fontFamily: 'system-ui' }}>
                <h1 style={{ fontSize: '3rem', color: '#2D6A1F' }}>404</h1>
                <p>Página no encontrada</p>
                <a href="/dashboard" style={{ color: '#7DC221' }}>← Volver al inicio</a>
              </div>
            }
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;