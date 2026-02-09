import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import { useAuth } from '../../hooks/useAuth';
import { authApi } from '../../api/auth';
import { showToast } from '../../components/ui/Toast';
import { ROUTES } from '../../constants';

const schema = z.object({
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(1, 'Vui lòng nhập mật khẩu'),
});

type LoginData = z.infer<typeof schema>;

export default function Login() {
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const { register, handleSubmit, formState: { errors } } = useForm<LoginData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: LoginData) => {
    try {
      setIsLoading(true);
      const res = await authApi.login(data);
      if (res.data.data) {
        login(res.data.data.token, res.data.data.user);
        navigate(ROUTES.DASHBOARD);
      }
    } catch {
      showToast('error', 'Email hoặc mật khẩu không đúng');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 dark:bg-gray-900">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold text-primary-600">RoomSplit</h1>
          <p className="mt-2 text-gray-500">Đăng nhập vào tài khoản</p>
        </div>

        <div className="card">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input label="Email" type="email" {...register('email')} error={errors.email?.message} />
            <Input label="Mật khẩu" type="password" {...register('password')} error={errors.password?.message} />

            <Button type="submit" className="w-full" isLoading={isLoading}>
              Đăng nhập
            </Button>
          </form>

          <div className="mt-4 text-center text-sm">
            <Link to={ROUTES.FORGOT_PASSWORD} className="text-primary-600 hover:underline">
              Quên mật khẩu?
            </Link>
          </div>
          <div className="mt-2 text-center text-sm text-gray-500">
            Chưa có tài khoản?{' '}
            <Link to={ROUTES.REGISTER} className="text-primary-600 hover:underline">
              Đăng ký
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
