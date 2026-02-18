import { useState } from 'react';
import { Download, Share, X } from 'lucide-react';
import { useInstallPrompt } from '../../hooks/usePWA';
import Button from '../ui/Button';

const DISMISSED_KEY = 'pwa-install-dismissed';

export default function InstallPWA() {
  const { canInstall, showIOSGuide, promptInstall } = useInstallPrompt();
  const [dismissed, setDismissed] = useState(
    () => sessionStorage.getItem(DISMISSED_KEY) === '1'
  );
  const [showGuide, setShowGuide] = useState(false);

  if (dismissed) return null;

  const handleDismiss = () => {
    setDismissed(true);
    sessionStorage.setItem(DISMISSED_KEY, '1');
  };

  // Android / Desktop — native install prompt
  if (canInstall) {
    return (
      <div className="fixed bottom-20 left-4 right-4 z-50 mx-auto max-w-sm animate-slide-up rounded-2xl bg-white p-4 shadow-xl dark:bg-slate-800 lg:bottom-6 lg:left-auto lg:right-6">
        <button
          onClick={handleDismiss}
          className="absolute right-3 top-3 rounded-lg p-1 hover:bg-gray-100 dark:hover:bg-slate-700"
        >
          <X className="h-4 w-4 text-gray-400" />
        </button>

        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-xl bg-primary-100 dark:bg-primary-900/30">
            <Download className="h-5 w-5 text-primary-600" />
          </div>
          <div className="flex-1">
            <p className="text-sm font-semibold">Cài đặt RoomSplit</p>
            <p className="mt-0.5 text-xs text-gray-500">
              Thêm vào màn hình chính để truy cập nhanh hơn
            </p>
          </div>
        </div>

        <div className="mt-3 flex gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={handleDismiss}
            className="flex-1"
          >
            Để sau
          </Button>
          <Button
            size="sm"
            onClick={promptInstall}
            className="flex-1"
          >
            Cài đặt
          </Button>
        </div>
      </div>
    );
  }

  // iOS — show manual guide
  if (showIOSGuide) {
    return (
      <>
        {!showGuide && (
          <button
            onClick={() => setShowGuide(true)}
            className="fixed bottom-20 right-4 z-50 flex items-center gap-2 rounded-full bg-primary-600 px-4 py-2.5 text-sm font-medium text-white shadow-lg animate-slide-up lg:bottom-6"
          >
            <Download className="h-4 w-4" />
            Cài ứng dụng
          </button>
        )}

        {showGuide && (
          <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/50 animate-fade-in">
            <div className="w-full max-w-sm rounded-t-2xl bg-white p-6 animate-slide-up dark:bg-slate-800">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-semibold">Cài đặt RoomSplit</h3>
                <button
                  onClick={() => {
                    setShowGuide(false);
                    handleDismiss();
                  }}
                  className="rounded-lg p-1 hover:bg-gray-100 dark:hover:bg-slate-700"
                >
                  <X className="h-5 w-5 text-gray-400" />
                </button>
              </div>

              <div className="mt-4 space-y-4">
                <div className="flex items-center gap-3">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-sm font-bold text-primary-600 dark:bg-primary-900/30">
                    1
                  </span>
                  <p className="text-sm">
                    Nhấn nút{' '}
                    <Share className="inline h-4 w-4 text-primary-600" />{' '}
                    <span className="font-medium">Chia sẻ</span> ở thanh dưới
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-sm font-bold text-primary-600 dark:bg-primary-900/30">
                    2
                  </span>
                  <p className="text-sm">
                    Cuộn xuống và nhấn{' '}
                    <span className="font-medium">"Thêm vào MH chính"</span>
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-sm font-bold text-primary-600 dark:bg-primary-900/30">
                    3
                  </span>
                  <p className="text-sm">
                    Nhấn <span className="font-medium">"Thêm"</span> ở góc trên bên phải
                  </p>
                </div>
              </div>

              <Button
                onClick={() => {
                  setShowGuide(false);
                  handleDismiss();
                }}
                className="mt-5 w-full"
              >
                Đã hiểu
              </Button>
            </div>
          </div>
        )}
      </>
    );
  }

  return null;
}
