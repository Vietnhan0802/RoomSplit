import { Link } from 'react-router-dom';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { ROUTES, PERSONAL_EXPENSE_CATEGORIES, INCOME_CATEGORIES } from '../../constants';
import type { Transaction } from '../../types';

interface RecentActivitiesProps {
  transactions: Transaction[];
}

function getRelativeTime(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffMinutes = Math.floor(diffMs / 60000);

  if (diffMinutes < 1) return 'Vừa xong';
  if (diffMinutes < 60) return `${diffMinutes} phút trước`;
  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} giờ trước`;
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `${diffDays} ngày trước`;
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} tuần trước`;
  return `${Math.floor(diffDays / 30)} tháng trước`;
}

function getCategoryLabel(tx: Transaction): string {
  if (tx.type === 'Expense') {
    return PERSONAL_EXPENSE_CATEGORIES.find((c) => c.key === tx.expenseCategory)?.label || tx.expenseCategory || '';
  }
  return INCOME_CATEGORIES.find((c) => c.key === tx.incomeCategory)?.label || tx.incomeCategory || '';
}

export default function RecentActivities({ transactions }: RecentActivitiesProps) {
  const recent = transactions.slice(0, 10);

  return (
    <Card>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-semibold">Hoạt động gần đây</h2>
        <Link to={ROUTES.TRANSACTIONS} className="text-sm text-primary-600 hover:underline">
          Xem tất cả
        </Link>
      </div>
      {recent.length === 0 ? (
        <p className="py-8 text-center text-sm text-gray-400">Chưa có hoạt động nào</p>
      ) : (
        <div className="space-y-3">
          {recent.map((tx) => {
            const isIncome = tx.type === 'Income';
            const icon = isIncome ? '💰' : '💸';
            const catLabel = getCategoryLabel(tx);
            const description = tx.description || catLabel;

            return (
              <div key={tx.id} className="flex items-center gap-3">
                <span className="text-lg">{icon}</span>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-medium">
                    {isIncome ? `Nhận ${description}` : `Chi ${description}`}
                  </p>
                  <p className="text-xs text-gray-400">
                    {catLabel} &middot; {getRelativeTime(tx.createdAt)}
                  </p>
                </div>
                <span className={`whitespace-nowrap text-sm font-medium ${isIncome ? 'text-green-600' : 'text-red-600'}`}>
                  {isIncome ? '+' : '-'}{formatCurrency(tx.amount)}
                </span>
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
}
