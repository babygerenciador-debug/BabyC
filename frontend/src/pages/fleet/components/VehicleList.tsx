import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Eye, Trash2 } from 'lucide-react';
import VehicleFormModal from './VehicleFormModal';

interface VehicleDto {
  id: string;
  licensePlate: string;
  nickname: string;
  status: string;
  capacity?: number;
}

export default function VehicleList() {
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading } = useQuery<{ items: VehicleDto[], totalCount: number }>({
    queryKey: ['vehicles', searchTerm],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { searchTerm } });
      return res.data;
    }
  });

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Search size={18} style={{ color: 'var(--text-secondary)' }} />
          <input 
            type="text" 
            placeholder="Buscar por placa ou apelido..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ border: 'none', background: 'transparent', outline: 'none' }}
          />
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Novo Veículo</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando veículos...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Placa</th>
                <th>Apelido</th>
                <th>Status</th>
                <th>Capacidade</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((vehicle) => (
                <tr key={vehicle.id}>
                  <td style={{ fontWeight: 500 }}>{vehicle.licensePlate}</td>
                  <td>{vehicle.nickname}</td>
                  <td>
                    <span className="status-badge">{vehicle.status}</span>
                  </td>
                  <td>{vehicle.capacity || 'N/A'}</td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon"><Eye size={18} /></button>
                      <button className="btn-icon"><Edit2 size={18} /></button>
                      <button className="btn-icon" style={{ color: 'var(--error)' }}><Trash2 size={18} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum veículo encontrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <VehicleFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
