import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const schema = z.object({
  year: z.number().int().min(2020).max(2100),
  month: z.number().int().min(1).max(12),
  ownerSalary: z.number().min(0, 'Salário não pode ser negativo'),
});

type FormData = z.infer<typeof schema>;

interface Props {
  onClose: () => void;
}

const FORM_ID = 'open-month-form';

export default function OpenMonthModal({ onClose }: Props) {
  const queryClient = useQueryClient();

  const now = new Date();
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      year: now.getFullYear(),
      month: now.getMonth() + 1,
      ownerSalary: 0,
    },
  });

  const mutation = useMutation({
    mutationFn: (data: FormData) => api.post('/finance/months/open', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-months'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      onClose();
    },
  });

  const onSubmit = (data: FormData) => mutation.mutate(data);

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Criar Novo Mês Financeiro"
      maxWidth="450px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Criando...' : 'Criar Mês'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
        <div className="form-section">
          <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr' }}>
            <div className="input-group">
              <label>Ano *</label>
              <input type="number" {...register('year', { valueAsNumber: true })} />
              {errors.year && <span className="error-msg">{errors.year.message}</span>}
            </div>
            <div className="input-group">
              <label>Mês *</label>
              <select {...register('month', { valueAsNumber: true })}>
                {[1,2,3,4,5,6,7,8,9,10,11,12].map(m => (
                  <option key={m} value={m}>{m.toString().padStart(2, '0')}</option>
                ))}
              </select>
              {errors.month && <span className="error-msg">{errors.month.message}</span>}
            </div>
          </div>
          <div className="input-group">
            <label>Salário do Dono (R$) *</label>
            <input type="number" step="0.01" {...register('ownerSalary', { valueAsNumber: true })} />
            {errors.ownerSalary && <span className="error-msg">{errors.ownerSalary.message}</span>}
          </div>
        </div>
      </form>
    </BaseModal>
  );
}
