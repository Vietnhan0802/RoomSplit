import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { PERSONAL_EXPENSE_CATEGORIES } from '../../constants';
import type { CategoryComparisonItem } from '../../types';

interface ComparisonSectionProps {
  data: CategoryComparisonItem[];
  month: number;
  year: number;
}

function getPrevMonth(month: number, year: number): { month: number; year: number } {
  return month === 1 ? { month: 12, year: year - 1 } : { month: month - 1, year };
}

export default function ComparisonSection({ data, month, year }: ComparisonSectionProps) {
  const prev = getPrevMonth(month, year);

  if (data.length === 0) return null;

  return (
    <Card>
      <h2 className="mb-4 font-semibold">So sánh với tháng trước</h2>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b dark:border-gray-700">
              <th className="pb-2 text-left font-medium text-gray-500">Danh mục</th>
              <th className="pb-2 text-right font-medium text-gray-500">T{prev.month}/{prev.year}</th>
              <th className="pb-2 text-right font-medium text-gray-500">T{month}/{year}</th>
              <th className="pb-2 text-right font-medium text-gray-500">Thay đổi</th>
            </tr>
          </thead>
          <tbody>
            {data.map((item) => {
              const catLabel = PERSONAL_EXPENSE_CATEGORIES.find(
                (c) => c.key === item.category
              )?.label || item.category;
              const isIncrease = item.amountChange > 0;
              const isDecrease = item.amountChange < 0;

              return (
                <tr key={item.category} className="border-b last:border-0 dark:border-gray-700">
                  <td className="py-2 font-medium">{catLabel}</td>
                  <td className="py-2 text-right text-gray-500">
                    {formatCurrency(item.prevMonthAmount)}
                  </td>
                  <td className="py-2 text-right font-medium">
                    {formatCurrency(item.currentMonthAmount)}
                  </td>
                  <td className="py-2 text-right">
                    <span className={isIncrease ? 'text-red-500' : isDecrease ? 'text-green-500' : 'text-gray-400'}>
                      {isIncrease ? '+' : ''}{formatCurrency(item.amountChange)}
                      {item.percentChange !== 0 && (
                        <span className="ml-1 text-xs">
                          ({item.percentChange >= 0 ? '+' : ''}{item.percentChange}%)
                        </span>
                      )}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
