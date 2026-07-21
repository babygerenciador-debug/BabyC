import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { format } from 'date-fns';

interface MovementDto {
  id: string;
  productName: string;
  type: 'In' | 'Out' | 'Transfer';
  fromLocationType?: 'Central' | 'Vehicle';
  fromVehicleId?: string;
  toLocationType?: 'Central' | 'Vehicle';
  toVehicleId?: string;
  quantity: number;
  date: string;
  notes?: string;
}

export default function MovementsList() {
  const [selectedProductId, setSelectedProductId] = useState<string>('');

  // Fetch Products for Dropdown
  const { data: products } = useQuery({
    queryKey: ['products-dropdown'],
    queryFn: async () => {
      const res = await api.get('/inventory/products', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  const { data, isLoading } = useQuery<{ items: MovementDto[] }>({
    queryKey: ['movements', selectedProductId],
    queryFn: async () => {
      const res = await api.get(`/inventory/movements/product/${selectedProductId}`);
      return res.data;
    },
    enabled: !!selectedProductId // Só busca se tiver produto selecionado
  });

  const getMovementLabel = (type: string) => {
    if (type === 'In') return <span style={{ color: 'var(--success)', fontWeight: 600 }}>Entrada</span>;
    if (type === 'Out') return <span style={{ color: 'var(--error)', fontWeight: 600 }}>Saída</span>;
    if (type === 'Transfer') return <span style={{ color: 'var(--info)', fontWeight: 600 }}>Transferência</span>;
    return type;
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="input-group" style={{ margin: 0, minWidth: '300px' }}>
          <select 
            value={selectedProductId} 
            onChange={(e) => setSelectedProductId(e.target.value)}
            style={{ padding: '0.5rem', borderRadius: 'var(--border-radius-md)' }}
          >
            <option value="">-- Selecione uma Peça/Produto --</option>
            {products?.map((p: any) => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="data-table-container">
        {!selectedProductId ? (
          <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            Selecione um produto acima para carregar o histórico de movimentações.
          </div>
        ) : isLoading ? (
          <p>Carregando histórico...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Tipo</th>
                <th>Peça</th>
                <th>Qtd.</th>
                <th>Origem</th>
                <th>Destino</th>
                <th>Obs</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((mov) => (
                <tr key={mov.id}>
                  <td>{format(new Date(mov.date), 'dd/MM/yyyy HH:mm')}</td>
                  <td>{getMovementLabel(mov.type)}</td>
                  <td style={{ fontWeight: 500 }}>{mov.productName}</td>
                  <td style={{ fontWeight: 600 }}>{mov.quantity}</td>
                  <td>{mov.fromLocationType === 'Central' ? 'Almoxarifado' : mov.fromVehicleId?.substring(0,8) || '-'}</td>
                  <td>{mov.toLocationType === 'Central' ? 'Almoxarifado' : mov.toVehicleId?.substring(0,8) || '-'}</td>
                  <td style={{ maxWidth: '200px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {mov.notes || '-'}
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhuma movimentação registrada para este produto.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
