import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { DollarSign, TrendingUp, TrendingDown, Landmark, Save, Wallet } from 'lucide-react';
import './CashFlowDashboard.css';

interface CashFlowSummaryDto {
  ownerSalary: number;
  ownerTaxAmount: number;
  netOwnerSalary: number;
  totalRevenues: number;
  totalExpenses: number;
  netBalance: number;
}

export default function CashFlowDashboard() {
  const [ownerSalaryInput, setOwnerSalaryInput] = useState<string>('');
  const [activeSalary, setActiveSalary] = useState<number>(0);

  // Load from LocalStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem('@fleetos:ownerSalary');
    if (saved) {
      setOwnerSalaryInput(saved);
      setActiveSalary(Number(saved));
    }
  }, []);

  const handleSaveSalary = () => {
    const num = Number(ownerSalaryInput);
    if (!isNaN(num) && num >= 0) {
      localStorage.setItem('@fleetos:ownerSalary', num.toString());
      setActiveSalary(num);
    }
  };

  const { data, isLoading } = useQuery<CashFlowSummaryDto>({
    queryKey: ['cash-flow-summary', activeSalary],
    queryFn: async () => {
      const res = await api.get('/finance/transactions/summary', { 
        params: { ownerSalary: activeSalary } 
      });
      return res.data;
    }
  });

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
  };

  return (
    <div className="cash-flow-container animate-fade-in">
      <div className="salary-config-panel glass-panel">
        <div className="salary-info">
          <h3>Defina o Salário Base (Dono)</h3>
          <p>Baseado na regra de negócio: O cálculo do Fluxo de Caixa será (Salário Bruto - 27% Impostos). O que sobrar soma-se às receitas e deduz-se as despesas.</p>
        </div>
        <div className="salary-input-group">
          <div className="input-wrapper">
            <DollarSign size={18} className="input-icon" />
            <input 
              type="number" 
              step="0.01" 
              placeholder="Ex: 10000.00" 
              value={ownerSalaryInput}
              onChange={(e) => setOwnerSalaryInput(e.target.value)}
            />
          </div>
          <button className="btn-primary" onClick={handleSaveSalary}>
            <Save size={18} /> Aplicar e Calcular
          </button>
        </div>
      </div>

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
            <div className="kpi-content" style={{ textAlign: 'center', width: '100%' }}>
              <span style={{ fontSize: '1.2rem', textTransform: 'uppercase', letterSpacing: '1px' }}>
                Saldo Final / Lucro Real da Empresa
              </span>
              <h1 style={{ fontSize: '3rem', margin: '1rem 0' }}>{formatCurrency(data.netBalance)}</h1>
              <p style={{ opacity: 0.9 }}>
                Fórmula: (Salário Líquido + Receitas) - Despesas
              </p>
            </div>
          </div>

        </div>
      ) : null}
    </div>
  );
}
