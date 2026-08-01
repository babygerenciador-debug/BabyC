import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

interface FinancialMonthDto {
  id: string;
  label: string;
  ownerSalary: number;
}

interface Props {
  month: FinancialMonthDto;
  onClose: () => void;
}

export default function CloseMonthModal({ month, onClose }: Props) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => api.post(`/finance/months/${month.id}/close`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-months'] });
      queryClient.invalidateQueries({ queryKey: ['financial-month-report'] });
      queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      onClose();
    },
  });

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Fechar Mês"
      maxWidth="400px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="button" className="btn-primary" style={{ backgroundColor: 'var(--error)' }} onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : null}
            <span>{mutation.isPending ? 'Fechando...' : `Fechar ${month.label}`}</span>
          </button>
        </div>
      }
    >
      <p>Tem certeza que deseja fechar o mês <strong>{month.label}</strong>?</p>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
        Após fechado, o mês ficará somente leitura. Transações deste mês não poderão ser alteradas.
      </p>
    </BaseModal>
  );
}
