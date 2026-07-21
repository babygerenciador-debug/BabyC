import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Trash2 } from 'lucide-react';

export default function FinanceSettings() {
  const queryClient = useQueryClient();
  const [newCatName, setNewCatName] = useState('');
  const [newCatType, setNewCatType] = useState<'Revenue' | 'Expense'>('Expense');
  
  const [newCcName, setNewCcName] = useState('');

  const { data: categories, isLoading: loadingCat } = useQuery({
    queryKey: ['finance-categories'],
    queryFn: async () => {
      const res = await api.get('/finance/categories');
      return res.data;
    }
  });

  const { data: costCenters, isLoading: loadingCc } = useQuery({
    queryKey: ['cost-centers'],
    queryFn: async () => {
      const res = await api.get('/finance/cost-centers');
      return res.data;
    }
  });

  const addCategoryMutation = useMutation({
    mutationFn: () => api.post('/finance/categories', { name: newCatName, type: newCatType }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance-categories'] });
      setNewCatName('');
    }
  });

  const addCcMutation = useMutation({
    mutationFn: () => api.post('/finance/cost-centers', { name: newCcName }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers'] });
      setNewCcName('');
    }
  });

  const deleteCatMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/finance/categories/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['finance-categories'] })
  });

  const deleteCcMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/finance/cost-centers/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cost-centers'] })
  });

  return (
    <div className="animate-fade-in" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
      
      {/* Categorias Financeiras */}
      <div className="card">
        <h3>Categorias Financeiras</h3>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem', fontSize: '0.9rem' }}>
          Ex: Manutenção, Salários, Impostos, Fretes
        </p>

        <div className="input-group" style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
          <input 
            type="text" 
            placeholder="Nova Categoria..." 
            value={newCatName}
            onChange={(e) => setNewCatName(e.target.value)}
            style={{ flex: 1 }}
          />
          <select 
            value={newCatType} 
            onChange={(e) => setNewCatType(e.target.value as any)}
            style={{ width: '120px' }}
          >
            <option value="Expense">Despesa</option>
            <option value="Revenue">Receita</option>
          </select>
          <button 
            className="btn-primary" 
            onClick={() => newCatName && addCategoryMutation.mutate()}
            disabled={addCategoryMutation.isPending}
          >
            <Plus size={18} /> Add
          </button>
        </div>

        <div className="data-table-container">
          <table className="data-table">
            <tbody>
              {loadingCat ? <tr><td>Carregando...</td></tr> : categories?.map((cat: any) => (
                <tr key={cat.id}>
                  <td style={{ fontWeight: 500 }}>{cat.name}</td>
                  <td style={{ width: '100px' }}>
                    <span className="badge-status" style={{ 
                      backgroundColor: cat.type === 'Revenue' ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
                      color: cat.type === 'Revenue' ? 'var(--success)' : 'var(--error)'
                    }}>
                      {cat.type === 'Revenue' ? 'Receita' : 'Despesa'}
                    </span>
                  </td>
                  <td style={{ width: '50px', textAlign: 'right' }}>
                    <button className="btn-icon" style={{ color: 'var(--error)' }} onClick={() => deleteCatMutation.mutate(cat.id)}>
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Centros de Custo */}
      <div className="card">
        <h3>Centros de Custo</h3>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem', fontSize: '0.9rem' }}>
          Ex: Administração, Frota Própria, Marketing
        </p>

        <div className="input-group" style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
          <input 
            type="text" 
            placeholder="Novo Centro de Custo..." 
            value={newCcName}
            onChange={(e) => setNewCcName(e.target.value)}
            style={{ flex: 1 }}
          />
          <button 
            className="btn-primary" 
            onClick={() => newCcName && addCcMutation.mutate()}
            disabled={addCcMutation.isPending}
          >
            <Plus size={18} /> Add
          </button>
        </div>

        <div className="data-table-container">
          <table className="data-table">
            <tbody>
              {loadingCc ? <tr><td>Carregando...</td></tr> : costCenters?.map((cc: any) => (
                <tr key={cc.id}>
                  <td style={{ fontWeight: 500 }}>{cc.name}</td>
                  <td style={{ width: '50px', textAlign: 'right' }}>
                    <button className="btn-icon" style={{ color: 'var(--error)' }} onClick={() => deleteCcMutation.mutate(cc.id)}>
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

    </div>
  );
}
