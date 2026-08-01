import { Outlet, Navigate, NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { useThemeStore } from '../../store/useThemeStore';
import { LayoutDashboard, Truck, Wrench, Package, DollarSign, Bell, LogOut, Menu, X, Users, Hammer, Sun, Moon, CheckCheck, Info, AlertTriangle, AlertCircle } from 'lucide-react';
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import './MainLayout.css';
import { useSignalR } from '../../hooks/useSignalR';

interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: string;
  createdAt: string;
}

export default function MainLayout() {
  const { user, logout } = useAuthStore();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const { mode, toggle: toggleTheme } = useThemeStore();
  const queryClient = useQueryClient();

  useSignalR();

  const { data: notifications } = useQuery<NotificationDto[]>({
    queryKey: ['notifications'],
    queryFn: async () => {
      const res = await api.get('/notifications/my');
      return res.data;
    },
    refetchInterval: 10000,
    refetchOnWindowFocus: true,
  });

  const markRead = useMutation({
    mutationFn: (id: string) => api.post(`/notifications/${id}/read`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  const toggleMenu = () => setIsMobileMenuOpen(!isMobileMenuOpen);

  const navItems = [
    { to: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
    { to: '/trips', icon: Truck, label: 'Viagens' },
    { to: '/drivers', icon: Users, label: 'Motoristas' },
    { to: '/fleet', icon: Wrench, label: 'Frota' },
    { to: '/maintenance', icon: Hammer, label: 'Manutenção' },
    { to: '/inventory', icon: Package, label: 'Estoque' },
    { to: '/finances', icon: DollarSign, label: 'Financeiro' },
  ];

  const unreadCount = notifications?.length ?? 0;

  return (
    <div className="layout-container">
      <aside className={`sidebar ${isMobileMenuOpen ? 'open' : ''}`}>
        <div className="sidebar-header">
          <div className="company-logo">
            <img src="/LOGO.png" alt="Baby Turismo" className="sidebar-logo" />
          </div>
          <button className="mobile-close-btn" onClick={toggleMenu}>
            <X size={24} />
          </button>
        </div>

        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
              onClick={() => setIsMobileMenuOpen(false)}
            >
              <item.icon size={20} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            <span className="user-name">{user.name}</span>
            <span className="user-role">{user.role}</span>
          </div>
          <button className="logout-button" onClick={logout}>
            <LogOut size={20} />
            <span>Sair</span>
          </button>
        </div>
      </aside>

      {isMobileMenuOpen && <div className="sidebar-overlay" onClick={toggleMenu} />}

      <main className="main-content">
        <header className="topbar">
          <button className="mobile-menu-btn" onClick={toggleMenu}>
            <Menu size={24} />
          </button>

          <div className="topbar-right">
            <button className="theme-toggle" onClick={toggleTheme} title={mode === 'light' ? 'Modo escuro' : 'Modo claro'}>
              {mode === 'light' ? <Moon size={18} /> : <Sun size={18} />}
            </button>
            <div className="notification-wrap">
              <button className="notification-btn" onClick={() => setShowNotifications(!showNotifications)}>
                <Bell size={20} />
                {unreadCount > 0 && <span className="notification-badge">{unreadCount}</span>}
              </button>
              {showNotifications && (
                <>
                  <div className="notif-backdrop" onClick={() => setShowNotifications(false)} />
                  <div className="notif-panel">
                    <div className="notif-header">
                      <h3>Notificações</h3>
                      {unreadCount > 0 && <span className="notif-count">{unreadCount} não lida(s)</span>}
                    </div>
                    <div className="notif-list">
                      {notifications?.length === 0 && (
                        <p className="notif-empty">Nenhuma notificação</p>
                      )}
                      {notifications?.map((n) => {
                        const iconMap: Record<string, React.ReactNode> = {
                          Warning: <AlertTriangle size={16} />,
                          Error: <AlertCircle size={16} />,
                        };
                        const icon = iconMap[n.type] || <Info size={16} />;
                        const iconClass = n.type === 'Warning' ? 'warning' : n.type === 'Error' ? 'error' : 'info';
                        return (
                          <div key={n.id} className="notif-item" onClick={() => markRead.mutate(n.id)}>
                            <div className={`notif-icon ${iconClass}`}>{icon}</div>
                            <div className="notif-body">
                              <strong>{n.title}</strong>
                              <p className="notif-msg">{n.message}</p>
                              <span className="notif-time">
                                {new Date(n.createdAt).toLocaleString('pt-BR')}
                              </span>
                            </div>
                            <button
                              className="notif-dismiss"
                              title="Marcar como lida"
                              onClick={(e) => { e.stopPropagation(); markRead.mutate(n.id); }}
                            >
                              <CheckCheck size={16} />
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>
        </header>

        <div className="content-area">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
