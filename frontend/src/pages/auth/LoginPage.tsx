import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Building2, KeyRound, Mail, Loader2, Shield, Truck } from 'lucide-react';
import { useAuthStore } from '../../store/useAuthStore';
import { api } from '../../services/api';
import './Login.css';

export default function LoginPage() {
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
      const { accessToken: token, user, theme } = response.data;
      
      login(token, user, theme);
      
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
      <div className="login-left-panel">
        <div className="login-glass-card animate-fade-in">
          <div className="login-header">
            <div className="logo-container">
              <Building2 size={32} className="logo-icon" />
            </div>
            <h1>Bem-vindo ao FleetOS</h1>
            <p>Gerencie sua frota com inteligência e controle financeiro.</p>
          </div>

          <div className="role-selector" style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem' }}>
            <button
              type="button"
              onClick={() => setSelectedRole('admin')}
              style={{
                flex: 1,
                padding: '1rem',
                border: selectedRole === 'admin' ? '2px solid var(--brand-color)' : '2px solid var(--border-color)',
                borderRadius: 'var(--border-radius-sm)',
                background: selectedRole === 'admin' ? 'var(--brand-light)' : 'transparent',
                cursor: 'pointer',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                gap: '0.5rem',
                transition: 'all 0.2s'
              }}
            >
              <Shield size={24} style={{ color: selectedRole === 'admin' ? 'var(--brand-color)' : 'var(--text-secondary)' }} />
              <span style={{ fontWeight: 500 }}>Administrador</span>
            </button>
            <button
              type="button"
              onClick={() => setSelectedRole('driver')}
              style={{
                flex: 1,
                padding: '1rem',
                border: selectedRole === 'driver' ? '2px solid var(--brand-color)' : '2px solid var(--border-color)',
                borderRadius: 'var(--border-radius-sm)',
                background: selectedRole === 'driver' ? 'var(--brand-light)' : 'transparent',
                cursor: 'pointer',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                gap: '0.5rem',
                transition: 'all 0.2s'
              }}
            >
              <Truck size={24} style={{ color: selectedRole === 'driver' ? 'var(--brand-color)' : 'var(--text-secondary)' }} />
              <span style={{ fontWeight: 500 }}>Motorista</span>
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
                  placeholder="admin@empresa.com.br"
                  required
                />
              </div>
            </div>

            <div className="input-group">
              <label>Senha</label>
              <div className="input-wrapper">
                <KeyRound size={18} className="input-icon" />
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
              {loading ? <Loader2 className="spinner" size={20} /> : 'Entrar no Sistema'}
            </button>
          </form>
        </div>
      </div>
      <div className="login-right-panel">
        {/* Placeholder para uma imagem ilustrativa gerada do sistema */}
        <div className="login-illustration">
          <div className="floating-card c1 glass-panel">
            <span>Lucro Líquido Real</span>
            <h3>R$ 15.420,00</h3>
          </div>
          <div className="floating-card c2 glass-panel">
            <span>Estoque Alerta</span>
            <h3 style={{ color: 'var(--warning)' }}>Pastilha Freio: 2 und</h3>
          </div>
        </div>
      </div>
    </div>
  );
}
