import { ChevronLeft, ChevronRight } from 'lucide-react';

interface MonthNavigatorProps {
  month: number;
  year: number;
  onChange: (month: number, year: number) => void;
}

export default function MonthNavigator({ month, year, onChange }: MonthNavigatorProps) {
  const prevMonth = () => {
    if (month === 1) onChange(12, year - 1);
    else onChange(month - 1, year);
  };

  const nextMonth = () => {
    if (month === 12) onChange(1, year + 1);
    else onChange(month + 1, year);
  };

  return (
    <div className="flex items-center justify-between">
      <button
        onClick={prevMonth}
        className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
      >
        <ChevronLeft className="h-5 w-5" />
      </button>
      <h2 className="text-lg font-semibold">
        Tháng {month}, {year}
      </h2>
      <button
        onClick={nextMonth}
        className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
      >
        <ChevronRight className="h-5 w-5" />
      </button>
    </div>
  );
}
