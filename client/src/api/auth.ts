import apiClient from './client';
import type { ApiResponse, AuthResponse, User } from '../types';

export const authApi = {
  register: (data: { fullName: string; email: string; password: string; phone?: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>('/auth/register', data),

  login: (data: { email: string; password: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>('/auth/login', data),

  getMe: () => apiClient.get<ApiResponse<User>>('/auth/me'),
};
