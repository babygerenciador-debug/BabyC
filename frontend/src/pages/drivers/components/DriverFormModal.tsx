import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Save, Loader2 } from 'lucide-react';
import BaseModal from '../../../components/shared/BaseModal';

const driverSchema = z.object({
  name: z.string().min(3, 'Nome é obrigatório'),
  email: z.string().email('Email inválido'),
  password: z.string()
    .min(8, 'Senha deve ter no mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Senha deve conter pelo menos uma letra maiúscula')
    .regex(/[a-z]/, 'Senha deve conter pelo menos uma letra minúscula')
    .regex(/[0-9]/, 'Senha deve conter pelo menos um número')
    .regex(/[^a-zA-Z0-9]/, 'Senha deve conter pelo menos um caractere especial'),
  cpf: z.string().min(11, 'CPF é obrigatório'),
  cnhNumber: z.string().min(9, 'CNH é obrigatória'),
  cnhCategory: z.string().min(1, 'Categoria é obrigatória'),
  cnhExpirationDate: z.string().min(1, 'Data de vencimento é obrigatória'),
});

type DriverFormData = z.infer<typeof driverSchema>;

interface Props {
  onClose: () => void;
}

const FORM_ID = 'driver-form';

export default function DriverFormModal({ onClose }: Props) {
  const queryClient = useQueryClient();
  const { register, handleSubmit, formState: { errors } } = useForm<DriverFormData>({
    resolver: zodResolver(driverSchema)
  });

  const mutation = useMutation({
    mutationFn: (data: DriverFormData) => api.post('/drivers', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['drivers'] });
      onClose();
    }
  });

  const onSubmit = (data: DriverFormData) => {
    mutation.mutate(data);
  };

  return (
    <BaseModal
      open
      onClose={onClose}
      title="Cadastrar Motorista"
      maxWidth="600px"
      footer={
        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" form={FORM_ID} className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="spinner" size={18} /> : <Save size={18} />}
            <span>{mutation.isPending ? 'Salvando...' : 'Salvar Motorista'}</span>
          </button>
        </div>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit(onSubmit)} className="modal-form">
        <div className="form-grid">
          <div className="input-group" style={{ gridColumn: '1 / -1' }}>
            <label>Nome Completo *</label>
            <input {...register('name')} placeholder="Nome do motorista" />
            {errors.name && <span className="error-msg">{errors.name.message}</span>}
          </div>
          <div className="input-group">
            <label>Email *</label>
            <input type="email" {...register('email')} placeholder="email@exemplo.com" />
            {errors.email && <span className="error-msg">{errors.email.message}</span>}
          </div>
          <div className="input-group">
            <label>Senha *</label>
            <input type="password" {...register('password')} placeholder="Mínimo 6 caracteres" />
            {errors.password && <span className="error-msg">{errors.password.message}</span>}
          </div>
          <div className="input-group">
            <label>CPF *</label>
            <input {...register('cpf')} placeholder="000.000.000-00" />
            {errors.cpf && <span className="error-msg">{errors.cpf.message}</span>}
          </div>
          <div className="input-group">
            <label>Data de Nascimento</label>
            <input type="date" />
          </div>
          <div className="input-group">
            <label>Número CNH *</label>
            <input {...register('cnhNumber')} placeholder="Número da CNH" />
            {errors.cnhNumber && <span className="error-msg">{errors.cnhNumber.message}</span>}
          </div>
          <div className="input-group">
            <label>Categoria CNH *</label>
            <select {...register('cnhCategory')}>
              <option value="">Selecione...</option>
              <option value="A">A</option>
              <option value="B">B</option>
              <option value="C">C</option>
              <option value="D">D</option>
              <option value="E">E</option>
              <option value="AB">AB</option>
              <option value="AC">AC</option>
              <option value="AD">AD</option>
              <option value="AE">AE</option>
            </select>
            {errors.cnhCategory && <span className="error-msg">{errors.cnhCategory.message}</span>}
          </div>
          <div className="input-group">
            <label>Vencimento CNH *</label>
            <input type="date" {...register('cnhExpirationDate')} />
            {errors.cnhExpirationDate && <span className="error-msg">{errors.cnhExpirationDate.message}</span>}
          </div>
        </div>
      </form>
    </BaseModal>
  );
}
