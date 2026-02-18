import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import type { ReportOverview } from '../../types';

interface OverviewSectionProps {
  data: ReportOverview;
}

export default function OverviewSection({ data }: OverviewSectionProps) {
  const barData = [
    { name: 'Thu nhập', amount: data.totalIncome, fill: '#10b981' },
    { name: 'Chi tiêu', amount: data.totalExpense, fill: '#ef4444' },
    { name: 'Tiết kiệm', amount: Math.max(0, data.netSavings), fill: '#3b82f6' },
  ];

  return (
    <div className="grid gap-4 lg:grid-cols-3">
      <Card className="lg:col-span-2">
        <h2 className="mb-4 font-semibold">Tổng quan</h2>
        <ResponsiveContainer width="100%" height={250}>
          <BarChart data={barData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis tickFormatter={(v: number) => `${(v / 1_000_000).toFixed(1)}M`} />
            <Tooltip formatter={(value) => formatCurrency(Number(value))} />
            <Bar dataKey="amount" radius={[4, 4, 0, 0]}>
              {barData.map((entry, i) => (
                <Cell key={i} fill={entry.fill} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </Card>

      <Card>
        <h2 className="mb-4 font-semibold">Tiết kiệm</h2>
        <div className="flex flex-col items-center justify-center py-6">
          <p className={`text-3xl font-bold ${data.netSavings >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {formatCurrency(data.netSavings)}
          </p>
          <p className="mt-1 text-sm text-gray-500">
            {data.savingsPercent >= 0 ? `${data.savingsPercent}% thu nhập` : 'Chi nhiều hơn thu'}
          </p>
          <div className="mt-4 w-full space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-500">Thu nhập</span>
              <span className="font-medium text-green-600">
                {formatCurrency(data.totalIncome)}
                {data.incomeChangePercent !== 0 && (
                  <span className={`ml-1 text-xs ${data.incomeChangePercent >= 0 ? 'text-green-500' : 'text-red-500'}`}>
                    ({data.incomeChangePercent >= 0 ? '+' : ''}{data.incomeChangePercent}%)
                  </span>
                )}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-500">Chi tiêu</span>
              <span className="font-medium text-red-600">
                {formatCurrency(data.totalExpense)}
                {data.expenseChangePercent !== 0 && (
                  <span className={`ml-1 text-xs ${data.expenseChangePercent <= 0 ? 'text-green-500' : 'text-red-500'}`}>
                    ({data.expenseChangePercent >= 0 ? '+' : ''}{data.expenseChangePercent}%)
                  </span>
                )}
              </span>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
}
