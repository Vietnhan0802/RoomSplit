import { useNavigate } from 'react-router-dom';
import Button from '../components/ui/Button';

export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-4 text-center dark:bg-slate-900">
      <span className="text-8xl">🔍</span>
      <h1 className="mt-6 text-4xl font-bold text-gray-900 dark:text-gray-100">404</h1>
      <p className="mt-2 text-lg text-gray-600 dark:text-gray-400">
        Trang không tồn tại
      </p>
      <p className="mt-1 text-sm text-gray-500 dark:text-gray-500">
        Trang bạn đang tìm không có hoặc đã bị di chuyển.
      </p>
      <Button onClick={() => navigate('/dashboard')} className="mt-6">
        Quay về trang chủ
      </Button>
    </div>
  );
}
