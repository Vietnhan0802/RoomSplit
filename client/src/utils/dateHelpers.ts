import { format, formatDistanceToNow, startOfMonth, endOfMonth, eachDayOfInterval, isSameDay } from 'date-fns';
import { vi } from 'date-fns/locale';

export function formatDate(date: string | Date): string {
  return format(new Date(date), 'dd/MM/yyyy', { locale: vi });
}

export function formatDateTime(date: string | Date): string {
  return format(new Date(date), 'dd/MM/yyyy HH:mm', { locale: vi });
}

export function formatRelative(date: string | Date): string {
  return formatDistanceToNow(new Date(date), { addSuffix: true, locale: vi });
}

export function getMonthRange(month: number, year: number) {
  const start = startOfMonth(new Date(year, month - 1));
  const end = endOfMonth(new Date(year, month - 1));
  return { start, end };
}

export function getDaysInMonth(month: number, year: number): Date[] {
  const { start, end } = getMonthRange(month, year);
  return eachDayOfInterval({ start, end });
}

export { isSameDay };
