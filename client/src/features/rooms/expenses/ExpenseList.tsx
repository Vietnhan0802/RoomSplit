import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Plus } from 'lucide-react';
import Button from '../../../components/ui/Button';
import Modal from '../../../components/ui/Modal';
import ExpenseCard from '../../../components/shared/ExpenseCard';
import ExpenseForm from '../../../components/forms/ExpenseForm';
import { CardSkeleton } from '../../../components/ui/Skeleton';
import { roomsApi } from '../../../api/rooms';
import { showToast } from '../../../components/ui/showToast';
import type { Expense } from '../../../types';

export default function ExpenseList() {
  const { roomId } = useParams<{ roomId: string }>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (roomId) {
      roomsApi.getExpenses(roomId).then((res) => setExpenses(res.data.data || [])).finally(() => setIsLoading(false));
    }
  }, [roomId]);

  const handleCreate = async (data: Parameters<typeof roomsApi.createExpense>[1]) => {
    if (!roomId) return;
    try {
      setIsSubmitting(true);
      const res = await roomsApi.createExpense(roomId, { ...data, expenseDate: new Date(data.expenseDate).toISOString() });
      if (res.data.data) {
        setExpenses((prev) => [res.data.data!, ...prev]);
        setShowForm(false);
        showToast('success', 'Thêm chi phí thành công!');
      }
    } catch {
      showToast('error', 'Thêm chi phí thất bại');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return <div className="space-y-3">{[1, 2, 3].map((i) => <CardSkeleton key={i} />)}</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Chi phí chung</h2>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Plus className="mr-1.5 h-4 w-4" /> Thêm
        </Button>
      </div>

      {expenses.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có chi phí nào</p>
      ) : (
        <div className="space-y-3">
          {expenses.map((exp) => (
            <ExpenseCard key={exp.id} expense={exp} />
          ))}
        </div>
      )}

      <Modal isOpen={showForm} onClose={() => setShowForm(false)} title="Thêm chi phí">
        <ExpenseForm onSubmit={handleCreate} isLoading={isSubmitting} />
      </Modal>
    </div>
  );
}
