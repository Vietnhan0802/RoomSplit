import { useState } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { List, Calendar, PiggyBank, BarChart3, Plus } from 'lucide-react';
import { cn } from '../../utils/cn';
import { formatCurrency } from '../../utils/formatCurrency';
import MonthNavigator from '../../components/shared/MonthNavigator';
import { FinanceProvider, useFinance } from './FinanceContext';
import AddTransactionSheet from './transactions/AddTransactionSheet';

const tabs = [
  { to: 'calendar', icon: Calendar, label: 'Lịch' },
  { to: 'transactions', icon: List, label: 'Giao dịch' },
  { to: 'budgets', icon: PiggyBank, label: 'Ngân sách' },
  { to: 'summary', icon: BarChart3, label: 'Tổng quan' },
];

function FinanceContent() {
  const { month, year, setMonthYear, summary, triggerRefresh } = useFinance();
  const [showAddForm, setShowAddForm] = useState(false);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Tài chính cá nhân</h1>
      </div>

      <MonthNavigator month={month} year={year} onChange={setMonthYear} />

      {/* Summary cards */}
      {summary && (
        <div className="grid grid-cols-3 gap-3">
          <div className="rounded-xl bg-green-50 p-3 dark:bg-green-900/20">
            <p className="text-xs text-green-600 dark:text-green-400">Thu nhập</p>
            <p className="text-sm font-bold text-green-700 dark:text-green-300 truncate">
              {formatCurrency(summary.totalIncome)}
            </p>
          </div>
          <div className="rounded-xl bg-red-50 p-3 dark:bg-red-900/20">
            <p className="text-xs text-red-600 dark:text-red-400">Chi tiêu</p>
            <p className="text-sm font-bold text-red-700 dark:text-red-300 truncate">
              {formatCurrency(summary.totalExpense)}
            </p>
          </div>
          <div className={cn(
            'rounded-xl p-3',
            summary.netAmount >= 0
              ? 'bg-blue-50 dark:bg-blue-900/20'
              : 'bg-red-50 dark:bg-red-900/20'
          )}>
            <p className={cn(
              'text-xs',
              summary.netAmount >= 0
                ? 'text-blue-600 dark:text-blue-400'
                : 'text-red-600 dark:text-red-400'
            )}>
              Còn lại
            </p>
            <p className={cn(
              'text-sm font-bold truncate',
              summary.netAmount >= 0
                ? 'text-blue-700 dark:text-blue-300'
                : 'text-red-700 dark:text-red-300'
            )}>
              {formatCurrency(summary.netAmount)}
            </p>
          </div>
        </div>
      )}

      {/* Tab navigation */}
      <nav className="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800 overflow-x-auto">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              cn(
                'flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-2 text-sm font-medium transition-colors whitespace-nowrap',
                isActive ? 'bg-white shadow-sm dark:bg-gray-700' : 'text-gray-500 hover:text-gray-700'
              )
            }
          >
            <tab.icon className="h-4 w-4" />
            <span className="hidden sm:inline">{tab.label}</span>
          </NavLink>
        ))}
      </nav>

      <Outlet />

      {/* FAB button */}
      <button
        onClick={() => setShowAddForm(true)}
        className="fixed bottom-20 right-4 z-40 flex h-14 w-14 items-center justify-center rounded-full bg-primary-600 text-white shadow-lg hover:bg-primary-700 transition-colors lg:bottom-6 lg:right-6"
      >
        <Plus className="h-6 w-6" />
      </button>

      {/* Add transaction sheet */}
      <AddTransactionSheet
        isOpen={showAddForm}
        onClose={() => setShowAddForm(false)}
        onSuccess={() => {
          setShowAddForm(false);
          triggerRefresh();
        }}
      />
    </div>
  );
}

export default function FinancePage() {
  return (
    <FinanceProvider>
      <FinanceContent />
    </FinanceProvider>
  );
}
