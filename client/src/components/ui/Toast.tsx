import { useEffect, useState } from 'react';
import { CheckCircle, XCircle, AlertCircle, X } from 'lucide-react';
import { cn } from '../../utils/cn';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  message: string;
}

let toastListeners: ((toast: ToastMessage) => void)[] = [];

export function showToast(type: ToastMessage['type'], message: string) {
  const toast: ToastMessage = { id: Date.now().toString(), type, message };
  toastListeners.forEach((fn) => fn(toast));
}

const DISMISS_MS = 3000;

export default function ToastContainer() {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  useEffect(() => {
    const listener = (toast: ToastMessage) => {
      setToasts((prev) => [...prev, toast]);
      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== toast.id));
      }, DISMISS_MS);
    };
    toastListeners.push(listener);
    return () => {
      toastListeners = toastListeners.filter((fn) => fn !== listener);
    };
  }, []);

  const config = {
    success: {
      icon: <CheckCircle className="h-5 w-5 text-success-500" />,
      bar: 'bg-success-500',
    },
    error: {
      icon: <XCircle className="h-5 w-5 text-danger-500" />,
      bar: 'bg-danger-500',
    },
    info: {
      icon: <AlertCircle className="h-5 w-5 text-primary-500" />,
      bar: 'bg-primary-500',
    },
  };

  return (
    <div className="fixed left-1/2 top-4 z-[100] flex w-full max-w-sm -translate-x-1/2 flex-col gap-2 px-4 sm:left-auto sm:right-4 sm:translate-x-0 sm:px-0">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={cn(
            'relative overflow-hidden rounded-xl bg-white p-4 shadow-lg animate-slide-down dark:bg-slate-800',
          )}
        >
          <div className="flex items-center gap-3">
            {config[toast.type].icon}
            <p className="flex-1 text-sm font-medium">{toast.message}</p>
            <button
              onClick={() => setToasts((prev) => prev.filter((t) => t.id !== toast.id))}
              className="rounded-lg p-0.5 hover:bg-gray-100 dark:hover:bg-gray-700"
            >
              <X className="h-4 w-4 text-gray-400" />
            </button>
          </div>
          <div className="absolute bottom-0 left-0 right-0 h-0.5">
            <div className={cn('h-full animate-progress-shrink', config[toast.type].bar)} />
          </div>
        </div>
      ))}
    </div>
  );
}
