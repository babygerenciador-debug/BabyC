import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import { AlertTriangle, Send, Loader2, CheckCircle } from 'lucide-react';

interface VehicleDto {
  id: string;
  licensePlate: string;
  nickname: string;
}

export default function IssuesTab() {
  const queryClient = useQueryClient();
  const [vehicleId, setVehicleId] = useState('');
  const [description, setDescription] = useState('');
  const [sent, setSent] = useState(false);

  const { data: vehicles } = useQuery<VehicleDto[]>({
    queryKey: ['my-vehicles'],
    queryFn: async () => {
      const res = await api.get('/driver/vehicles');
      return res.data;
    }
  });

  const mutation = useMutation({
    mutationFn: (data: { vehicleId: string; description: string }) =>
      api.post('/driver/issues', data),
    onSuccess: () => {
      setSent(true);
      setDescription('');
      queryClient.invalidateQueries({ queryKey: ['my-issues'] });
      queryClient.invalidateQueries({ queryKey: ['vehicleIssues'] });
      setTimeout(() => setSent(false), 3000);
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!vehicleId || !description.trim()) return;
    mutation.mutate({ vehicleId, description: description.trim() });
  };

  return (
    <div>
      <div style={{
        background: 'var(--bg-card)',
        borderRadius: 'var(--radius-md)',
        padding: '1.5rem',
        boxShadow: 'var(--shadow-sm)'
      }}>
        <h3 style={{ marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <AlertTriangle size={20} style={{ color: 'var(--warning)' }} />
          Reportar Alerta
        </h3>

        {sent ? (
          <div style={{
            textAlign: 'center', padding: '2rem', color: 'var(--success)',
            display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '0.5rem'
          }}>
            <CheckCircle size={48} />
            <p style={{ fontWeight: 600, margin: 0 }}>Alerta enviado com sucesso!</p>
            <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', margin: 0 }}>
              A administração foi notificada.
            </p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>Veículo *</label>
              <select value={vehicleId} onChange={e => setVehicleId(e.target.value)}
                style={{
                  width: '100%', padding: '0.75rem', borderRadius: 'var(--radius-md)',
                  border: '1px solid var(--border-color)', fontSize: '1rem'
                }}>
                <option value="">Selecione...</option>
                {vehicles?.map(v => (
                  <option key={v.id} value={v.id}>{v.licensePlate} - {v.nickname}</option>
                ))}
              </select>
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>Descrição do Alerta *</label>
              <textarea value={description} onChange={e => setDescription(e.target.value)}
                placeholder="Ex: Pneu dianteiro esquerdo está careca, precisa de troca..."
                rows={4}
                style={{
                  width: '100%', padding: '0.75rem', borderRadius: 'var(--radius-md)',
                  border: '1px solid var(--border-color)', fontSize: '1rem', resize: 'vertical'
                }} />
            </div>

            <button type="submit" disabled={mutation.isPending || !vehicleId || !description.trim()}
              style={{
                width: '100%', padding: '0.75rem', background: 'var(--warning)', color: 'white',
                border: 'none', borderRadius: 'var(--radius-md)', fontSize: '1rem', fontWeight: 600,
                cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.5rem'
              }}>
              {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Send size={18} />}
              {mutation.isPending ? 'Enviando...' : 'Enviar Alerta'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
