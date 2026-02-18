import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { PERSONAL_EXPENSE_CATEGORIES } from '../../constants';
import type { Transaction } from '../../types';

interface TopExpensesSectionProps {
  expenses: Transaction[];
}

export default function TopExpensesSection({ expenses }: TopExpensesSectionProps) {
  if (expenses.length === 0) return null;

  return (
    <Card>
      <h2 className="mb-4 font-semibold">Top chi tiêu</h2>
      <div className="space-y-3">
        {expenses.map((tx, index) => {
          const catLabel = PERSONAL_EXPENSE_CATEGORIES.find(
            (c) => c.key === tx.expenseCategory
          )?.label || tx.expenseCategory || '';
          const thumbnail = tx.images?.[0]?.thumbnailUrl || tx.images?.[0]?.imageUrl;

          return (
            <div key={tx.id} className="flex items-center gap-3">
              <span className="flex h-6 w-6 items-center justify-center rounded-full bg-gray-100 text-xs font-bold text-gray-600 dark:bg-gray-700 dark:text-gray-300">
                {index + 1}
              </span>

              {thumbnail ? (
                <img
                  src={thumbnail}
                  alt=""
                  className="h-10 w-10 rounded-lg object-cover"
                />
              ) : (
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-red-50 text-lg dark:bg-red-900/20">
                  💸
                </div>
              )}

              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">
                  {tx.description || catLabel}
                </p>
                <p className="text-xs text-gray-400">
                  {catLabel} &middot; {new Date(tx.date).toLocaleDateString('vi-VN')}
                </p>
              </div>

              <span className="whitespace-nowrap font-medium text-red-600">
                -{formatCurrency(tx.amount)}
              </span>
            </div>
          );
        })}
      </div>
    </Card>
  );
}
