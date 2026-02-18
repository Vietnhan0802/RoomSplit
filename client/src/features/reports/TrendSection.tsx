import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import type { MonthlyTrendItem } from '../../types';

interface TrendSectionProps {
  data: MonthlyTrendItem[];
}

export default function TrendSection({ data }: TrendSectionProps) {
  const chartData = data.map((item) => ({
    name: `T${item.month}/${item.year}`,
    'Thu nhập': item.income,
    'Chi tiêu': item.expense,
    'Tiết kiệm': item.savings,
  }));

  return (
    <Card>
      <h2 className="mb-4 font-semibold">Xu hướng 6 tháng</h2>
      {chartData.length === 0 ? (
        <p className="py-12 text-center text-gray-400">Chưa có dữ liệu</p>
      ) : (
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis tickFormatter={(v: number) => `${(v / 1_000_000).toFixed(1)}M`} />
            <Tooltip formatter={(value) => formatCurrency(Number(value))} />
            <Legend />
            <Line
              type="monotone"
              dataKey="Thu nhập"
              stroke="#10b981"
              strokeWidth={2}
              dot={{ r: 4 }}
            />
            <Line
              type="monotone"
              dataKey="Chi tiêu"
              stroke="#ef4444"
              strokeWidth={2}
              dot={{ r: 4 }}
            />
            <Line
              type="monotone"
              dataKey="Tiết kiệm"
              stroke="#3b82f6"
              strokeWidth={2}
              strokeDasharray="5 5"
              dot={{ r: 4 }}
            />
          </LineChart>
        </ResponsiveContainer>
      )}
    </Card>
  );
}
