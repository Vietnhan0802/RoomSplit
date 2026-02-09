import apiClient from './client';
import type { ApiResponse } from '../types';

export const filesApi = {
  upload: (file: File, folder: 'receipts' | 'avatars') => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post<ApiResponse<string>>(`/files/upload/${folder}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};
