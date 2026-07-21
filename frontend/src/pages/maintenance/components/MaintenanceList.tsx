import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Trash2 } from 'lucide-react';
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

export default function MaintenanceList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

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

  const typeLabels: Record<string, string> = {
    Preventive: 'Preventiva',
    Corrective: 'Corretiva',
    Predictive: 'Preditiva',
    Inspection: 'Inspeção',
  };

  const statusLabels: Record<string, string> = {
    Scheduled: 'Agendada',
    InProgress: 'Em Andamento',
    Completed: 'Concluída',
    Cancelled: 'Cancelada',
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
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
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
                    <span className="status-badge" style={{
                      backgroundColor: item.status === 'Completed' ? 'var(--success)' :
                        item.status === 'InProgress' ? 'var(--warning)' :
                        item.status === 'Scheduled' ? 'var(--brand-color)' : 'var(--error)'
                    }}>
                      {statusLabels[item.status] || item.status}
                    </span>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon" title="Editar"><Edit2 size={18} /></button>
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

      {isModalOpen && <MaintenanceFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
