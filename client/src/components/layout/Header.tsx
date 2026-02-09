import { Bell, Moon, Sun, Menu } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';

interface HeaderProps {
  onToggleSidebar: () => void;
  isDark: boolean;
  onToggleTheme: () => void;
}

export default function Header({ onToggleSidebar, isDark, onToggleTheme }: HeaderProps) {
  const { user } = useAuth();

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 dark:border-gray-700 dark:bg-gray-800 lg:px-6">
      <button onClick={onToggleSidebar} className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700 lg:hidden">
        <Menu className="h-5 w-5" />
      </button>

      <div className="flex-1" />

      <div className="flex items-center gap-3">
        <button onClick={onToggleTheme} className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700">
          {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </button>

        <button className="relative rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700">
          <Bell className="h-5 w-5" />
        </button>

        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-sm font-medium text-primary-700">
            {user?.fullName?.charAt(0).toUpperCase()}
          </div>
          <span className="hidden text-sm font-medium md:block">{user?.fullName}</span>
        </div>
      </div>
    </header>
  );
}
