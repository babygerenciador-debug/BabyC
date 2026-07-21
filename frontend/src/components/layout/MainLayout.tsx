import { Outlet, Navigate, NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { LayoutDashboard, Truck, Wrench, Package, DollarSign, Bell, LogOut, Menu, X, Users, Hammer } from 'lucide-react';
import { useState } from 'react';
import './MainLayout.css';
import { useSignalR } from '../../hooks/useSignalR';

export default function MainLayout() {
  const { user, logout } = useAuthStore();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  
  // Inicializa a conexão do WebSocket (SignalR)
  useSignalR();

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

  return (
    <div className="layout-container">
      {/* Sidebar */}
      <aside className={`sidebar ${isMobileMenuOpen ? 'open' : ''}`}>
        <div className="sidebar-header">
          <div className="company-logo">
            {/* O logo dinâmico poderia entrar aqui */}
            <h2>FleetOS</h2>
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

      {/* Overlay para mobile */}
      {isMobileMenuOpen && <div className="sidebar-overlay" onClick={toggleMenu} />}

      {/* Main Content */}
      <main className="main-content">
        <header className="topbar">
          <button className="mobile-menu-btn" onClick={toggleMenu}>
            <Menu size={24} />
          </button>
          
          <div className="topbar-right">
            <button className="notification-btn">
              <Bell size={20} />
              <span className="notification-badge"></span>
            </button>
          </div>
        </header>

        <div className="content-area">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
