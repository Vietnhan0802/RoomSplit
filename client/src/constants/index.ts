import type { ExpenseCategory, TaskFrequency } from '../types';

// Shared expense categories (for room expenses)
export const SHARED_EXPENSE_CATEGORIES: { value: number; label: string; key: ExpenseCategory }[] = [
  { value: 0, label: 'Tiền trọ', key: 'Rent' },
  { value: 1, label: 'Điện', key: 'Electricity' },
  { value: 2, label: 'Nước', key: 'Water' },
  { value: 3, label: 'Internet', key: 'Internet' },
  { value: 4, label: 'Đi chợ', key: 'Groceries' },
  { value: 5, label: 'Vệ sinh', key: 'Cleaning' },
  { value: 6, label: 'Sửa chữa', key: 'Maintenance' },
  { value: 7, label: 'Khác', key: 'Other' },
];

// Keep backward compatibility alias
export const EXPENSE_CATEGORIES = SHARED_EXPENSE_CATEGORIES;

// Personal expense categories (match backend ExpenseCategory enum 0-11)
export const PERSONAL_EXPENSE_CATEGORIES: { value: number; label: string; key: string; icon: string }[] = [
  { value: 0, label: 'Ăn uống', key: 'Food', icon: 'UtensilsCrossed' },
  { value: 1, label: 'Di chuyển', key: 'Transport', icon: 'Car' },
  { value: 2, label: 'Giải trí', key: 'Entertainment', icon: 'Gamepad2' },
  { value: 3, label: 'Mua sắm', key: 'Shopping', icon: 'ShoppingBag' },
  { value: 4, label: 'Sức khỏe', key: 'Health', icon: 'Heart' },
  { value: 5, label: 'Giáo dục', key: 'Education', icon: 'GraduationCap' },
  { value: 6, label: 'Thể thao', key: 'Sports', icon: 'Dumbbell' },
  { value: 7, label: 'Hóa đơn', key: 'Bills', icon: 'Receipt' },
  { value: 8, label: 'Tiền trọ', key: 'Rent', icon: 'Home' },
  { value: 9, label: 'Cà phê', key: 'Coffee', icon: 'Coffee' },
  { value: 10, label: 'Cá nhân', key: 'PersonalCare', icon: 'Sparkles' },
  { value: 11, label: 'Khác', key: 'Other', icon: 'MoreHorizontal' },
];

// Personal income categories (match backend IncomeCategory enum 0-6)
export const INCOME_CATEGORIES: { value: number; label: string; key: string; icon: string }[] = [
  { value: 0, label: 'Lương', key: 'Salary', icon: 'Wallet' },
  { value: 1, label: 'Thưởng', key: 'Bonus', icon: 'Gift' },
  { value: 2, label: 'Freelance', key: 'Freelance', icon: 'Laptop' },
  { value: 3, label: 'Quà tặng', key: 'Gift', icon: 'Heart' },
  { value: 4, label: 'Hoàn tiền', key: 'Refund', icon: 'RotateCcw' },
  { value: 5, label: 'Đầu tư', key: 'Investment', icon: 'TrendingUp' },
  { value: 6, label: 'Khác', key: 'Other', icon: 'MoreHorizontal' },
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
  SUMMARY: '/finance/summary',
  REPORTS: '/reports',
  PROFILE: '/profile',
} as const;

export const CATEGORY_COLORS: Record<string, string> = {
  // Shared expense categories
  Electricity: '#f59e0b',
  Water: '#3b82f6',
  Internet: '#8b5cf6',
  Groceries: '#10b981',
  Cleaning: '#06b6d4',
  Maintenance: '#f97316',
  // Personal expense categories
  Food: '#f59e0b',
  Transport: '#3b82f6',
  Entertainment: '#8b5cf6',
  Shopping: '#ec4899',
  Health: '#ef4444',
  Education: '#06b6d4',
  Sports: '#10b981',
  Bills: '#f97316',
  Rent: '#ef4444',
  Coffee: '#92400e',
  PersonalCare: '#e879f9',
  // Income categories
  Salary: '#10b981',
  Bonus: '#14b8a6',
  Freelance: '#6366f1',
  Gift: '#f472b6',
  Refund: '#a78bfa',
  Investment: '#22c55e',
  // Common
  Other: '#6b7280',
};
