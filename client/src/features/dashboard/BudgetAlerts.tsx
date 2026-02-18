import { AlertTriangle } from 'lucide-react';
import { Link } from 'react-router-dom';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { PERSONAL_EXPENSE_CATEGORIES, ROUTES } from '../../constants';
import type { BudgetStatus } from '../../types';

interface BudgetAlertsProps {
  budgets: BudgetStatus[];
}

export default function BudgetAlerts({ budgets }: BudgetAlertsProps) {
  const alertBudgets = budgets
    .filter((b) => b.percentUsed >= 80)
    .sort((a, b) => b.percentUsed - a.percentUsed)
    .slice(0, 3);

  if (alertBudgets.length === 0) return null;

  return (
    <Card>
      <div className="mb-3 flex items-center justify-between">
        <h2 className="flex items-center gap-2 font-semibold">
          <AlertTriangle className="h-4 w-4 text-amber-500" />
          Cảnh báo ngân sách
        </h2>
        <Link to={ROUTES.BUDGETS} className="text-sm text-primary-600 hover:underline">
          Xem tất cả
        </Link>
      </div>
      <div className="space-y-3">
        {alertBudgets.map((budget) => {
          const catLabel = PERSONAL_EXPENSE_CATEGORIES.find(
            (c) => c.key === budget.expenseCategory
          )?.label || budget.expenseCategory;
          const isOver = budget.percentUsed >= 100;

          return (
            <div key={budget.id} className="space-y-1">
              <div className="flex items-center justify-between text-sm">
                <span className={isOver ? 'font-medium text-red-600' : 'text-amber-600'}>
                  {isOver ? '🚨' : '⚠️'} {catLabel} đã dùng {Math.round(budget.percentUsed)}% ngân sách
                </span>
              </div>
              <div className="flex items-center justify-between text-xs text-gray-500">
                <span>{formatCurrency(budget.spentAmount)} / {formatCurrency(budget.monthlyLimit)}</span>
                <span className={isOver ? 'text-red-500' : 'text-amber-500'}>
                  {isOver ? `Vượt ${formatCurrency(budget.spentAmount - budget.monthlyLimit)}` : `Còn ${formatCurrency(budget.remainingAmount)}`}
                </span>
              </div>
              <div className="h-2 rounded-full bg-gray-200 dark:bg-gray-700">
                <div
                  className={`h-2 rounded-full transition-all ${isOver ? 'bg-red-500' : 'bg-amber-500'}`}
                  style={{ width: `${Math.min(budget.percentUsed, 100)}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </Card>
  );
}
