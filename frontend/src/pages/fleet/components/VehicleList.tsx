import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Plus, Search, Edit2, Eye, Trash2, Loader2, Truck, User, Tag, Hash, Box, FileText, Calendar } from 'lucide-react';
import VehicleFormModal from './VehicleFormModal';
import BaseModal from '../../../components/shared/BaseModal';

interface VehicleDto {
  id: string;
  licensePlate: string;
  nickname: string;
  status: string;
  capacity?: number;
  driverName?: string;
  driverCpf?: string;
}

export default function VehicleList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editVehicleId, setEditVehicleId] = useState<string | null>(null);
  const [viewVehicleId, setViewVehicleId] = useState<string | null>(null);

  const { data, isLoading } = useQuery<{ items: VehicleDto[], totalCount: number }>({
    queryKey: ['vehicles', searchTerm],
    queryFn: async () => {
      const res = await api.get('/vehicles', { params: { searchTerm } });
      return res.data;
    }
  });

  const { data: vehicleDetail, isLoading: loadingDetail } = useQuery({
    queryKey: ['vehicle', viewVehicleId],
    queryFn: async () => {
      const res = await api.get(`/vehicles/${viewVehicleId}`);
      return res.data;
    },
    enabled: !!viewVehicleId,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/vehicles/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles-dropdown'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles-select'] });
      queryClient.invalidateQueries({ queryKey: ['drivers'] });
    }
  });

  return (
    <>
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        <div className="toolbar" style={{ padding: '1rem 1.25rem', margin: 0, borderBottom: '1px solid var(--border-color)' }}>
          <div className="search-box">
            <Search size={18} />
            <input placeholder="Buscar por placa ou apelido..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)} />
          </div>
          <button className="btn-primary" onClick={() => { setEditVehicleId(null); setIsModalOpen(true); }}>
            <Plus size={18} />
            <span>Novo Veículo</span>
          </button>
        </div>

        {isLoading ? (
          <div className="loading-state"><Loader2 className="spinner" size={24} /></div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Placa</th>
                <th>Apelido</th>
                <th>Motorista</th>
                <th>Status</th>
                <th>Capacidade</th>
                <th style={{ textAlign: 'right' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((vehicle) => (
                <tr key={vehicle.id}>
                  <td style={{ fontWeight: 500 }}>{vehicle.licensePlate}</td>
                  <td>{vehicle.nickname}</td>
                  <td>{vehicle.driverName || vehicle.driverCpf || '-'}</td>
                  <td>
                    <span className="status-badge">{vehicle.status}</span>
                  </td>
                  <td>{vehicle.capacity || 'N/A'}</td>
                  <td style={{ textAlign: 'right' }}>
                    <div className="action-buttons" style={{ justifyContent: 'flex-end' }}>
                      <button className="btn-icon" title="Visualizar" onClick={() => setViewVehicleId(vehicle.id)}><Eye size={18} /></button>
                      <button className="btn-icon" title="Editar" onClick={() => { setEditVehicleId(vehicle.id); setIsModalOpen(true); }}><Edit2 size={18} /></button>
                      <button className="btn-icon" style={{ color: 'var(--error)' }} title="Excluir" onClick={() => { if (window.confirm(`Excluir veículo ${vehicle.licensePlate}?`)) deleteMutation.mutate(vehicle.id); }}><Trash2 size={18} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {!data?.items?.length && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '2rem' }}>
                    Nenhum veículo encontrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {isModalOpen && (
        <VehicleFormModal
          vehicle={editVehicleId ? { id: editVehicleId } : undefined}
          onClose={() => {
            setIsModalOpen(false);
            setEditVehicleId(null);
            queryClient.invalidateQueries({ queryKey: ['vehicles'] });
            queryClient.invalidateQueries({ queryKey: ['vehicles-dropdown'] });
            queryClient.invalidateQueries({ queryKey: ['drivers'] });
          }}
        />
      )}

      <BaseModal
        open={!!viewVehicleId}
        onClose={() => setViewVehicleId(null)}
        title="Detalhes do Veículo"
        maxWidth="520px"
      >
        {loadingDetail ? (
          <div className="loading-state"><Loader2 className="spinner" size={24} /></div>
        ) : vehicleDetail ? (
          <div className="detail-grid">
            <div className="detail-section">
              <div className="detail-section-title"><Truck size={14} /> Identificação</div>
              <div className="detail-section-body">
                <div className="detail-row">
                  <span className="detail-label">Placa</span>
                  <span className="detail-value">{vehicleDetail.licensePlate}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Apelido</span>
                  <span className="detail-value">{vehicleDetail.nickname}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Status</span>
                  <span className="detail-value"><span className="status-badge">{vehicleDetail.status}</span></span>
                </div>
                {vehicleDetail.driverName && (
                  <div className="detail-row">
                    <span className="detail-label">Motorista</span>
                    <span className="detail-value"><User size={13} style={{ marginRight: 4, verticalAlign: 'middle' }} />{vehicleDetail.driverName}</span>
                  </div>
                )}
              </div>
            </div>
            {(vehicleDetail.brand || vehicleDetail.model || vehicleDetail.year || vehicleDetail.color) && (
              <div className="detail-section">
                <div className="detail-section-title"><Tag size={14} /> Especificações</div>
                <div className="detail-section-body">
                  {vehicleDetail.brand && <div className="detail-row"><span className="detail-label">Marca</span><span className="detail-value">{vehicleDetail.brand}</span></div>}
                  {vehicleDetail.model && <div className="detail-row"><span className="detail-label">Modelo</span><span className="detail-value">{vehicleDetail.model}</span></div>}
                  {vehicleDetail.year && <div className="detail-row"><span className="detail-label">Ano</span><span className="detail-value">{vehicleDetail.year}</span></div>}
                  {vehicleDetail.color && <div className="detail-row"><span className="detail-label">Cor</span><span className="detail-value"><span className="color-dot" style={{ backgroundColor: vehicleDetail.color }} />{vehicleDetail.color}</span></div>}
                </div>
              </div>
            )}
            {(vehicleDetail.chassi || vehicleDetail.renavam || vehicleDetail.anttNumber) && (
              <div className="detail-section">
                <div className="detail-section-title"><FileText size={14} /> Documentação</div>
                <div className="detail-section-body">
                  {vehicleDetail.chassi && <div className="detail-row"><span className="detail-label">Chassi</span><span className="detail-value" style={{ fontSize: '0.8rem' }}>{vehicleDetail.chassi}</span></div>}
                  {vehicleDetail.renavam && <div className="detail-row"><span className="detail-label">Renavam</span><span className="detail-value">{vehicleDetail.renavam}</span></div>}
                  {vehicleDetail.anttNumber && <div className="detail-row"><span className="detail-label">ANTT</span><span className="detail-value">{vehicleDetail.anttNumber}</span></div>}
                </div>
              </div>
            )}
            {(vehicleDetail.capacity) && (
              <div className="detail-section">
                <div className="detail-section-title"><Box size={14} /> Capacidade</div>
                <div className="detail-section-body">
                  {vehicleDetail.capacity && <div className="detail-row"><span className="detail-label">Capacidade</span><span className="detail-value">{vehicleDetail.capacity} kg</span></div>}
                </div>
              </div>
            )}
          </div>
        ) : null}
      </BaseModal>
    </>
  );
}
