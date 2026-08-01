import { useState } from 'react';
import { Bus, Droplets, ClipboardCheck } from 'lucide-react';
import './FleetPage.css';
import VehicleList from './components/VehicleList';
import FuelLogList from './components/FuelLogList';
import ChecklistTab from './components/ChecklistTab';

type Tab = 'vehicles' | 'fuellogs' | 'checklist';

export default function FleetPage() {
  const [activeTab, setActiveTab] = useState<Tab>('vehicles');

  return (
    <div className="fleet-container animate-fade-in">
      <div className="fleet-header">
        <div>
          <h1>Frota</h1>
          <p>Gerencie seus veículos, documentação e diário de abastecimento.</p>
        </div>
      </div>

      <div className="tabs">
        <button 
          className={`tab-btn ${activeTab === 'vehicles' ? 'active' : ''}`}
          onClick={() => setActiveTab('vehicles')}
        >
          <Bus size={18} />
          <span>Veículos</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'fuellogs' ? 'active' : ''}`}
          onClick={() => setActiveTab('fuellogs')}
        >
          <Droplets size={18} />
          <span>Abastecimentos</span>
        </button>
        <button 
          className={`tab-btn ${activeTab === 'checklist' ? 'active' : ''}`}
          onClick={() => setActiveTab('checklist')}
        >
          <ClipboardCheck size={18} />
          <span>Checklist</span>
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'vehicles' && <VehicleList />}
        {activeTab === 'fuellogs' && <FuelLogList />}
        {activeTab === 'checklist' && <ChecklistTab />}
      </div>
    </div>
  );
}
