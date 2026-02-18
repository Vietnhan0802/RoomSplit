import { RefreshCw } from 'lucide-react';
import { useSWUpdate } from '../../hooks/usePWA';

export default function SWUpdatePrompt() {
  const { needRefresh, updateSW } = useSWUpdate();

  if (!needRefresh) return null;

  return (
    <div className="fixed bottom-20 left-4 right-4 z-50 mx-auto max-w-sm animate-slide-up rounded-2xl bg-white p-4 shadow-xl dark:bg-slate-800 lg:bottom-6 lg:left-auto lg:right-6">
      <div className="flex items-start gap-3">
        <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-xl bg-primary-100 dark:bg-primary-900/30">
          <RefreshCw className="h-5 w-5 text-primary-600" />
        </div>
        <div className="flex-1">
          <p className="text-sm font-semibold">Phiên bản mới</p>
          <p className="mt-0.5 text-xs text-gray-500">
            Có bản cập nhật mới. Tải lại để sử dụng phiên bản mới nhất.
          </p>
        </div>
      </div>

      <div className="mt-3 flex gap-2">
        <button
          onClick={() => window.location.reload()}
          className="flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm font-medium text-gray-600 hover:bg-gray-50 dark:border-slate-600 dark:text-gray-300 dark:hover:bg-slate-700"
        >
          Để sau
        </button>
        <button
          onClick={updateSW}
          className="flex-1 rounded-lg bg-primary-600 px-3 py-2 text-sm font-medium text-white hover:bg-primary-700"
        >
          Cập nhật
        </button>
      </div>
    </div>
  );
}
