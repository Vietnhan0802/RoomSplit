import { useState, useEffect } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import Modal from '../../../components/ui/Modal';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import AmountInput from '../../../components/shared/AmountInput';
import { financeApi } from '../../../api/finance';
import { useFinance } from '../FinanceContext';
import { showToast } from '../../../components/ui/showToast';
import { formatCurrency } from '../../../utils/formatCurrency';
import { PERSONAL_EXPENSE_CATEGORIES } from '../../../constants';
import { cn } from '../../../utils/cn';
import type { BudgetStatus } from '../../../types';

export default function BudgetList() {
  const { month, year, refreshKey, triggerRefresh } = useFinance();
  const [budgets, setBudgets] = useState<BudgetStatus[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingBudget, setEditingBudget] = useState<BudgetStatus | null>(null);
  const [formCategory, setFormCategory] = useState(0);
  const [formLimit, setFormLimit] = useState(0);

  useEffect(() => {
    let cancelled = false;
    financeApi
      .getBudgetStatus(month, year)
      .then((res) => { if (!cancelled) setBudgets(res.data.data || []); })
      .catch(() => { if (!cancelled) setBudgets([]); })
      .finally(() => { if (!cancelled) setIsLoading(false); });
    return () => { cancelled = true; };
  }, [month, year, refreshKey]);

  const openCreate = () => {
    setEditingBudget(null);
    setFormCategory(0);
    setFormLimit(0);
    setShowForm(true);
  };

  const openEdit = (budget: BudgetStatus) => {
    setEditingBudget(budget);
    const catIndex = PERSONAL_EXPENSE_CATEGORIES.findIndex(
      (c) => c.key === budget.expenseCategory
    );
    setFormCategory(catIndex >= 0 ? PERSONAL_EXPENSE_CATEGORIES[catIndex].value : 0);
    setFormLimit(budget.monthlyLimit);
    setShowForm(true);
  };

  const handleSubmit = async () => {
    try {
      if (editingBudget) {
        await financeApi.updateBudget(editingBudget.id, {
          expenseCategory: formCategory,
          monthlyLimit: formLimit,
        });
        showToast('success', 'Cập nhật ngân sách thành công!');
      } else {
        await financeApi.createBudget({
          expenseCategory: formCategory,
          monthlyLimit: formLimit,
          month,
          year,
        });
        showToast('success', 'Tạo ngân sách thành công!');
      }
      setShowForm(false);
      triggerRefresh();
    } catch {
      showToast('error', editingBudget ? 'Cập nhật thất bại' : 'Tạo ngân sách thất bại');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Xóa ngân sách này?')) return;
    try {
      await financeApi.deleteBudget(id);
      setBudgets((prev) => prev.filter((b) => b.id !== id));
      triggerRefresh();
      showToast('success', 'Đã xóa ngân sách');
    } catch {
      showToast('error', 'Xóa thất bại');
    }
  };

  const getProgressColor = (percent: number) => {
    if (percent >= 90) return 'bg-red-500';
    if (percent >= 70) return 'bg-yellow-500';
    return 'bg-green-500';
  };

  if (isLoading) {
    return <p className="py-8 text-center text-gray-400">Đang tải...</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold">Ngân sách</h2>
        <Button size="sm" onClick={openCreate}>
          <Plus className="mr-1.5 h-4 w-4" /> Thêm
        </Button>
      </div>

      {budgets.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có ngân sách</p>
      ) : (
        <div className="space-y-3">
          {budgets.map((b) => {
            const pct = Math.min(b.percentUsed, 100);
            const cat = PERSONAL_EXPENSE_CATEGORIES.find(
              (c) => c.key === b.expenseCategory
            );

            return (
              <Card key={b.id}>
                <div className="space-y-3">
                  {/* Header */}
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <CategoryIcon
                        category={b.expenseCategory}
                        size="sm"
                      />
                      <span className="font-medium text-sm">
                        {cat?.label || b.expenseCategory}
                      </span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => openEdit(b)}
                        className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </button>
                      <button
                        onClick={() => handleDelete(b.id)}
                        className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-500 dark:hover:bg-red-900/20"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  </div>

                  {/* Amount */}
                  <div className="flex items-baseline justify-between">
                    <span
                      className={cn(
                        'text-lg font-bold',
                        b.isOverBudget ? 'text-red-600' : ''
                      )}
                    >
                      {formatCurrency(b.spentAmount)}
                    </span>
                    <span className="text-sm text-gray-400">
                      / {formatCurrency(b.monthlyLimit)}
                    </span>
                  </div>

                  {/* Progress bar */}
                  <div className="h-2 rounded-full bg-gray-200 dark:bg-gray-700">
                    <div
                      className={cn(
                        'h-2 rounded-full transition-all',
                        getProgressColor(b.percentUsed),
                        b.percentUsed >= 90 && 'animate-pulse'
                      )}
                      style={{ width: `${pct}%` }}
                    />
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-3 gap-2 text-xs text-gray-500">
                    <div>
                      <p className="text-gray-400">Còn lại</p>
                      <p
                        className={cn(
                          'font-medium',
                          b.remainingAmount < 0 ? 'text-red-500' : 'text-green-600'
                        )}
                      >
                        {formatCurrency(Math.abs(b.remainingAmount))}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-400">TB/ngày</p>
                      <p className="font-medium">{formatCurrency(b.dailyAverageSpent)}</p>
                    </div>
                    <div>
                      <p className="text-gray-400">Dự kiến</p>
                      <p
                        className={cn(
                          'font-medium',
                          b.projectedMonthEnd > b.monthlyLimit
                            ? 'text-red-500'
                            : 'text-green-600'
                        )}
                      >
                        {formatCurrency(b.projectedMonthEnd)}
                      </p>
                    </div>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      )}

      {/* Create/Edit modal */}
      <Modal
        isOpen={showForm}
        onClose={() => setShowForm(false)}
        title={editingBudget ? 'Sửa ngân sách' : 'Thêm ngân sách'}
      >
        <div className="space-y-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Danh mục
            </label>
            <select
              value={formCategory}
              onChange={(e) => setFormCategory(+e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm dark:border-gray-600 dark:bg-gray-700"
            >
              {PERSONAL_EXPENSE_CATEGORIES.map((c) => (
                <option key={c.value} value={c.value}>
                  {c.label}
                </option>
              ))}
            </select>
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Hạn mức
            </label>
            <AmountInput
              value={formLimit}
              onChange={(e) => setFormLimit(+e.target.value)}
            />
          </div>
          <Button className="w-full" onClick={handleSubmit}>
            {editingBudget ? 'Cập nhật' : 'Tạo ngân sách'}
          </Button>
        </div>
      </Modal>
    </div>
  );
}
