import apiClient from './client';
import type { ApiResponse, Room, Expense, RoomTask } from '../types';

export const roomsApi = {
  getMyRooms: () => apiClient.get<ApiResponse<Room[]>>('/rooms'),

  createRoom: (data: { name: string; description?: string }) =>
    apiClient.post<ApiResponse<Room>>('/rooms', data),

  joinRoom: (inviteCode: string) =>
    apiClient.post<ApiResponse<Room>>(`/rooms/join/${inviteCode}`),

  // Expenses
  getExpenses: (roomId: string) =>
    apiClient.get<ApiResponse<Expense[]>>(`/rooms/${roomId}/expenses`),

  createExpense: (roomId: string, data: {
    description: string;
    amount: number;
    category: number;
    splitType: number;
    expenseDate: string;
    note?: string;
    splits?: { userId: string; amount?: number; percentage?: number }[];
  }) => apiClient.post<ApiResponse<Expense>>(`/rooms/${roomId}/expenses`, data),

  // Tasks
  getTasks: (roomId: string) =>
    apiClient.get<ApiResponse<RoomTask[]>>(`/rooms/${roomId}/tasks`),

  createTask: (roomId: string, data: {
    title: string;
    description?: string;
    frequency: number;
    isRotating: boolean;
  }) => apiClient.post<ApiResponse<RoomTask>>(`/rooms/${roomId}/tasks`, data),

  completeTask: (roomId: string, taskId: string) =>
    apiClient.post(`/rooms/${roomId}/tasks/${taskId}/complete`),
};
