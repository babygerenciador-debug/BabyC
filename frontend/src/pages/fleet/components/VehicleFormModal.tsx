import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const vehicleSchema = z.object({
  licensePlate: z.string().min(7, 'Placa deve ter no mínimo 7 caracteres (ex: ABC1234)'),
  nickname: z.string().min(2, 'Apelido é obrigatório'),
  driverId: z.string().optional(),
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
  vehicle?: { id: string };
  onClose: () => void;
}

const FORM_ID = 'vehicle-form';

export default function VehicleFormModal({ vehicle, onClose }: Props) {
  const queryClient = useQueryClient();
  const isEditing = !!vehicle;
  const [errorMsg, setErrorMsg] = useState('');

  const { data: vehicleData, isLoading: loadingVehicle } = useQuery({
    queryKey: ['vehicle', vehicle?.id],
    queryFn: async () => {
      const res = await api.get(`/vehicles/${vehicle!.id}`);
      return res.data;
    },
    enabled: isEditing,
  });

  const { data: drivers } = useQuery({
    queryKey: ['drivers-select'],
    queryFn: async () => {
      const res = await api.get('/drivers', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { register, handleSubmit, formState: { errors }, reset } = useForm<VehicleFormData>({
    resolver: zodResolver(vehicleSchema)
  });

  useEffect(() => {
    if (vehicleData) {
      reset({
        licensePlate: vehicleData.licensePlate || '',
        nickname: vehicleData.nickname || '',
        driverId: vehicleData.assignedDriverId || '',
        chassi: vehicleData.chassi || '',
        brand: vehicleData.brand || '',
        model: vehicleData.model || '',
        year: vehicleData.year || undefined,
        color: vehicleData.color || '',
        renavam: vehicleData.renavam || '',
        anttNumber: vehicleData.anttNumber || '',
        capacity: vehicleData.capacity || undefined,
        anttExpiry: vehicleData.anttExpiry ? vehicleData.anttExpiry.substring(0, 10) : '',
        artespExpiry: vehicleData.artespExpiry ? vehicleData.artespExpiry.substring(0, 10) : '',
        insuranceExpiry: vehicleData.insuranceExpiry ? vehicleData.insuranceExpiry.substring(0, 10) : '',
        licensingExpiry: vehicleData.licensingExpiry ? vehicleData.licensingExpiry.substring(0, 10) : '',
        fuelAlertMode: vehicleData.fuelAlertMode || '',
        fuelAlertDays: vehicleData.fuelAlertDays || undefined,
      });
    }
  }, [vehicleData, reset]);

  const mutation = useMutation({
    mutationFn: (data: VehicleFormData) => {
      const payload: Record<string, any> = {
        licensePlate: data.licensePlate,
        nickname: data.nickname,
        chassi: data.chassi,
        brand: data.brand,
        model: data.model,
        color: data.color,
        renavam: data.renavam,
        anttNumber: data.anttNumber,
        year: data.year ? Number(data.year) : undefined,
        capacity: data.capacity ? Number(data.capacity) : undefined,
        anttExpiry: safeToISO(data.anttExpiry),
        artespExpiry: safeToISO(data.artespExpiry),
        insuranceExpiry: safeToISO(data.insuranceExpiry),
        licensingExpiry: safeToISO(data.licensingExpiry),
        fuelAlertMode: data.fuelAlertMode,
        fuelAlertDays: data.fuelAlertDays ? Number(data.fuelAlertDays) : undefined,
      };
      if (data.driverId) {
        payload.driverId = data.driverId;
      } else {
        payload.driverCpf = '';
      }
      if (isEditing) {
        payload.id = vehicle!.id;
        return api.put(`/vehicles/${vehicle!.id}`, payload);
      }
      return api.post('/vehicles', payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles-dropdown'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles-select'] });
      queryClient.invalidateQueries({ queryKey: ['drivers'] });
      onClose();
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.description || err?.response?.data?.title || 'Erro ao salvar veículo.';
      setErrorMsg(msg);
    }
  });

  function safeToISO(dateStr: string | undefined | null): string | undefined {
    if (!dateStr) return undefined;
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? undefined : d.toISOString();
  }

  const onSubmit = (data: VehicleFormData) => {
    setErrorMsg('');
    mutation.mutate(data);
  };

  if (isEditing && loadingVehicle) {
    return (
      <BaseModal open onClose={onClose} title="Carregando Veículo" maxWidth="500px">
        <div style={{ textAlign: 'center', padding: '2rem' }}>
          <Loader2 className="spinner" size={24} />
          <p>Carregando dados do veículo...</p>
        </div>
      </BaseModal>
    );
  }

  return (
    <BaseModal
      open
      onClose={onClose}
      title={isEditing ? 'Editar Veículo' : 'Cadastrar Novo Veículo'}
      maxWidth="1100px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Salvando...' : isEditing ? 'Atualizar Veículo' : 'Salvar Veículo'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
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
          <h3>Motorista Responsável</h3>
          <div className="form-grid">
            <div className="input-group" style={{ gridColumn: '1 / -1' }}>
              <label>Vincular Motorista</label>
              <select {...register('driverId')}>
                <option value="">Nenhum (sem motorista)</option>
                {drivers?.map((d: any) => (
                  <option key={d.id} value={d.id}>{d.name} - CNH: {d.cnhNumber}</option>
                ))}
              </select>
              <small style={{ opacity: 0.6 }}>Selecione um motorista para vincular ao veículo, ou "Nenhum" para desvincular</small>
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

        {errorMsg && (
          <div className="input-group">
            <span className="error-msg" style={{ display: 'block', padding: '0.5rem', background: 'var(--error-bg, #ffeeee)', borderRadius: 'var(--radius-sm)' }}>{errorMsg}</span>
          </div>
        )}
      </form>
    </BaseModal>
  );
}