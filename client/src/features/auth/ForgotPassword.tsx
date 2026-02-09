import { Link } from 'react-router-dom';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import { ROUTES } from '../../constants';

export default function ForgotPassword() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 dark:bg-gray-900">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold text-primary-600">RoomSplit</h1>
          <p className="mt-2 text-gray-500">Khôi phục mật khẩu</p>
        </div>

        <div className="card">
          <form className="space-y-4">
            <Input label="Email" type="email" placeholder="Nhập email đã đăng ký" />
            <Button type="submit" className="w-full">
              Gửi link khôi phục
            </Button>
          </form>

          <div className="mt-4 text-center text-sm text-gray-500">
            <Link to={ROUTES.LOGIN} className="text-primary-600 hover:underline">
              Quay lại đăng nhập
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
