import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../../store/useAuthStore';
import { api } from '../../services/api';
import { User, LogOut, Mail, Truck } from 'lucide-react';

interface DriverDto {
  id: string;
  userId: string;
  name: string;
  email: string;
  cnhNumber: string;
  assignedVehicle?: string;
}

export default function ProfileTab() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const { data: profile } = useQuery<DriverDto>({
    queryKey: ['driver-profile'],
    queryFn: async () => {
      const res = await api.get('/driver/me');
      return res.data;
    },
  });

  const handleLogout = () => {
    if (confirm('Deseja realmente sair?')) {
      logout();
      window.location.href = '/login';
    }
  };

  return (
    <div>
      <div style={{
        background: 'var(--bg-card)',
        borderRadius: 'var(--radius-md)',
        padding: '1.5rem',
        boxShadow: 'var(--shadow-sm)',
        marginBottom: '1rem'
      }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '1rem',
          marginBottom: '1.5rem'
        }}>
          <div style={{
            width: '64px',
            height: '64px',
            borderRadius: '50%',
            background: 'var(--brand-light)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <User size={32} style={{ color: 'var(--brand-color)' }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.25rem' }}>{user?.name}</h2>
            <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
              Motorista
            </p>
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.75rem',
            padding: '0.75rem',
            background: 'var(--bg-color)',
            borderRadius: 'var(--radius-sm)'
          }}>
            <Mail size={20} style={{ color: 'var(--text-secondary)' }} />
            <div>
              <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Email</p>
              <p style={{ margin: 0, fontWeight: 500 }}>{user?.email}</p>
            </div>
          </div>

          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.75rem',
            padding: '0.75rem',
            background: 'var(--bg-color)',
            borderRadius: 'var(--radius-sm)'
          }}>
            <Truck size={20} style={{ color: 'var(--text-secondary)' }} />
            <div>
              <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Ônibus Vinculado</p>
              <p style={{ margin: 0, fontWeight: 500 }}>
                {profile?.assignedVehicle || 'Nenhum veículo vinculado'}
              </p>
            </div>
          </div>
        </div>
      </div>

      <button
        onClick={handleLogout}
        style={{
          width: '100%',
          padding: '1rem',
          background: 'var(--error)',
          color: 'white',
          border: 'none',
          borderRadius: 'var(--radius-md)',
          fontSize: '1rem',
          fontWeight: 600,
          cursor: 'pointer',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: '0.5rem'
        }}
      >
        <LogOut size={18} />
        Sair do Sistema
      </button>
    </div>
  );
}
