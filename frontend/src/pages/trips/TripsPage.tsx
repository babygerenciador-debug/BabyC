import TripList from './components/TripList';
import './TripsPage.css';

export default function TripsPage() {
  return (
    <div className="page-container animate-fade-in">
      <div className="page-header">
        <h1>Viagens</h1>
        <p>Gerencie as viagens agendadas e em andamento.</p>
      </div>
      <TripList />
    </div>
  );
}
