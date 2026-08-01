import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { DollarSign, TrendingUp, TrendingDown, Landmark, Wallet } from 'lucide-react';
import './CashFlowDashboard.css';

interface CashFlowSummaryDto {
  ownerSalary: number;
  ownerTaxAmount: number;
  netOwnerSalary: number;
  totalRevenues: number;
  totalExpenses: number;
  netBalance: number;
}

interface FinancialMonthDto {
  id: string;
  year: number;
  monthNumber: number;
  label: string;
  ownerSalary: number;
  status: string;
}

export default function CashFlowDashboard() {
  const { data: openMonth } = useQuery<FinancialMonthDto>({
    queryKey: ['open-financial-month'],
    queryFn: async () => {
      const res = await api.get('/finance/months/open');
      return res.data;
    },
  });

  const { data, isLoading } = useQuery<CashFlowSummaryDto>({
    queryKey: ['cash-flow-summary', openMonth?.id],
    queryFn: async () => {
      const res = await api.get('/finance/transactions/summary');
      return res.data;
    },
    refetchInterval: 30000,
  });

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
  };

  return (
    <div className="cash-flow-container animate-fade-in">
      {openMonth && (
        <div className="glass-panel" style={{ padding: '1rem', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <Wallet size={20} style={{ color: 'var(--brand-color)' }} />
          <div>
            <strong>Mês Ativo:</strong> {openMonth.label} — Salário: {formatCurrency(openMonth.ownerSalary)}
          </div>
        </div>
      )}

      {isLoading ? (
        <p>Calculando fluxo de caixa...</p>
      ) : data ? (
        <div className="dashboard-grid">
          
          <div className="kpi-card glass-panel highlight-brand">
            <div className="kpi-icon"><Wallet size={24} /></div>
            <div className="kpi-content">
              <span>Seu Salário Líquido (Pós-Imposto)</span>
              <h2>{formatCurrency(data.netOwnerSalary)}</h2>
              <p className="kpi-detail">Bruto: {formatCurrency(data.ownerSalary)} (-27% Retido)</p>
            </div>
          </div>

          <div className="kpi-card glass-panel">
            <div className="kpi-icon text-error"><Landmark size={24} /></div>
            <div className="kpi-content">
              <span>Imposto Retido (27%)</span>
              <h2 className="text-error">{formatCurrency(data.ownerTaxAmount)}</h2>
              <p className="kpi-detail">Destinado à Receita Federal</p>
            </div>
          </div>

          <div className="kpi-card glass-panel">
            <div className="kpi-icon text-success"><TrendingUp size={24} /></div>
            <div className="kpi-content">
              <span>Total de Receitas (Viagens)</span>
              <h2 className="text-success">{formatCurrency(data.totalRevenues)}</h2>
              <p className="kpi-detail">Faturamento Operacional Bruto</p>
            </div>
          </div>

          <div className="kpi-card glass-panel">
            <div className="kpi-icon text-error"><TrendingDown size={24} /></div>
            <div className="kpi-content">
              <span>Despesas Operacionais</span>
              <h2 className="text-error">{formatCurrency(data.totalExpenses)}</h2>
              <p className="kpi-detail">Abastecimentos, Peças, Salários de Motoristas</p>
            </div>
          </div>

          <div className={`kpi-card glass-panel summary-card ${data.netBalance >= 0 ? 'positive' : 'negative'}`}>
            <div className="kpi-content">
              <span className="summary-label">Saldo Final / Lucro Real da Empresa</span>
              <h1 className="summary-value">{formatCurrency(data.netBalance)}</h1>
              <p className="summary-formula">(Salário Bruto − 27% Imposto) − Despesas + Receitas</p>
            </div>
          </div>

        </div>
      ) : null}
    </div>
  );
}
