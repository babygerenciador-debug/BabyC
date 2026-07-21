import { useQuery } from '@tanstack/react-query';
import { api } from '../../services/api';
import { Truck, AlertTriangle, ArrowUpRight, ArrowDownRight, DollarSign, Settings } from 'lucide-react';
import './Dashboard.css';

interface DashboardSummaryDto {
  totalVehicles: number;
  availableVehicles: number;
  inTripVehicles: number;
  inMaintenanceVehicles: number;
  totalTripsThisMonth: number;
  ongoingTrips: number;
  lowStockItemsCount: number;
  monthRevenues: number;
  monthExpenses: number;
  monthBalance: number;
}

export default function DashboardPage() {
  const { data, isLoading, error, refetch } = useQuery<DashboardSummaryDto>({
    queryKey: ['dashboardSummary'],
    queryFn: async () => {
      const response = await api.get('/dashboard/summary');
      return response.data;
    }
  });

  if (isLoading) {
    return <div className="loading-state">Carregando dados do painel...</div>;
  }

  if (error) {
    return (
      <div className="error-state">
        <AlertTriangle size={32} />
        <p>Ocorreu um erro ao carregar o dashboard.</p>
        <button onClick={() => refetch()}>Tentar Novamente</button>
      </div>
    );
  }

  const fmt = (val: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

  return (
    <div className="dashboard-container animate-fade-in">
      <div className="dashboard-header">
        <div>
          <h1>Visão Geral</h1>
          <p>Acompanhe o desempenho da sua frota em tempo real.</p>
        </div>
      </div>

      <div className="kpi-grid">
        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success)' }}>
            <Truck size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Viagens Ativas</span>
            <h2 className="kpi-value">{data?.ongoingTrips || 0}</h2>
            <div className="kpi-indicator neutral">
              <span>{data?.totalTripsThisMonth || 0} este mês</span>
            </div>
          </div>
        </div>

        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', color: 'var(--info)' }}>
            <Truck size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Frota Disponível</span>
            <h2 className="kpi-value">{data?.availableVehicles || 0} / {data?.totalVehicles || 0}</h2>
            <div className="kpi-indicator neutral">
              <span>{data?.inTripVehicles || 0} em viagem</span>
            </div>
          </div>
        </div>

        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning)' }}>
            <Settings size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Em Manutenção</span>
            <h2 className="kpi-value">{data?.inMaintenanceVehicles || 0}</h2>
            <div className="kpi-indicator neutral">
              <span>Veículos</span>
            </div>
          </div>
        </div>

        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error)' }}>
            <AlertTriangle size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Estoque Baixo</span>
            <h2 className="kpi-value">{data?.lowStockItemsCount || 0}</h2>
            <div className="kpi-indicator negative">
              <ArrowDownRight size={16} />
              <span>Itens abaixo do mínimo</span>
            </div>
          </div>
        </div>
      </div>

      <div className="charts-grid">
        <div className="chart-card card">
          <h3>Resumo Financeiro do Mês</h3>
          <div className="finance-details">
            <div className="fin-row">
              <span>Receitas</span>
              <span className="val-positive">{fmt(data?.monthRevenues || 0)}</span>
            </div>
            <div className="fin-row">
              <span>Despesas</span>
              <span className="val-negative">- {fmt(data?.monthExpenses || 0)}</span>
            </div>
            <div className="fin-divider"></div>
            <div className="fin-row total">
              <span>Saldo do Mês</span>
              <span className={(data?.monthBalance ?? 0) >= 0 ? 'val-positive' : 'val-negative'}>
                {fmt(data?.monthBalance || 0)}
              </span>
            </div>
          </div>
        </div>
        
        <div className="chart-card card">
          <h3>Status Recentes</h3>
          <p style={{ color: 'var(--text-secondary)' }}>O histórico de alertas e checklists será exibido aqui usando os gráficos Recharts ou ECharts...</p>
        </div>
      </div>
    </div>
  );
}
