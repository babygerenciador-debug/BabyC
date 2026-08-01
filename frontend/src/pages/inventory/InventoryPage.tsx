import { useState } from 'react';
import { Package, Tags, ArrowLeftRight } from 'lucide-react';
import './InventoryPage.css';
import StockBalanceList from './components/StockBalanceList';
import ProductsList from './components/ProductsList';
import MovementsList from './components/MovementsList';

type Tab = 'stock' | 'products' | 'movements';

export default function InventoryPage() {
  const [activeTab, setActiveTab] = useState<Tab>('stock');

  return (
    <div className="inventory-container animate-fade-in">
      <div className="inventory-header">
        <div>
          <h1>Estoque Inteligente</h1>
          <p>Controle peças no Almoxarifado Central e nos Bagageiros dos Ônibus.</p>
        </div>
      </div>

      <div className="tabs">
        <button 
          className={`tab-btn ${activeTab === 'stock' ? 'active' : ''}`}
          onClick={() => setActiveTab('stock')}
        >
          <Package size={18} />
          <span>Saldo de Estoque</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'products' ? 'active' : ''}`}
          onClick={() => setActiveTab('products')}
        >
          <Tags size={18} />
          <span>Peças e Categorias</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'movements' ? 'active' : ''}`}
          onClick={() => setActiveTab('movements')}
        >
          <ArrowLeftRight size={18} />
          <span>Movimentações</span>
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'stock' && <StockBalanceList />}
        {activeTab === 'products' && <ProductsList />}
        {activeTab === 'movements' && <MovementsList />}
      </div>
    </div>
  );
}
