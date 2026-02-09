import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Button from '../ui/Button';
import Input from '../ui/Input';
import AmountInput from '../shared/AmountInput';
import { EXPENSE_CATEGORIES, SPLIT_TYPES } from '../../constants';

const schema = z.object({
  description: z.string().min(1, 'Vui lòng nhập mô tả'),
  amount: z.number().min(1, 'Số tiền phải lớn hơn 0'),
  category: z.number(),
  splitType: z.number(),
  expenseDate: z.string().min(1, 'Vui lòng chọn ngày'),
  note: z.string().optional(),
});

type ExpenseFormData = z.infer<typeof schema>;

interface ExpenseFormProps {
  onSubmit: (data: ExpenseFormData) => void;
  isLoading?: boolean;
}

export default function ExpenseForm({ onSubmit, isLoading }: ExpenseFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<ExpenseFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      category: 0,
      splitType: 0,
      expenseDate: new Date().toISOString().split('T')[0],
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input label="Mô tả" {...register('description')} error={errors.description?.message} />

      <AmountInput
        label="Số tiền"
        {...register('amount', { valueAsNumber: true })}
        error={errors.amount?.message}
      />

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Danh mục</label>
        <select {...register('category', { valueAsNumber: true })} className="input-field">
          {EXPENSE_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>{cat.label}</option>
          ))}
        </select>
      </div>

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Cách chia</label>
        <select {...register('splitType', { valueAsNumber: true })} className="input-field">
          {SPLIT_TYPES.map((st) => (
            <option key={st.value} value={st.value}>{st.label}</option>
          ))}
        </select>
      </div>

      <Input label="Ngày chi" type="date" {...register('expenseDate')} error={errors.expenseDate?.message} />
      <Input label="Ghi chú" {...register('note')} />

      <Button type="submit" className="w-full" isLoading={isLoading}>
        Thêm chi phí
      </Button>
    </form>
  );
}
