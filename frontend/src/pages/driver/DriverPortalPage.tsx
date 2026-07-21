import { useState } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import { useSignalR } from '../../hooks/useSignalR';
import { Truck, Fuel, User } from 'lucide-react';
import TripsTab from './TripsTab';
import FuelLogTab from './FuelLogTab';
import ProfileTab from './ProfileTab';

type Tab = 'trips' | 'fuel' | 'profile';

export default function DriverPortalPage() {
  const user = useAuthStore((s) => s.user);
  const [activeTab, setActiveTab] = useState<Tab>('trips');

  useSignalR();

  return (
    <div style={{
      minHeight: '100vh',
      background: 'var(--bg-primary)',
      display: 'flex',
      flexDirection: 'column',
      maxWidth: '600px',
      margin: '0 auto'
    }}>
      {/* Header */}
      <header style={{
        background: 'var(--card-bg)',
        padding: '1rem',
        borderBottom: '1px solid var(--border-color)',
        display: 'flex',
        alignItems: 'center',
        gap: '0.75rem'
      }}>
        <Truck size={24} style={{ color: 'var(--brand-color)' }} />
        <div>
          <h2 style={{ margin: 0, fontSize: '1.125rem' }}>Portal do Motorista</h2>
          <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
            Olá, {user?.name}
          </p>
        </div>
      </header>

      {/* Content */}
      <main style={{
        flex: 1,
        padding: '1rem',
        overflowY: 'auto',
        paddingBottom: '5rem'
      }}>
        {activeTab === 'trips' && <TripsTab />}
        {activeTab === 'fuel' && <FuelLogTab />}
        {activeTab === 'profile' && <ProfileTab />}
      </main>

      {/* Bottom Tab Bar */}
      <nav style={{
        position: 'fixed',
        bottom: 0,
        left: 0,
        right: 0,
        background: 'var(--card-bg)',
        borderTop: '1px solid var(--border-color)',
        display: 'flex',
        justifyContent: 'space-around',
        padding: '0.5rem 0',
        maxWidth: '600px',
        margin: '0 auto',
        boxShadow: '0 -2px 10px rgba(0,0,0,0.1)'
      }}>
        <button
          onClick={() => setActiveTab('trips')}
          style={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '0.25rem',
            padding: '0.5rem',
            background: 'transparent',
            border: 'none',
            cursor: 'pointer',
            color: activeTab === 'trips' ? 'var(--brand-color)' : 'var(--text-secondary)',
            transition: 'color 0.2s'
          }}
        >
          <Truck size={24} />
          <span style={{ fontSize: '0.75rem', fontWeight: activeTab === 'trips' ? 600 : 400 }}>
            Viagens
          </span>
        </button>

        <button
          onClick={() => setActiveTab('fuel')}
          style={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '0.25rem',
            padding: '0.5rem',
            background: 'transparent',
            border: 'none',
            cursor: 'pointer',
            color: activeTab === 'fuel' ? 'var(--brand-color)' : 'var(--text-secondary)',
            transition: 'color 0.2s'
          }}
        >
          <Fuel size={24} />
          <span style={{ fontSize: '0.75rem', fontWeight: activeTab === 'fuel' ? 600 : 400 }}>
            Abastecer
          </span>
        </button>

        <button
          onClick={() => setActiveTab('profile')}
          style={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '0.25rem',
            padding: '0.5rem',
            background: 'transparent',
            border: 'none',
            cursor: 'pointer',
            color: activeTab === 'profile' ? 'var(--brand-color)' : 'var(--text-secondary)',
            transition: 'color 0.2s'
          }}
        >
          <User size={24} />
          <span style={{ fontSize: '0.75rem', fontWeight: activeTab === 'profile' ? 600 : 400 }}>
            Perfil
          </span>
        </button>
      </nav>
    </div>
  );
}
