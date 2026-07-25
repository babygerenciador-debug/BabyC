import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { queryClient } from './services/queryClient';
import LoginPage from './pages/auth/LoginPage';
import MainLayout from './components/layout/MainLayout';
import DashboardPage from './pages/dashboard/DashboardPage';
import FleetPage from './pages/fleet/FleetPage';
import TripsPage from './pages/trips/TripsPage';
import DriversPage from './pages/drivers/DriversPage';
import MaintenancePage from './pages/maintenance/MaintenancePage';
import InventoryPage from './pages/inventory/InventoryPage';
import FinancesPage from './pages/finances/FinancesPage';
import DriverPortalPage from './pages/driver/DriverPortalPage';
import { useAuthStore } from './store/useAuthStore';
import type { ReactNode } from 'react';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}

function AdminRoute({ children }: { children: ReactNode }) {
  const user = useAuthStore((s) => s.user);
  const token = useAuthStore((s) => s.token);
  if (!user || !token || isTokenExpired(token)) return <Navigate to="/login" replace />;
  if (user.role === 'Driver') return <Navigate to="/driver" replace />;
  return <>{children}</>;
}

function DriverRoute({ children }: { children: ReactNode }) {
  const user = useAuthStore((s) => s.user);
  const token = useAuthStore((s) => s.token);
  if (!user || !token || isTokenExpired(token)) return <Navigate to="/login" replace />;
  if (user.role !== 'Driver') return <Navigate to="/dashboard" replace />;
  return <>{children}</>;
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Toaster richColors position="top-right" />
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<AdminRoute><MainLayout /></AdminRoute>}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="trips" element={<TripsPage />} />
            <Route path="drivers" element={<DriversPage />} />
            <Route path="fleet" element={<FleetPage />} />
            <Route path="maintenance" element={<MaintenancePage />} />
            <Route path="inventory" element={<InventoryPage />} />
            <Route path="finances" element={<FinancesPage />} />
          </Route>
          <Route path="/driver" element={<DriverRoute><DriverPortalPage /></DriverRoute>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
