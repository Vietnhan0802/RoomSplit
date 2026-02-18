// Auth
export interface User {
  id: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  phone?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

// Room
export interface Room {
  id: string;
  name: string;
  description?: string;
  inviteCode: string;
  createdBy: User;
  memberCount: number;
  createdAt: string;
}

export interface RoomDetail {
  id: string;
  name: string;
  description?: string;
  inviteCode: string;
  createdBy: User;
  members: RoomMember[];
  createdAt: string;
}

export interface RoomMember {
  id: string;
  user: User;
  role: 'Admin' | 'Member';
  joinedAt: string;
}

// Expense
export type ExpenseCategory =
  | 'Rent'
  | 'Electricity'
  | 'Water'
  | 'Internet'
  | 'Groceries'
  | 'Cleaning'
  | 'Maintenance'
  | 'Other';

export type SplitType = 'Equal' | 'Percentage' | 'Custom';

export interface Expense {
  id: string;
  description: string;
  amount: number;
  category: ExpenseCategory;
  splitType: SplitType;
  expenseDate: string;
  receiptUrl?: string;
  note?: string;
  isSettled: boolean;
  paidBy: User;
  splits: ExpenseSplit[];
  createdAt: string;
}

export interface ExpenseSplit {
  id: string;
  user: User;
  amount: number;
  percentage?: number;
  isPaid: boolean;
}

// Task
export type TaskFrequency = 'Daily' | 'Weekly' | 'BiWeekly' | 'Monthly';
export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Skipped';

export interface RoomTask {
  id: string;
  title: string;
  description?: string;
  frequency: TaskFrequency;
  isRotating: boolean;
  currentRotationIndex: number;
  assignments: TaskAssignment[];
  createdAt: string;
}

export interface TaskAssignment {
  id: string;
  assignedTo: User;
  dueDate: string;
  status: TaskStatus;
  completedAt?: string;
}

// Transaction
export type TransactionType = 'Income' | 'Expense';

export type ExpenseCategoryKey =
  | 'Food' | 'Transport' | 'Entertainment' | 'Shopping'
  | 'Health' | 'Education' | 'Sports' | 'Bills'
  | 'Rent' | 'Coffee' | 'PersonalCare' | 'Other';

export type IncomeCategoryKey =
  | 'Salary' | 'Bonus' | 'Freelance' | 'Gift'
  | 'Refund' | 'Investment' | 'Other';

export type TransactionCategory = ExpenseCategoryKey | IncomeCategoryKey;

export interface TransactionImage {
  id: string;
  imageUrl: string;
  thumbnailUrl?: string;
  originalFileName: string;
  fileSize: number;
  uploadedAt: string;
}

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  description: string;
  incomeCategory?: string;
  expenseCategory?: string;
  date: string;
  imageUrl?: string;
  note?: string;
  tags?: string;
  createdAt: string;
  images: TransactionImage[];
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CalendarDay {
  date: string;
  totalIncome: number;
  totalExpense: number;
  transactionCount: number;
  hasImages: boolean;
  transactions: Transaction[];
}

export interface MonthSummary {
  totalIncome: number;
  totalExpense: number;
  netAmount: number;
  transactionCount: number;
}

export interface CalendarResponse {
  days: CalendarDay[];
  monthSummary: MonthSummary;
}

export interface CategorySummary {
  category: string;
  amount: number;
  count: number;
  percentage: number;
}

export interface DailyTrend {
  date: string;
  income: number;
  expense: number;
}

export interface Comparison {
  incomeChangePercent: number;
  expenseChangePercent: number;
}

export interface SummaryResponse {
  totalIncome: number;
  totalExpense: number;
  netAmount: number;
  byCategory: CategorySummary[];
  dailyTrend: DailyTrend[];
  topExpenses: Transaction[];
  comparedToLastMonth: Comparison;
}

// Budget
export interface Budget {
  id: string;
  expenseCategory: string;
  monthlyLimit: number;
  spentAmount: number;
  month: number;
  year: number;
}

export interface BudgetStatus {
  id: string;
  expenseCategory: string;
  monthlyLimit: number;
  spentAmount: number;
  month: number;
  year: number;
  dailyAverageSpent: number;
  projectedMonthEnd: number;
  isOverBudget: boolean;
  remainingAmount: number;
  percentUsed: number;
}

// Reports
export interface ReportOverview {
  totalIncome: number;
  totalExpense: number;
  netSavings: number;
  savingsPercent: number;
  prevMonthIncome: number;
  prevMonthExpense: number;
  incomeChangePercent: number;
  expenseChangePercent: number;
  topExpenses: Transaction[];
}

export interface CategoryBreakdownItem {
  category: string;
  amount: number;
  count: number;
  percentage: number;
  transactions: Transaction[];
}

export interface MonthlyTrendItem {
  month: number;
  year: number;
  income: number;
  expense: number;
  savings: number;
}

export interface DailySpendingItem {
  date: string;
  amount: number;
}

export interface DailySpendingResponse {
  days: DailySpendingItem[];
  dailyAverage: number;
  maxDay: string | null;
  maxAmount: number;
}

export interface CategoryComparisonItem {
  category: string;
  prevMonthAmount: number;
  currentMonthAmount: number;
  amountChange: number;
  percentChange: number;
}

// API
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}
