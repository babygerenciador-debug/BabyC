import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { format } from 'date-fns';
import { Droplets, TrendingUp } from 'lucide-react';

interface FuelLogDto {
  id: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  date: string;
  odometer: number;
  liters: number;
  totalCost: number;
}

interface EnrichedLog extends FuelLogDto {
  kmDriven: number | null;
  costPerKm: number | null;
}

export default function FuelReport() {
  const { data, isLoading } = useQuery<{ items: FuelLogDto[], totalCount: number }>({
    queryKey: ['fuellogs-finance'],
    queryFn: async () => {
      const res = await api.get('/fuellogs', { params: { pageSize: 100 } });
      return res.data;
    }
  });

  const logs = useMemo(() => {
    const raw = data?.items ?? [];
    const byVehicle: Record<string, FuelLogDto[]> = {};
    for (const log of raw) {
      if (!byVehicle[log.vehicleId]) byVehicle[log.vehicleId] = [];
      byVehicle[log.vehicleId].push(log);
    }
    for (const arr of Object.values(byVehicle)) {
      arr.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
    }
    const idx = new Map<string, number>();
    return raw.map(log => {
      const arr = byVehicle[log.vehicleId];
      const i = arr.indexOf(log);
      const prevOdo = i > 0 ? arr[i - 1].odometer : null;
      const kmDriven = prevOdo !== null ? log.odometer - prevOdo : null;
      const costPerKm = kmDriven !== null && kmDriven > 0 ? log.totalCost / kmDriven : null;
      return { ...log, kmDriven, costPerKm };
    });
  }, [data]);

  const totalLiters = logs.reduce((s, l) => s + l.liters, 0);
  const totalCost = logs.reduce((s, l) => s + l.totalCost, 0);
  const avgPrice = totalLiters > 0 ? totalCost / totalLiters : 0;

  const fmt = (val: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

  return (
    <div className="card animate-fade-in">
      <div className="toolbar">
        <div className="search-box">
          <TrendingUp size={18} style={{ color: 'var(--text-secondary)' }} />
          <span>Relatório de Abastecimentos</span>
        </div>
      </div>

      <div className="kpi-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', marginBottom: '1.5rem' }}>
        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success)' }}>
            <Droplets size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total Gasto em Combustível</span>
            <h2 className="kpi-value">{fmt(totalCost)}</h2>
          </div>
        </div>
        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', color: 'var(--info)' }}>
            <Droplets size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total de Litros</span>
            <h2 className="kpi-value">{totalLiters.toFixed(1)} L</h2>
          </div>
        </div>
        <div className="kpi-card card">
          <div className="kpi-icon-wrapper" style={{ backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning)' }}>
            <Droplets size={24} />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Preço Médio por Litro</span>
            <h2 className="kpi-value">{fmt(avgPrice)}</h2>
          </div>
        </div>
      </div>

      <div className="data-table-container">
        {isLoading ? (
          <p>Carregando abastecimentos...</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Veículo</th>
                <th>Km Rodado</th>
                <th>Litros</th>
                <th>Custo Total</th>
                <th>Custo por Km</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id}>
                  <td>{format(new Date(log.date), 'dd/MM/yyyy')}</td>
                  <td>{log.vehicleLicensePlate}</td>
                  <td>{log.kmDriven !== null ? `${log.kmDriven} km` : '-'}</td>
                  <td>{log.liters.toFixed(1)} L</td>
                  <td style={{ color: 'var(--error)', fontWeight: 500 }}>{fmt(log.totalCost)}</td>
                  <td style={{ fontWeight: 500 }}>{log.costPerKm !== null ? fmt(log.costPerKm) : '-'}</td>
                </tr>
              ))}
              {!logs.length && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum abastecimento encontrado.
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
