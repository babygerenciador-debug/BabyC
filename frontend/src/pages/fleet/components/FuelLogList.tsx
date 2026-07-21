import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Filter, Eye } from 'lucide-react';
import { format } from 'date-fns';
import FuelLogFormModal from './FuelLogFormModal';

interface FuelLogDto {
  id: string;
  vehicleId: string;
  licensePlate?: string; // Supondo que venha populado ou join
  date: string;
  odometer: number;
  liters: number;
  totalCost: number;
  receiptUrl?: string;
}

export default function FuelLogList() {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading } = useQuery<{ items: FuelLogDto[], totalCount: number }>({
    queryKey: ['fuellogs'],
    queryFn: async () => {
      const res = await api.get('/fuellogs');
      return res.data;
    }
  });

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Filter size={18} style={{ color: 'var(--text-secondary)' }} />
          <span>Filtros avançados (Veículo, Data)...</span>
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Registrar Abastecimento</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando abastecimentos...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Veículo (ID)</th>
                <th>Odômetro</th>
                <th>Litros</th>
                <th>Custo Total</th>
                <th>Comprovante</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((log) => (
                <tr key={log.id}>
                  <td>{format(new Date(log.date), 'dd/MM/yyyy HH:mm')}</td>
                  <td>{log.vehicleId.substring(0,8)}...</td>
                  <td>{log.odometer} km</td>
                  <td>{log.liters} L</td>
                  <td style={{ color: 'var(--error)', fontWeight: 500 }}>
                    {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(log.totalCost)}
                  </td>
                  <td>
                    {log.receiptUrl ? (
                      <a href={log.receiptUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--info)' }}>Ver Anexo</a>
                    ) : '-'}
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon" title="Ver detalhes" onClick={() => alert(`Data: ${format(new Date(log.date), 'dd/MM/yyyy HH:mm')}\nVeículo: ${log.licensePlate || log.vehicleId}\nOdômetro: ${log.odometer} km\nLitros: ${log.liters} L\nCusto: R$ ${log.totalCost.toFixed(2)}`)}><Eye size={18} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum abastecimento registrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <FuelLogFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
