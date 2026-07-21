import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Trash2 } from 'lucide-react';
import ProductFormModal from './ProductFormModal';

interface ProductDto {
  id: string;
  categoryId: string;
  categoryName: string;
  name: string;
  sku?: string;
  description?: string;
  averageUnitPrice: number;
}

export default function ProductsList() {
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery<{ items: ProductDto[], totalCount: number }>({
    queryKey: ['products', searchTerm],
    queryFn: async () => {
      const res = await api.get('/inventory/products', { params: { searchTerm, pageSize: 50 } });
      return res.data;
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/inventory/products/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    }
  });

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <Search size={18} style={{ color: 'var(--text-secondary)' }} />
          <input 
            type="text" 
            placeholder="Buscar peças, pneus, óleos..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ border: 'none', background: 'transparent', outline: 'none' }}
          />
        </div>
        <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Novo Produto</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando catálogo...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Nome / Peça</th>
                <th>SKU</th>
                <th>Categoria</th>
                <th>Custo Médio (R$)</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((product) => (
                <tr key={product.id}>
                  <td style={{ fontWeight: 500 }}>{product.name}</td>
                  <td>{product.sku || '-'}</td>
                  <td><span className="status-badge" style={{ backgroundColor: 'var(--brand-light)', color: 'var(--brand-color)' }}>{product.categoryName}</span></td>
                  <td>{new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(product.averageUnitPrice)}</td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon"><Edit2 size={18} /></button>
                      <button 
                        className="btn-icon" 
                        style={{ color: 'var(--error)' }}
                        onClick={() => {
                          if (window.confirm('Tem certeza que deseja remover este produto?')) {
                            deleteMutation.mutate(product.id);
                          }
                        }}
                      ><Trash2 size={18} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum produto encontrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <ProductFormModal onClose={() => setIsModalOpen(false)} />}
    </div>
  );
}
