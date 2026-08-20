import { useState, useRef } from 'react';
import { useAuth } from '../context/AuthContext';
import { authAPI } from '../api/api';
import Layout from '../components/Layout';
import './Profile.css';

const ROLE_LABELS = {
  Admin: 'Administrador',
  RRHH: 'Recursos Humanos',
  Jefatura: 'Jefatura',
  Empleado: 'Empleado',
};

const Profile = () => {
  const { user } = useAuth();
  const fileInputRef = useRef(null);

  const avatarKey = `sigep_avatar_${user?.id}`;
  const [avatar, setAvatar] = useState(() => localStorage.getItem(avatarKey) || null);
  const [uploading, setUploading] = useState(false);
  const [success, setSuccess] = useState(false);

  const initials = user?.fullName
    ? user.fullName.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
    : user?.username?.slice(0, 2).toUpperCase() || '??';

  const roleLabel = ROLE_LABELS[user?.role] || user?.role;

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) return;
    if (file.size > 2 * 1024 * 1024) {
      alert('La imagen no debe superar 2 MB.');
      return;
    }

    setUploading(true);
    const reader = new FileReader();
    reader.onload = (ev) => {
      const dataUrl = ev.target.result;
      localStorage.setItem(avatarKey, dataUrl);
      setAvatar(dataUrl);
      setUploading(false);
      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000);
      // Dispatch event so Layout updates instantly
      window.dispatchEvent(new Event('sigep_avatar_updated'));
    };
    reader.readAsDataURL(file);
  };

  const handleRemoveAvatar = () => {
    localStorage.removeItem(avatarKey);
    setAvatar(null);
    window.dispatchEvent(new Event('sigep_avatar_updated'));
  };

  const infoRows = [
    { label: 'Nombre completo', value: user?.fullName || '—' },
    { label: 'Usuario', value: user?.username },
    { label: 'Rol', value: roleLabel },
    { label: 'ID de empleado', value: user?.employeeId || '—' },
  ];

  // --- Cambio de contraseña ---
  const [pwdForm, setPwdForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [pwdError, setPwdError] = useState('');
  const [pwdSuccess, setPwdSuccess] = useState('');
  const [pwdSaving, setPwdSaving] = useState(false);

  const handlePwdChange = (field) => (e) => {
    setPwdForm({ ...pwdForm, [field]: e.target.value });
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    setPwdError('');
    setPwdSuccess('');

    if (!pwdForm.currentPassword || !pwdForm.newPassword || !pwdForm.confirmPassword) {
      setPwdError('Completa los tres campos.');
      return;
    }
    if (pwdForm.newPassword.length < 6) {
      setPwdError('La nueva contraseña debe tener al menos 6 caracteres.');
      return;
    }
    if (pwdForm.newPassword !== pwdForm.confirmPassword) {
      setPwdError('La confirmación no coincide con la nueva contraseña.');
      return;
    }

    try {
      setPwdSaving(true);
      await authAPI.changePassword(pwdForm.currentPassword, pwdForm.newPassword);
      setPwdSuccess('Contraseña actualizada correctamente.');
      setPwdForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch (err) {
      setPwdError(err.response?.data?.message || 'No se pudo cambiar la contraseña.');
    } finally {
      setPwdSaving(false);
    }
  };

  return (
    <Layout>
      <div className="profile-page">
        <div className="page-header">
          <div>
            <h1 className="page-title-main">Mi Perfil</h1>
            <p className="page-subtitle">Información de tu cuenta y foto de perfil</p>
          </div>
        </div>

        <div className="profile-grid">
          {/* Avatar card */}
          <div className="card profile-avatar-card">
            <div className="avatar-display">
              {avatar
                ? <img src={avatar} alt="Avatar" className="avatar-img" />
                : <div className="avatar-placeholder">{initials}</div>
              }
            </div>

            <div className="avatar-meta">
              <p className="avatar-name">{user?.fullName || user?.username}</p>
              <span className={`badge badge-${user?.role === 'Admin' ? 'danger' : user?.role === 'RRHH' ? 'primary' : user?.role === 'Jefatura' ? 'warning' : 'secondary'}`}>
                {roleLabel}
              </span>
            </div>

            {success && (
              <div className="alert alert-success avatar-success">
                Foto actualizada correctamente
              </div>
            )}

            <div className="avatar-actions">
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                style={{ display: 'none' }}
                onChange={handleFileChange}
              />
              <button
                className="btn btn-primary"
                onClick={() => fileInputRef.current?.click()}
                disabled={uploading}
              >
                {uploading ? 'Cargando...' : avatar ? 'Cambiar foto' : 'Subir foto'}
              </button>
              {avatar && (
                <button className="btn btn-ghost" onClick={handleRemoveAvatar}>
                  Eliminar
                </button>
              )}
            </div>

            <p className="avatar-hint">
              Formatos: JPG, PNG, GIF · Máximo 2 MB
            </p>
          </div>

          {/* Info card */}
          <div className="card profile-info-card">
            <h3 className="profile-section-title">Información de la cuenta</h3>
            <div className="profile-info-list">
              {infoRows.map(row => (
                <div key={row.label} className="profile-info-row">
                  <span className="profile-info-label">{row.label}</span>
                  <span className="profile-info-value">{row.value}</span>
                </div>
              ))}
            </div>

            <div className="profile-note info-box">
              <strong>Nota:</strong> Para modificar tus datos personales, contacta al área de Recursos Humanos.
            </div>
          </div>

          {/* Cambio de contraseña */}
          <div className="card profile-info-card profile-password-card">
            <h3 className="profile-section-title">Cambiar contraseña</h3>

            {pwdError && <div className="alert alert-error">{pwdError}</div>}
            {pwdSuccess && <div className="alert alert-success">{pwdSuccess}</div>}

            <form onSubmit={handleChangePassword} className="profile-password-form">
              <div className="form-group">
                <label>Contraseña actual</label>
                <input
                  type="password"
                  value={pwdForm.currentPassword}
                  onChange={handlePwdChange('currentPassword')}
                  autoComplete="current-password"
                />
              </div>
              <div className="form-group">
                <label>Nueva contraseña</label>
                <input
                  type="password"
                  value={pwdForm.newPassword}
                  onChange={handlePwdChange('newPassword')}
                  autoComplete="new-password"
                />
              </div>
              <div className="form-group">
                <label>Confirmar nueva contraseña</label>
                <input
                  type="password"
                  value={pwdForm.confirmPassword}
                  onChange={handlePwdChange('confirmPassword')}
                  autoComplete="new-password"
                />
              </div>
              <button type="submit" className="btn btn-primary" disabled={pwdSaving}>
                {pwdSaving ? 'Guardando...' : 'Cambiar contraseña'}
              </button>
            </form>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Profile;