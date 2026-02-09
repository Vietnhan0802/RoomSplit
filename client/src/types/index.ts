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

export type TransactionCategory =
  | 'Salary'
  | 'Food'
  | 'Transport'
  | 'Shopping'
  | 'Entertainment'
  | 'Health'
  | 'Education'
  | 'Bills'
  | 'RoomExpense'
  | 'Savings'
  | 'Other';

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  category: TransactionCategory;
  description?: string;
  transactionDate: string;
  note?: string;
  createdAt: string;
}

// Budget
export interface Budget {
  id: string;
  category: TransactionCategory;
  limitAmount: number;
  spentAmount: number;
  month: number;
  year: number;
}

// API
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}
