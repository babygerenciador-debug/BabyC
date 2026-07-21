import { useState } from 'react';
import { LineChart, Receipt, Settings } from 'lucide-react';
import './FinancesPage.css';
import CashFlowDashboard from './components/CashFlowDashboard';
import TransactionsList from './components/TransactionsList';
import FinanceSettings from './components/FinanceSettings';

type Tab = 'dashboard' | 'transactions' | 'settings';

export default function FinancesPage() {
  const [activeTab, setActiveTab] = useState<Tab>('dashboard');

  return (
    <div className="finances-container animate-fade-in">
      <div className="finances-header">
        <div>
          <h1>Financeiro</h1>
          <p>Visão de lucros, cálculo de impostos (27%) e fluxo de caixa da empresa.</p>
        </div>
      </div>

      <div className="finances-tabs">
        <button 
          className={`tab-btn ${activeTab === 'dashboard' ? 'active' : ''}`}
          onClick={() => setActiveTab('dashboard')}
        >
          <LineChart size={18} />
          <span>Dashboard & Lucratividade</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'transactions' ? 'active' : ''}`}
          onClick={() => setActiveTab('transactions')}
        >
          <Receipt size={18} />
          <span>Contas a Pagar</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'settings' ? 'active' : ''}`}
          onClick={() => setActiveTab('settings')}
        >
          <Settings size={18} />
          <span>Centros de Custo e Categorias</span>
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'dashboard' && <CashFlowDashboard />}
        {activeTab === 'transactions' && <TransactionsList />}
        {activeTab === 'settings' && <FinanceSettings />}
      </div>
    </div>
  );
}
