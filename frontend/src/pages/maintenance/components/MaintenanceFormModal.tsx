import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const maintenanceSchema = z.object({
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  type: z.string().min(1, 'Tipo é obrigatório'),
  status: z.string().min(1, 'Status é obrigatório'),
  date: z.string().min(1, 'Data é obrigatória'),
  odometer: z.string().min(1, 'Odômetro é obrigatório'),
  description: z.string().min(3, 'Descrição é obrigatória'),
  totalCost: z.string().min(1, 'Custo é obrigatório'),
  providerName: z.string().optional(),
  notes: z.string().optional(),
});

type MaintenanceFormData = z.infer<typeof maintenanceSchema>;

interface MaintenanceData {
  id: string;
  vehicleId: string;
  type: string;
  status: string;
  date: string;
  odometer: number;
  description: string;
  totalCost: number;
  providerName?: string;
  notes?: string;
}

interface Props {
  maintenance?: MaintenanceData;
  onClose: () => void;
}

const FORM_ID = 'maintenance-form';

export default function MaintenanceFormModal({ maintenance, onClose }: Props) {
  const queryClient = useQueryClient();
  const isEditing = !!maintenance;

  const { register, handleSubmit, reset, formState: { errors } } = useForm<MaintenanceFormData>({
    resolver: zodResolver(maintenanceSchema)
  });

  useEffect(() => {
    if (maintenance) {
      reset({
        vehicleId: maintenance.vehicleId,
        type: maintenance.type,
        status: maintenance.status,
        date: maintenance.date.slice(0, 10),
        odometer: String(maintenance.odometer),
        description: maintenance.description,
        totalCost: String(maintenance.totalCost),
        providerName: maintenance.providerName || '',
        notes: maintenance.notes || '',
      });
    }
  }, [maintenance, reset]);

  const { data: vehicles } = useQuery<{ items: { id: string; licensePlate: string; nickname: string }[] }>({
    queryKey: ['vehicles-select'],
    queryFn: async () => {
      const res = await api.get('/vehicles');
      return res.data;
    }
  });

  const mutation = useMutation({
    mutationFn: (data: MaintenanceFormData) => {
      const payload = {
        ...data,
        odometer: parseInt(data.odometer),
        totalCost: parseFloat(data.totalCost),
        date: new Date(data.date).toISOString(),
      };
      if (isEditing) {
        return api.put(`/maintenances/${maintenance!.id}`, { ...payload, id: maintenance!.id });
      }
      return api.post('/maintenances', payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenances'] });
      onClose();
    }
  });

  const onSubmit = (data: MaintenanceFormData) => mutation.mutate(data);

  return (
    <BaseModal
      open
      onClose={onClose}
      title={isEditing ? 'Editar Manutenção' : 'Nova Manutenção'}
      maxWidth="600px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Salvando...' : 'Salvar'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
        <div className="form-grid">
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
            <label>Tipo *</label>
            <select {...register('type')}>
              <option value="">Selecione...</option>
              <option value="Preventive">Preventiva</option>
              <option value="Corrective">Corretiva</option>
              <option value="Inspection">Inspeção</option>
            </select>
            {errors.type && <span className="error-msg">{errors.type.message}</span>}
          </div>
          <div className="input-group">
            <label>Status *</label>
            <select {...register('status')}>
              <option value="">Selecione...</option>
              <option value="Scheduled">Agendada</option>
              <option value="InProgress">Em Andamento</option>
              <option value="Completed">Concluída</option>
              <option value="Cancelled">Cancelada</option>
            </select>
            {errors.status && <span className="error-msg">{errors.status.message}</span>}
          </div>
          <div className="input-group">
            <label>Data *</label>
            <input type="date" {...register('date')} />
            {errors.date && <span className="error-msg">{errors.date.message}</span>}
          </div>
          <div className="input-group">
            <label>Odômetro (km) *</label>
            <input type="number" {...register('odometer')} placeholder="100000" />
            {errors.odometer && <span className="error-msg">{errors.odometer.message}</span>}
          </div>
          <div className="input-group">
            <label>Custo Total (R$) *</label>
            <input type="number" step="0.01" {...register('totalCost')} placeholder="0.00" />
            {errors.totalCost && <span className="error-msg">{errors.totalCost.message}</span>}
          </div>
          <div className="input-group">
            <label>Fornecedor</label>
            <input {...register('providerName')} placeholder="Nome da oficina" />
          </div>
          <div className="input-group" style={{ gridColumn: '1 / -1' }}>
            <label>Descrição *</label>
            <textarea {...register('description')} placeholder="Descrição do serviço..." rows={3} />
            {errors.description && <span className="error-msg">{errors.description.message}</span>}
          </div>
          <div className="input-group" style={{ gridColumn: '1 / -1' }}>
            <label>Observações</label>
            <textarea {...register('notes')} placeholder="Observações adicionais..." rows={2} />
          </div>
        </div>
      </form>
    </BaseModal>
  );
}
