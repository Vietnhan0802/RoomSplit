import { useState, useEffect } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import Card from '../../../components/ui/Card';
import { financeApi } from '../../../api/finance';
import { getDaysInMonth, isSameDay } from '../../../utils/dateHelpers';
import { formatCurrency } from '../../../utils/formatCurrency';
import type { Transaction } from '../../../types';

export default function CalendarView() {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  useEffect(() => {
    financeApi.getTransactions(month, year).then((res) => setTransactions(res.data.data || []));
  }, [month, year]);

  const days = getDaysInMonth(month, year);
  const weekDays = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

  const firstDayOfWeek = days[0].getDay();
  const emptyDays = Array.from({ length: firstDayOfWeek }, (_, i) => i);

  const getDayTotal = (date: Date) => {
    return transactions
      .filter((t) => isSameDay(new Date(t.transactionDate), date))
      .reduce((sum, t) => sum + (t.type === 'Expense' ? -t.amount : t.amount), 0);
  };

  const prevMonth = () => {
    if (month === 1) { setMonth(12); setYear(year - 1); }
    else setMonth(month - 1);
  };

  const nextMonth = () => {
    if (month === 12) { setMonth(1); setYear(year + 1); }
    else setMonth(month + 1);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <button onClick={prevMonth} className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700">
          <ChevronLeft className="h-5 w-5" />
        </button>
        <h2 className="text-lg font-semibold">Tháng {month}/{year}</h2>
        <button onClick={nextMonth} className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700">
          <ChevronRight className="h-5 w-5" />
        </button>
      </div>

      <Card>
        <div className="grid grid-cols-7 gap-1">
          {weekDays.map((d) => (
            <div key={d} className="p-2 text-center text-xs font-medium text-gray-500">{d}</div>
          ))}
          {emptyDays.map((i) => <div key={`e-${i}`} />)}
          {days.map((date) => {
            const total = getDayTotal(date);
            const isToday = isSameDay(date, new Date());
            return (
              <div
                key={date.toISOString()}
                className={`rounded-lg p-1.5 text-center ${isToday ? 'bg-primary-50 ring-1 ring-primary-300 dark:bg-primary-900/20' : ''}`}
              >
                <div className="text-sm">{date.getDate()}</div>
                {total !== 0 && (
                  <div className={`text-[10px] font-medium ${total > 0 ? 'text-green-600' : 'text-red-500'}`}>
                    {total > 0 ? '+' : ''}{formatCurrency(total)}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </Card>
    </div>
  );
}
