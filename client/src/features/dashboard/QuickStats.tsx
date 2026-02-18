import { TrendingUp, TrendingDown, Home, CheckSquare } from 'lucide-react';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import type { SummaryResponse, BudgetStatus } from '../../types';

interface QuickStatsProps {
  summary: SummaryResponse | null;
  budgets: BudgetStatus[];
  todayTaskCount: number;
  onTaskClick: () => void;
}

export default function QuickStats({ summary, todayTaskCount, onTaskClick }: QuickStatsProps) {
  const incomeChange = summary?.comparedToLastMonth.incomeChangePercent ?? 0;
  const expenseChange = summary?.comparedToLastMonth.expenseChangePercent ?? 0;

  const stats = [
    {
      icon: TrendingUp,
      label: 'Thu nhập tháng này',
      value: formatCurrency(summary?.totalIncome ?? 0),
      change: incomeChange,
      color: 'green',
      bgClass: 'bg-green-100 dark:bg-green-900/30',
      iconClass: 'text-green-600',
      valueClass: 'text-green-600',
    },
    {
      icon: TrendingDown,
      label: 'Chi tiêu tháng này',
      value: formatCurrency(summary?.totalExpense ?? 0),
      change: expenseChange,
      color: 'red',
      bgClass: 'bg-red-100 dark:bg-red-900/30',
      iconClass: 'text-red-600',
      valueClass: 'text-red-600',
    },
    {
      icon: Home,
      label: 'Tiền phòng',
      value: summary
        ? formatCurrency(Math.abs(summary.netAmount))
        : '0đ',
      subtitle: summary && summary.netAmount >= 0 ? 'Tiết kiệm được' : 'Cần tiết kiệm thêm',
      color: 'blue',
      bgClass: 'bg-blue-100 dark:bg-blue-900/30',
      iconClass: 'text-blue-600',
      valueClass: 'text-blue-600',
    },
    {
      icon: CheckSquare,
      label: 'Tasks hôm nay',
      value: `${todayTaskCount} việc cần làm`,
      color: 'purple',
      bgClass: 'bg-purple-100 dark:bg-purple-900/30',
      iconClass: 'text-purple-600',
      valueClass: 'text-purple-600',
      onClick: onTaskClick,
    },
  ];

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {stats.map((stat) => (
        <Card
          key={stat.label}
          className={stat.onClick ? 'cursor-pointer' : ''}
          onClick={stat.onClick}
        >
          <div className="flex items-center gap-3">
            <div className={`rounded-lg p-2 ${stat.bgClass}`}>
              <stat.icon className={`h-5 w-5 ${stat.iconClass}`} />
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-xs text-gray-500 dark:text-gray-400">{stat.label}</p>
              <p className={`text-lg font-bold ${stat.valueClass}`}>{stat.value}</p>
              {'change' in stat && stat.change !== undefined && (
                <p className={`text-xs ${stat.change >= 0 ? 'text-green-500' : 'text-red-500'}`}>
                  {stat.change >= 0 ? '+' : ''}{stat.change}% so với tháng trước
                </p>
              )}
              {stat.subtitle && (
                <p className="text-xs text-gray-400">{stat.subtitle}</p>
              )}
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
}
