import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2, Plus, X } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const productSchema = z.object({
  name: z.string().min(2, 'O nome é obrigatório'),
  categoryId: z.string().min(1, 'Selecione uma categoria'),
  sku: z.string().optional(),
  description: z.string().optional(),
  averageUnitPrice: z.number().min(0, 'Preço não pode ser negativo').optional(),
  initialQuantity: z.number().int().min(0, 'Quantidade não pode ser negativa').optional(),
});

type ProductFormData = z.infer<typeof productSchema>;

interface Props {
  onClose: () => void;
}

const FORM_ID = 'product-form';

export default function ProductFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();
  const [isCreatingCategory, setIsCreatingCategory] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const res = await api.get('/inventory/categories');
      return res.data;
    }
  });

  const { register, handleSubmit, formState: { errors } } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema)
  });

  const productMutation = useMutation({
    mutationFn: (data: ProductFormData) => api.post('/inventory/products', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['stock-balance'] });
      onClose();
    }
  });

  const categoryMutation = useMutation({
    mutationFn: (name: string) => api.post('/inventory/categories', { name }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setIsCreatingCategory(false);
      setNewCategoryName('');
    }
  });

  const handleCreateCategory = (e: React.FormEvent) => {
    e.preventDefault();
    if (newCategoryName.trim()) {
      categoryMutation.mutate(newCategoryName);
    }
  };

  const onSubmit = (data: ProductFormData) => {
    productMutation.mutate(data);
  };

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Novo Produto / Peça"
      maxWidth="500px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={productMutation.isPending}>
            {productMutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{productMutation.isPending ? 'Salvando...' : 'Salvar Produto'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
        <div className="form-section">
          <div className="form-grid" style={{ gridTemplateColumns: '1fr' }}>
            
            <div className="input-group">
              <label>Nome da Peça *</label>
              <input {...register('name')} placeholder="Pastilha de Freio Dianteira" />
              {errors.name && <span className="error-msg">{errors.name.message}</span>}
            </div>

            <div className="input-group">
              <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                Categoria *
                {!isCreatingCategory && (
                  <button type="button" onClick={() => setIsCreatingCategory(true)} style={{ color: 'var(--brand-color)', fontSize: '0.8rem', display: 'flex', alignItems: 'center' }}>
                    <Plus size={14} /> Nova
                  </button>
                )}
              </label>

              {isCreatingCategory ? (
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <input 
                    value={newCategoryName}
                    onChange={(e) => setNewCategoryName(e.target.value)}
                    placeholder="Ex: Pneus" 
                  />
                  <button type="button" className="btn-primary" onClick={handleCreateCategory} disabled={categoryMutation.isPending}>
                    {categoryMutation.isPending ? '...' : 'Add'}
                  </button>
                  <button type="button" className="btn-secondary" onClick={() => setIsCreatingCategory(false)}>
                    <X size={16} />
                  </button>
                </div>
              ) : (
                <>
                  <select {...register('categoryId')}>
                    <option value="">Selecione...</option>
                    {categories?.map((cat: any) => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </select>
                  {errors.categoryId && <span className="error-msg">{errors.categoryId.message}</span>}
                </>
              )}
            </div>

            <div className="input-group">
              <label>SKU / Código (Opcional)</label>
              <input {...register('sku')} placeholder="Cód. Fornecedor" />
            </div>

            <div className="input-group">
              <label>Descrição (Opcional)</label>
              <textarea {...register('description')} rows={2}></textarea>
            </div>

            <div className="form-grid">
              <div className="input-group">
                <label>Preço Unitário (R$)</label>
                <input type="number" step="0.01" {...register('averageUnitPrice', { valueAsNumber: true })} placeholder="0,00" />
              </div>
              <div className="input-group">
                <label>Quantidade Inicial</label>
                <input type="number" {...register('initialQuantity', { valueAsNumber: true })} placeholder="0" />
              </div>
            </div>

          </div>
        </div>
      </form>
    </BaseModal>
  );
}
