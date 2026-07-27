import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, CheckCircle, XCircle, Filter, Trash2 } from 'lucide-react';
import { format } from 'date-fns';
import TransactionFormModal from './TransactionFormModal';

interface TransactionDto {
  id: string;
  categoryName: string;
  costCenterName?: string;
  type: 'Revenue' | 'Expense';
  amount: number;
  date: string;
  paymentDate?: string;
  description: string;
  status: 'Pending' | 'Paid' | 'Cancelled';
}

export default function TransactionsList() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery<{ items: TransactionDto[] }>({
    queryKey: ['transactions'],
    queryFn: async () => {
      const res = await api.get('/finance/transactions', { params: { pageSize: 50 } });
      return res.data;
    }
  });

  const payMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/finance/transactions/${id}/pay`, { id, paymentDate: new Date().toISOString() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      queryClient.invalidateQueries({ queryKey: ['financial-month-report'] });
    }
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/finance/transactions/${id}/cancel`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      queryClient.invalidateQueries({ queryKey: ['financial-month-report'] });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/finance/transactions/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      queryClient.invalidateQueries({ queryKey: ['financial-month-report'] });
    }
  });

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
  };

  const renderStatus = (status: string) => {
    switch (status) {
      case 'Pending': return <span className="badge-status status-pending">Pendente</span>;
      case 'Paid': return <span className="badge-status status-paid">Pago/Recebido</span>;
      case 'Cancelled': return <span className="badge-status status-cancelled">Cancelado</span>;
      default: return <span>{status}</span>;
    }
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Filter size={18} style={{ color: 'var(--text-secondary)' }} />
          <span>Filtros (Por Tipo, Status, Data)...</span>
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Nova Transação</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando transações...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Descrição</th>
                <th>Categoria</th>
                <th>Centro de Custo</th>
                <th>Valor (R$)</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((tx) => (
                <tr key={tx.id}>
                  <td>{format(new Date(tx.date), 'dd/MM/yyyy')}</td>
                  <td style={{ fontWeight: 500 }}>{tx.description}</td>
                  <td>{tx.categoryName}</td>
                  <td>{tx.costCenterName || '-'}</td>
                  <td className={tx.type === 'Revenue' ? 'transaction-type-in' : 'transaction-type-out'}>
                    {tx.type === 'Revenue' ? '+' : '-'} {formatCurrency(tx.amount)}
                  </td>
                  <td>{renderStatus(tx.status)}</td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      {tx.status === 'Pending' && (
                        <>
                          <button 
                            className="btn-icon" 
                            style={{ color: 'var(--success)' }} 
                            title="Marcar como Pago"
                            onClick={() => {
                              if(window.confirm('Confirmar o pagamento/recebimento?')) payMutation.mutate(tx.id);
                            }}
                          >
                            <CheckCircle size={18} />
                          </button>
                          <button 
                            className="btn-icon" 
                            style={{ color: 'var(--error)' }} 
                            title="Cancelar"
                            onClick={() => {
                              if(window.confirm('Deseja realmente cancelar esta transação?')) cancelMutation.mutate(tx.id);
                            }}
                          >
                            <XCircle size={18} />
                          </button>
                        </>
                      )}
                      <button 
                        className="btn-icon" 
                        style={{ color: 'var(--error)' }} 
                        title="Excluir"
                        onClick={() => {
                          if(window.confirm('Excluir permanentemente esta transação?')) deleteMutation.mutate(tx.id);
                        }}
                      >
                        <Trash2 size={18} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhuma transação financeira encontrada.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <TransactionFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
