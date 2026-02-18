import { useNavigate } from 'react-router-dom';
import Card from '../../components/ui/Card';
import { formatCurrency } from '../../utils/formatCurrency';
import { ROUTES } from '../../constants';
import type { CalendarDay } from '../../types';

const DAYS_SHORT = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

interface MiniCalendarProps {
  calendarDays: CalendarDay[];
}

export default function MiniCalendar({ calendarDays }: MiniCalendarProps) {
  const navigate = useNavigate();
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // Get current week (Mon-Sun)
  const dayOfWeek = today.getDay();
  const mondayOffset = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
  const monday = new Date(today);
  monday.setDate(today.getDate() + mondayOffset);

  const weekDays: { date: Date; data?: CalendarDay }[] = [];
  for (let i = 0; i < 7; i++) {
    const d = new Date(monday);
    d.setDate(monday.getDate() + i);
    const dayData = calendarDays.find((cd) => {
      const cdDate = new Date(cd.date);
      return cdDate.getDate() === d.getDate() &&
        cdDate.getMonth() === d.getMonth() &&
        cdDate.getFullYear() === d.getFullYear();
    });
    weekDays.push({ date: d, data: dayData });
  }

  return (
    <Card onClick={() => navigate(ROUTES.CALENDAR)} className="cursor-pointer">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="font-semibold">Tuần này</h2>
        <span className="text-sm text-primary-600">Xem lịch</span>
      </div>
      <div className="grid grid-cols-7 gap-1 text-center">
        {weekDays.map(({ date, data }) => {
          const isToday = date.getTime() === today.getTime();
          const expense = data?.totalExpense ?? 0;

          return (
            <div
              key={date.toISOString()}
              className={`rounded-lg p-2 ${isToday ? 'bg-primary-100 dark:bg-primary-900/30 ring-2 ring-primary-500' : ''}`}
            >
              <p className="text-xs text-gray-500 dark:text-gray-400">{DAYS_SHORT[date.getDay()]}</p>
              <p className={`text-sm font-medium ${isToday ? 'text-primary-700 dark:text-primary-300' : ''}`}>
                {date.getDate()}
              </p>
              {expense > 0 && (
                <p className="mt-1 text-[10px] text-red-500">
                  -{formatCurrency(expense).replace('₫', '').trim()}
                </p>
              )}
            </div>
          );
        })}
      </div>
    </Card>
  );
}
