import { useState } from 'react';
import { Pencil, Trash2 } from 'lucide-react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import ImageViewer from '../../../components/shared/ImageViewer';
import AddTransactionSheet from './AddTransactionSheet';
import { financeApi } from '../../../api/finance';
import { showToast } from '../../../components/ui/Toast';
import { formatCurrency } from '../../../utils/formatCurrency';
import { formatDate } from '../../../utils/dateHelpers';
import { cn } from '../../../utils/cn';
import type { Transaction } from '../../../types';

interface TransactionDetailModalProps {
  transaction: Transaction | null;
  isOpen: boolean;
  onClose: () => void;
  onUpdate: () => void;
}

export default function TransactionDetailModal({
  transaction,
  isOpen,
  onClose,
  onUpdate,
}: TransactionDetailModalProps) {
  const [showEdit, setShowEdit] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [viewerOpen, setViewerOpen] = useState(false);
  const [viewerIndex, setViewerIndex] = useState(0);

  if (!transaction) return null;

  const category = transaction.expenseCategory || transaction.incomeCategory || 'Other';
  const isIncome = transaction.type === 'Income';

  const handleDelete = async () => {
    if (!confirm('Xóa giao dịch này?')) return;
    try {
      setIsDeleting(true);
      await financeApi.deleteTransaction(transaction.id);
      showToast('success', 'Đã xóa giao dịch');
      onUpdate();
      onClose();
    } catch {
      showToast('error', 'Xóa thất bại');
    } finally {
      setIsDeleting(false);
    }
  };

  const viewerImages = transaction.images.map((img) => ({
    url: `/uploads${img.imageUrl}`,
    name: img.originalFileName,
  }));

  return (
    <>
      <Modal isOpen={isOpen && !showEdit} onClose={onClose} title="Chi tiết giao dịch">
        <div className="space-y-4">
          {/* Type badge + amount */}
          <div className="flex items-center justify-between">
            <span
              className={cn(
                'rounded-full px-3 py-1 text-xs font-medium',
                isIncome
                  ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                  : 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
              )}
            >
              {isIncome ? 'Thu nhập' : 'Chi tiêu'}
            </span>
            <span
              className={cn(
                'text-xl font-bold',
                isIncome ? 'text-green-600' : 'text-red-500'
              )}
            >
              {isIncome ? '+' : '-'}
              {formatCurrency(transaction.amount)}
            </span>
          </div>

          {/* Category + description */}
          <div className="flex items-center gap-3">
            <CategoryIcon category={category} size="md" />
            <div>
              <p className="font-semibold">{transaction.description}</p>
              <p className="text-sm text-gray-500">{formatDate(transaction.date)}</p>
            </div>
          </div>

          {/* Note */}
          {transaction.note && (
            <div className="rounded-lg bg-gray-50 p-3 dark:bg-gray-700/50">
              <p className="text-xs text-gray-500 mb-1">Ghi chú</p>
              <p className="text-sm">{transaction.note}</p>
            </div>
          )}

          {/* Images */}
          {transaction.images.length > 0 && (
            <div className="space-y-1">
              <p className="text-xs text-gray-500">Ảnh đính kèm</p>
              <div className="flex gap-2 flex-wrap">
                {transaction.images.map((img, i) => (
                  <button
                    key={img.id}
                    onClick={() => {
                      setViewerIndex(i);
                      setViewerOpen(true);
                    }}
                    className="h-20 w-20 overflow-hidden rounded-lg"
                  >
                    <img
                      src={`/uploads${img.thumbnailUrl || img.imageUrl}`}
                      alt={img.originalFileName}
                      className="h-full w-full object-cover"
                    />
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-2 pt-2">
            <Button
              variant="secondary"
              className="flex-1"
              onClick={() => setShowEdit(true)}
            >
              <Pencil className="mr-1.5 h-4 w-4" />
              Sửa
            </Button>
            <Button
              variant="danger"
              className="flex-1"
              onClick={handleDelete}
              isLoading={isDeleting}
            >
              <Trash2 className="mr-1.5 h-4 w-4" />
              Xóa
            </Button>
          </div>
        </div>
      </Modal>

      {/* Image viewer */}
      <ImageViewer
        images={viewerImages}
        initialIndex={viewerIndex}
        isOpen={viewerOpen}
        onClose={() => setViewerOpen(false)}
      />

      {/* Edit sheet */}
      <AddTransactionSheet
        isOpen={showEdit}
        onClose={() => setShowEdit(false)}
        onSuccess={() => {
          setShowEdit(false);
          onUpdate();
          onClose();
        }}
        editTransaction={transaction}
      />
    </>
  );
}
