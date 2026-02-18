import { useState, useEffect } from 'react';
import { WifiOff, RefreshCw } from 'lucide-react';
import { useOfflineQueue } from '../../hooks/useOfflineQueue';

export default function NetworkStatus() {
  const [isOffline, setIsOffline] = useState(!navigator.onLine);
  const { pendingCount, isSyncing, sync } = useOfflineQueue();

  useEffect(() => {
    const goOffline = () => setIsOffline(true);
    const goOnline = () => setIsOffline(false);

    window.addEventListener('offline', goOffline);
    window.addEventListener('online', goOnline);

    return () => {
      window.removeEventListener('offline', goOffline);
      window.removeEventListener('online', goOnline);
    };
  }, []);

  if (!isOffline && pendingCount === 0) return null;

  // Online but has pending requests — show sync bar
  if (!isOffline && pendingCount > 0) {
    return (
      <div className="fixed left-0 right-0 top-0 z-[200] flex items-center justify-center gap-2 bg-primary-500 px-4 py-2 text-sm font-medium text-white animate-slide-down">
        <RefreshCw className={`h-4 w-4 ${isSyncing ? 'animate-spin' : ''}`} />
        {isSyncing
          ? 'Đang đồng bộ...'
          : `${pendingCount} thay đổi chờ đồng bộ`}
        {!isSyncing && (
          <button
            onClick={sync}
            className="ml-2 rounded-md bg-white/20 px-2 py-0.5 text-xs hover:bg-white/30"
          >
            Đồng bộ
          </button>
        )}
      </div>
    );
  }

  // Offline banner
  return (
    <div className="fixed left-0 right-0 top-0 z-[200] flex items-center justify-center gap-2 bg-warning-500 px-4 py-2 text-sm font-medium text-white animate-slide-down">
      <WifiOff className="h-4 w-4" />
      Mất kết nối mạng
      {pendingCount > 0 && (
        <span className="ml-1 rounded-full bg-white/20 px-2 py-0.5 text-xs">
          {pendingCount} chờ đồng bộ
        </span>
      )}
    </div>
  );
}
