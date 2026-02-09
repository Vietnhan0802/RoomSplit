import type { ExpenseCategory, TransactionCategory, TaskFrequency } from '../types';

export const EXPENSE_CATEGORIES: { value: number; label: string; key: ExpenseCategory }[] = [
  { value: 0, label: 'Tiền trọ', key: 'Rent' },
  { value: 1, label: 'Điện', key: 'Electricity' },
  { value: 2, label: 'Nước', key: 'Water' },
  { value: 3, label: 'Internet', key: 'Internet' },
  { value: 4, label: 'Đi chợ', key: 'Groceries' },
  { value: 5, label: 'Vệ sinh', key: 'Cleaning' },
  { value: 6, label: 'Sửa chữa', key: 'Maintenance' },
  { value: 7, label: 'Khác', key: 'Other' },
];

export const TRANSACTION_CATEGORIES: { value: number; label: string; key: TransactionCategory }[] = [
  { value: 0, label: 'Lương', key: 'Salary' },
  { value: 1, label: 'Ăn uống', key: 'Food' },
  { value: 2, label: 'Di chuyển', key: 'Transport' },
  { value: 3, label: 'Mua sắm', key: 'Shopping' },
  { value: 4, label: 'Giải trí', key: 'Entertainment' },
  { value: 5, label: 'Sức khỏe', key: 'Health' },
  { value: 6, label: 'Giáo dục', key: 'Education' },
  { value: 7, label: 'Hóa đơn', key: 'Bills' },
  { value: 8, label: 'Chi phí phòng', key: 'RoomExpense' },
  { value: 9, label: 'Tiết kiệm', key: 'Savings' },
  { value: 10, label: 'Khác', key: 'Other' },
];

export const TASK_FREQUENCIES: { value: number; label: string; key: TaskFrequency }[] = [
  { value: 0, label: 'Hàng ngày', key: 'Daily' },
  { value: 1, label: 'Hàng tuần', key: 'Weekly' },
  { value: 2, label: '2 tuần/lần', key: 'BiWeekly' },
  { value: 3, label: 'Hàng tháng', key: 'Monthly' },
];

export const SPLIT_TYPES = [
  { value: 0, label: 'Chia đều' },
  { value: 1, label: 'Theo phần trăm' },
  { value: 2, label: 'Tùy chỉnh' },
];

export const TASK_TEMPLATES = [
  'Quét nhà',
  'Lau nhà',
  'Rửa bát',
  'Đổ rác',
  'Giặt đồ chung',
  'Dọn nhà vệ sinh',
  'Đi chợ',
  'Nấu ăn',
];

export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  FORGOT_PASSWORD: '/forgot-password',
  DASHBOARD: '/dashboard',
  ROOMS: '/rooms',
  ROOM_DETAIL: '/rooms/:roomId',
  ROOM_EXPENSES: '/rooms/:roomId/expenses',
  ROOM_TASKS: '/rooms/:roomId/tasks',
  ROOM_MEMBERS: '/rooms/:roomId/members',
  FINANCE: '/finance',
  TRANSACTIONS: '/finance/transactions',
  CALENDAR: '/finance/calendar',
  BUDGETS: '/finance/budgets',
  REPORTS: '/reports',
} as const;

export const CATEGORY_COLORS: Record<string, string> = {
  Rent: '#ef4444',
  Electricity: '#f59e0b',
  Water: '#3b82f6',
  Internet: '#8b5cf6',
  Groceries: '#10b981',
  Cleaning: '#06b6d4',
  Maintenance: '#f97316',
  Other: '#6b7280',
  Salary: '#10b981',
  Food: '#f59e0b',
  Transport: '#3b82f6',
  Shopping: '#ec4899',
  Entertainment: '#8b5cf6',
  Health: '#ef4444',
  Education: '#06b6d4',
  Bills: '#f97316',
  RoomExpense: '#6366f1',
  Savings: '#14b8a6',
};
