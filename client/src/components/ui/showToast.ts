export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  message: string;
}

export const toastListeners: ((toast: ToastMessage) => void)[] = [];

export function showToast(type: ToastMessage['type'], message: string) {
  const toast: ToastMessage = { id: Date.now().toString(), type, message };
  toastListeners.forEach((fn) => fn(toast));
}
