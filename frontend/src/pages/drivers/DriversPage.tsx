import DriverList from './components/DriverList';
import './DriversPage.css';

export default function DriversPage() {
  return (
    <div className="page-container animate-fade-in">
      <div className="page-header">
        <h1>Motoristas</h1>
        <p>Gerencie os motoristas cadastrados e suas CNHs.</p>
      </div>
      <DriverList />
    </div>
  );
}
