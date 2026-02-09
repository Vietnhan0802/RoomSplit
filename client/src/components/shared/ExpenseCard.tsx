import type { Expense } from '../../types';
import { formatCurrency } from '../../utils/formatCurrency';
import { formatDate } from '../../utils/dateHelpers';
import Badge from '../ui/Badge';
import UserAvatar from './UserAvatar';
import CategoryBadge from './CategoryBadge';

interface ExpenseCardProps {
  expense: Expense;
  onClick?: () => void;
}

export default function ExpenseCard({ expense, onClick }: ExpenseCardProps) {
  return (
    <div className="card cursor-pointer hover:shadow-md transition-shadow" onClick={onClick}>
      <div className="flex items-start justify-between">
        <div className="flex items-start gap-3">
          <UserAvatar user={expense.paidBy} />
          <div>
            <p className="font-medium">{expense.description}</p>
            <p className="text-xs text-gray-500">
              {expense.paidBy.fullName} - {formatDate(expense.expenseDate)}
            </p>
          </div>
        </div>
        <div className="text-right">
          <p className="font-semibold text-red-600">{formatCurrency(expense.amount)}</p>
          <CategoryBadge category={expense.category} />
        </div>
      </div>
      {expense.isSettled && (
        <Badge variant="success" className="mt-2">Đã thanh toán</Badge>
      )}
    </div>
  );
}
