import { NavLink, Outlet } from 'react-router-dom';
import { List, Calendar, PiggyBank } from 'lucide-react';
import { cn } from '../../utils/cn';

const tabs = [
  { to: 'transactions', icon: List, label: 'Giao dịch' },
  { to: 'calendar', icon: Calendar, label: 'Lịch' },
  { to: 'budgets', icon: PiggyBank, label: 'Ngân sách' },
];

export default function FinancePage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Tài chính cá nhân</h1>

      <nav className="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              cn(
                'flex flex-1 items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive ? 'bg-white shadow-sm dark:bg-gray-700' : 'text-gray-500 hover:text-gray-700'
              )
            }
          >
            <tab.icon className="h-4 w-4" />
            {tab.label}
          </NavLink>
        ))}
      </nav>

      <Outlet />
    </div>
  );
}
