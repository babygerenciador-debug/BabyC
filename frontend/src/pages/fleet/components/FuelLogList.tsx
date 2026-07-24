import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Filter, Eye, Droplets, Truck, User, DollarSign, FileText, Gauge } from 'lucide-react';
import { format } from 'date-fns';
import FuelLogFormModal from './FuelLogFormModal';
import BaseModal from '../../../components/shared/BaseModal';

interface FuelLogDto {
  id: string;
  vehicleId: string;
  vehicleLicensePlate: string;
  driverId?: string;
  driverName?: string;
  date: string;
  odometer: number;
  liters: number;
  totalCost: number;
  averageConsumption?: number;
  receiptUrl?: string;
  notes?: string;
}

export default function FuelLogList() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [viewLogId, setViewLogId] = useState<string | null>(null);
  const [selectedVehicle, setSelectedVehicle] = useState('');

  const { data, isLoading } = useQuery<{ items: FuelLogDto[]; totalCount: number }>({
    queryKey: ['fuel-logs', selectedVehicle],
    queryFn: async () => {
      const params: any = { pageSize: 100 };
      if (selectedVehicle) params.vehicleId = selectedVehicle;
      const res = await api.get('/fuellogs', { params });
      return res.data;
    }
  });

  const logs = data?.items ?? [];
  const logDetail = logs.find(l => l.id === viewLogId) ?? null;

  return (
    <>
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        <div className="toolbar" style={{ padding: '1rem 1.25rem', margin: 0, borderBottom: '1px solid var(--border-color)' }}>
          <div className="search-box">
            <Filter size={18} />
            <select value={selectedVehicle} onChange={e => setSelectedVehicle(e.target.value)} style={{ border: 'none', background: 'transparent', fontSize: '0.9rem', outline: 'none', flex: 1, color: 'inherit' }}>
              <option value="">Todos os veículos</option>
              {logs.reduce((acc, l) => {
                if (!acc.find(v => v.id === l.vehicleId)) acc.push({ id: l.vehicleId, plate: l.vehicleLicensePlate });
                return acc;
              }, [] as { id: string, plate: string }[]).map(v => (
                <option key={v.id} value={v.id}>{v.plate}</option>
              ))}
            </select>
          </div>
          <button className="btn-primary" onClick={() => setIsModalOpen(true)}>
            <Plus size={18} />
            <span>Novo Abastecimento</span>
          </button>
        </div>

        {isLoading ? (
          <div className="loading-state">Carregando...</div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Data</th>
                <th>Veículo</th>
                <th>Motorista</th>
                <th>Odômetro</th>
                <th>Litros</th>
                <th>Custo Total</th>
                <th>Comprovante</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id}>
                  <td>{format(new Date(log.date), 'dd/MM/yyyy HH:mm')}</td>
                  <td><strong>{log.vehicleLicensePlate}</strong></td>
                  <td>{log.driverName || '-'}</td>
                  <td>{log.odometer} km</td>
                  <td>{log.liters} L</td>
                  <td style={{ color: 'var(--error)', fontWeight: 500 }}>
                    {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(log.totalCost)}
                  </td>
                  <td>
                    {log.receiptUrl ? (
                      <a href={log.receiptUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--info)' }}>Ver Anexo</a>
                    ) : '-'}
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon" title="Ver detalhes" onClick={() => setViewLogId(log.id)}><Eye size={18} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {!logs.length && (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum abastecimento registrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && <FuelLogFormModal onClose={() => setIsModalOpen(false)} />}

      <BaseModal
        open={!!viewLogId}
        onClose={() => setViewLogId(null)}
        title="Detalhes do Abastecimento"
        maxWidth="480px"
      >
        {logDetail ? (
          <div className="detail-grid">
            <div className="detail-section">
              <div className="detail-section-title"><Truck size={14} /> Veículo</div>
              <div className="detail-section-body">
                <div className="detail-row">
                  <span className="detail-label">Placa</span>
                  <span className="detail-value">{logDetail.vehicleLicensePlate}</span>
                </div>
                {logDetail.driverName && (
                  <div className="detail-row">
                    <span className="detail-label">Motorista</span>
                    <span className="detail-value"><User size={13} style={{ marginRight: 4, verticalAlign: 'middle' }} />{logDetail.driverName}</span>
                  </div>
                )}
              </div>
            </div>
            <div className="detail-section">
              <div className="detail-section-title"><Droplets size={14} /> Abastecimento</div>
              <div className="detail-section-body">
                <div className="detail-row">
                  <span className="detail-label">Data</span>
                  <span className="detail-value">{format(new Date(logDetail.date), 'dd/MM/yyyy HH:mm')}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Odômetro</span>
                  <span className="detail-value"><Gauge size={13} style={{ marginRight: 4, verticalAlign: 'middle' }} />{logDetail.odometer} km</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Litros</span>
                  <span className="detail-value">{logDetail.liters} L</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Custo Total</span>
                  <span className="detail-value" style={{ color: 'var(--error)', fontWeight: 600 }}>
                    <DollarSign size={13} style={{ marginRight: 2, verticalAlign: 'middle' }} />
                    {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(logDetail.totalCost)}
                  </span>
                </div>
                {logDetail.averageConsumption != null && (
                  <div className="detail-row">
                    <span className="detail-label">Consumo Médio</span>
                    <span className="detail-value">{logDetail.averageConsumption.toFixed(1)} km/L</span>
                  </div>
                )}
              </div>
            </div>
            {logDetail.notes && (
              <div className="detail-section" style={{ gridColumn: '1 / -1' }}>
                <div className="detail-section-title"><FileText size={14} /> Observações</div>
                <div className="detail-section-body">
                  <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--text-primary)' }}>{logDetail.notes}</p>
                </div>
              </div>
            )}
            {logDetail.receiptUrl && (
              <div className="detail-section" style={{ gridColumn: '1 / -1' }}>
                <div className="detail-section-title"><FileText size={14} /> Comprovante</div>
                <div className="detail-section-body">
                  <a href={logDetail.receiptUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--info)', fontWeight: 500, fontSize: '0.875rem' }}>
                    Visualizar anexo
                  </a>
                </div>
              </div>
            )}
          </div>
        ) : null}
      </BaseModal>
    </>
  );
}
