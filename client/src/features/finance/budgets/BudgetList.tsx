import { useState, useEffect } from 'react';
import { Plus } from 'lucide-react';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import Modal from '../../../components/ui/Modal';
import { financeApi } from '../../../api/finance';
import { showToast } from '../../../components/ui/Toast';
import { formatCurrency } from '../../../utils/formatCurrency';
import { TRANSACTION_CATEGORIES } from '../../../constants';
import type { Budget } from '../../../types';

export default function BudgetList() {
  const now = new Date();
  const [budgets, setBudgets] = useState<Budget[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({ category: 1, limitAmount: 0 });
  const month = now.getMonth() + 1;
  const year = now.getFullYear();

  useEffect(() => {
    financeApi.getBudgets(month, year).then((res) => setBudgets(res.data.data || [])).finally(() => setIsLoading(false));
  }, [month, year]);

  const handleCreate = async () => {
    try {
      const res = await financeApi.createBudget({ ...formData, month, year });
      if (res.data.data) {
        setBudgets((prev) => [...prev, res.data.data!]);
        setShowForm(false);
        showToast('success', 'Tạo ngân sách thành công!');
      }
    } catch {
      showToast('error', 'Tạo ngân sách thất bại');
    }
  };

  if (isLoading) return <p className="text-center text-gray-400 py-8">Đang tải...</p>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Ngân sách tháng {month}/{year}</h2>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Plus className="mr-1.5 h-4 w-4" /> Thêm
        </Button>
      </div>

      {budgets.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có ngân sách</p>
      ) : (
        <div className="space-y-3">
          {budgets.map((b) => {
            const pct = b.limitAmount > 0 ? Math.min((b.spentAmount / b.limitAmount) * 100, 100) : 0;
            const isOverBudget = b.spentAmount > b.limitAmount;
            return (
              <Card key={b.id}>
                <div className="flex items-center justify-between mb-2">
                  <p className="font-medium">{b.category}</p>
                  <p className="text-sm">
                    <span className={isOverBudget ? 'text-red-600 font-semibold' : ''}>{formatCurrency(b.spentAmount)}</span>
                    <span className="text-gray-400"> / {formatCurrency(b.limitAmount)}</span>
                  </p>
                </div>
                <div className="h-2 rounded-full bg-gray-200 dark:bg-gray-700">
                  <div
                    className={`h-2 rounded-full transition-all ${isOverBudget ? 'bg-red-500' : pct > 80 ? 'bg-yellow-500' : 'bg-green-500'}`}
                    style={{ width: `${pct}%` }}
                  />
                </div>
              </Card>
            );
          })}
        </div>
      )}

      <Modal isOpen={showForm} onClose={() => setShowForm(false)} title="Thêm ngân sách">
        <div className="space-y-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Danh mục</label>
            <select
              value={formData.category}
              onChange={(e) => setFormData({ ...formData, category: +e.target.value })}
              className="input-field"
            >
              {TRANSACTION_CATEGORIES.map((c) => (
                <option key={c.value} value={c.value}>{c.label}</option>
              ))}
            </select>
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Hạn mức (VND)</label>
            <input
              type="number"
              value={formData.limitAmount}
              onChange={(e) => setFormData({ ...formData, limitAmount: +e.target.value })}
              className="input-field"
            />
          </div>
          <Button className="w-full" onClick={handleCreate}>Tạo ngân sách</Button>
        </div>
      </Modal>
    </div>
  );
}
