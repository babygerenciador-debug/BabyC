import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Trash2, DollarSign, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import MaintenanceFormModal from './MaintenanceFormModal';

interface MaintenanceDto {
  id: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  type: string;
  status: string;
  date: string;
  odometer: number;
  description: string;
  totalCost: number;
  providerName?: string;
  notes?: string;
}

const statusOptions = [
  { value: 'Scheduled', label: 'Agendada' },
  { value: 'InProgress', label: 'Em Andamento' },
  { value: 'Completed', label: 'Concluída' },
  { value: 'Cancelled', label: 'Cancelada' },
];

export default function MaintenanceList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editMaintenance, setEditMaintenance] = useState<MaintenanceDto | undefined>(undefined);
  const [openStatusId, setOpenStatusId] = useState<string | null>(null);
  const [paidIds, setPaidIds] = useState<Set<string>>(() => {
    try { return new Set(JSON.parse(localStorage.getItem('paidIds') || '[]')); } catch { return new Set(); }
  });

  const markPaid = (id: string) => {
    const next = new Set(paidIds);
    next.add(id);
    setPaidIds(next);
    localStorage.setItem('paidIds', JSON.stringify([...next]));
  };

  const { data, isLoading } = useQuery<{ items: MaintenanceDto[], totalCount: number }>({
    queryKey: ['maintenances', searchTerm],
    queryFn: async () => {
      const res = await api.get('/maintenances', { params: { searchTerm } });
      return res.data;
    }
  });

  const handleDelete = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir esta manutenção?')) return;
    await api.delete(`/maintenances/${id}`);
    queryClient.invalidateQueries({ queryKey: ['maintenances'] });
  };

  const payMutation = useMutation({
    mutationFn: async (item: MaintenanceDto) => {
      const catRes = await api.get('/finance/categories');
      const expenseCat = catRes.data?.find((c: any) => c.type === 'Expense');
      return api.post('/finance/transactions', {
        categoryId: expenseCat?.id || '',
        type: 'Expense',
        amount: item.totalCost,
        date: new Date().toISOString(),
        description: `Manutenção - ${item.description} (${item.vehicleLicensePlate})`,
        status: 'Paid',
        paymentDate: new Date().toISOString(),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenances'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      toast.success('Manutenção paga e registrada no financeiro');
    },
    onError: () => {
      toast.error('Erro ao pagar manutenção. Verifique se há categorias de despesa cadastradas.');
    }
  });

  const statusMutation = useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }) => {
      const res = await api.get(`/maintenances/${id}`);
      const item = res.data;
      await api.put(`/maintenances/${id}`, { ...item, id, status });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenances'] });
      toast.success('Status atualizado');
    },
    onError: () => {
      toast.error('Erro ao atualizar status');
    }
  });

  const statusLabels: Record<string, string> = {
    Scheduled: 'Agendada',
    scheduled: 'Agendada',
    InProgress: 'Em Andamento',
    inProgress: 'Em Andamento',
    Completed: 'Concluída',
    completed: 'Concluída',
    Cancelled: 'Cancelada',
    cancelled: 'Cancelada',
  };

  const statusColor: Record<string, string> = {
    Scheduled: 'var(--brand-color)',
    scheduled: 'var(--brand-color)',
    InProgress: 'var(--warning)',
    inProgress: 'var(--warning)',
    Completed: 'var(--success)',
    completed: 'var(--success)',
    Cancelled: 'var(--error)',
    cancelled: 'var(--error)',
  };

  const typeLabels: Record<string, string> = {
    Preventive: 'Preventiva',
    preventive: 'Preventiva',
    Corrective: 'Corretiva',
    corrective: 'Corretiva',
    Inspection: 'Inspeção',
    inspection: 'Inspeção',
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Search size={18} style={{ color: 'var(--text-secondary)' }} />
          <input
            type="text"
            placeholder="Buscar por descrição ou placa..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ border: 'none', background: 'transparent', outline: 'none' }}
          />
        </div>
        <button className="btn-primary" onClick={() => { setEditMaintenance(undefined); setIsModalOpen(true); }}>
          <Plus size={18} />
          <span>Nova Manutenção</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando manutenções...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Veículo</th>
                <th>Tipo</th>
                <th>Data</th>
                <th>Odômetro</th>
                <th>Descrição</th>
                <th>Fornecedor</th>
                <th>Custo</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((item) => (
                <tr key={item.id}>
                  <td style={{ fontWeight: 500 }}>{item.vehicleLicensePlate}</td>
                  <td>{typeLabels[item.type] || item.type}</td>
                  <td>{new Date(item.date).toLocaleDateString('pt-BR')}</td>
                  <td>{item.odometer.toLocaleString('pt-BR')} km</td>
                  <td>{item.description}</td>
                  <td>{item.providerName || '-'}</td>
                  <td>{item.totalCost.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
                  <td>
                    <span className="status-badge" style={{ backgroundColor: statusColor[item.status] || 'var(--text-secondary)' }}>
                      {statusLabels[item.status] || item.status}
                    </span>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      {['Completed', 'completed'].includes(item.status) && !paidIds.has(item.id) && (
                        <button
                          className="btn-icon"
                          title="Pagar"
                          style={{ color: 'var(--success)' }}
                          onClick={() => { markPaid(item.id); payMutation.mutate(item); }}
                          disabled={payMutation.isPending}
                        >
                          {payMutation.isPending ? <Loader2 className="spinner" size={18} /> : <DollarSign size={18} />}
                        </button>
                      )}
                      <select
                        className="status-select"
                        value={item.status}
                        onChange={(e) => {
                          if (e.target.value !== item.status) {
                            statusMutation.mutate({ id: item.id, status: e.target.value });
                          }
                        }}
                        disabled={statusMutation.isPending}
                      >
                        {statusOptions.map((opt) => (
                          <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                      </select>
                      <button className="btn-icon" title="Editar" onClick={() => { setEditMaintenance(item); setIsModalOpen(true); }}>
                        <Edit2 size={18} />
                      </button>
                      <button className="btn-icon" title="Excluir" style={{ color: 'var(--error)' }} onClick={() => handleDelete(item.id)}>
                        <Trash2 size={18} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={9} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhuma manutenção encontrada.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && (
        <MaintenanceFormModal
          maintenance={editMaintenance}
          onClose={() => {
            setIsModalOpen(false);
            setEditMaintenance(undefined);
            queryClient.invalidateQueries({ queryKey: ['maintenances'] });
          }}
        />
      )}
    </div>
  );
}
