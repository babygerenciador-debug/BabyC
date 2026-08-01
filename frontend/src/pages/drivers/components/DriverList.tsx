import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Trash2 } from 'lucide-react';
import DriverFormModal from './DriverFormModal';

interface DriverDto {
  id: string;
  userId: string;
  name: string;
  email: string;
  cpfLast4: string;
  cnhNumber: string;
  cnhCategory: string;
  cnhExpirationDate: string;
  isCnhExpired: boolean;
  status: string;
  phone?: string;
  isAvailable: boolean;
  assignedVehicle?: string;
}

function maskCnh(cnh: string): string {
  if (!cnh || cnh.length < 6) return cnh;
  return `***.${cnh.slice(-6, -4)}**-**`;
}

export default function DriverList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading } = useQuery<{ items: DriverDto[], totalCount: number }>({
    queryKey: ['drivers', searchTerm],
    queryFn: async () => {
      const res = await api.get('/drivers', { params: { searchTerm } });
      return res.data;
    }
  });

const handleToggleStatus = async (id: string, isAvailable: boolean) => {
    await api.patch(`/drivers/${id}/availability`, { id, isAvailable: !isAvailable });
    queryClient.invalidateQueries({ queryKey: ['drivers'] });
};

  const handleDelete = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir este motorista?')) return;
    await api.delete(`/drivers/${id}`);
    queryClient.invalidateQueries({ queryKey: ['drivers'] });
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Search size={18} style={{ color: 'var(--text-secondary)' }} />
          <input
            type="text"
            placeholder="Buscar por nome ou CNH..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ border: 'none', background: 'transparent', outline: 'none' }}
          />
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Novo Motorista</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando motoristas...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Email</th>
                <th>CNH</th>
                <th>Categoria</th>
                <th>Vencimento CNH</th>
                <th>Veículo</th>
                <th>Disponível</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((driver) => (
                <tr key={driver.id}>
                  <td style={{ fontWeight: 500 }}>{driver.name}</td>
                  <td>{driver.email}</td>
                  <td title={driver.cnhNumber}>{maskCnh(driver.cnhNumber)}</td>
                  <td>{driver.cnhCategory}</td>
                  <td>{new Date(driver.cnhExpirationDate).toLocaleDateString('pt-BR')}</td>
                  <td>{driver.assignedVehicle || '-'}</td>
                  <td>
                    <span className="status-badge" style={{
                      backgroundColor: driver.isAvailable ? 'var(--success)' : 'var(--error)'
                    }}>
                      {driver.isAvailable ? 'Sim' : 'Não'}
                    </span>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon" title="Alterar Disponibilidade" onClick={() => handleToggleStatus(driver.id, driver.isAvailable)}>
                        <Edit2 size={18} />
                      </button>
                      <button className="btn-icon" title="Excluir" style={{ color: 'var(--error)' }} onClick={() => handleDelete(driver.id)}>
                        <Trash2 size={18} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum motorista encontrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <DriverFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
