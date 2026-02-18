import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Home, Wallet, BarChart3, UserCircle } from 'lucide-react';
import { cn } from '../../utils/cn';
import { ROUTES } from '../../constants';

const navItems = [
  { to: ROUTES.DASHBOARD, icon: LayoutDashboard, label: 'Tổng quan' },
  { to: ROUTES.FINANCE, icon: Wallet, label: 'Tài chính' },
  { to: ROUTES.ROOMS, icon: Home, label: 'Phòng trọ' },
  { to: ROUTES.REPORTS, icon: BarChart3, label: 'Báo cáo' },
  { to: ROUTES.PROFILE, icon: UserCircle, label: 'Cá nhân' },
];

export default function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 z-30 border-t border-gray-200 bg-white/80 backdrop-blur-sm dark:border-gray-700 dark:bg-slate-800/80 lg:hidden"
      style={{ paddingBottom: 'env(safe-area-inset-bottom, 0px)' }}
    >
      <div className="flex items-center justify-around">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'relative flex flex-1 flex-col items-center gap-0.5 py-2 text-[10px] transition-colors',
                isActive
                  ? 'text-primary-600 dark:text-primary-400'
                  : 'text-gray-400 dark:text-gray-500'
              )
            }
          >
            {({ isActive }) => (
              <>
                <item.icon className="h-5 w-5" />
                <span>{item.label}</span>
                {isActive && (
                  <span className="absolute -top-px left-1/2 h-0.5 w-8 -translate-x-1/2 rounded-full bg-primary-600 dark:bg-primary-400" />
                )}
              </>
            )}
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
