import apiClient from './client';
import type {
  ApiResponse,
  Transaction,
  Budget,
  BudgetStatus,
  PaginatedResponse,
  CalendarResponse,
  SummaryResponse,
  ReportOverview,
  CategoryBreakdownItem,
  MonthlyTrendItem,
  DailySpendingResponse,
  CategoryComparisonItem,
} from '../types';

export interface TransactionQueryParams {
  type?: number;
  startDate?: string;
  endDate?: string;
  category?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  order?: string;
}

export const financeApi = {
  // Transactions
  getTransactions: (params: TransactionQueryParams = {}) =>
    apiClient.get<ApiResponse<PaginatedResponse<Transaction>>>('/transactions', { params }),

  getTransaction: (id: string) =>
    apiClient.get<ApiResponse<Transaction>>(`/transactions/${id}`),

  createTransaction: (data: FormData) =>
    apiClient.post<ApiResponse<Transaction>>('/transactions', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  updateTransaction: (id: string, data: FormData) =>
    apiClient.put<ApiResponse<Transaction>>(`/transactions/${id}`, data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  deleteTransaction: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/transactions/${id}`),

  getCalendar: (month: number, year: number) =>
    apiClient.get<ApiResponse<CalendarResponse>>('/transactions/calendar', {
      params: { month, year },
    }),

  getSummary: (month: number, year: number) =>
    apiClient.get<ApiResponse<SummaryResponse>>('/transactions/summary', {
      params: { month, year },
    }),

  // Budgets
  getBudgets: (month: number, year: number) =>
    apiClient.get<ApiResponse<Budget[]>>('/budgets', {
      params: { month, year },
    }),

  getBudgetStatus: (month: number, year: number) =>
    apiClient.get<ApiResponse<BudgetStatus[]>>('/budgets/status', {
      params: { month, year },
    }),

  createBudget: (data: {
    expenseCategory: number;
    monthlyLimit: number;
    month: number;
    year: number;
  }) => apiClient.post<ApiResponse<Budget>>('/budgets', data),

  updateBudget: (id: string, data: {
    expenseCategory: number;
    monthlyLimit: number;
  }) => apiClient.put<ApiResponse<Budget>>(`/budgets/${id}`, data),

  deleteBudget: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/budgets/${id}`),

  // Reports
  getReportOverview: (month: number, year: number) =>
    apiClient.get<ApiResponse<ReportOverview>>('/reports/overview', { params: { month, year } }),

  getCategoryBreakdown: (month: number, year: number) =>
    apiClient.get<ApiResponse<CategoryBreakdownItem[]>>('/reports/category-breakdown', { params: { month, year } }),

  getMonthlyTrend: (months: number = 6) =>
    apiClient.get<ApiResponse<MonthlyTrendItem[]>>('/reports/monthly-trend', { params: { months } }),

  getDailySpending: (month: number, year: number) =>
    apiClient.get<ApiResponse<DailySpendingResponse>>('/reports/daily-spending', { params: { month, year } }),

  getCategoryComparison: (month: number, year: number) =>
    apiClient.get<ApiResponse<CategoryComparisonItem[]>>('/reports/comparison', { params: { month, year } }),
};
