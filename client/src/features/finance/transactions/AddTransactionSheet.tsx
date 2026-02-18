import { useState, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import AmountInput from '../../../components/shared/AmountInput';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import ImageUploadSection from '../../../components/shared/ImageUploadSection';
import { financeApi } from '../../../api/finance';
import { showToast } from '../../../components/ui/showToast';
import { PERSONAL_EXPENSE_CATEGORIES, INCOME_CATEGORIES } from '../../../constants';
import { cn } from '../../../utils/cn';
import type { Transaction, TransactionImage } from '../../../types';

const schema = z.object({
  type: z.number(),
  amount: z.number().min(1, 'Nhập số tiền'),
  description: z.string().min(1, 'Nhập mô tả').max(200),
  date: z.string().min(1, 'Chọn ngày'),
  note: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

interface AddTransactionSheetProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  initialDate?: string;
  editTransaction?: Transaction;
}

export default function AddTransactionSheet({
  isOpen,
  onClose,
  onSuccess,
  initialDate,
  editTransaction,
}: AddTransactionSheetProps) {
  const isEdit = !!editTransaction;
  const defaultType = editTransaction ? (editTransaction.type === 'Income' ? 0 : 1) : 1;

  const { register, handleSubmit, watch, setValue, formState: { errors }, reset } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      type: defaultType,
      amount: editTransaction?.amount || 0,
      description: editTransaction?.description || '',
      date: editTransaction?.date?.split('T')[0] || initialDate || new Date().toISOString().split('T')[0],
      note: editTransaction?.note || '',
    },
  });

  const txType = watch('type');
  const [selectedCategory, setSelectedCategory] = useState<number>(
    editTransaction
      ? (editTransaction.type === 'Income'
          ? INCOME_CATEGORIES.findIndex(c => c.key === editTransaction.incomeCategory)
          : PERSONAL_EXPENSE_CATEGORIES.findIndex(c => c.key === editTransaction.expenseCategory))
      : 0
  );
  const [images, setImages] = useState<File[]>([]);
  const [existingImages, setExistingImages] = useState<TransactionImage[]>(
    editTransaction?.images || []
  );
  const [removedImageIds, setRemovedImageIds] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const categories = txType === 0 ? INCOME_CATEGORIES : PERSONAL_EXPENSE_CATEGORIES;

  const handleAddImages = useCallback((files: File[]) => {
    setImages(prev => [...prev, ...files]);
  }, []);

  const handleRemoveImage = useCallback((index: number) => {
    setImages(prev => prev.filter((_, i) => i !== index));
  }, []);

  const handleRemoveExisting = useCallback((imageId: string) => {
    setExistingImages(prev => prev.filter(img => img.id !== imageId));
    setRemovedImageIds(prev => [...prev, imageId]);
  }, []);

  const onSubmit = async (data: FormData) => {
    try {
      setIsSubmitting(true);
      const formData = new FormData();
      formData.append('type', data.type.toString());
      formData.append('amount', data.amount.toString());
      formData.append('description', data.description);
      formData.append('date', new Date(data.date).toISOString());
      if (data.note) formData.append('note', data.note);

      // Set category based on type
      const cat = categories[selectedCategory];
      if (data.type === 0) {
        formData.append('incomeCategory', cat.value.toString());
      } else {
        formData.append('expenseCategory', cat.value.toString());
      }

      // Append images
      images.forEach(img => formData.append('images', img));

      if (isEdit && editTransaction) {
        if (removedImageIds.length > 0) {
          formData.append('removeImageIds', removedImageIds.join(','));
        }
        images.forEach(img => formData.append('newImages', img));
        await financeApi.updateTransaction(editTransaction.id, formData);
        showToast('success', 'Cập nhật giao dịch thành công!');
      } else {
        await financeApi.createTransaction(formData);
        showToast('success', 'Thêm giao dịch thành công!');
      }

      reset();
      setImages([]);
      setSelectedCategory(0);
      onSuccess();
    } catch {
      showToast('error', isEdit ? 'Cập nhật thất bại' : 'Thêm giao dịch thất bại');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEdit ? 'Sửa giao dịch' : 'Thêm giao dịch'}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Type toggle */}
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => { setValue('type', 0); setSelectedCategory(0); }}
            className={cn(
              'flex-1 rounded-lg border-2 py-2.5 text-sm font-medium transition-colors',
              txType === 0
                ? 'border-green-500 bg-green-50 text-green-700 dark:bg-green-900/20 dark:text-green-400'
                : 'border-gray-200 text-gray-500 dark:border-gray-600'
            )}
          >
            Thu nhập
          </button>
          <button
            type="button"
            onClick={() => { setValue('type', 1); setSelectedCategory(0); }}
            className={cn(
              'flex-1 rounded-lg border-2 py-2.5 text-sm font-medium transition-colors',
              txType === 1
                ? 'border-red-500 bg-red-50 text-red-700 dark:bg-red-900/20 dark:text-red-400'
                : 'border-gray-200 text-gray-500 dark:border-gray-600'
            )}
          >
            Chi tiêu
          </button>
        </div>

        {/* Amount */}
        <AmountInput
          label="Số tiền"
          {...register('amount', { valueAsNumber: true })}
          error={errors.amount?.message}
        />

        {/* Category grid */}
        <div className="space-y-1">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
            Danh mục
          </label>
          <div className={cn(
            'grid gap-2',
            txType === 0 ? 'grid-cols-3' : 'grid-cols-4'
          )}>
            {categories.map((cat, i) => (
              <button
                key={cat.key}
                type="button"
                onClick={() => setSelectedCategory(i)}
                className={cn(
                  'flex flex-col items-center gap-1 rounded-lg p-2 text-xs transition-all',
                  selectedCategory === i
                    ? 'bg-primary-50 ring-2 ring-primary-400 dark:bg-primary-900/20'
                    : 'hover:bg-gray-50 dark:hover:bg-gray-800'
                )}
              >
                <CategoryIcon category={cat.key} size="sm" />
                <span className="truncate w-full text-center">{cat.label}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Description */}
        <Input
          label="Mô tả"
          placeholder="VD: Cơm trưa, Grab..."
          {...register('description')}
          error={errors.description?.message}
        />

        {/* Date */}
        <Input
          label="Ngày"
          type="date"
          {...register('date')}
          error={errors.date?.message}
        />

        {/* Note */}
        <Input label="Ghi chú" placeholder="Tùy chọn" {...register('note')} />

        {/* Images */}
        <ImageUploadSection
          images={images}
          existingImages={isEdit ? existingImages : undefined}
          onAdd={handleAddImages}
          onRemove={handleRemoveImage}
          onRemoveExisting={isEdit ? handleRemoveExisting : undefined}
          maxImages={3}
        />

        {/* Submit */}
        <Button type="submit" className="w-full" isLoading={isSubmitting}>
          {isEdit ? 'Cập nhật' : 'Lưu giao dịch'}
        </Button>
      </form>
    </Modal>
  );
}
