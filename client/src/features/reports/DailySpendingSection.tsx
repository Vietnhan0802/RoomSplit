import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine, Cell } from 'recharts';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import type { DailySpendingResponse } from '../../types';

interface DailySpendingSectionProps {
  data: DailySpendingResponse;
}

export default function DailySpendingSection({ data }: DailySpendingSectionProps) {
  const maxDayStr = data.maxDay ? new Date(data.maxDay).toISOString().split('T')[0] : null;

  const chartData = data.days.map((day) => {
    const dateObj = new Date(day.date);
    const dayStr = dateObj.toISOString().split('T')[0];
    return {
      name: dateObj.getDate().toString(),
      amount: day.amount,
      isMax: dayStr === maxDayStr,
    };
  });

  return (
    <Card>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-semibold">Chi tiêu hàng ngày</h2>
        <span className="text-sm text-gray-500">
          TB: {formatCurrency(data.dailyAverage)}/ngày
        </span>
      </div>

      {chartData.every((d) => d.amount === 0) ? (
        <p className="py-12 text-center text-gray-400">Chưa có dữ liệu</p>
      ) : (
        <>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tickFormatter={(v: number) => `${(v / 1_000).toFixed(0)}k`} />
              <Tooltip
                formatter={(value) => formatCurrency(Number(value))}
                labelFormatter={(label) => `Ngày ${label}`}
              />
              <ReferenceLine
                y={data.dailyAverage}
                stroke="#f59e0b"
                strokeDasharray="5 5"
                label={{ value: 'TB', position: 'right', fill: '#f59e0b', fontSize: 12 }}
              />
              <Bar dataKey="amount" radius={[2, 2, 0, 0]}>
                {chartData.map((entry, i) => (
                  <Cell key={i} fill={entry.isMax ? '#ef4444' : '#93c5fd'} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>

          {data.maxDay && (
            <p className="mt-2 text-center text-sm text-gray-500">
              Chi nhiều nhất: ngày {new Date(data.maxDay).getDate()} — {formatCurrency(data.maxAmount)}
            </p>
          )}
        </>
      )}
    </Card>
  );
}
