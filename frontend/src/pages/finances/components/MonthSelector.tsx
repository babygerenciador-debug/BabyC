import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Calendar, Plus, Lock, Unlock } from 'lucide-react';
import { useState } from 'react';
import OpenMonthModal from './OpenMonthModal';
import CloseMonthModal from './CloseMonthModal';

interface FinancialMonthDto {
  id: string;
  year: number;
  monthNumber: number;
  label: string;
  ownerSalary: number;
  status: string;
  openedAt: string;
  closedAt?: string;
}

interface Props {
  selectedMonthId: string | null;
  onSelectMonth: (id: string) => void;
}

export default function MonthSelector({ selectedMonthId, onSelectMonth }: Props) {
  const [showOpenModal, setShowOpenModal] = useState(false);
  const [showCloseModal, setShowCloseModal] = useState(false);

  const { data: months } = useQuery<FinancialMonthDto[]>({
    queryKey: ['financial-months'],
    queryFn: async () => {
      const res = await api.get('/finance/months');
      return res.data;
    },
  });

  const openMonth = months?.find(m => m.status === 'open');
  const selectedMonth = months?.find(m => m.id === selectedMonthId);

  return (
    <>
      <div className="month-selector" style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
        <Calendar size={20} style={{ color: 'var(--text-secondary)' }} />
        <select
          value={selectedMonthId ?? ''}
          onChange={e => onSelectMonth(e.target.value)}
          style={{ minWidth: '180px' }}
        >
          {months?.map(m => (
            <option key={m.id} value={m.id}>
              {m.label} {m.status === 'open' ? '(Aberto)' : '(Fechado)'}
            </option>
          ))}
        </select>

        {!openMonth && (
          <button className="btn-primary" onClick={() => setShowOpenModal(true)} style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
            <Plus size={16} />
            Abrir Mês
          </button>
        )}

        {openMonth && selectedMonthId === openMonth.id && (
          <button className="btn-secondary" onClick={() => setShowCloseModal(true)} style={{ display: 'flex', alignItems: 'center', gap: '0.35rem', color: 'var(--error)' }}>
            <Lock size={16} />
            Fechar Mês
          </button>
        )}
      </div>

      {showOpenModal && <OpenMonthModal onClose={() => setShowOpenModal(false)} />}
      {showCloseModal && selectedMonth && <CloseMonthModal month={selectedMonth} onClose={() => setShowCloseModal(false)} />}
    </>
  );
}
