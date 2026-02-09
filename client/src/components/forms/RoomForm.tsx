import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Button from '../ui/Button';
import Input from '../ui/Input';

const schema = z.object({
  name: z.string().min(1, 'Vui lòng nhập tên phòng').max(100),
  description: z.string().optional(),
});

type RoomFormData = z.infer<typeof schema>;

interface RoomFormProps {
  onSubmit: (data: RoomFormData) => void;
  isLoading?: boolean;
}

export default function RoomForm({ onSubmit, isLoading }: RoomFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<RoomFormData>({
    resolver: zodResolver(schema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input label="Tên phòng" {...register('name')} error={errors.name?.message} placeholder="VD: Phòng 301" />
      <Input label="Mô tả" {...register('description')} placeholder="VD: Chung cư ABC..." />
      <Button type="submit" className="w-full" isLoading={isLoading}>
        Tạo phòng
      </Button>
    </form>
  );
}
