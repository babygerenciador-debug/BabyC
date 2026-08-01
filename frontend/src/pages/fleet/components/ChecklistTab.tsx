import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Pencil, Trash2, Loader2, Save, X, ClipboardCheck, CheckCircle, Clock, AlertCircle } from 'lucide-react';

interface ChecklistItem {
  id: string;
  title: string;
  isActive: boolean;
  sortOrder: number;
}

interface ReportRow {
  date: string;
  vehicleLicensePlate: string;
  driverName: string;
  status: string;
  totalItems: number;
  completedItems: number;
}

const statusLabel: Record<string, { label: string; icon: React.ReactNode; color: string }> = {
  Pending: { label: 'Pendente', icon: <Clock size={16} />, color: 'var(--text-secondary)' },
  Partial: { label: 'Parcial', icon: <AlertCircle size={16} />, color: 'var(--warning)' },
  Completed: { label: 'Concluído', icon: <CheckCircle size={16} />, color: 'var(--success)' },
};

export default function ChecklistTab() {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formTitle, setFormTitle] = useState('');
  const [showCreate, setShowCreate] = useState(false);

  const { data: items, isLoading } = useQuery<ChecklistItem[]>({
    queryKey: ['checklist-items'],
    queryFn: async () => {
      const res = await api.get('/checklist-admin/items');
      return res.data;
    }
  });

  const { data: report } = useQuery<ReportRow[]>({
    queryKey: ['checklist-report'],
    queryFn: async () => {
      const res = await api.get('/checklist-admin/report');
      return res.data;
    }
  });

  const createMutation = useMutation({
    mutationFn: (title: string) =>
      api.post('/checklist-admin/items', { title }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['checklist-items'] });
      setShowCreate(false);
      setFormTitle('');
    }
  });

  const updateMutation = useMutation({
    mutationFn: (data: ChecklistItem) =>
      api.put(`/checklist-admin/items/${data.id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['checklist-items'] });
      setEditingId(null);
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/checklist-admin/items/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['checklist-items'] });
    }
  });

  if (isLoading) return <p>Carregando...</p>;

  return (
    <>
      <div className="card">
        <div className="toolbar" style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
          <h3 style={{ margin: 0 }}>Itens do Checklist</h3>
          <button className="btn-primary" onClick={() => setShowCreate(true)}>
            <Plus size={18} /><span>Novo Item</span>
          </button>
        </div>

        {showCreate && (
          <div style={{ background: 'var(--bg-card)', padding: '1rem', borderRadius: 'var(--radius-md)', marginBottom: '1rem', border: '1px solid var(--border-color)' }}>
            <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', alignItems: 'center' }}>
              <input value={formTitle} onChange={e => setFormTitle(e.target.value)} placeholder="Ex: Verificar nível do óleo" style={{ flex: 1, padding: '0.5rem' }} />
              <button className="btn-primary" disabled={!formTitle || createMutation.isPending} onClick={() => createMutation.mutate(formTitle)}>
                {createMutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
                <span>Salvar</span>
              </button>
              <button className="btn-secondary" onClick={() => { setShowCreate(false); setFormTitle(''); }}>
                <X size={18} /><span>Cancelar</span>
              </button>
            </div>
          </div>
        )}

        <table className="data-table">
          <thead>
            <tr>
              <th>Item</th>
              <th>Ativo</th>
              <th style={{ textAlign: 'right' }}>Ações</th>
            </tr>
          </thead>
          <tbody>
            {items?.map(item => (
              <tr key={item.id}>
                {editingId === item.id ? (
                  <>
                    <td>
                      <input value={formTitle} onChange={e => setFormTitle(e.target.value)}
                        style={{ width: '100%', padding: '0.25rem' }} />
                    </td>
                    <td>
                      <input type="checkbox" checked={item.isActive}
                        onChange={e => updateMutation.mutate({ ...item, isActive: e.target.checked })} />
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <button className="btn-icon" onClick={() => {
                        updateMutation.mutate({ ...item, title: formTitle });
                      }}><Save size={16} /></button>
                      <button className="btn-icon" onClick={() => setEditingId(null)}><X size={16} /></button>
                    </td>
                  </>
                ) : (
                  <>
                    <td>{item.title}</td>
                    <td>
                      <input type="checkbox" checked={item.isActive}
                        onChange={e => updateMutation.mutate({ ...item, isActive: e.target.checked })} />
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <button className="btn-icon" onClick={() => {
                        setEditingId(item.id);
                        setFormTitle(item.title);
                      }}><Pencil size={16} /></button>
                      <button className="btn-icon" style={{ color: 'var(--error)' }}
                        onClick={() => deleteMutation.mutate(item.id)}><Trash2 size={16} /></button>
                    </td>
                  </>
                )}
              </tr>
            ))}
          </tbody>
        </table>

        {(!items || items.length === 0) && (
          <p style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
            Nenhum item de checklist cadastrado.
          </p>
        )}
      </div>

      <div className="card" style={{ marginTop: '1.5rem' }}>
        <div className="toolbar" style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
          <ClipboardCheck size={20} />
          <h3 style={{ margin: 0 }}>Checklists dos Motoristas</h3>
        </div>

        {(!report || report.length === 0) ? (
          <p style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
            Nenhum checklist realizado ainda.
          </p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Motorista</th>
                <th>Veículo</th>
                <th>Status</th>
                <th>Itens</th>
              </tr>
            </thead>
            <tbody>
              {report.map((row, i) => {
                const st = statusLabel[row.status] ?? statusLabel.Pending;
                return (
                  <tr key={i}>
                    <td>{row.date}</td>
                    <td><strong>{row.driverName}</strong></td>
                    <td>{row.vehicleLicensePlate}</td>
                    <td>
                      <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem', color: st.color }}>
                        {st.icon} {st.label}
                      </span>
                    </td>
                    <td>{row.completedItems}/{row.totalItems}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}
