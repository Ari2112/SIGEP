import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Employees from './pages/Employees';
import Vacations from './pages/Vacations';
import Permissions from './pages/Permissions';
import Attendance from './pages/Attendance';
import OvertimeHours from './pages/OvertimeHours';

function App() {
  return (
    <AuthProvider>
      <Router future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
          <Route path="/employees" element={<ProtectedRoute><Employees /></ProtectedRoute>} />
          <Route path="/vacations" element={<ProtectedRoute><Vacations /></ProtectedRoute>} />
          <Route path="/permissions" element={<ProtectedRoute><Permissions /></ProtectedRoute>} />
          <Route path="/attendance" element={<ProtectedRoute><Attendance /></ProtectedRoute>} />
          <Route path="/overtime" element={<ProtectedRoute><OvertimeHours /></ProtectedRoute>} />

          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          <Route
            path="*"
            element={
              <div style={{ padding: '50px', textAlign: 'center' }}>
                <h1>404 - Página no encontrada</h1>
                <a href="/dashboard">Volver al inicio</a>
              </div>
            }
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
