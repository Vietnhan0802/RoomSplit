import apiClient from './client';
import type { ApiResponse, Transaction, Budget } from '../types';

export const financeApi = {
  getTransactions: (month?: number, year?: number) =>
    apiClient.get<ApiResponse<Transaction[]>>('/transactions', {
      params: { month, year },
    }),

  createTransaction: (data: {
    type: number;
    amount: number;
    category: number;
    description?: string;
    transactionDate: string;
    note?: string;
  }) => apiClient.post<ApiResponse<Transaction>>('/transactions', data),

  deleteTransaction: (id: string) =>
    apiClient.delete(`/transactions/${id}`),

  getBudgets: (month: number, year: number) =>
    apiClient.get<ApiResponse<Budget[]>>('/budgets', {
      params: { month, year },
    }),

  createBudget: (data: {
    category: number;
    limitAmount: number;
    month: number;
    year: number;
  }) => apiClient.post<ApiResponse<Budget>>('/budgets', data),
};
