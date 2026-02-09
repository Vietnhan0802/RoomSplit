import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Home, Wallet, BarChart3 } from 'lucide-react';
import { cn } from '../../utils/cn';
import { ROUTES } from '../../constants';

const navItems = [
  { to: ROUTES.DASHBOARD, icon: LayoutDashboard, label: 'Tổng quan' },
  { to: ROUTES.ROOMS, icon: Home, label: 'Phòng' },
  { to: ROUTES.FINANCE, icon: Wallet, label: 'Tài chính' },
  { to: ROUTES.REPORTS, icon: BarChart3, label: 'Báo cáo' },
];

export default function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 z-30 border-t border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800 lg:hidden">
      <div className="flex items-center justify-around">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex flex-1 flex-col items-center gap-1 py-2 text-xs transition-colors',
                isActive
                  ? 'text-primary-600 dark:text-primary-400'
                  : 'text-gray-500 dark:text-gray-400'
              )
            }
          >
            <item.icon className="h-5 w-5" />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
