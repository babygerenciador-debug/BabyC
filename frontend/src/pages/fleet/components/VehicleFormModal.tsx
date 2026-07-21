import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { X, Save, Loader2 } from 'lucide-react';
import './VehicleFormModal.css';

const vehicleSchema = z.object({
  licensePlate: z.string().min(7, 'Placa deve ter no mínimo 7 caracteres (ex: ABC1234)'),
  nickname: z.string().min(2, 'Apelido é obrigatório'),
  driverCpf: z.string().optional(),
  chassi: z.string().optional(),
  brand: z.string().optional(),
  model: z.string().optional(),
  year: z.number().min(1980).max(2100).optional(),
  color: z.string().optional(),
  renavam: z.string().optional(),
  anttNumber: z.string().optional(),
  capacity: z.number().min(1).optional(),
  anttExpiry: z.string().optional(),
  artespExpiry: z.string().optional(),
  insuranceExpiry: z.string().optional(),
  licensingExpiry: z.string().optional(),
  fuelAlertMode: z.string().optional(),
  fuelAlertDays: z.number().min(1).optional()
});

type VehicleFormData = z.infer<typeof vehicleSchema>;

interface Props {
  onClose: () => void;
}

export default function VehicleFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();
  const { register, handleSubmit, formState: { errors } } = useForm<VehicleFormData>({
    resolver: zodResolver(vehicleSchema)
  });

  const mutation = useMutation({
    mutationFn: (data: VehicleFormData) => api.post('/vehicles', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      onClose();
    }
  });

  function safeToISO(dateStr: string | undefined | null): string | undefined {
    if (!dateStr) return undefined;
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? undefined : d.toISOString();
  }

  const onSubmit = (data: VehicleFormData) => {
    const payload = {
      ...data,
      anttExpiry: safeToISO(data.anttExpiry),
      artespExpiry: safeToISO(data.artespExpiry),
      insuranceExpiry: safeToISO(data.insuranceExpiry),
      licensingExpiry: safeToISO(data.licensingExpiry),
      year: data.year ? Number(data.year) : undefined,
      capacity: data.capacity ? Number(data.capacity) : undefined,
      fuelAlertDays: data.fuelAlertDays ? Number(data.fuelAlertDays) : undefined,
    };
    mutation.mutate(payload);
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel animate-fade-in">
        <div className="modal-header">
          <h2>Cadastrar Novo Veículo</h2>
          <button className="btn-icon" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="modal-form">
          <div className="form-section">
            <h3>Dados Básicos</h3>
            <div className="form-grid">
              <div className="input-group">
                <label>Placa *</label>
                <input {...register('licensePlate')} placeholder="ABC-1234" />
                {errors.licensePlate && <span className="error-msg">{errors.licensePlate.message}</span>}
              </div>
              <div className="input-group">
                <label>Apelido *</label>
                <input {...register('nickname')} placeholder="Busão 01" />
                {errors.nickname && <span className="error-msg">{errors.nickname.message}</span>}
              </div>
              <div className="input-group">
                <label>Marca</label>
                <input {...register('brand')} placeholder="Marcopolo" />
              </div>
              <div className="input-group">
                <label>Modelo</label>
                <input {...register('model')} placeholder="Paradiso 1800 DD" />
              </div>
              <div className="input-group">
                <label>Ano de Fabricação</label>
                <input type="number" {...register('year', { valueAsNumber: true })} placeholder="2020" />
              </div>
              <div className="input-group">
                <label>Capacidade (Passageiros)</label>
                <input type="number" {...register('capacity', { valueAsNumber: true })} placeholder="44" />
              </div>
              <div className="input-group">
                <label>Chassi</label>
                <input {...register('chassi')} placeholder="9BW..." />
              </div>
              <div className="input-group">
                <label>Cor</label>
                <input {...register('color')} placeholder="Branco" />
              </div>
            </div>
          </div>

          <div className="form-section">
            <h3>Documentação e Prazos</h3>
            <div className="form-grid">
              <div className="input-group">
                <label>Renavam</label>
                <input {...register('renavam')} />
              </div>
              <div className="input-group">
                <label>Registro ANTT</label>
                <input {...register('anttNumber')} />
              </div>
              <div className="input-group">
                <label>Vencimento ANTT</label>
                <input type="date" {...register('anttExpiry')} />
              </div>
              <div className="input-group">
                <label>Vencimento Artesp</label>
                <input type="date" {...register('artespExpiry')} />
              </div>
              <div className="input-group">
                <label>Vencimento Licenciamento</label>
                <input type="date" {...register('licensingExpiry')} />
              </div>
              <div className="input-group">
                <label>Vencimento Seguro</label>
                <input type="date" {...register('insuranceExpiry')} />
              </div>
            </div>
          </div>

          <div className="form-section">
            <h3>Alertas Inteligentes</h3>
            <div className="form-grid">
              <div className="input-group">
                <label>Modo de Alerta de Abastecimento</label>
                <select {...register('fuelAlertMode')}>
                  <option value="">Nenhum</option>
                  <option value="ByDays">Por Dias Corridos</option>
                  <option value="ByMileage">Por Quilometragem</option>
                </select>
              </div>
              <div className="input-group">
                <label>Notificar Após Quantos Dias/KM?</label>
                <input type="number" {...register('fuelAlertDays', { valueAsNumber: true })} placeholder="Ex: 10" />
              </div>
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
              <span>{mutation.isPending ? 'Salvando...' : 'Salvar Veículo'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
