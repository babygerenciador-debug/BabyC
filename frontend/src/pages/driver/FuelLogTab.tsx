import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../services/api';
import { Fuel, Save } from 'lucide-react';
import { useState, useEffect } from 'react';

interface VehicleDto {
  id: string;
  licensePlate: string;
  nickname: string;
}

export default function FuelLogTab() {
  const queryClient = useQueryClient();
  const [vehicleId, setVehicleId] = useState('');
  const [odometer, setOdometer] = useState('');
  const [liters, setLiters] = useState('');
  const [totalCost, setTotalCost] = useState('');

  const { data: vehicles } = useQuery<VehicleDto[]>({
    queryKey: ['my-vehicles'],
    queryFn: async () => {
      const res = await api.get('/driver/vehicles');
      return res.data;
    }
  });

  useEffect(() => {
    if (vehicles?.length === 1 && !vehicleId)
      setVehicleId(vehicles[0].id);
  }, [vehicles, vehicleId]);

  const mutation = useMutation({
    mutationFn: (data: { vehicleId: string; odometer: number; liters: number; totalCost: number }) => {
      return api.post('/driver/fuel-logs', {
        vehicleId: data.vehicleId,
        odometer: data.odometer,
        liters: data.liters,
        totalCost: data.totalCost,
        date: new Date().toISOString()
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-fuel-logs'] });
      setOdometer('');
      setLiters('');
      setTotalCost('');
      alert('Abastecimento registrado com sucesso!');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!vehicleId || !odometer || !liters || !totalCost) {
      alert('Preencha todos os campos');
      return;
    }
    mutation.mutate({
      vehicleId,
      odometer: Number(odometer),
      liters: Number(liters),
      totalCost: Number(totalCost)
    });
  };

  return (
    <div>
      <div style={{
        background: 'var(--bg-card)',
        borderRadius: 'var(--radius-md)',
        padding: '1.5rem',
        boxShadow: 'var(--shadow-sm)'
      }}>
        <h3 style={{ marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Fuel size={20} style={{ color: 'var(--brand-color)' }} />
          Registrar Abastecimento
        </h3>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>
              Veículo *
            </label>
            <select
              value={vehicleId}
              onChange={(e) => setVehicleId(e.target.value)}
              style={{
                width: '100%',
                padding: '0.75rem',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                fontSize: '1rem'
              }}
            >
              <option value="">Selecione...</option>
              {vehicles?.map((v) => (
                <option key={v.id} value={v.id}>{v.licensePlate} - {v.nickname}</option>
              ))}
            </select>
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>
              Quilometragem *
            </label>
            <input
              type="number"
              value={odometer}
              onChange={(e) => setOdometer(e.target.value)}
              placeholder="Ex: 125000"
              style={{
                width: '100%',
                padding: '0.75rem',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                fontSize: '1rem'
              }}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>
              Litros *
            </label>
            <input
              type="number"
              step="0.01"
              value={liters}
              onChange={(e) => setLiters(e.target.value)}
              placeholder="Ex: 150.5"
              style={{
                width: '100%',
                padding: '0.75rem',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                fontSize: '1rem'
              }}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>
              Valor Total (R$) *
            </label>
            <input
              type="number"
              step="0.01"
              value={totalCost}
              onChange={(e) => setTotalCost(e.target.value)}
              placeholder="Ex: 850.00"
              style={{
                width: '100%',
                padding: '0.75rem',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                fontSize: '1rem'
              }}
            />
          </div>

          <button
            type="submit"
            disabled={mutation.isPending}
            style={{
              width: '100%',
              padding: '1rem',
              background: 'var(--brand-color)',
              color: 'white',
              border: 'none',
              borderRadius: 'var(--radius-md)',
              fontSize: '1rem',
              fontWeight: 600,
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '0.5rem'
            }}
          >
            <Save size={18} />
            {mutation.isPending ? 'Salvando...' : 'Registrar Abastecimento'}
          </button>
        </form>
      </div>
    </div>
  );
}
