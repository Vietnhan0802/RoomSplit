import { useState, useEffect } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import Modal from '../../../components/ui/Modal';
import TransactionForm from '../../../components/forms/TransactionForm';
import CategoryBadge from '../../../components/shared/CategoryBadge';
import { financeApi } from '../../../api/finance';
import { showToast } from '../../../components/ui/Toast';
import { formatCurrency } from '../../../utils/formatCurrency';
import { formatDate } from '../../../utils/dateHelpers';
import type { Transaction } from '../../../types';

export default function TransactionList() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const now = new Date();
  const [month] = useState(now.getMonth() + 1);
  const [year] = useState(now.getFullYear());

  useEffect(() => {
    financeApi.getTransactions(month, year).then((res) => setTransactions(res.data.data || [])).finally(() => setIsLoading(false));
  }, [month, year]);

  const handleCreate = async (data: Parameters<typeof financeApi.createTransaction>[0]) => {
    try {
      setIsSubmitting(true);
      const res = await financeApi.createTransaction({ ...data, transactionDate: new Date(data.transactionDate).toISOString() });
      if (res.data.data) {
        setTransactions((prev) => [res.data.data!, ...prev]);
        setShowForm(false);
        showToast('success', 'Thêm giao dịch thành công!');
      }
    } catch {
      showToast('error', 'Thêm giao dịch thất bại');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await financeApi.deleteTransaction(id);
      setTransactions((prev) => prev.filter((t) => t.id !== id));
      showToast('success', 'Đã xóa giao dịch');
    } catch {
      showToast('error', 'Xóa thất bại');
    }
  };

  if (isLoading) return <p className="text-center text-gray-400 py-8">Đang tải...</p>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Giao dịch tháng {month}/{year}</h2>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Plus className="mr-1.5 h-4 w-4" /> Thêm
        </Button>
      </div>

      {transactions.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có giao dịch</p>
      ) : (
        <div className="space-y-2">
          {transactions.map((tx) => (
            <Card key={tx.id}>
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="font-medium">{tx.description || tx.category}</p>
                    <CategoryBadge category={tx.category} />
                  </div>
                  <p className="text-xs text-gray-500">{formatDate(tx.transactionDate)}</p>
                </div>
                <div className="flex items-center gap-3">
                  <span className={tx.type === 'Income' ? 'font-semibold text-green-600' : 'font-semibold text-red-600'}>
                    {tx.type === 'Income' ? '+' : '-'}{formatCurrency(tx.amount)}
                  </span>
                  <button onClick={() => handleDelete(tx.id)} className="text-gray-400 hover:text-red-500">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal isOpen={showForm} onClose={() => setShowForm(false)} title="Thêm giao dịch">
        <TransactionForm onSubmit={handleCreate} isLoading={isSubmitting} />
      </Modal>
    </div>
  );
}
