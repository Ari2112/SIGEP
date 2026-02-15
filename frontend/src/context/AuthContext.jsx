import { createContext, useContext, useState, useEffect } from 'react';
import { authAPI } from '../api/api';

const AuthContext = createContext(null);

// ============ MOCK DATA - REMOVER EN PRODUCCIÓN ============
const MOCK_USERS = {
  'admin': { id: 1, username: 'admin', role: 'Admin', employeeId: 1, fullName: 'Administrador Sistema', token: 'mock-token-admin' },
  'rrhh': { id: 2, username: 'rrhh', role: 'RRHH', employeeId: 2, fullName: 'María González', token: 'mock-token-rrhh' },
  'supervisor': { id: 3, username: 'supervisor', role: 'Jefatura', employeeId: 3, fullName: 'Carlos Ramírez', token: 'mock-token-supervisor' },
  'juan.perez': { id: 4, username: 'juan.perez', role: 'Empleado', employeeId: 4, fullName: 'Juan Pérez', token: 'mock-token-empleado' },
};
const MOCK_PASSWORD = 'admin123';
const USE_MOCK = true; // Cambiar a false cuando el backend esté listo
// ============ FIN MOCK DATA ============

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    
    if (token && savedUser) {
      setUser(JSON.parse(savedUser));
    }
    setLoading(false);
  }, []);

  const login = async (username, password) => {
    // ============ MOCK LOGIN - REMOVER EN PRODUCCIÓN ============
    if (USE_MOCK) {
      const mockUser = MOCK_USERS[username];
      if (mockUser && password === MOCK_PASSWORD) {
        localStorage.setItem('token', mockUser.token);
        localStorage.setItem('user', JSON.stringify(mockUser));
        setUser(mockUser);
        return { success: true };
      }
      return { success: false, message: 'Usuario o contraseña incorrectos' };
    }
    // ============ FIN MOCK LOGIN ============

    try {
      const response = await authAPI.login(username, password);
      const userData = response.data;
      
      localStorage.setItem('token', userData.token);
      localStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);
      
      return { success: true };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Error al iniciar sesión',
      };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
