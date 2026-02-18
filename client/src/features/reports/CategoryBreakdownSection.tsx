import { useState } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { CATEGORY_COLORS, PERSONAL_EXPENSE_CATEGORIES } from '../../constants';
import type { CategoryBreakdownItem, Transaction } from '../../types';

interface CategoryBreakdownSectionProps {
  data: CategoryBreakdownItem[];
}

export default function CategoryBreakdownSection({ data }: CategoryBreakdownSectionProps) {
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  const pieData = data.map((item) => ({
    name: PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === item.category)?.label || item.category,
    value: item.amount,
    color: CATEGORY_COLORS[item.category] || '#6b7280',
    key: item.category,
  }));

  const selectedItem = data.find((d) => d.category === selectedCategory);

  return (
    <Card>
      <h2 className="mb-4 font-semibold">Chi tiêu theo danh mục</h2>

      {pieData.length === 0 ? (
        <p className="py-12 text-center text-gray-400">Chưa có dữ liệu</p>
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          <div>
            <ResponsiveContainer width="100%" height={280}>
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={100}
                  dataKey="value"
                  onClick={(_, index) => {
                    const key = pieData[index].key;
                    setSelectedCategory(selectedCategory === key ? null : key);
                  }}
                  style={{ cursor: 'pointer' }}
                >
                  {pieData.map((entry, i) => (
                    <Cell
                      key={i}
                      fill={entry.color}
                      opacity={selectedCategory && selectedCategory !== entry.key ? 0.4 : 1}
                    />
                  ))}
                </Pie>
                <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              </PieChart>
            </ResponsiveContainer>
          </div>

          <div className="space-y-2">
            {pieData.map((item) => (
              <button
                key={item.key}
                onClick={() => setSelectedCategory(selectedCategory === item.key ? null : item.key)}
                className={`flex w-full items-center justify-between rounded-lg p-2 text-left transition-colors hover:bg-gray-50 dark:hover:bg-gray-700 ${
                  selectedCategory === item.key ? 'bg-gray-50 dark:bg-gray-700' : ''
                }`}
              >
                <div className="flex items-center gap-2">
                  <div className="h-3 w-3 rounded-full" style={{ backgroundColor: item.color }} />
                  <span className="text-sm">{item.name}</span>
                </div>
                <span className="text-sm font-medium">{formatCurrency(item.value)}</span>
              </button>
            ))}
          </div>
        </div>
      )}

      {selectedItem && selectedItem.transactions.length > 0 && (
        <div className="mt-4 border-t pt-4 dark:border-gray-700">
          <h3 className="mb-2 text-sm font-medium text-gray-600 dark:text-gray-400">
            Giao dịch trong {PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === selectedCategory)?.label || selectedCategory}
          </h3>
          <div className="max-h-60 space-y-2 overflow-y-auto">
            {selectedItem.transactions.map((tx: Transaction) => (
              <div key={tx.id} className="flex items-center justify-between rounded-lg p-2 text-sm">
                <div>
                  <p className="font-medium">{tx.description || tx.expenseCategory}</p>
                  <p className="text-xs text-gray-400">
                    {new Date(tx.date).toLocaleDateString('vi-VN')}
                  </p>
                </div>
                <span className="font-medium text-red-600">-{formatCurrency(tx.amount)}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </Card>
  );
}
