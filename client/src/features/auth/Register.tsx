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
  fullName: z.string().min(1, 'Vui lòng nhập họ tên').max(100),
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
  phone: z.string().optional(),
});

type RegisterData = z.infer<typeof schema>;

export default function Register() {
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const { register, handleSubmit, formState: { errors } } = useForm<RegisterData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: RegisterData) => {
    try {
      setIsLoading(true);
      const res = await authApi.register(data);
      if (res.data.data) {
        login(res.data.data.token, res.data.data.user);
        navigate(ROUTES.DASHBOARD);
        showToast('success', 'Đăng ký thành công!');
      }
    } catch {
      showToast('error', 'Email đã được sử dụng');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 dark:bg-gray-900">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold text-primary-600">RoomSplit</h1>
          <p className="mt-2 text-gray-500">Tạo tài khoản mới</p>
        </div>

        <div className="card">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input label="Họ tên" {...register('fullName')} error={errors.fullName?.message} />
            <Input label="Email" type="email" {...register('email')} error={errors.email?.message} />
            <Input label="Số điện thoại" type="tel" {...register('phone')} />
            <Input label="Mật khẩu" type="password" {...register('password')} error={errors.password?.message} />

            <Button type="submit" className="w-full" isLoading={isLoading}>
              Đăng ký
            </Button>
          </form>

          <div className="mt-4 text-center text-sm text-gray-500">
            Đã có tài khoản?{' '}
            <Link to={ROUTES.LOGIN} className="text-primary-600 hover:underline">
              Đăng nhập
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
