import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import { Truck, AlertTriangle, ArrowDownRight, Settings, AlertCircle, CheckCircle, Clock, XCircle, Eye, DollarSign } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, LabelList, PieChart, Pie, Cell } from 'recharts';
import BaseModal from '../../components/shared/BaseModal';
import './Dashboard.css';

interface DashboardSummaryDto {
  totalVehicles: number;
  availableVehicles: number;
  inTripVehicles: number;
  inMaintenanceVehicles: number;
  totalTripsThisMonth: number;
  ongoingTrips: number;
  lowStockItemsCount: number;
  unreadNotificationsCount: number;
  monthRevenues: number;
  monthExpenses: number;
  monthBalance: number;
  monthRealProfit: number;
}

interface ReportRow {
  date: string;
  vehicleLicensePlate: string;
  driverName: string;
  status: string;
  totalItems: number;
  completedItems: number;
}

interface VehicleIssueDto {
  id: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  driverId?: string;
  driverName?: string;
  description: string;
  status: string;
  createdAt: string;
  resolvedAt?: string;
}

const statusConfig: Record<string, { label: string; color: string; icon: React.ReactNode }> = {
  Pending: { label: 'Pendente', color: 'var(--warning)', icon: <Clock size={14} /> },
  InReview: { label: 'Em Análise', color: 'var(--info)', icon: <Eye size={14} /> },
  Resolved: { label: 'Resolvido', color: 'var(--success)', icon: <CheckCircle size={14} /> },
  Ignored: { label: 'Ignorado', color: 'var(--text-secondary)', icon: <XCircle size={14} /> },
};

const ChartTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null;
  return (
    <div className="chart-tooltip">
      <div className="chart-tooltip-date">{label}</div>
      {payload.map((entry: any) => (
        <div key={entry.name} className="chart-tooltip-row">
          <span className="chart-tooltip-dot" style={{ background: entry.color }} />
          <span>{entry.name}: <strong>{entry.value}</strong></span>
        </div>
      ))}
    </div>
  );
};

export default function DashboardPage() {
  const queryClient = useQueryClient();

  const [resolveTarget, setResolveTarget] = useState<{ id: string } | null>(null);
  const [registerExpense, setRegisterExpense] = useState(false);
  const [expenseAmount, setExpenseAmount] = useState('');
  const [expenseDescription, setExpenseDescription] = useState('');

  const { data, isLoading, error, refetch } = useQuery<DashboardSummaryDto>({
    queryKey: ['dashboardSummary'],
    queryFn: async () => {
      const response = await api.get('/dashboard/summary');
      return response.data;
    },
    refetchInterval: 30000,
    refetchOnMount: true,
    refetchOnWindowFocus: true,
  });

  const { data: report } = useQuery<ReportRow[]>({
    queryKey: ['checklistDashboardReport'],
    queryFn: async () => {
      const res = await api.get('/checklist-admin/report');
      return res.data;
    },
    refetchInterval: 30000,
  });

  const { data: issues } = useQuery<VehicleIssueDto[]>({
    queryKey: ['vehicleIssues'],
    queryFn: async () => {
      const res = await api.get('/VehicleIssues', { headers: { 'Cache-Control': 'no-cache' } });
      return res.data;
    },
    refetchInterval: 3000,
    refetchIntervalInBackground: true,
    refetchOnMount: true,
    refetchOnWindowFocus: true,
  });

  const updateStatus = useMutation({
    mutationFn: ({ id, status, expenseAmount: ea, expenseDescription: ed }: { id: string; status: string; expenseAmount?: number; expenseDescription?: string }) =>
      api.patch(`/VehicleIssues/${id}/status`, { status, expenseAmount: ea, expenseDescription: ed }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicleIssues'], refetchType: 'all' });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'], refetchType: 'all' });
      queryClient.invalidateQueries({ queryKey: ['transactions'], refetchType: 'all' });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'], refetchType: 'all' });
      setResolveTarget(null);
      setRegisterExpense(false);
      setExpenseAmount('');
      setExpenseDescription('');
    },
  });

  const openResolveModal = (id: string) => {
    setResolveTarget({ id });
    setRegisterExpense(false);
    setExpenseAmount('');
    setExpenseDescription('');
  };

  const doResolve = () => {
    if (!resolveTarget) return;
    if (registerExpense) {
      const amt = parseFloat(expenseAmount);
      if (isNaN(amt) || amt <= 0) return;
      if (!expenseDescription.trim()) return;
      updateStatus.mutate({ id: resolveTarget.id, status: 'Resolved', expenseAmount: amt, expenseDescription: expenseDescription.trim() });
    } else {
      updateStatus.mutate({ id: resolveTarget.id, status: 'Resolved' });
    }
  };

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

  const chartData = (() => {
    if (!report) return [];
    const grouped: Record<string, { completed: number; total: number }> = {};
    for (const row of report) {
      const d = row.date.slice(0, 10);
      if (!grouped[d]) grouped[d] = { completed: 0, total: 0 };
      grouped[d].completed += row.completedItems;
      grouped[d].total += row.totalItems;
    }
    return Object.entries(grouped)
      .sort(([a], [b]) => a.localeCompare(b))
      .slice(-7)
      .map(([date, v]) => ({
        date: date.slice(5),
        Completos: v.completed,
        Pendentes: v.total - v.completed,
      }));
  })();

  const openIssues = issues?.filter(i => i.status === 'Pending' || i.status === 'InReview') ?? [];

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
          <div className="finance-wrap">
            <div className="finance-chart">
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <defs>
                    <linearGradient id="gradRevenue" x1="0" y1="0" x2="1" y2="1">
                      <stop offset="0%" stopColor="#22c55e" stopOpacity={1} />
                      <stop offset="100%" stopColor="#16a34a" stopOpacity={1} />
                    </linearGradient>
                    <linearGradient id="gradExpense" x1="0" y1="0" x2="1" y2="1">
                      <stop offset="0%" stopColor="#ef4444" stopOpacity={1} />
                      <stop offset="100%" stopColor="#dc2626" stopOpacity={1} />
                    </linearGradient>
                  </defs>
                  <Pie
                    data={[
                      { name: 'Receitas', value: Math.max(data?.monthRevenues ?? 0, 0) },
                      { name: 'Despesas', value: Math.max(data?.monthExpenses ?? 0, 0) },
                    ]}
                    cx="50%" cy="50%"
                    innerRadius={55}
                    outerRadius={82}
                    startAngle={90}
                    endAngle={-270}
                    paddingAngle={4}
                    cornerRadius={4}
                    dataKey="value"
                    animationDuration={700}
                    stroke="var(--bg-card)"
                    strokeWidth={2}
                  >
                    <Cell fill="url(#gradRevenue)" />
                    <Cell fill="url(#gradExpense)" />
                  </Pie>
                  <text x="50%" y="50%" textAnchor="middle" dominantBaseline="middle" fontSize={14} fontWeight={700} fill="var(--text-primary)">
                    Total
                  </text>
                  <Tooltip
                    content={({ active, payload }) =>
                      active && payload?.length ? (
                        <div className="chart-tooltip">
                          {payload.map(p => (
                            <div key={p.name} className="chart-tooltip-row">
                              <span className="chart-tooltip-dot" style={{ background: p.color }} />
                              <span>{p.name}: <strong>{fmt(p.value as number)}</strong></span>
                              <span style={{ color: 'var(--text-secondary)', marginLeft: 8, fontSize: 11 }}>
                                {((p.value as number) / Math.max((data?.monthRevenues ?? 0) + (data?.monthExpenses ?? 0), 1) * 100).toFixed(1)}%
                              </span>
                            </div>
                          ))}
                        </div>
                      ) : null
                    }
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <div className="finance-numbers">
              <div className="fin-row">
                <span>Receitas</span>
                <span className="val-positive">{fmt(data?.monthRevenues || 0)}</span>
              </div>
              <div className="fin-row">
                <span>Despesas</span>
                <span className="val-negative">- {fmt(data?.monthExpenses || 0)}</span>
              </div>
              <div className="fin-divider" />
              <div className="fin-row total">
                <span>Saldo do Mês</span>
                <span className={(data?.monthBalance ?? 0) >= 0 ? 'val-positive' : 'val-negative'}>
                  {fmt(data?.monthBalance || 0)}
                </span>
              </div>
              <div className="fin-divider" />
              <div className="fin-row total" style={{ color: 'var(--brand-color)', fontWeight: 700 }}>
                <span>Lucro Real</span>
                <span className={(data?.monthRealProfit ?? 0) >= 0 ? 'val-positive' : 'val-negative'}>
                  {fmt(data?.monthRealProfit || 0)}
                </span>
              </div>
            </div>
          </div>
        </div>

        <div className="chart-card card">
          <h3>Alertas dos Motoristas</h3>
          <div className="status-recentes-content">
            {chartData.length > 0 && (
              <div className="chart-section">
                <span className="section-label">Checklists por Dia</span>
                <ResponsiveContainer width="100%" height={210}>
                  <BarChart data={chartData} margin={{ top: 18, right: 8, left: -16, bottom: 4 }}>
                    <defs>
                      <linearGradient id="gradComplete" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="var(--info)" stopOpacity={1} />
                        <stop offset="100%" stopColor="var(--info)" stopOpacity={0.55} />
                      </linearGradient>
                      <linearGradient id="gradPending" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="var(--border-color)" stopOpacity={0.8} />
                        <stop offset="100%" stopColor="var(--border-color)" stopOpacity={0.35} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="4 4" stroke="var(--border-color)" strokeOpacity={0.5} vertical={false} />
                    <XAxis
                      dataKey="date"
                      tick={{ fontSize: 11, fill: 'var(--text-secondary)', fontWeight: 500 }}
                      axisLine={false}
                      tickLine={false}
                    />
                    <YAxis
                      tick={{ fontSize: 11, fill: 'var(--text-secondary)' }}
                      axisLine={false}
                      tickLine={false}
                      allowDecimals={false}
                    />
                    <Tooltip content={<ChartTooltip />} cursor={{ fill: 'var(--bg-hover)', opacity: 0.3 }} />
                    <Bar
                      dataKey="Completos"
                      stackId="a"
                      fill="url(#gradComplete)"
                      radius={[4, 4, 0, 0]}
                      animationDuration={600}
                      animationEasing="ease-out"
                      maxBarSize={40}
                    >
                      <LabelList
                        dataKey="Completos"
                        position="top"
                        style={{ fontSize: 10, fontWeight: 700, fill: 'var(--text-secondary)' }}
                      />
                    </Bar>
                    <Bar
                      dataKey="Pendentes"
                      stackId="a"
                      fill="url(#gradPending)"
                      radius={[4, 4, 0, 0]}
                      animationDuration={600}
                      animationEasing="ease-out"
                      maxBarSize={40}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            )}

            <div className="issues-section">
              <span className="section-label">Alertas Abertos</span>
              {openIssues.length > 0 ? (
                <div className="issues-list">
                  {openIssues.map(issue => {
                    const st = statusConfig[issue.status] ?? { label: issue.status, color: 'var(--text-secondary)', icon: null };
                    return (
                      <div key={issue.id} className="issue-item" style={{ flexDirection: 'column', gap: '0.5rem' }}>
                        <div style={{ display: 'flex', alignItems: 'flex-start', gap: '0.5rem', width: '100%' }}>
                          <AlertCircle size={16} style={{ color: 'var(--warning)', flexShrink: 0, marginTop: 2 }} />
                          <div className="issue-info" style={{ flex: 1 }}>
                            <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                              <strong style={{ fontSize: '0.85rem' }}>{issue.vehicleLicensePlate}</strong>
                              {issue.driverName && (
                                <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                                  — {issue.driverName}
                                </span>
                              )}
                              <span style={{ fontSize: '0.7rem', color: st.color, display: 'inline-flex', alignItems: 'center', gap: 2 }}>
                                {st.icon} {st.label}
                              </span>
                            </div>
                            <span className="issue-desc" style={{ fontSize: '0.8rem' }}>{issue.description}</span>
                            <span className="issue-meta" style={{ fontSize: '0.7rem' }}>
                              {new Date(issue.createdAt).toLocaleDateString('pt-BR')}
                            </span>
                          </div>
                        </div>
                        {issue.status === 'Pending' && (
                          <div style={{ display: 'flex', gap: '0.25rem', paddingLeft: '1.5rem' }}>
                            <button className="btn-sm" style={{ fontSize: '0.7rem', padding: '0.2rem 0.5rem' }}
                              onClick={() => updateStatus.mutate({ id: issue.id, status: 'InReview' })}>
                              <Eye size={12} /> Em Análise
                            </button>
                            <button className="btn-sm" style={{ fontSize: '0.7rem', padding: '0.2rem 0.5rem', color: 'var(--success)' }}
                              onClick={() => openResolveModal(issue.id)}>
                              <CheckCircle size={12} /> Resolver
                            </button>
                          </div>
                        )}
                        {issue.status === 'InReview' && (
                          <div style={{ display: 'flex', gap: '0.25rem', paddingLeft: '1.5rem' }}>
                            <button className="btn-sm" style={{ fontSize: '0.7rem', padding: '0.2rem 0.5rem', color: 'var(--success)' }}
                              onClick={() => openResolveModal(issue.id)}>
                              <CheckCircle size={12} /> Resolver
                            </button>
                            <button className="btn-sm" style={{ fontSize: '0.7rem', padding: '0.2rem 0.5rem', color: 'var(--text-secondary)' }}
                              onClick={() => updateStatus.mutate({ id: issue.id, status: 'Ignored' })}>
                              <XCircle size={12} /> Ignorar
                            </button>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              ) : (
                <p style={{ color: 'var(--text-secondary)', textAlign: 'center', padding: '1rem 0', fontSize: '0.85rem', margin: 0 }}>
                  Nenhum alerta no momento.
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      <BaseModal open={!!resolveTarget} onClose={() => setResolveTarget(null)} title="Resolver Alerta" maxWidth="420px"
        footer={
          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
            <button className="btn btn-secondary" onClick={() => setResolveTarget(null)}>Cancelar</button>
            <button className="btn btn-primary" onClick={doResolve} disabled={registerExpense && (!expenseAmount || !expenseDescription)}>
              <CheckCircle size={16} /> Resolver
            </button>
          </div>
        }>
        <div className="modal-form">
          <p style={{ marginBottom: '1rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            Deseja registrar um gasto relacionado a este alerta?
          </p>
          <label className="checkbox-label">
            <input type="checkbox" checked={registerExpense} onChange={e => setRegisterExpense(e.target.checked)} />
            <span className="check-box" />
            Registrar gasto no financeiro
          </label>
          {registerExpense && (
            <div className="form-section" style={{ marginBottom: '1rem' }}>
              <div className="input-group" style={{ marginBottom: '0.75rem' }}>
                <label>Valor (R$)</label>
                <input type="number" step="0.01" min="0.01" placeholder="Ex: 150.00" value={expenseAmount} onChange={e => setExpenseAmount(e.target.value)} />
              </div>
              <div className="input-group">
                <label>Descrição do gasto</label>
                <input type="text" placeholder="Ex: Reparo no pneu" value={expenseDescription} onChange={e => setExpenseDescription(e.target.value)} />
              </div>
            </div>
          )}
        </div>
      </BaseModal>
    </div>
  );
}
