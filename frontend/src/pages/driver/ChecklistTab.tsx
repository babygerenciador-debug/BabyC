import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import { ClipboardCheck, CheckCircle, Loader2 } from 'lucide-react';
import { useState } from 'react';

interface ChecklistItemDto {
  id: string;
  checklistItemId: string;
  title: string;
  isCompleted: boolean;
  completedAt?: string;
}

interface DailyChecklistDto {
  id: string;
  vehicleId: string;
  driverId: string;
  date: string;
  status: string;
  completedAt?: string;
  items: ChecklistItemDto[];
}

interface VehicleDto {
  id: string;
  licensePlate: string;
  nickname: string;
}

export default function ChecklistTab() {
  const queryClient = useQueryClient();
  const [completedIds, setCompletedIds] = useState<Set<string>>(new Set());

  const { data: vehicles } = useQuery<VehicleDto[]>({
    queryKey: ['driver-vehicles'],
    queryFn: async () => {
      const res = await api.get('/driver/vehicles');
      return res.data;
    }
  });

  const vehicleId = vehicles?.[0]?.id;

  const { data: checklist, isLoading } = useQuery<DailyChecklistDto | null>({
    queryKey: ['driver-checklist', vehicleId],
    queryFn: async () => {
      if (!vehicleId) return null;
      const res = await api.get('/driver/checklist', { params: { vehicleId } });
      return res.data;
    },
    enabled: !!vehicleId
  });

  const completeMutation = useMutation({
    mutationFn: async (ids: string[]) => {
      await api.post('/driver/checklist/complete', {
        vehicleId,
        checklistItemIds: ids
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['driver-checklist'] });
      setCompletedIds(new Set());
    }
  });

  const handleToggle = (itemId: string) => {
    const next = new Set(completedIds);
    if (next.has(itemId)) next.delete(itemId);
    else next.add(itemId);
    setCompletedIds(next);
  };

  const handleSave = () => {
    if (completedIds.size === 0) return;
    completeMutation.mutate(Array.from(completedIds));
  };

  if (isLoading) {
    return <div style={{ padding: '2rem', textAlign: 'center' }}>Carregando checklist...</div>;
  }

  if (!vehicleId) {
    return (
      <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
        <ClipboardCheck size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
        <p>Nenhum veículo vinculado ao seu perfil.</p>
      </div>
    );
  }

  if (!checklist?.items?.length) {
    return (
      <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
        <ClipboardCheck size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
        <p>Nenhum item de checklist configurado para hoje.</p>
      </div>
    );
  }

  const isCompleted = checklist.status === 'Completed';

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div style={{
        background: 'var(--bg-card)',
        borderRadius: 'var(--radius-md)',
        padding: '1rem',
        boxShadow: 'var(--shadow-sm)'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
          <ClipboardCheck size={24} style={{ color: isCompleted ? 'var(--success)' : 'var(--brand-color)' }} />
          <div>
            <h3 style={{ margin: 0, fontSize: '1rem' }}>Checklist Diário</h3>
            <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
              {checklist.date}
              {vehicles?.[0] && ` — ${vehicles[0].licensePlate}`}
            </p>
          </div>
          {isCompleted && (
            <span style={{ marginLeft: 'auto', color: 'var(--success)', fontWeight: 600, fontSize: '0.875rem' }}>
              <CheckCircle size={18} style={{ display: 'inline', verticalAlign: 'middle', marginRight: '0.25rem' }} />
              Concluído
            </span>
          )}
        </div>

        {checklist.items.map(item => {
          const done = item.isCompleted || completedIds.has(item.checklistItemId);
          return (
            <label key={item.checklistItemId} style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem',
              padding: '0.75rem',
              background: 'var(--bg-color)',
              borderRadius: 'var(--radius-sm)',
              marginBottom: '0.5rem',
              cursor: isCompleted ? 'default' : 'pointer',
              opacity: done ? 0.7 : 1,
              textDecoration: done ? 'line-through' : 'none'
            }}>
              <input
                type="checkbox"
                checked={done}
                disabled={isCompleted}
                onChange={() => handleToggle(item.checklistItemId)}
                style={{ width: '1.25rem', height: '1.25rem', cursor: isCompleted ? 'default' : 'pointer' }}
              />
              <span style={{ flex: 1, fontSize: '0.95rem' }}>{item.title}</span>
              {item.completedAt && (
                <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  {new Date(item.completedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                </span>
              )}
            </label>
          );
        })}

        {!isCompleted && completedIds.size > 0 && (
          <button
            onClick={handleSave}
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
              gap: '0.5rem',
              marginTop: '0.5rem'
            }}
          >
            {completeMutation.isPending ? <Loader2 className="spinner" size={18} /> : <CheckCircle size={18} />}
            {completeMutation.isPending ? 'Salvando...' : `Salvar (${completedIds.size})`}
          </button>
        )}
      </div>
    </div>
  );
}
