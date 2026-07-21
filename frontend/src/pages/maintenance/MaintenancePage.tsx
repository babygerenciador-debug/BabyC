import MaintenanceList from './components/MaintenanceList';
import './MaintenancePage.css';

export default function MaintenancePage() {
  return (
    <div className="page-container animate-fade-in">
      <div className="page-header">
        <h1>Manutenções</h1>
        <p>Gerencie as manutenções preventivas e corretivas da frota.</p>
      </div>
      <MaintenanceList />
    </div>
  );
}
