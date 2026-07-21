import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { X, Save, Loader2 } from 'lucide-react';

const tripSchema = z.object({
  driverId: z.string().min(1, 'Motorista é obrigatório'),
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  origin: z.string().min(2, 'Origem é obrigatória'),
  destination: z.string().min(2, 'Destino é obrigatório'),
  scheduledStartDate: z.string().min(1, 'Data de início é obrigatória'),
  scheduledEndDate: z.string().min(1, 'Data de término é obrigatória'),
  tripValue: z.number().min(0, 'Valor deve ser positivo'),
  paymentStatus: z.enum(['Pending', 'Paid']),
  notes: z.string().optional(),
});

type TripFormData = z.infer<typeof tripSchema>;

interface Props {
  onClose: () => void;
}

export default function TripFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();
  const { register, handleSubmit, formState: { errors } } = useForm<TripFormData>({
    resolver: zodResolver(tripSchema),
    defaultValues: {
      scheduledStartDate: new Date().toISOString().substring(0, 16),
      scheduledEndDate: new Date(Date.now() + 3600000).toISOString().substring(0, 16),
      tripValue: 0,
      paymentStatus: 'Pending',
    }
  });

  const { data: drivers } = useQuery<{ items: { id: string; name: string; cnhNumber: string }[] }>({
    queryKey: ['drivers-select'],
    queryFn: async () => {
      const res = await api.get('/drivers');
      return res.data;
    }
  });

  const { data: vehicles } = useQuery<{ items: { id: string; licensePlate: string; nickname: string }[] }>({
    queryKey: ['vehicles-select'],
    queryFn: async () => {
      const res = await api.get('/vehicles');
      return res.data;
    }
  });

  const mutation = useMutation({
    mutationFn: (data: TripFormData) => api.post('/trips', {
      ...data,
      scheduledStartDate: new Date(data.scheduledStartDate).toISOString(),
      scheduledEndDate: new Date(data.scheduledEndDate).toISOString(),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trips'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      onClose();
    }
  });

  const onSubmit = (data: TripFormData) => mutation.mutate(data);

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel animate-fade-in">
        <div className="modal-header">
          <h2>Nova Viagem</h2>
          <button className="btn-icon" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="modal-form">
          <div className="form-grid">
            <div className="input-group">
              <label>Motorista *</label>
              <select {...register('driverId')}>
                <option value="">Selecione...</option>
                {drivers?.items?.map((d) => (
                  <option key={d.id} value={d.id}>{d.name}</option>
                ))}
              </select>
              {errors.driverId && <span className="error-msg">{errors.driverId.message}</span>}
            </div>
            <div className="input-group">
              <label>Veículo *</label>
              <select {...register('vehicleId')}>
                <option value="">Selecione...</option>
                {vehicles?.items?.map((v) => (
                  <option key={v.id} value={v.id}>{v.licensePlate} - {v.nickname}</option>
                ))}
              </select>
              {errors.vehicleId && <span className="error-msg">{errors.vehicleId.message}</span>}
            </div>
            <div className="input-group">
              <label>Origem *</label>
              <input {...register('origin')} placeholder="Cidade de origem" />
              {errors.origin && <span className="error-msg">{errors.origin.message}</span>}
            </div>
            <div className="input-group">
              <label>Destino *</label>
              <input {...register('destination')} placeholder="Cidade de destino" />
              {errors.destination && <span className="error-msg">{errors.destination.message}</span>}
            </div>
            <div className="input-group">
              <label>Data/Hora Início *</label>
              <input type="datetime-local" {...register('scheduledStartDate')} />
              {errors.scheduledStartDate && <span className="error-msg">{errors.scheduledStartDate.message}</span>}
            </div>
            <div className="input-group">
              <label>Data/Hora Término *</label>
              <input type="datetime-local" {...register('scheduledEndDate')} />
              {errors.scheduledEndDate && <span className="error-msg">{errors.scheduledEndDate.message}</span>}
            </div>
            <div className="input-group">
              <label>Valor da Viagem (R$) *</label>
              <input type="number" step="0.01" min="0" {...register('tripValue', { valueAsNumber: true })} />
              {errors.tripValue && <span className="error-msg">{errors.tripValue.message}</span>}
            </div>
            <div className="input-group">
              <label>Pagamento *</label>
              <div style={{ display: 'flex', gap: '1rem' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontWeight: 500 }}>
                  <input type="radio" value="Pending" {...register('paymentStatus')} />
                  Pendente
                </label>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontWeight: 500 }}>
                  <input type="radio" value="Paid" {...register('paymentStatus')} />
                  Pago
                </label>
              </div>
              {errors.paymentStatus && <span className="error-msg">{errors.paymentStatus.message}</span>}
            </div>
            <div className="input-group" style={{ gridColumn: '1 / -1' }}>
              <label>Observações</label>
              <textarea {...register('notes')} placeholder="Observações da viagem..." rows={3} />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
              <span>{mutation.isPending ? 'Salvando...' : 'Criar Viagem'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
