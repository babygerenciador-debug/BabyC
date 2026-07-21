import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Play, CheckCircle, XCircle, RefreshCw, DollarSign } from 'lucide-react';
import TripFormModal from './TripFormModal';

interface TripDto {
  id: string;
  driverId: string;
  driverName: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  origin: string;
  destination: string;
  scheduledStartDate: string;
  scheduledEndDate: string;
  tripValue: number;
  paymentStatus: string;
  status: string;
  notes?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  createdAt: string;
}

export default function TripList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading } = useQuery<{ items: TripDto[], totalCount: number }>({
    queryKey: ['trips', searchTerm, statusFilter],
    queryFn: async () => {
      const res = await api.get('/trips', { params: { searchTerm, status: statusFilter || undefined } });
      return res.data;
    }
  });

  const { data: vehiclesData } = useQuery<{ items: { id: string; licensePlate: string; nickname: string; status: string }[] }>({
    queryKey: ['available-vehicles'],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { status: 'Available' } });
      return res.data;
    }
  });

  const handleStartTrip = async (id: string) => {
    await api.post(`/trips/${id}/start`);
    queryClient.invalidateQueries({ queryKey: ['trips'] });
  };

  const handleCompleteTrip = async (id: string) => {
    await api.post(`/trips/${id}/complete`, { checklistCompleted: true });
    queryClient.invalidateQueries({ queryKey: ['trips'] });
  };

  const handleCancelTrip = async (id: string) => {
    if (!confirm('Tem certeza que deseja cancelar esta viagem?')) return;
    await api.delete(`/trips/${id}`);
    queryClient.invalidateQueries({ queryKey: ['trips'] });
  };

  const handleSwapVehicle = async (tripId: string, currentVehicleId: string) => {
    const available = vehiclesData?.items?.filter(v => v.id !== currentVehicleId) || [];
    if (!available.length) {
      alert('Nenhum outro veículo disponível para troca.');
      return;
    }

    const options = available.map((v, i) => `${i + 1}. ${v.nickname} (${v.licensePlate})`).join('\n');
    const choice = prompt(`Selecione o novo veículo:\n\n${options}\n\nDigite o número:`);
    if (!choice) return;

    const idx = parseInt(choice) - 1;
    if (idx < 0 || idx >= available.length) {
      alert('Opção inválida.');
      return;
    }

    await api.patch(`/trips/${tripId}/swap-vehicle`, { newVehicleId: available[idx].id });
    queryClient.invalidateQueries({ queryKey: ['trips'] });
  };

  const handlePayTrip = async (id: string) => {
    await api.patch(`/trips/${id}/pay`);
    queryClient.invalidateQueries({ queryKey: ['trips'] });
    queryClient.invalidateQueries({ queryKey: ['transactions'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
  };

  const statusColors: Record<string, string> = {
    Created: 'var(--brand-color)',
    InProgress: 'var(--warning)',
    Completed: 'var(--success)',
    Cancelled: 'var(--error)',
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Search size={18} style={{ color: 'var(--text-secondary)' }} />
          <input
            type="text"
            placeholder="Buscar por origem ou destino..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ border: 'none', background: 'transparent', outline: 'none' }}
          />
        </div>
        <div className="search-box">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            style={{ padding: '0.5rem', borderRadius: 'var(--border-radius-sm)' }}
          >
            <option value="">Todos os status</option>
                <option value="Created">Agendado</option>
            <option value="InProgress">Em Andamento</option>
            <option value="Completed">Concluído</option>
            <option value="Cancelled">Cancelado</option>
          </select>
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Nova Viagem</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando viagens...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Motorista</th>
                <th>Veículo</th>
                <th>Origem</th>
                <th>Destino</th>
                <th>Valor</th>
                <th>Pagamento</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((trip) => (
                <tr key={trip.id}>
                  <td style={{ fontWeight: 500 }}>{trip.driverName}</td>
                  <td>{trip.vehicleLicensePlate}</td>
                  <td>{trip.origin}</td>
                  <td>{trip.destination}</td>
                  <td>{formatCurrency(trip.tripValue)}</td>
                  <td>
                    <span className="status-badge" style={{ 
                      backgroundColor: trip.paymentStatus === 'Paid' ? 'rgba(16, 185, 129, 0.1)' : 'rgba(245, 158, 11, 0.1)',
                      color: trip.paymentStatus === 'Paid' ? 'var(--success)' : 'var(--warning)'
                    }}>
                      {trip.paymentStatus === 'Paid' ? 'Pago' : 'Pendente'}
                    </span>
                  </td>
                  <td>
                    <span className="status-badge" style={{ backgroundColor: statusColors[trip.status] || 'var(--text-secondary)' }}>
                       {trip.status === 'Created' ? 'Agendado' :
                       trip.status === 'InProgress' ? 'Em Andamento' :
                       trip.status === 'Completed' ? 'Concluído' :
                       trip.status === 'Cancelled' ? 'Cancelado' : trip.status}
                    </span>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      {trip.status === 'Created' && (
                        <button className="btn-icon" title="Iniciar Viagem" onClick={() => handleStartTrip(trip.id)}>
                          <Play size={18} style={{ color: 'var(--success)' }} />
                        </button>
                      )}
                      {trip.status === 'InProgress' && (
                        <button className="btn-icon" title="Concluir Viagem" onClick={() => handleCompleteTrip(trip.id)}>
                          <CheckCircle size={18} style={{ color: 'var(--brand-color)' }} />
                        </button>
                      )}
                      {(trip.status === 'Created' || trip.status === 'InProgress') && (
                        <>
                          {trip.paymentStatus === 'Pending' && trip.tripValue > 0 && (
                            <button className="btn-icon" title="Marcar como Pago" onClick={() => handlePayTrip(trip.id)}>
                              <DollarSign size={18} style={{ color: 'var(--success)' }} />
                            </button>
                          )}
                          <button className="btn-icon" title="Trocar Veículo" onClick={() => handleSwapVehicle(trip.id, trip.vehicleId)}>
                            <RefreshCw size={18} style={{ color: 'var(--brand-color)' }} />
                          </button>
                          <button className="btn-icon" title="Cancelar Viagem" onClick={() => handleCancelTrip(trip.id)}>
                            <XCircle size={18} style={{ color: 'var(--error)' }} />
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhuma viagem encontrada.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <TripFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
