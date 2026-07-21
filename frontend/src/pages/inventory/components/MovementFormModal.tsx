import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { X, Save, Loader2 } from 'lucide-react';

const movementSchema = z.object({
  type: z.enum(['Receive', 'Consume', 'Transfer']),
  productId: z.string().min(1, 'Selecione o produto'),
  quantity: z.number().min(1, 'A quantidade deve ser maior que zero'),
  date: z.string().min(1, 'Data é obrigatória'),
  
  // Para Entrada/Saída
  locationType: z.string().optional(),
  vehicleId: z.string().optional(),
  
  // Apenas para Entrada
  unitPrice: z.number().min(0).optional(),
  
  // Para Transferência
  fromVehicleId: z.string().optional(),
  toVehicleId: z.string().optional(),

  notes: z.string().optional()
});

type MovementFormData = z.infer<typeof movementSchema>;

interface Props {
  onClose: () => void;
  initialProductId?: string;
}

export default function MovementFormModal({ onClose, initialProductId }: Props) {
  const queryClient = useQueryClient();
  const [movementType, setMovementType] = useState<'Receive' | 'Consume' | 'Transfer'>('Receive');
  const [locationType, setLocationType] = useState<'Central' | 'Vehicle'>('Central');

  // Load Products
  const { data: products } = useQuery({
    queryKey: ['products-dropdown'],
    queryFn: async () => {
      const res = await api.get('/inventory/products', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  // Load Vehicles
  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { register, handleSubmit, watch, formState: { errors } } = useForm<MovementFormData>({
    resolver: zodResolver(movementSchema),
    defaultValues: {
      productId: initialProductId || '',
      type: 'Receive',
      date: new Date().toISOString().substring(0, 16),
      locationType: 'Central'
    }
  });

  // Listen to type changes
  const selectedType = watch('type');
  const selectedLoc = watch('locationType');

  function safeToISO(dateStr: string | undefined | null): string | null {
    if (!dateStr) return null;
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? null : d.toISOString();
  }

  const mutation = useMutation({
    mutationFn: async (data: MovementFormData) => {
      if (data.type === 'Receive') {
        return api.post('/inventory/movements/receive', {
          productId: data.productId,
          locationType: data.locationType,
          vehicleId: data.locationType === 'Vehicle' ? data.vehicleId : null,
          quantity: data.quantity,
          unitPrice: data.unitPrice || 0,
          date: safeToISO(data.date) || new Date().toISOString(),
          notes: data.notes
        });
      } else if (data.type === 'Consume') {
        return api.post('/inventory/movements/consume', {
          productId: data.productId,
          locationType: data.locationType,
          vehicleId: data.locationType === 'Vehicle' ? data.vehicleId : null,
          quantity: data.quantity,
          date: safeToISO(data.date) || new Date().toISOString(),
          notes: data.notes
        });
      } else {
        return api.post('/inventory/movements/transfer', {
          productId: data.productId,
          fromVehicleId: data.fromVehicleId || null,
          toVehicleId: data.toVehicleId || null,
          quantity: data.quantity,
          date: safeToISO(data.date) || new Date().toISOString(),
          notes: data.notes
        });
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-balance'] });
      queryClient.invalidateQueries({ queryKey: ['movements'] });
      onClose();
    }
  });

  const onSubmit = (data: MovementFormData) => {
    mutation.mutate(data);
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel animate-fade-in" style={{ maxWidth: '600px' }}>
        <div className="modal-header">
          <h2>Registrar Movimentação de Estoque</h2>
          <button className="btn-icon" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="modal-form">
          <div className="form-section">
            <div className="form-grid" style={{ gridTemplateColumns: '1fr' }}>
              
              <div className="input-group">
                <label>Tipo de Movimentação *</label>
                <div style={{ display: 'flex', gap: '1rem' }}>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input type="radio" value="Receive" {...register('type')} />
                    Entrada (Nova Peça)
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input type="radio" value="Consume" {...register('type')} />
                    Saída (Consumo/Uso)
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input type="radio" value="Transfer" {...register('type')} />
                    Transferência
                  </label>
                </div>
              </div>

              <div className="input-group">
                <label>Produto / Peça *</label>
                <select {...register('productId')}>
                  <option value="">Selecione...</option>
                  {products?.map((p: any) => (
                    <option key={p.id} value={p.id}>{p.name} - SKU: {p.sku || 'N/A'}</option>
                  ))}
                </select>
                {errors.productId && <span className="error-msg">{errors.productId.message}</span>}
              </div>

              {selectedType !== 'Transfer' && (
                <div className="input-group">
                  <label>Localização do Estoque *</label>
                  <select {...register('locationType')}>
                    <option value="Central">Almoxarifado Central</option>
                    <option value="Vehicle">Bagageiro de Ônibus</option>
                  </select>
                </div>
              )}

              {selectedType !== 'Transfer' && selectedLoc === 'Vehicle' && (
                <div className="input-group">
                  <label>Qual Ônibus? *</label>
                  <select {...register('vehicleId')}>
                    <option value="">Selecione...</option>
                    {vehicles?.map((v: any) => (
                      <option key={v.id} value={v.id}>{v.licensePlate} - {v.nickname}</option>
                    ))}
                  </select>
                </div>
              )}

              {selectedType === 'Transfer' && (
                <div className="form-grid">
                  <div className="input-group">
                    <label>Origem (Vazio = Almoxarifado)</label>
                    <select {...register('fromVehicleId')}>
                      <option value="">Almoxarifado Central</option>
                      {vehicles?.map((v: any) => (
                        <option key={v.id} value={v.id}>{v.licensePlate}</option>
                      ))}
                    </select>
                  </div>
                  <div className="input-group">
                    <label>Destino (Vazio = Almoxarifado)</label>
                    <select {...register('toVehicleId')}>
                      <option value="">Almoxarifado Central</option>
                      {vehicles?.map((v: any) => (
                        <option key={v.id} value={v.id}>{v.licensePlate}</option>
                      ))}
                    </select>
                  </div>
                </div>
              )}

              <div className="form-grid">
                <div className="input-group">
                  <label>Quantidade *</label>
                  <input type="number" {...register('quantity', { valueAsNumber: true })} />
                  {errors.quantity && <span className="error-msg">{errors.quantity.message}</span>}
                </div>

                {selectedType === 'Receive' && (
                  <div className="input-group">
                    <label>Preço Unitário de Custo (R$)</label>
                    <input type="number" step="0.01" {...register('unitPrice', { valueAsNumber: true })} />
                  </div>
                )}
                
                <div className="input-group">
                  <label>Data *</label>
                  <input type="datetime-local" {...register('date')} />
                  {errors.date && <span className="error-msg">{errors.date.message}</span>}
                </div>
              </div>

              <div className="input-group">
                <label>Observações (Motivo, Ordem de Serviço...)</label>
                <textarea {...register('notes')} rows={2}></textarea>
              </div>

            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
              <span>{mutation.isPending ? 'Processando...' : 'Confirmar'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
