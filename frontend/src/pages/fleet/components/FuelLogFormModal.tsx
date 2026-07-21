import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { X, Save, Loader2, UploadCloud } from 'lucide-react';
import './FuelLogFormModal.css';

const fuelLogSchema = z.object({
  vehicleId: z.string().min(1, 'Selecione um veículo'),
  date: z.string().min(1, 'Data é obrigatória'),
  odometer: z.number().min(0, 'Hodômetro inválido'),
  liters: z.number().min(0.1, 'Litragem inválida'),
  totalCost: z.number().min(0.1, 'Custo total inválido'),
  notes: z.string().optional()
});

type FuelLogFormData = z.infer<typeof fuelLogSchema>;

interface Props {
  onClose: () => void;
}

export default function FuelLogFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();
  const [file, setFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState('');

  // Busca lista de veículos para o dropdown
  const { data: vehiclesData } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { register, handleSubmit, formState: { errors } } = useForm<FuelLogFormData>({
    resolver: zodResolver(fuelLogSchema)
  });

  const mutation = useMutation({
    mutationFn: (data: any) => api.post('/fuellogs', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fuellogs'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      onClose();
    }
  });

  const onSubmit = async (data: FuelLogFormData) => {
    let receiptUrl = null;

    if (file) {
      try {
        setIsUploading(true);
        const formData = new FormData();
        formData.append('file', file);
        
        const uploadRes = await api.post('/fuellogs/upload-receipt', formData);
        
        receiptUrl = uploadRes.data.url;
      } catch (err) {
        setUploadError('Erro ao fazer upload da imagem.');
        setIsUploading(false);
        return;
      }
    }

    const payload = {
      vehicleId: data.vehicleId,
      date: new Date(data.date).toISOString(),
      odometer: Number(data.odometer),
      liters: Number(data.liters),
      totalCost: Number(data.totalCost),
      notes: data.notes,
      receiptUrl
    };

    mutation.mutate(payload);
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel animate-fade-in" style={{ maxWidth: '600px' }}>
        <div className="modal-header">
          <h2>Registrar Abastecimento</h2>
          <button className="btn-icon" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="modal-form">
          <div className="form-section">
            <div className="form-grid" style={{ gridTemplateColumns: '1fr' }}>
              
              <div className="input-group">
                <label>Veículo *</label>
                <select {...register('vehicleId')}>
                  <option value="">Selecione...</option>
                  {vehiclesData?.map((v: any) => (
                    <option key={v.id} value={v.id}>{v.licensePlate} - {v.nickname}</option>
                  ))}
                </select>
                {errors.vehicleId && <span className="error-msg">{errors.vehicleId.message}</span>}
              </div>

              <div className="form-grid">
                <div className="input-group">
                  <label>Data *</label>
                  <input type="datetime-local" {...register('date')} />
                  {errors.date && <span className="error-msg">{errors.date.message}</span>}
                </div>
                
                <div className="input-group">
                  <label>Hodômetro Atual (KM) *</label>
                  <input type="number" {...register('odometer', { valueAsNumber: true })} />
                  {errors.odometer && <span className="error-msg">{errors.odometer.message}</span>}
                </div>

                <div className="input-group">
                  <label>Litros Abastecidos *</label>
                  <input type="number" step="0.01" {...register('liters', { valueAsNumber: true })} />
                  {errors.liters && <span className="error-msg">{errors.liters.message}</span>}
                </div>

                <div className="input-group">
                  <label>Custo Total (R$) *</label>
                  <input type="number" step="0.01" {...register('totalCost', { valueAsNumber: true })} />
                  {errors.totalCost && <span className="error-msg">{errors.totalCost.message}</span>}
                </div>
              </div>

              <div className="input-group">
                <label>Comprovante (Nota Fiscal)</label>
                <div className="file-upload-area">
                  <input 
                    type="file" 
                    id="receipt" 
                    accept="image/*,.pdf" 
                    onChange={(e) => setFile(e.target.files?.[0] || null)}
                    className="file-input"
                  />
                  <label htmlFor="receipt" className="file-label">
                    <UploadCloud size={24} />
                    <span>{file ? file.name : 'Clique para selecionar a foto/PDF'}</span>
                  </label>
                </div>
                {uploadError && <span className="error-msg">{uploadError}</span>}
              </div>

              <div className="input-group">
                <label>Observações</label>
                <textarea {...register('notes')} placeholder="Ex: Abastecido em rota para São Paulo..." rows={3}></textarea>
              </div>

            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending || isUploading}>
              {(mutation.isPending || isUploading) ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
              <span>{(mutation.isPending || isUploading) ? 'Salvando...' : 'Salvar Abastecimento'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
