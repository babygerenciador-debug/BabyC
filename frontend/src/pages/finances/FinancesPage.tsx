import { useState, useEffect } from 'react';
import { LineChart, Receipt, Settings, Droplets, FileText, Calendar } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../services/api';
import './FinancesPage.css';
import CashFlowDashboard from './components/CashFlowDashboard';
import TransactionsList from './components/TransactionsList';
import FinanceSettings from './components/FinanceSettings';
import FuelReport from './components/FuelReport';
import MonthSelector from './components/MonthSelector';
import MonthlyReport from './components/MonthlyReport';

type Tab = 'dashboard' | 'transactions' | 'settings' | 'fuel' | 'report';

interface FinancialMonthDto {
  id: string;
  year: number;
  monthNumber: number;
  label: string;
  status: string;
}

export default function FinancesPage() {
  const [activeTab, setActiveTab] = useState<Tab>('dashboard');
  const [selectedMonthId, setSelectedMonthId] = useState<string | null>(null);

  const { data: months } = useQuery<FinancialMonthDto[]>({
    queryKey: ['financial-months'],
    queryFn: async () => {
      const res = await api.get('/finance/months');
      return res.data;
    },
  });

  useEffect(() => {
    if (activeTab === 'report' && !selectedMonthId && months && months.length > 0) {
      const reportMonth = months.find(m => m.status === 'closed_with_report') || months[0];
      setSelectedMonthId(reportMonth.id);
    }
  }, [activeTab, months, selectedMonthId]);

  return (
    <div className="finances-container animate-fade-in">
      <div className="finances-header">
        <div>
          <h1>Financeiro</h1>
          <p>Visão de lucros, cálculo de impostos (27%) e fluxo de caixa da empresa.</p>
        </div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '0.75rem' }}>
        <div className="tabs" style={{ margin: 0 }}>
          <button
            className={`tab-btn ${activeTab === 'dashboard' ? 'active' : ''}`}
            onClick={() => setActiveTab('dashboard')}
          >
            <LineChart size={18} />
            <span>Dashboard</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'transactions' ? 'active' : ''}`}
            onClick={() => setActiveTab('transactions')}
          >
            <Receipt size={18} />
            <span>Contas</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'report' ? 'active' : ''}`}
            onClick={() => setActiveTab('report')}
          >
            <FileText size={18} />
            <span>Relatório Mensal</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'settings' ? 'active' : ''}`}
            onClick={() => setActiveTab('settings')}
          >
            <Settings size={18} />
            <span>Categorias</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'fuel' ? 'active' : ''}`}
            onClick={() => setActiveTab('fuel')}
          >
            <Droplets size={18} />
            <span>Abastecimentos</span>
          </button>
        </div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
        <MonthSelector
          selectedMonthId={selectedMonthId}
          onSelectMonth={setSelectedMonthId}
        />
      </div>

      <div className="tab-content">
        {activeTab === 'dashboard' && <CashFlowDashboard />}
        {activeTab === 'transactions' && <TransactionsList />}
        {activeTab === 'report' && selectedMonthId && (
          <MonthlyReport monthId={selectedMonthId} />
        )}
        {activeTab === 'report' && !selectedMonthId && (
          <div style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-secondary)' }}>
            <FileText size={48} style={{ margin: '0 auto 1rem', opacity: 0.3 }} />
            <p>Selecione um mês no seletor acima para visualizar o relatório</p>
          </div>
        )}
        {activeTab === 'settings' && <FinanceSettings />}
        {activeTab === 'fuel' && <FuelReport />}
      </div>
    </div>
  );
}
