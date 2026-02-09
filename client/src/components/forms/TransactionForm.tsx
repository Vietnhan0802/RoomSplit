import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Button from '../ui/Button';
import Input from '../ui/Input';
import AmountInput from '../shared/AmountInput';
import { TRANSACTION_CATEGORIES } from '../../constants';

const schema = z.object({
  type: z.number(),
  amount: z.number().min(1, 'Số tiền phải lớn hơn 0'),
  category: z.number(),
  description: z.string().optional(),
  transactionDate: z.string().min(1, 'Vui lòng chọn ngày'),
  note: z.string().optional(),
});

type TransactionFormData = z.infer<typeof schema>;

interface TransactionFormProps {
  onSubmit: (data: TransactionFormData) => void;
  isLoading?: boolean;
}

export default function TransactionForm({ onSubmit, isLoading }: TransactionFormProps) {
  const { register, handleSubmit, watch, formState: { errors } } = useForm<TransactionFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      type: 1,
      category: 1,
      transactionDate: new Date().toISOString().split('T')[0],
    },
  });

  const txType = watch('type');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="flex gap-2">
        <label className="flex flex-1 cursor-pointer items-center justify-center rounded-lg border p-3 has-[:checked]:border-primary-500 has-[:checked]:bg-primary-50">
          <input type="radio" value={0} {...register('type', { valueAsNumber: true })} className="sr-only" />
          <span className={txType === 0 ? 'font-medium text-green-600' : 'text-gray-500'}>Thu nhập</span>
        </label>
        <label className="flex flex-1 cursor-pointer items-center justify-center rounded-lg border p-3 has-[:checked]:border-primary-500 has-[:checked]:bg-primary-50">
          <input type="radio" value={1} {...register('type', { valueAsNumber: true })} className="sr-only" />
          <span className={txType === 1 ? 'font-medium text-red-600' : 'text-gray-500'}>Chi tiêu</span>
        </label>
      </div>

      <AmountInput
        label="Số tiền"
        {...register('amount', { valueAsNumber: true })}
        error={errors.amount?.message}
      />

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Danh mục</label>
        <select {...register('category', { valueAsNumber: true })} className="input-field">
          {TRANSACTION_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>{cat.label}</option>
          ))}
        </select>
      </div>

      <Input label="Mô tả" {...register('description')} />
      <Input label="Ngày" type="date" {...register('transactionDate')} error={errors.transactionDate?.message} />
      <Input label="Ghi chú" {...register('note')} />

      <Button type="submit" className="w-full" isLoading={isLoading}>
        Thêm giao dịch
      </Button>
    </form>
  );
}
