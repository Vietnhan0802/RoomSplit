import { useState, useEffect, useCallback } from 'react';
import { Trash2, ChevronDown } from 'lucide-react';
import Card from '../../../components/ui/Card';
import CategoryIcon from '../../../components/shared/CategoryIcon';
import ImageViewer from '../../../components/shared/ImageViewer';
import { financeApi, type TransactionQueryParams } from '../../../api/finance';
import { useFinance } from '../FinanceContext';
import { showToast } from '../../../components/ui/showToast';
import { formatCurrency } from '../../../utils/formatCurrency';
import { PERSONAL_EXPENSE_CATEGORIES, INCOME_CATEGORIES } from '../../../constants';
import { cn } from '../../../utils/cn';
import type { Transaction } from '../../../types';

type FilterType = 'all' | 'income' | 'expense';

function getDateLabel(dateStr: string): string {
  const date = new Date(dateStr);
  const today = new Date();
  const yesterday = new Date();
  yesterday.setDate(today.getDate() - 1);

  if (date.toDateString() === today.toDateString()) return 'Hôm nay';
  if (date.toDateString() === yesterday.toDateString()) return 'Hôm qua';

  return date.toLocaleDateString('vi-VN', {
    weekday: 'long',
    day: 'numeric',
    month: 'numeric',
  });
}

function groupByDate(transactions: Transaction[]): Map<string, Transaction[]> {
  const groups = new Map<string, Transaction[]>();
  for (const tx of transactions) {
    const key = tx.date.split('T')[0];
    const arr = groups.get(key) || [];
    arr.push(tx);
    groups.set(key, arr);
  }
  return groups;
}

export default function TransactionList() {
  const { month, year, refreshKey, triggerRefresh } = useFinance();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [filter, setFilter] = useState<FilterType>('all');
  const [categoryFilter, setCategoryFilter] = useState<number | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [showCategoryDropdown, setShowCategoryDropdown] = useState(false);

  // Image viewer state
  const [viewerImages, setViewerImages] = useState<{ url: string; name?: string }[]>([]);
  const [viewerIndex, setViewerIndex] = useState(0);
  const [viewerOpen, setViewerOpen] = useState(false);

  const fetchTransactions = useCallback(
    async (pageNum: number, append = false) => {
      try {
        if (!append) setIsLoading(true);

        const startDate = new Date(year, month - 1, 1).toISOString();
        const endDate = new Date(year, month, 0, 23, 59, 59).toISOString();

        const params: TransactionQueryParams = {
          startDate,
          endDate,
          page: pageNum,
          pageSize: 20,
          sortBy: 'date',
          order: 'desc',
        };

        if (filter === 'income') params.type = 0;
        if (filter === 'expense') params.type = 1;
        if (categoryFilter !== null) params.category = categoryFilter;

        const res = await financeApi.getTransactions(params);
        if (res.data.data) {
          const { items, totalPages: tp } = res.data.data;
          setTransactions((prev) => (append ? [...prev, ...items] : items));
          setTotalPages(tp);
        }
      } catch {
        showToast('error', 'Không thể tải giao dịch');
      } finally {
        setIsLoading(false);
      }
    },
    [month, year, filter, categoryFilter]
  );

  useEffect(() => {
    setPage(1);
    fetchTransactions(1);
  }, [fetchTransactions, refreshKey]);

  const handleLoadMore = () => {
    const next = page + 1;
    setPage(next);
    fetchTransactions(next, true);
  };

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    if (!confirm('Xóa giao dịch này?')) return;
    try {
      await financeApi.deleteTransaction(id);
      setTransactions((prev) => prev.filter((t) => t.id !== id));
      triggerRefresh();
      showToast('success', 'Đã xóa giao dịch');
    } catch {
      showToast('error', 'Xóa thất bại');
    }
  };

  const handleThumbnailClick = (e: React.MouseEvent, tx: Transaction) => {
    e.stopPropagation();
    const imgs = tx.images.map((img) => ({
      url: `/uploads${img.imageUrl}`,
      name: img.originalFileName,
    }));
    setViewerImages(imgs);
    setViewerIndex(0);
    setViewerOpen(true);
  };

  const currentCategories = filter === 'income' ? INCOME_CATEGORIES : PERSONAL_EXPENSE_CATEGORIES;
  const grouped = groupByDate(transactions);

  const filterButtons: { key: FilterType; label: string }[] = [
    { key: 'all', label: 'Tất cả' },
    { key: 'income', label: 'Thu nhập' },
    { key: 'expense', label: 'Chi tiêu' },
  ];

  return (
    <div className="space-y-3">
      {/* Filter bar */}
      <div className="flex items-center gap-2 flex-wrap">
        {/* Type filter pills */}
        <div className="flex rounded-lg bg-gray-100 p-0.5 dark:bg-gray-800">
          {filterButtons.map((f) => (
            <button
              key={f.key}
              onClick={() => {
                setFilter(f.key);
                setCategoryFilter(null);
              }}
              className={cn(
                'rounded-md px-3 py-1.5 text-xs font-medium transition-colors',
                filter === f.key
                  ? 'bg-white shadow-sm dark:bg-gray-700'
                  : 'text-gray-500 hover:text-gray-700'
              )}
            >
              {f.label}
            </button>
          ))}
        </div>

        {/* Category dropdown */}
        {filter !== 'all' && (
          <div className="relative">
            <button
              onClick={() => setShowCategoryDropdown(!showCategoryDropdown)}
              className="flex items-center gap-1 rounded-lg border border-gray-200 px-3 py-1.5 text-xs dark:border-gray-600"
            >
              {categoryFilter !== null
                ? currentCategories.find((c) => c.value === categoryFilter)?.label || 'Danh mục'
                : 'Danh mục'}
              <ChevronDown className="h-3 w-3" />
            </button>
            {showCategoryDropdown && (
              <div className="absolute left-0 top-full z-20 mt-1 max-h-48 w-40 overflow-y-auto rounded-lg border border-gray-200 bg-white shadow-lg dark:border-gray-600 dark:bg-gray-800">
                <button
                  onClick={() => {
                    setCategoryFilter(null);
                    setShowCategoryDropdown(false);
                  }}
                  className={cn(
                    'w-full px-3 py-2 text-left text-xs hover:bg-gray-50 dark:hover:bg-gray-700',
                    categoryFilter === null && 'font-semibold text-primary-600'
                  )}
                >
                  Tất cả
                </button>
                {currentCategories.map((cat) => (
                  <button
                    key={cat.value}
                    onClick={() => {
                      setCategoryFilter(cat.value);
                      setShowCategoryDropdown(false);
                    }}
                    className={cn(
                      'w-full px-3 py-2 text-left text-xs hover:bg-gray-50 dark:hover:bg-gray-700',
                      categoryFilter === cat.value && 'font-semibold text-primary-600'
                    )}
                  >
                    {cat.label}
                  </button>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Transaction list */}
      {isLoading ? (
        <p className="py-8 text-center text-gray-400">Đang tải...</p>
      ) : transactions.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có giao dịch</p>
      ) : (
        <div className="space-y-4">
          {Array.from(grouped.entries()).map(([dateKey, txs]) => {
            const dayTotal = txs.reduce(
              (sum, t) => sum + (t.type === 'Income' ? t.amount : -t.amount),
              0
            );
            return (
              <div key={dateKey}>
                {/* Date group header */}
                <div className="mb-1.5 flex items-center justify-between px-1">
                  <span className="text-xs font-semibold text-gray-500">
                    {getDateLabel(dateKey)}
                  </span>
                  <span
                    className={cn(
                      'text-xs font-medium',
                      dayTotal >= 0 ? 'text-green-600' : 'text-red-500'
                    )}
                  >
                    {dayTotal >= 0 ? '+' : ''}
                    {formatCurrency(dayTotal)}
                  </span>
                </div>

                {/* Transactions */}
                <Card className="divide-y divide-gray-50 dark:divide-gray-700 !p-0">
                  {txs.map((tx) => (
                    <div
                      key={tx.id}
                      className="flex items-center gap-3 px-4 py-3"
                    >
                      <CategoryIcon
                        category={tx.expenseCategory || tx.incomeCategory || 'Other'}
                        size="sm"
                      />
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {tx.description}
                        </p>
                        {tx.note && (
                          <p className="text-xs text-gray-400 truncate">{tx.note}</p>
                        )}
                      </div>

                      {/* Image indicator */}
                      {tx.images?.length > 0 && (
                        <button
                          onClick={(e) => handleThumbnailClick(e, tx)}
                          className="flex-shrink-0 h-8 w-8 rounded overflow-hidden"
                        >
                          <img
                            src={`/uploads${tx.images[0].thumbnailUrl || tx.images[0].imageUrl}`}
                            alt=""
                            className="h-full w-full object-cover"
                          />
                        </button>
                      )}

                      <span
                        className={cn(
                          'text-sm font-semibold flex-shrink-0',
                          tx.type === 'Income' ? 'text-green-600' : 'text-red-500'
                        )}
                      >
                        {tx.type === 'Income' ? '+' : '-'}
                        {formatCurrency(tx.amount)}
                      </span>

                      <button
                        onClick={(e) => handleDelete(e, tx.id)}
                        className="flex-shrink-0 text-gray-300 hover:text-red-500 transition-colors"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </Card>
              </div>
            );
          })}

          {/* Load more */}
          {page < totalPages && (
            <button
              onClick={handleLoadMore}
              className="w-full rounded-lg border border-gray-200 py-2.5 text-sm font-medium text-gray-600 hover:bg-gray-50 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-800"
            >
              Tải thêm
            </button>
          )}
        </div>
      )}

      {/* Image viewer */}
      <ImageViewer
        images={viewerImages}
        initialIndex={viewerIndex}
        isOpen={viewerOpen}
        onClose={() => setViewerOpen(false)}
      />
    </div>
  );
}
