import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Search, AlertTriangle, ArrowRightLeft } from 'lucide-react';
import MovementFormModal from './MovementFormModal';

interface StockBalanceDto {
  id: string;
  productId: string;
  productName: string;
  locationType: 'Main' | 'Vehicle';
  vehicleId?: string;
  vehicleLicensePlate?: string;
  quantity: number;
  minimumStockLevel: number;
  isBelowMinimum: boolean;
}

export default function StockBalanceList() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedLocation, setSelectedLocation] = useState<'main' | string>('main');
  const [isMovementModalOpen, setIsMovementModalOpen] = useState(false);
  const [selectedProductId, setSelectedProductId] = useState<string | undefined>();

  // Busca os veículos para o dropdown
  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { pageSize: 100 } });
      return res.data.items;
    }
  });

  // Busca o estoque baseado no local selecionado
  const { data, isLoading } = useQuery<{ items: StockBalanceDto[] }>({
    queryKey: ['stock-balance', selectedLocation, searchTerm],
    queryFn: async () => {
      const url = selectedLocation === 'main' 
        ? '/inventory/stock/main' 
        : `/inventory/stock/vehicle/${selectedLocation}`;
      const res = await api.get(url, { params: { searchTerm } });
      return res.data;
    }
  });

  const handleOpenMovement = (productId?: string) => {
    setSelectedProductId(productId);
    setIsMovementModalOpen(true);
  };

  return (
    <div className="card animate-fade-in">
      <div className="toolbar" style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
        <div style={{ display: 'flex', gap: '1rem', flex: 1 }}>
          <div className="input-group" style={{ margin: 0, minWidth: '250px' }}>
            <select 
              value={selectedLocation} 
              onChange={(e) => setSelectedLocation(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: 'var(--radius-md)' }}
            >
              <option value="main">Estoque Geral</option>
              {vehicles?.map((v: any) => (
                <option key={v.id} value={v.id}>Bagageiro: {v.licensePlate} - {v.nickname}</option>
              ))}
            </select>
          </div>
          <div className="search-box" style={{ flex: 1 }}>
            <Search size={18} style={{ color: 'var(--text-secondary)' }} />
            <input 
              type="text" 
              placeholder="Buscar por nome da peça..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ border: 'none', background: 'transparent', outline: 'none', width: '100%' }}
            />
          </div>
        </div>
        <button className="btn-primary" onClick={() => handleOpenMovement()}>
          <ArrowRightLeft size={18} />
          <span>Nova Movimentação</span>
        </button>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando saldo do estoque...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Produto / Peça</th>
                <th>Local do Estoque</th>
                <th>Qtd. Atual</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((stock) => {
                const isAlert = stock.isBelowMinimum || stock.quantity <= 2;
                
                return (
                  <tr key={stock.id} className={isAlert ? 'row-alert' : ''}>
                    <td style={{ fontWeight: 500 }}>{stock.productName}</td>
                    <td>
                      {stock.locationType === 'Main' 
                        ? <span style={{ fontWeight: 600 }}>Estoque Geral</span>
                        : `Bagageiro: ${stock.vehicleLicensePlate || 'Desconhecido'}`
                      }
                    </td>
                    <td style={{ fontWeight: 600, fontSize: '1.1rem', color: isAlert ? 'var(--error)' : 'inherit' }}>
                      {stock.quantity}
                    </td>
                    <td>
                      {isAlert ? (
                        <span className="badge-alert">
                          <AlertTriangle size={14} /> Reposição Necessária (&lt;= 2)
                        </span>
                      ) : (
                        <span className="badge-ok">Normal</span>
                      )}
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                        <button 
                          className="btn-secondary" 
                          style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                          onClick={() => handleOpenMovement(stock.productId)}
                        >
                          Ajustar/Mover
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum saldo encontrado neste local.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isMovementModalOpen && (
        <MovementFormModal 
          onClose={() => setIsMovementModalOpen(false)} 
          initialProductId={selectedProductId} 
        />
      )}
    </div>
  );
}
