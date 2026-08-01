import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

interface MovementFormData {
  type: 'Receive' | 'Consume' | 'Transfer';
  productId: string;
  quantity: number;
  date: string;
  locationType?: string;
  vehicleId?: string;
  unitPrice?: number;
  fromVehicleId?: string;
  toVehicleId?: string;
  notes?: string;
}

interface Props {
  onClose: () => void;
  initialProductId?: string;
}

export default function MovementFormModal({ onClose, initialProductId }: Props) {
  const queryClient = useQueryClient();

  const { data: products } = useQuery({
    queryKey: ['products-dropdown'],
    queryFn: async () => {
      const res = await api.get('/inventory/products', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { register, watch, getValues, setError, formState: { errors } } = useForm<MovementFormData>({
    defaultValues: {
      productId: initialProductId || '',
      type: 'Receive',
      date: new Date().toISOString().substring(0, 16),
      locationType: 'Main'
    }
  });

  const selectedType = watch('type');
  const selectedLoc = watch('locationType');

  function safeToISO(dateStr: string | undefined | null): string | null {
    if (!dateStr) return null;
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? null : d.toISOString();
  }

  const [errorMsg, setErrorMsg] = useState('');

  function validate(): boolean {
    const v = getValues();
    if (!v.productId) { setError('productId', { message: 'Selecione o produto' }); return false; }
    if (!v.quantity || v.quantity < 1) { setError('quantity', { message: 'A quantidade deve ser maior que zero' }); return false; }
    if (!v.date) { setError('date', { message: 'Data é obrigatória' }); return false; }
    return true;
  }

  const mutation = useMutation({
    mutationFn: async (data: MovementFormData) => {
      setErrorMsg('');
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
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.description || err?.response?.data?.title || 'Erro ao processar movimentação.';
      setErrorMsg(msg);
    }
  });

  const handleConfirm = () => {
    if (!validate()) return;
    mutation.mutate(getValues() as MovementFormData);
  };

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Registrar Movimentação de Estoque"
      maxWidth="600px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="button" className="btn-primary" disabled={mutation.isPending} onClick={handleConfirm}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Processando...' : 'Confirmar'}</span>
          </button>
        </div>
      }
    >
      <form className="modal-form">
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
                  <option value="Main">Estoque Geral</option>
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
                    <option value="">Estoque Geral</option>
                    {vehicles?.map((v: any) => (
                      <option key={v.id} value={v.id}>{v.licensePlate}</option>
                    ))}
                  </select>
                </div>
                <div className="input-group">
                  <label>Destino (Vazio = Estoque Geral)</label>
                  <select {...register('toVehicleId')}>
                    <option value="">Estoque Geral</option>
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

        {errorMsg && (
          <div className="input-group">
            <span className="error-msg" style={{ display: 'block', padding: '0.5rem', background: 'var(--error-bg, #ffeeee)', borderRadius: 'var(--radius-sm)' }}>{errorMsg}</span>
          </div>
        )}
      </form>
    </BaseModal>
  );
}
