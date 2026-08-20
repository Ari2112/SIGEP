//  Este archivo es la memoria de sesión compartida de toda la aplicación.
//  Aquí vive la información de QUIÉN está conectado, y las funciones para
//  iniciar y cerrar sesión. 
import { createContext, useContext, useState, useEffect } from 'react';
import { authAPI } from '../api/api';

const AuthContext = createContext(null);

// Mientras el backend no estaba listo, se usaban estos usuarios falsos para
// poder probar las pantallas. Hoy ya no se usan (USE_MOCK está en false),
// pero se dejan documentados por si se necesita probar sin servidor.
const MOCK_USERS = {
  'admin': { id: 1, username: 'admin', role: 'Admin', employeeId: 1, fullName: 'Administrador Sistema', token: 'mock-token-admin' },
  'rrhh': { id: 2, username: 'rrhh', role: 'RRHH', employeeId: 2, fullName: 'María González', token: 'mock-token-rrhh' },
  'supervisor': { id: 3, username: 'supervisor', role: 'Jefatura', employeeId: 3, fullName: 'Carlos Ramírez', token: 'mock-token-supervisor' },
  'juan.perez': { id: 4, username: 'juan.perez', role: 'Empleado', employeeId: 4, fullName: 'Juan Pérez', token: 'mock-token-empleado' },
};
const MOCK_PASSWORD = 'admin123';
const USE_MOCK = false; // false = usar el backend real. true = usar datos de prueba.

// AuthProvider es el envoltorio que se coloca alrededor de toda la app para
// que todas las pantallas tengan acceso a la información de sesión.
export const AuthProvider = ({ children }) => {
  // "user" guarda los datos del usuario conectado (o null si no hay nadie).
  const [user, setUser] = useState(null);

  // "loading" indica si todavía estamos revisando si había una sesión guardada.
  const [loading, setLoading] = useState(true);

  //  useEffect: esto se ejecuta UNA sola vez, cuando la app arranca. Sirve para
  //  recordar al usuario si ya había iniciado sesión antes (así no tiene que
  //  volver a loguearse cada vez que refresca la página).

  useEffect(() => {
    // Buscamos en el almacenamiento del navegador si quedó guardado un token
    // y los datos del usuario de una sesión anterior.
    const token = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');

    if (token && savedUser) {
      const raw = JSON.parse(savedUser);

      // Normalizamos los datos: en algún momento se guardaron con nombres en
      // mayúscula inicial (UserId, Role...) y en otros con minúscula (userId,
      // role...). Con "??" tomamos el primero que exista, para que funcione sin
      // importar cómo quedó guardado. Es una red de seguridad ante datos viejos.
      const normalized = {
        id:         raw.id         ?? raw.UserId     ?? raw.userId,
        username:   raw.username   ?? raw.Username,
        role:       raw.role       ?? raw.Role       ?? 'Empleado',
        employeeId: raw.employeeId ?? raw.EmployeeId ?? null,
        fullName:   raw.fullName   ?? raw.FullName   ?? '',
        token:      raw.token      ?? raw.Token      ?? token,
      };
      setUser(normalized);
    }
    // Terminamos de revisar y ya podemos mostrar la app.
    setLoading(false);
  }, []);
  //  login: valida al usuario contra el backend (o contra los datos de prueba
  //  si USE_MOCK estuviera activo) y, si todo va bien, guarda la sesión.
  const login = async (username, password) => {
    // Modo de prueba que para este momento ya esta desactivado
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
    // ---- Fin modo de prueba ----

    try {
      // Llamamos al backend real para validar las credenciales.
      const response = await authAPI.login(username, password);
      const data = response.data;

      // Igual que arriba, aceptamos los datos vengan en mayúscula o minúscula.
      const userData = {
        id:         data.userId     ?? data.UserId     ?? data.id,
        username:   data.username   ?? data.Username   ?? username,
        role:       data.role       ?? data.Role       ?? 'Empleado',
        employeeId: data.employeeId ?? data.EmployeeId ?? null,
        fullName:   data.fullName   ?? data.FullName   ?? '',
        token:      data.token      ?? data.Token      ?? '',
      };

      // Guardamos la sesión en el navegador para recordarla más adelante.
      localStorage.setItem('token', userData.token);
      localStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);

      return { success: true };
    } catch (error) {
      // Si el backend respondió con un error
      return {
        success: false,
        message: error.response?.data?.message || 'Error al iniciar sesión',
      };
    }
  };
  //  logout: cierra la sesión. Borra los datos guardados y deja "user" vacío.
  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  // Ponemos a disposición de toda la app: el usuario, las funciones de
  // iniciar/cerrar sesión y el estado de carga.
  return (
    <AuthContext.Provider value={{ user, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};
//  useAuth: un "atajo" para que cualquier pantalla acceda fácilmente a la
//  información de sesión, escribiendo solo: const { user } = useAuth();
export const useAuth = () => {
  const context = useContext(AuthContext);
  // Si alguien intenta usar esto fuera del AuthProvider, avisamos con un error
  // claro para detectar el problema rápido durante el desarrollo.
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};