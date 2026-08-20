//  Esta es la PANTALLA de inicio de sesión: el formulario donde la persona
//  escribe su usuario y contraseña.

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import logoSIGEP from '../assets/logoSIGEP.jpeg';
import './Login.css';

const Login = () => {
  // Estas son las cajitas de memoria de la pantalla. React las vuelve a
  // dibujar automáticamente cada vez que cambian:
  const [username, setUsername] = useState('');   // lo que la persona escribe como usuario
  const [password, setPassword] = useState('');   // lo que escribe como contraseña
  const [error, setError] = useState('');         // mensaje de error a mostrar (si hay)
  const [loading, setLoading] = useState(false);  // si estamos esperando respuesta del servidor

  const { login } = useAuth();

  // navigate nos permite cambiar de pantalla por código, que es basicamente ir al dashboard.
  const navigate = useNavigate();
  //  handleSubmit: se ejecuta cuando la persona presiona "Iniciar Sesión".
  const handleSubmit = async (e) => {
    e.preventDefault();   // evita que la página se recargue sola al enviar el formulario
    setError('');         // limpiamos cualquier error anterior
    setLoading(true);     // mostramos el estado "cargando..."

    // Le pedimos al backend que verifique el usuario y la contraseña.
    const result = await login(username, password);

    // Si las credenciales fueron correctas, lo llevamos al panel principal.
    if (result.success) {
      navigate('/dashboard');
    } else {
      // Si no, mostramos en pantalla el mensaje de error que devolvió el sistema.
      setError(result.message);
    }

    setLoading(false);   // quitamos el estado "cargando..."
  };

  //  la parte visual del formulario
  return (
    <div className="login-container">
      <div className="login-card">

        {/* Logo y títulos de la institución */}
        <div className="login-logo">
          <img src={logoSIGEP} alt="Logo Centro Agrícola Cantonal Coronado" />
        </div>
        <h1 className="login-title">SIGEP</h1>
        <p className="login-subtitle">Sistema Integral de Gestión de Personal</p>
        <p className="login-org">Centro Agrícola Cantonal Coronado</p>

        {/* Este bloque solo aparece si HAY un error que mostrar */}
        {error && (
          <div className="alert alert-error">
            {error}
          </div>
        )}

        {/* El formulario en sí. Al enviarlo, llama a handleSubmit */}
        <form onSubmit={handleSubmit}>

          {/* Campo de usuario. Cada vez que la persona escribe, se actualiza
              la cajita de memoria username. */}
          <div className="form-group">
            <label htmlFor="username">Usuario</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              disabled={loading}   // se bloquea mientras se está validando
              autoFocus            // el cursor empieza aquí automáticamente
            />
          </div>

          {/* Campo de contraseña. Igual que el anterior, pero oculta el texto. */}
          <div className="form-group">
            <label htmlFor="password">Contraseña</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={loading}
            />
          </div>

          {/* Botón de envío. Mientras carga, cambia el texto y se desactiva
              para evitar que la persona presione dos veces. */}
          <button
            type="submit"
            className="btn btn-primary btn-block"
            disabled={loading}
          >
            {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;