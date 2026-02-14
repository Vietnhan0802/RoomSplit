import { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip } from 'recharts';
import Card from '../../components/ui/Card';
import { financeApi } from '../../api/finance';
import { formatCurrency } from '../../utils/formatCurrency';
import { CATEGORY_COLORS, PERSONAL_EXPENSE_CATEGORIES } from '../../constants';
import type { SummaryResponse } from '../../types';

export default function Reports() {
  const now = new Date();
  const [data, setData] = useState<SummaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    financeApi
      .getSummary(now.getMonth() + 1, now.getFullYear())
      .then((res) => setData(res.data.data || null))
      .finally(() => setIsLoading(false));
  }, []);

  if (isLoading) return <p className="text-center text-gray-400 py-8">Đang tải...</p>;
  if (!data) return <p className="text-center text-gray-400 py-8">Chưa có dữ liệu</p>;

  const pieData = data.byCategory.map((cat) => ({
    name: PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === cat.category)?.label || cat.category,
    value: cat.amount,
    color: CATEGORY_COLORS[cat.category] || '#6b7280',
  }));

  const barData = [
    { name: 'Thu nhập', amount: data.totalIncome, fill: '#10b981' },
    { name: 'Chi tiêu', amount: data.totalExpense, fill: '#ef4444' },
    { name: 'Tiết kiệm', amount: Math.max(0, data.netAmount), fill: '#3b82f6' },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Báo cáo tháng {now.getMonth() + 1}/{now.getFullYear()}</h1>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <h2 className="mb-4 font-semibold">Tổng quan</h2>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={barData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis tickFormatter={(v: number) => `${(v / 1000).toFixed(0)}k`} />
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
          <h2 className="mb-4 font-semibold">Chi tiêu theo danh mục</h2>
          {pieData.length === 0 ? (
            <p className="py-12 text-center text-gray-400">Chưa có dữ liệu</p>
          ) : (
            <ResponsiveContainer width="100%" height={250}>
              <PieChart>
                <Pie data={pieData} cx="50%" cy="50%" outerRadius={80} dataKey="value" label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                  {pieData.map((entry, i) => (
                    <Cell key={i} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              </PieChart>
            </ResponsiveContainer>
          )}
        </Card>
      </div>

      <Card>
        <h2 className="mb-4 font-semibold">Chi tiết theo danh mục</h2>
        <div className="space-y-3">
          {pieData
            .sort((a, b) => b.value - a.value)
            .map((item) => {
              const totalExpense = data.totalExpense || 1;
              const pct = (item.value / totalExpense) * 100;
              return (
                <div key={item.name}>
                  <div className="flex justify-between text-sm">
                    <span>{item.name}</span>
                    <span className="font-medium">{formatCurrency(item.value)} ({pct.toFixed(1)}%)</span>
                  </div>
                  <div className="mt-1 h-2 rounded-full bg-gray-200 dark:bg-gray-700">
                    <div className="h-2 rounded-full" style={{ width: `${pct}%`, backgroundColor: item.color }} />
                  </div>
                </div>
              );
            })}
        </div>
      </Card>
    </div>
  );
}
