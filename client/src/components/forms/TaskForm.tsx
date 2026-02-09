import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { TASK_FREQUENCIES, TASK_TEMPLATES } from '../../constants';

const schema = z.object({
  title: z.string().min(1, 'Vui lòng nhập tên công việc'),
  description: z.string().optional(),
  frequency: z.number(),
  isRotating: z.boolean(),
});

type TaskFormData = z.infer<typeof schema>;

interface TaskFormProps {
  onSubmit: (data: TaskFormData) => void;
  isLoading?: boolean;
}

export default function TaskForm({ onSubmit, isLoading }: TaskFormProps) {
  const { register, handleSubmit, setValue, formState: { errors } } = useForm<TaskFormData>({
    resolver: zodResolver(schema),
    defaultValues: { frequency: 1, isRotating: true },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Input label="Tên công việc" {...register('title')} error={errors.title?.message} />
        <div className="mt-2 flex flex-wrap gap-1">
          {TASK_TEMPLATES.map((t) => (
            <button
              key={t}
              type="button"
              onClick={() => setValue('title', t)}
              className="rounded-full bg-gray-100 px-2.5 py-1 text-xs text-gray-600 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300"
            >
              {t}
            </button>
          ))}
        </div>
      </div>

      <Input label="Mô tả" {...register('description')} />

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Tần suất</label>
        <select {...register('frequency', { valueAsNumber: true })} className="input-field">
          {TASK_FREQUENCIES.map((f) => (
            <option key={f.value} value={f.value}>{f.label}</option>
          ))}
        </select>
      </div>

      <label className="flex items-center gap-2">
        <input type="checkbox" {...register('isRotating')} className="rounded border-gray-300" />
        <span className="text-sm">Phân công luân phiên</span>
      </label>

      <Button type="submit" className="w-full" isLoading={isLoading}>
        Tạo công việc
      </Button>
    </form>
  );
}
