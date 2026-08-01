import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const txSchema = z.object({
  type: z.enum(['Revenue', 'Expense']),
  amount: z.number().min(0.01, 'Valor inválido'),
  date: z.string().min(1, 'Data é obrigatória'),
  description: z.string().min(2, 'Descrição é obrigatória'),
  categoryId: z.string().min(1, 'Selecione uma categoria'),
  costCenterId: z.string().optional(),
  status: z.enum(['Pending', 'Paid']),
});

type TxFormData = z.infer<typeof txSchema>;

interface Props {
  onClose: () => void;
}

const FORM_ID = 'tx-form';

export default function TransactionFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();

  const { data: categories } = useQuery({
    queryKey: ['finance-categories'],
    queryFn: async () => {
      const res = await api.get('/finance/categories');
      return res.data;
    }
  });

  const { data: costCenters } = useQuery({
    queryKey: ['cost-centers'],
    queryFn: async () => {
      const res = await api.get('/finance/cost-centers');
      return res.data;
    }
  });

  const { data: openMonth } = useQuery({
    queryKey: ['open-financial-month'],
    queryFn: async () => {
      const res = await api.get('/finance/months/open');
      return res.data;
    },
  });

  const { register, handleSubmit, watch, formState: { errors } } = useForm<TxFormData>({
    resolver: zodResolver(txSchema),
    defaultValues: {
      type: 'Expense',
      status: 'Pending',
      date: new Date().toISOString().substring(0, 10)
    }
  });

  const selectedType = watch('type');
  
  const filteredCategories = categories?.filter((c: any) => c.type === selectedType);

  const mutation = useMutation({
    mutationFn: (data: TxFormData) => {
      const payload: Record<string, unknown> = {
        categoryId: data.categoryId,
        costCenterId: data.costCenterId || null,
        financialMonthId: openMonth?.id,
        type: data.type,
        amount: data.amount,
        date: new Date(data.date).toISOString(),
        description: data.description,
        status: data.status,
      };
      if (data.status === 'Paid') {
        payload.paymentDate = new Date(data.date).toISOString();
      }
      return api.post('/finance/transactions', payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
      queryClient.invalidateQueries({ queryKey: ['financial-month-report'] });
      onClose();
    }
  });

  const onSubmit = (data: TxFormData) => {
    if (!openMonth?.id) {
      alert('Não há mês aberto. Abra um mês financeiro antes de lançar transações.');
      return;
    }
    mutation.mutate(data);
  };

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Nova Transação"
      maxWidth="500px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={mutation.isPending || !openMonth?.id}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Salvando...' : 'Lançar Transação'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
        <div className="form-section">
          <div className="form-grid" style={{ gridTemplateColumns: '1fr' }}>
            
            <div className="input-group">
              <label>Tipo *</label>
              <select {...register('type')}>
                <option value="Expense">Despesa (Saída)</option>
                <option value="Revenue">Receita (Entrada)</option>
              </select>
            </div>

            <div className="input-group">
              <label>Descrição *</label>
              <input {...register('description')} placeholder="Ex: Conta de Luz Oficina" />
              {errors.description && <span className="error-msg">{errors.description.message}</span>}
            </div>

            <div className="form-grid">
              <div className="input-group">
                <label>Valor (R$) *</label>
                <input type="number" step="0.01" {...register('amount', { valueAsNumber: true })} />
                {errors.amount && <span className="error-msg">{errors.amount.message}</span>}
              </div>
              <div className="input-group">
                <label>Data Vencimento/Ocorrência *</label>
                <input type="date" {...register('date')} />
                {errors.date && <span className="error-msg">{errors.date.message}</span>}
              </div>
            </div>

            <div className="input-group">
              <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                Categoria *
                <button type="button" onClick={() => {
                  const name = prompt('Nome da nova categoria:');
                  if (name?.trim()) {
                    api.post('/finance/categories', { name, type: selectedType }).then(() => {
                      queryClient.invalidateQueries({ queryKey: ['finance-categories'] });
                    });
                  }
                }} style={{ color: 'var(--brand-color)', fontSize: '0.8rem', display: 'flex', alignItems: 'center' }}>
                  + Nova
                </button>
              </label>
              <select {...register('categoryId')}>
                <option value="">Selecione...</option>
                {filteredCategories?.map((c: any) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
              {errors.categoryId && <span className="error-msg">{errors.categoryId.message}</span>}
            </div>

            <div className="input-group">
              <label>Centro de Custo (Opcional)</label>
              <select {...register('costCenterId')}>
                <option value="">Geral / Administrativo</option>
                {costCenters?.map((cc: any) => (
                  <option key={cc.id} value={cc.id}>{cc.name}</option>
                ))}
              </select>
            </div>

            <div className="input-group">
              <label>Status Atual *</label>
              <select {...register('status')}>
                <option value="Pending">Pendente (A Pagar / A Receber)</option>
                <option value="Paid">Já Pago / Já Recebido</option>
              </select>
            </div>

          </div>
        </div>
      </form>
    </BaseModal>
  );
}
