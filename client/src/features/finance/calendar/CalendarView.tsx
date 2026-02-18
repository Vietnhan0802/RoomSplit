import { useState, useEffect, useCallback } from 'react';
import { Camera, Plus, ChevronDown } from 'lucide-react';
import Card from '../../../components/ui/Card';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import ImageViewer from '../../../components/shared/ImageViewer';
import AddTransactionSheet from '../transactions/AddTransactionSheet';
import { financeApi } from '../../../api/finance';
import { useFinance } from '../FinanceContext';
import { getDaysInMonth, isSameDay } from '../../../utils/dateHelpers';
import { formatCurrency } from '../../../utils/formatCurrency';
import { cn } from '../../../utils/cn';
import type { CalendarDay, Transaction } from '../../../types';

function formatShort(amount: number): string {
  if (amount >= 1_000_000) return `${(amount / 1_000_000).toFixed(amount % 1_000_000 === 0 ? 0 : 1)}tr`;
  if (amount >= 1_000) return `${Math.round(amount / 1_000)}k`;
  return amount.toString();
}

export default function CalendarView() {
  const { month, year, refreshKey, triggerRefresh } = useFinance();
  const [calendarDays, setCalendarDays] = useState<CalendarDay[]>([]);
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Image viewer state
  const [viewerImages, setViewerImages] = useState<{ url: string; name?: string }[]>([]);
  const [viewerIndex, setViewerIndex] = useState(0);
  const [viewerOpen, setViewerOpen] = useState(false);

  // Add transaction for specific day
  const [addForDate, setAddForDate] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    financeApi.getCalendar(month, year)
      .then((res) => {
        if (!cancelled && res.data.data) {
          setCalendarDays(res.data.data.days);
        }
      })
      .catch(() => { if (!cancelled) setCalendarDays([]); })
      .finally(() => { if (!cancelled) setIsLoading(false); });
    return () => { cancelled = true; };
  }, [month, year, refreshKey]);

  const days = getDaysInMonth(month, year);
  const weekDays = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

  // Monday = 0 for the grid
  const firstDay = days[0].getDay();
  const emptyDays = firstDay === 0 ? 6 : firstDay - 1;

  const getDayData = useCallback(
    (date: Date): CalendarDay | undefined => {
      const dateStr = date.toISOString().split('T')[0];
      return calendarDays.find((d) => d.date.split('T')[0] === dateStr);
    },
    [calendarDays]
  );

  const selectedDayData = selectedDate
    ? calendarDays.find((d) => d.date.split('T')[0] === selectedDate)
    : null;

  const handleDayClick = (date: Date) => {
    const dateStr = date.toISOString().split('T')[0];
    setSelectedDate((prev) => (prev === dateStr ? null : dateStr));
  };

  const handleThumbnailClick = (tx: Transaction, imgIndex: number) => {
    const imgs = tx.images.map((img) => ({
      url: `/uploads${img.imageUrl}`,
      name: img.originalFileName,
    }));
    setViewerImages(imgs);
    setViewerIndex(imgIndex);
    setViewerOpen(true);
  };

  if (isLoading) {
    return <p className="py-8 text-center text-gray-400">Đang tải...</p>;
  }

  return (
    <div className="space-y-3">
      {/* Calendar grid */}
      <Card>
        <div className="grid grid-cols-7 gap-px">
          {/* Week day headers */}
          {weekDays.map((d) => (
            <div
              key={d}
              className={cn(
                'py-2 text-center text-xs font-medium',
                d === 'CN' ? 'text-red-500' : 'text-gray-500'
              )}
            >
              {d}
            </div>
          ))}

          {/* Empty cells */}
          {Array.from({ length: emptyDays }).map((_, i) => (
            <div key={`e-${i}`} className="min-h-[60px]" />
          ))}

          {/* Day cells */}
          {days.map((date) => {
            const dayData = getDayData(date);
            const isToday = isSameDay(date, new Date());
            const dateStr = date.toISOString().split('T')[0];
            const isSelected = selectedDate === dateStr;
            const isSunday = date.getDay() === 0;

            return (
              <button
                key={dateStr}
                type="button"
                onClick={() => handleDayClick(date)}
                className={cn(
                  'relative flex min-h-[60px] flex-col items-center rounded-lg p-1 transition-all text-left',
                  isToday && 'ring-2 ring-primary-400 bg-primary-50 dark:bg-primary-900/20',
                  isSelected && !isToday && 'bg-gray-100 dark:bg-gray-700',
                  !isToday && !isSelected && 'hover:bg-gray-50 dark:hover:bg-gray-800'
                )}
              >
                <span
                  className={cn(
                    'text-sm font-medium',
                    isToday && 'text-primary-600 dark:text-primary-400',
                    isSunday && !isToday && 'text-red-500'
                  )}
                >
                  {date.getDate()}
                </span>
                {dayData && dayData.totalIncome > 0 && (
                  <span className="text-[9px] font-medium text-green-600 leading-tight">
                    +{formatShort(dayData.totalIncome)}
                  </span>
                )}
                {dayData && dayData.totalExpense > 0 && (
                  <span className="text-[9px] font-medium text-red-500 leading-tight">
                    -{formatShort(dayData.totalExpense)}
                  </span>
                )}
                {dayData?.hasImages && (
                  <Camera className="h-2.5 w-2.5 text-gray-400 mt-0.5" />
                )}
              </button>
            );
          })}
        </div>
      </Card>

      {/* Selected day detail panel */}
      {selectedDate && (
        <Card className="animate-in slide-in-from-top-2">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold">
              {new Date(selectedDate + 'T00:00:00').toLocaleDateString('vi-VN', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
              })}
            </h3>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setAddForDate(selectedDate)}
                className="flex items-center gap-1 rounded-lg bg-primary-50 px-2 py-1 text-xs font-medium text-primary-600 hover:bg-primary-100 dark:bg-primary-900/20 dark:text-primary-400"
              >
                <Plus className="h-3 w-3" />
                Thêm
              </button>
              <button
                onClick={() => setSelectedDate(null)}
                className="rounded-lg p-1 text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                <ChevronDown className="h-4 w-4" />
              </button>
            </div>
          </div>

          {!selectedDayData || selectedDayData.transactions.length === 0 ? (
            <p className="py-4 text-center text-sm text-gray-400">
              Chưa có giao dịch
            </p>
          ) : (
            <div className="space-y-2">
              {/* Day totals */}
              <div className="flex gap-3 text-xs">
                {selectedDayData.totalIncome > 0 && (
                  <span className="text-green-600">
                    Thu: {formatCurrency(selectedDayData.totalIncome)}
                  </span>
                )}
                {selectedDayData.totalExpense > 0 && (
                  <span className="text-red-500">
                    Chi: {formatCurrency(selectedDayData.totalExpense)}
                  </span>
                )}
              </div>

              {/* Transaction list */}
              <div className="divide-y divide-gray-100 dark:divide-gray-700">
                {selectedDayData.transactions.map((tx) => (
                  <div key={tx.id} className="flex items-center gap-3 py-2">
                    <CategoryIcon
                      category={tx.expenseCategory || tx.incomeCategory || 'Other'}
                      size="sm"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium truncate">
                        {tx.description}
                      </p>
                      {tx.note && (
                        <p className="text-xs text-gray-400 truncate">{tx.note}</p>
                      )}
                    </div>

                    {/* Thumbnail */}
                    {tx.images?.length > 0 && (
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleThumbnailClick(tx, 0);
                        }}
                        className="h-8 w-8 flex-shrink-0 overflow-hidden rounded"
                      >
                        <img
                          src={`/uploads${tx.images[0].thumbnailUrl || tx.images[0].imageUrl}`}
                          alt=""
                          className="h-full w-full object-cover"
                        />
                      </button>
                    )}

                    <span
                      className={cn(
                        'text-sm font-semibold flex-shrink-0',
                        tx.type === 'Income' ? 'text-green-600' : 'text-red-500'
                      )}
                    >
                      {tx.type === 'Income' ? '+' : '-'}
                      {formatCurrency(tx.amount)}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </Card>
      )}

      {/* Image viewer */}
      <ImageViewer
        images={viewerImages}
        initialIndex={viewerIndex}
        isOpen={viewerOpen}
        onClose={() => setViewerOpen(false)}
      />

      {/* Add transaction for specific day */}
      <AddTransactionSheet
        isOpen={!!addForDate}
        onClose={() => setAddForDate(null)}
        onSuccess={() => {
          setAddForDate(null);
          triggerRefresh();
        }}
        initialDate={addForDate || undefined}
      />
    </div>
  );
}
