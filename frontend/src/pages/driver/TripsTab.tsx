import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import { Play, CheckCircle, MapPin, Clock } from 'lucide-react';
import { useState } from 'react';

interface TripDto {
  id: string;
  driverId: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  origin: string;
  destination: string;
  notes?: string;
  status: string;
  scheduledStartDate: string;
  actualStartDate?: string;
}

export default function TripsTab() {
  const queryClient = useQueryClient();
  const [checklistNotes, setChecklistNotes] = useState('');

  const { data: trips, isLoading } = useQuery<{ items: TripDto[] }>({
    queryKey: ['driver-trips'],
    queryFn: async () => {
      const res = await api.get('/driver/trips');
      return res.data;
    }
  });

  const startMutation = useMutation({
    mutationFn: (tripId: string) => api.post(`/driver/trips/${tripId}/start`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['driver-trips'] });
    }
  });

  const completeMutation = useMutation({
    mutationFn: (tripId: string) => api.post(`/driver/trips/${tripId}/complete`, {
      checklistCompleted: true,
      checklistNotes: checklistNotes || null
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['driver-trips'] });
      setChecklistNotes('');
    }
  });

  const formatDateTime = (dateStr?: string) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const statusColors: Record<string, string> = {
    Created: 'var(--brand-color)',
    InProgress: 'var(--warning)',
    Completed: 'var(--success)',
    Cancelled: 'var(--error)',
  };

  if (isLoading) {
    return <div style={{ padding: '2rem', textAlign: 'center' }}>Carregando viagens...</div>;
  }

  if (!trips?.items?.length) {
    return (
      <div style={{
        padding: '2rem',
        textAlign: 'center',
        color: 'var(--text-secondary)'
      }}>
        <MapPin size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
        <p>Nenhuma viagem atribuída</p>
      </div>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      {trips.items.map((trip) => (
        <div key={trip.id} style={{
          background: 'var(--bg-card)',
          borderRadius: 'var(--radius-md)',
          padding: '1rem',
          boxShadow: 'var(--shadow-sm)'
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.75rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <MapPin size={18} style={{ color: 'var(--brand-color)' }} />
              <span style={{ fontWeight: 600 }}>{trip.origin}</span>
            </div>
            <span className="status-badge" style={{
              backgroundColor: statusColors[trip.status],
              color: 'white',
              padding: '0.25rem 0.75rem',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.875rem',
              fontWeight: 500
            }}>
              {trip.status === 'Created' ? 'Agendada' :
               trip.status === 'InProgress' ? 'Em Andamento' :
               trip.status === 'Completed' ? 'Concluída' : trip.status}
            </span>
          </div>

          <div style={{ marginBottom: '0.75rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
              <MapPin size={16} style={{ color: 'var(--text-secondary)' }} />
              <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Destino:</span>
              <span style={{ fontWeight: 500 }}>{trip.destination}</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
              <Clock size={16} style={{ color: 'var(--text-secondary)' }} />
              <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Início:</span>
              <span style={{ fontWeight: 500 }}>{formatDateTime(trip.scheduledStartDate)}</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Veículo:</span>
              <span style={{ fontWeight: 500 }}>{trip.vehicleLicensePlate}</span>
            </div>
          </div>

          {trip.notes && (
            <div style={{
              padding: '0.75rem',
              background: 'var(--bg-color)',
              borderRadius: 'var(--radius-sm)',
              marginBottom: '0.75rem',
              fontSize: '0.875rem',
              color: 'var(--text-secondary)'
            }}>
              {trip.notes}
            </div>
          )}

          {trip.status === 'Created' && (
            <button
              onClick={() => startMutation.mutate(trip.id)}
              disabled={startMutation.isPending}
              style={{
                width: '100%',
                padding: '0.75rem',
                background: 'var(--success)',
                color: 'white',
                border: 'none',
                borderRadius: 'var(--radius-sm)',
                fontSize: '1rem',
                fontWeight: 600,
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '0.5rem'
              }}
            >
              <Play size={18} />
              {startMutation.isPending ? 'Iniciando...' : 'Iniciar Viagem'}
            </button>
          )}

          {trip.status === 'InProgress' && (
            <div>
              <textarea
                value={checklistNotes}
                onChange={(e) => setChecklistNotes(e.target.value)}
                placeholder="Observações do checklist..."
                rows={2}
                style={{
                  width: '100%',
                  padding: '0.75rem',
                  borderRadius: 'var(--radius-md)',
                  border: '1px solid var(--border-color)',
                  fontFamily: 'inherit',
                  fontSize: '0.875rem',
                  marginBottom: '0.75rem',
                  resize: 'vertical'
                }}
              />
              <button
                onClick={() => completeMutation.mutate(trip.id)}
                disabled={completeMutation.isPending}
                style={{
                  width: '100%',
                  padding: '0.75rem',
                  background: 'var(--brand-color)',
                  color: 'white',
                  border: 'none',
                  borderRadius: 'var(--radius-md)',
                  fontSize: '1rem',
                  fontWeight: 600,
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '0.5rem'
                }}
              >
                <CheckCircle size={18} />
                {completeMutation.isPending ? 'Concluindo...' : 'Concluir Viagem'}
              </button>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
