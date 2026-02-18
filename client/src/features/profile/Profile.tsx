import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import { useAuth } from '../../hooks/useAuth';
import { showToast } from '../../components/ui/showToast';

const schema = z.object({
  fullName: z.string().min(1, 'Vui lòng nhập họ tên').max(100),
});

type ProfileData = z.infer<typeof schema>;

export default function Profile() {
  const { user, updateProfile, refreshUser } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);

  const { register, handleSubmit, formState: { errors } } = useForm<ProfileData>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: user?.fullName || '',
    },
  });

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setAvatarFile(file);
      setAvatarPreview(URL.createObjectURL(file));
    }
  };

  const onSubmit = async (data: ProfileData) => {
    try {
      setIsLoading(true);
      await updateProfile(data.fullName, avatarFile || undefined);
      await refreshUser();
      showToast('success', 'Cập nhật hồ sơ thành công!');
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      const message = err.response?.data?.message || 'Có lỗi xảy ra';
      showToast('error', message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Hồ sơ cá nhân</h1>

      <div className="card">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Avatar Upload */}
          <div>
            <label className="block text-sm font-medium mb-2">Ảnh đại diện</label>
            <div className="flex items-center gap-4">
              <img
                src={avatarPreview || user?.avatarUrl || '/default-avatar.png'}
                alt="Avatar"
                className="w-20 h-20 rounded-full object-cover"
              />
              <input
                type="file"
                accept="image/*"
                onChange={handleAvatarChange}
                className="block w-full text-sm"
              />
            </div>
          </div>

          <Input
            label="Họ tên"
            {...register('fullName')}
            error={errors.fullName?.message}
          />

          <Input
            label="Email"
            value={user?.email}
            disabled
          />

          <Button type="submit" className="w-full" isLoading={isLoading}>
            Lưu thay đổi
          </Button>
        </form>
      </div>
    </div>
  );
}
