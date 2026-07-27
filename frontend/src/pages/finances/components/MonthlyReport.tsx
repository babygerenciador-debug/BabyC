import { useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../services/api';
import { Download, FileText, TrendingUp, TrendingDown, Landmark, Wallet } from 'lucide-react';
import { format } from 'date-fns';

interface FinancialMonthDto {
  id: string;
  year: number;
  monthNumber: number;
  label: string;
  ownerSalary: number;
  status: string;
  openedAt: string;
  closedAt?: string;
}

interface FinancialTransactionDto {
  id: string;
  categoryName: string;
  costCenterName?: string;
  type: 'Revenue' | 'Expense';
  amount: number;
  date: string;
  paymentDate?: string;
  description: string;
  status: 'Pending' | 'Paid' | 'Cancelled';
}

interface FinancialMonthReportDto {
  month: FinancialMonthDto;
  totalRevenues: number;
  totalExpenses: number;
  netBalance: number;
  ownerSalary: number;
  ownerTaxAmount: number;
  netOwnerSalary: number;
  transactions: FinancialTransactionDto[];
}

interface Props {
  monthId: string;
}

export default function MonthlyReport({ monthId }: Props) {
  const reportRef = useRef<HTMLDivElement>(null);

  const { data, isLoading } = useQuery<FinancialMonthReportDto>({
    queryKey: ['financial-month-report', monthId],
    queryFn: async () => {
      const res = await api.get(`/finance/months/${monthId}/report`);
      return res.data;
    },
    enabled: !!monthId,
  });

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

  const handleDownloadPdf = async () => {
    const html2canvas = (await import('html2canvas')).default;
    const jsPDF = (await import('jspdf')).default;

    if (!reportRef.current) return;

    const canvas = await html2canvas(reportRef.current, {
      scale: 2,
      backgroundColor: '#ffffff',
    });

    const imgData = canvas.toDataURL('image/png');
    const imgWidth = 210;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;

    const pdf = new jsPDF('p', 'mm', 'a4');
    let heightLeft = imgHeight;
    let position = 0;
    const pageHeight = 297;

    pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
    heightLeft -= pageHeight;

    while (heightLeft > 0) {
      position = heightLeft - imgHeight;
      pdf.addPage();
      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;
    }

    pdf.save(`relatorio-${data?.month.label?.replace(/\//g, '-') ?? monthId}.pdf`);
  };

  if (isLoading) return <p>Carregando relatório...</p>;
  if (!data) return null;

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h2 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <FileText size={22} />
          Relatório Mensal — {data.month.label}
        </h2>
        <button className="btn-primary" onClick={handleDownloadPdf} style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
          <Download size={18} />
          Baixar PDF
        </button>
      </div>

      <div ref={reportRef} style={{ background: 'white', color: '#1a1a1a', padding: '2rem', borderRadius: '8px' }}>
        <div style={{ textAlign: 'center', marginBottom: '1.5rem', borderBottom: '2px solid #e5e7eb', paddingBottom: '1rem' }}>
          <h1 style={{ fontSize: '1.5rem', margin: 0, color: '#1a1a1a' }}>Baby Turismo</h1>
          <p style={{ color: '#6b7280', margin: '0.25rem 0 0' }}>Relatório Financeiro — {data.month.label}</p>
          <p style={{ color: '#6b7280', fontSize: '0.85rem', margin: '0.25rem 0 0' }}>
            Status: {data.month.status === 'open' ? 'Aberto' : 'Fechado'}
            {data.month.closedAt ? ` — Fechado em ${format(new Date(data.month.closedAt), 'dd/MM/yyyy HH:mm')}` : ''}
          </p>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
          <div style={{ padding: '1rem', background: '#f0fdf4', borderRadius: '8px', textAlign: 'center' }}>
            <p style={{ color: '#166534', fontSize: '0.85rem', margin: 0 }}>Receitas</p>
            <p style={{ color: '#16a34a', fontSize: '1.25rem', fontWeight: 700, margin: '0.25rem 0 0' }}>{formatCurrency(data.totalRevenues)}</p>
          </div>
          <div style={{ padding: '1rem', background: '#fef2f2', borderRadius: '8px', textAlign: 'center' }}>
            <p style={{ color: '#991b1b', fontSize: '0.85rem', margin: 0 }}>Despesas</p>
            <p style={{ color: '#dc2626', fontSize: '1.25rem', fontWeight: 700, margin: '0.25rem 0 0' }}>{formatCurrency(data.totalExpenses)}</p>
          </div>
          <div style={{ padding: '1rem', background: data.netBalance >= 0 ? '#f0fdf4' : '#fef2f2', borderRadius: '8px', textAlign: 'center' }}>
            <p style={{ color: '#6b7280', fontSize: '0.85rem', margin: 0 }}>Saldo Líquido</p>
            <p style={{ color: data.netBalance >= 0 ? '#16a34a' : '#dc2626', fontSize: '1.25rem', fontWeight: 700, margin: '0.25rem 0 0' }}>{formatCurrency(data.netBalance)}</p>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', marginBottom: '1.5rem', padding: '1rem', background: '#f9fafb', borderRadius: '8px' }}>
          <div style={{ textAlign: 'center' }}>
            <p style={{ color: '#6b7280', fontSize: '0.8rem', margin: 0 }}>Salário Bruto</p>
            <p style={{ fontWeight: 600, margin: '0.25rem 0 0' }}>{formatCurrency(data.ownerSalary)}</p>
          </div>
          <div style={{ textAlign: 'center' }}>
            <p style={{ color: '#6b7280', fontSize: '0.8rem', margin: 0 }}>Imposto (27%)</p>
            <p style={{ fontWeight: 600, margin: '0.25rem 0 0', color: '#dc2626' }}>{formatCurrency(data.ownerTaxAmount)}</p>
          </div>
          <div style={{ textAlign: 'center' }}>
            <p style={{ color: '#6b7280', fontSize: '0.8rem', margin: 0 }}>Salário Líquido</p>
            <p style={{ fontWeight: 600, margin: '0.25rem 0 0', color: '#16a34a' }}>{formatCurrency(data.netOwnerSalary)}</p>
          </div>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.85rem' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #e5e7eb' }}>
              <th style={{ textAlign: 'left', padding: '0.5rem', color: '#6b7280' }}>Data</th>
              <th style={{ textAlign: 'left', padding: '0.5rem', color: '#6b7280' }}>Descrição</th>
              <th style={{ textAlign: 'left', padding: '0.5rem', color: '#6b7280' }}>Categoria</th>
              <th style={{ textAlign: 'right', padding: '0.5rem', color: '#6b7280' }}>Valor</th>
            </tr>
          </thead>
          <tbody>
            {data.transactions.map(tx => (
              <tr key={tx.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
                <td style={{ padding: '0.5rem' }}>{format(new Date(tx.date), 'dd/MM/yyyy')}</td>
                <td style={{ padding: '0.5rem', fontWeight: 500 }}>{tx.description}</td>
                <td style={{ padding: '0.5rem' }}>{tx.categoryName}</td>
                <td style={{ padding: '0.5rem', textAlign: 'right', color: tx.type === 'Revenue' ? '#16a34a' : '#dc2626' }}>
                  {tx.type === 'Revenue' ? '+' : '-'}{formatCurrency(tx.amount)}
                </td>
              </tr>
            ))}
            {data.transactions.length === 0 && (
              <tr>
                <td colSpan={4} style={{ textAlign: 'center', padding: '1rem', color: '#9ca3af' }}>
                  Nenhuma transação paga neste mês.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
