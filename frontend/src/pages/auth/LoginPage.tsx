import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, Loader2, Shield, Truck, Sun, Moon } from 'lucide-react';
import { useAuthStore } from '../../store/useAuthStore';
import { useThemeStore } from '../../store/useThemeStore';
import { api } from '../../services/api';
import './Login.css';

export default function LoginPage() {
  const { mode, toggle: toggleTheme } = useThemeStore();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [selectedRole, setSelectedRole] = useState<'admin' | 'driver' | null>(null);
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await api.post('/auth/login', { identifier: email, password });
      const { accessToken: token, refreshToken, user, theme } = response.data;

      login(token, refreshToken, user, theme);

      if (user.role === 'Driver') {
        navigate('/driver');
      } else {
        navigate('/dashboard');
      }
    } catch (err: any) {
      setError(err.response?.data?.title || 'Falha ao autenticar. Verifique suas credenciais.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-bg">
        <img src="/ONIBUS.png" alt="" />
        <div className="login-bg-overlay" />
      </div>

      <button className="login-theme-toggle" onClick={toggleTheme} title={mode === 'light' ? 'Modo escuro' : 'Modo claro'}>
        {mode === 'light' ? <Moon size={18} /> : <Sun size={18} />}
      </button>

      <div className="login-card animate-fade-in">
        <div className="login-card-inner">
          <div className="login-brand">
            <div className="login-logo-wrap">
              <img src="/LOGO.png" alt="Baby Turismo" className="login-logo" />
            </div>
          </div>

          <h1 className="login-title">Acessar o Sistema</h1>
          

          <div className="role-selector">
            <button
              type="button"
              className={`role-btn ${selectedRole === 'admin' ? 'active' : ''}`}
              onClick={() => setSelectedRole('admin')}
            >
              <Shield size={20} />
              <span>Administrador</span>
            </button>
            <button
              type="button"
              className={`role-btn ${selectedRole === 'driver' ? 'active' : ''}`}
              onClick={() => setSelectedRole('driver')}
            >
              <Truck size={20} />
              <span>Motorista</span>
            </button>
          </div>

          {error && <div className="login-error">{error}</div>}

          <form onSubmit={handleLogin} className="login-form">
            <div className="input-group">
              <label>E-mail Corporativo</label>
              <div className="input-wrapper">
                <Mail size={18} className="input-icon" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="seu@gmail.com"
                  required
                />
              </div>
            </div>

            <div className="input-group">
              <label>Senha</label>
              <div className="input-wrapper">
                <div className="input-icon">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                </div>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  required
                />
              </div>
            </div>

            <button type="submit" className="login-button" disabled={loading}>
              {loading ? <Loader2 className="spinner" size={20} /> : 'Acessar Painel'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
