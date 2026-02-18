import { useState, useEffect } from 'react';
import {
  PieChart, Pie, Cell, ResponsiveContainer, Tooltip,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Legend,
} from 'recharts';
import Card from '../../../components/ui/Card';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import { financeApi } from '../../../api/finance';
import { useFinance } from '../FinanceContext';
import { formatCurrency } from '../../../utils/formatCurrency';
import { CATEGORY_COLORS, PERSONAL_EXPENSE_CATEGORIES } from '../../../constants';
import { cn } from '../../../utils/cn';
import type { SummaryResponse } from '../../../types';

const CHART_COLORS = [
  '#f59e0b', '#3b82f6', '#8b5cf6', '#ec4899', '#ef4444',
  '#06b6d4', '#10b981', '#f97316', '#92400e', '#e879f9',
  '#6366f1', '#6b7280',
];

export default function SummaryView() {
  const { month, year, refreshKey } = useFinance();
  const [data, setData] = useState<SummaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    financeApi
      .getSummary(month, year)
      .then((res) => { if (!cancelled) setData(res.data.data || null); })
      .catch(() => { if (!cancelled) setData(null); })
      .finally(() => { if (!cancelled) setIsLoading(false); });
    return () => { cancelled = true; };
  }, [month, year, refreshKey]);

  if (isLoading) {
    return <p className="py-8 text-center text-gray-400">Đang tải...</p>;
  }

  if (!data) {
    return <p className="py-8 text-center text-gray-400">Không có dữ liệu</p>;
  }

  const pieData = data.byCategory.map((cat, i) => ({
    name: PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === cat.category)?.label || cat.category,
    value: cat.amount,
    color: CATEGORY_COLORS[cat.category] || CHART_COLORS[i % CHART_COLORS.length],
  }));

  const barData = data.dailyTrend.map((d) => ({
    date: new Date(d.date).getDate().toString(),
    'Thu nhập': d.income,
    'Chi tiêu': d.expense,
  }));

  const comparison = data.comparedToLastMonth;

  return (
    <div className="space-y-4">
      {/* Overview cards */}
      <div className="grid grid-cols-2 gap-3">
        <Card>
          <p className="text-xs text-gray-500">Tổng thu</p>
          <p className="text-lg font-bold text-green-600">
            {formatCurrency(data.totalIncome)}
          </p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Tổng chi</p>
          <p className="text-lg font-bold text-red-500">
            {formatCurrency(data.totalExpense)}
          </p>
        </Card>
      </div>

      {/* Month comparison */}
      {comparison && (
        <Card>
          <h3 className="text-sm font-semibold mb-2">So với tháng trước</h3>
          <div className="flex gap-4">
            <div className="flex-1">
              <p className="text-xs text-gray-400">Thu nhập</p>
              <p
                className={cn(
                  'text-sm font-semibold',
                  comparison.incomeChangePercent >= 0 ? 'text-green-600' : 'text-red-500'
                )}
              >
                {comparison.incomeChangePercent >= 0 ? '+' : ''}
                {comparison.incomeChangePercent.toFixed(1)}%
              </p>
            </div>
            <div className="flex-1">
              <p className="text-xs text-gray-400">Chi tiêu</p>
              <p
                className={cn(
                  'text-sm font-semibold',
                  comparison.expenseChangePercent <= 0 ? 'text-green-600' : 'text-red-500'
                )}
              >
                {comparison.expenseChangePercent >= 0 ? '+' : ''}
                {comparison.expenseChangePercent.toFixed(1)}%
              </p>
            </div>
          </div>
        </Card>
      )}

      {/* Expense by category pie chart */}
      {pieData.length > 0 && (
        <Card>
          <h3 className="text-sm font-semibold mb-3">Chi tiêu theo danh mục</h3>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie
                data={pieData}
                cx="50%"
                cy="50%"
                innerRadius={50}
                outerRadius={80}
                paddingAngle={2}
                dataKey="value"
              >
                {pieData.map((entry, i) => (
                  <Cell key={i} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip
                formatter={(value) => formatCurrency(Number(value))}
              />
            </PieChart>
          </ResponsiveContainer>
          {/* Legend */}
          <div className="mt-2 grid grid-cols-2 gap-1">
            {data.byCategory.map((cat) => (
              <div key={cat.category} className="flex items-center gap-2 text-xs">
                <span
                  className="h-2.5 w-2.5 rounded-full flex-shrink-0"
                  style={{
                    backgroundColor:
                      CATEGORY_COLORS[cat.category] || '#6b7280',
                  }}
                />
                <span className="truncate">
                  {PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === cat.category)?.label ||
                    cat.category}
                </span>
                <span className="ml-auto text-gray-400">
                  {cat.percentage.toFixed(0)}%
                </span>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Daily trend bar chart */}
      {barData.length > 0 && (
        <Card>
          <h3 className="text-sm font-semibold mb-3">Xu hướng hàng ngày</h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={barData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="date" tick={{ fontSize: 10 }} />
              <YAxis tick={{ fontSize: 10 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
              <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Bar dataKey="Thu nhập" fill="#10b981" radius={[2, 2, 0, 0]} />
              <Bar dataKey="Chi tiêu" fill="#ef4444" radius={[2, 2, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      )}

      {/* Top expenses */}
      {data.topExpenses.length > 0 && (
        <Card>
          <h3 className="text-sm font-semibold mb-3">Chi tiêu lớn nhất</h3>
          <div className="divide-y divide-gray-100 dark:divide-gray-700">
            {data.topExpenses.map((tx, i) => (
              <div key={tx.id} className="flex items-center gap-3 py-2">
                <span className="text-xs font-medium text-gray-400 w-5">
                  {i + 1}
                </span>
                <CategoryIcon
                  category={tx.expenseCategory || 'Other'}
                  size="sm"
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{tx.description}</p>
                  <p className="text-xs text-gray-400">
                    {new Date(tx.date).toLocaleDateString('vi-VN')}
                  </p>
                </div>
                <span className="text-sm font-semibold text-red-500">
                  -{formatCurrency(tx.amount)}
                </span>
              </div>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
}
