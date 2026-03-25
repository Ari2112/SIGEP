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
import Payroll from './pages/Payroll';
import Settlements from './pages/Settlements';
import AnnualBonus from './pages/AnnualBonus';
import PerformanceEval from './pages/PerformanceEval';
import Disabilities from './pages/Disabilities';
import Reports from './pages/Reports';
import Profile from './pages/Profile';

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
          <Route path="/payroll" element={<ProtectedRoute><Payroll /></ProtectedRoute>} />
          <Route path="/settlements" element={<ProtectedRoute><Settlements /></ProtectedRoute>} />
          <Route path="/annual-bonus" element={<ProtectedRoute><AnnualBonus /></ProtectedRoute>} />
          <Route path="/performance" element={<ProtectedRoute><PerformanceEval /></ProtectedRoute>} />
          <Route path="/disabilities" element={<ProtectedRoute><Disabilities /></ProtectedRoute>} />
          <Route path="/reports" element={<ProtectedRoute><Reports /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          <Route
            path="*"
            element={
              <div style={{ padding: '50px', textAlign: 'center', fontFamily: 'system-ui' }}>
                <h1 style={{ fontSize: '3rem', color: '#667eea' }}>404</h1>
                <p>Página no encontrada</p>
                <a href="/dashboard" style={{ color: '#667eea' }}>← Volver al inicio</a>
              </div>
            }
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
