import { useParams, Outlet, NavLink } from 'react-router-dom';
import { Receipt, ListChecks, Users } from 'lucide-react';
import { cn } from '../../utils/cn';

const tabs = [
  { to: 'expenses', icon: Receipt, label: 'Chi phí' },
  { to: 'tasks', icon: ListChecks, label: 'Công việc' },
  { to: 'members', icon: Users, label: 'Thành viên' },
];

export default function RoomDetail() {
  const { roomId } = useParams();

  return (
    <div className="space-y-4">
      <nav className="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={`/rooms/${roomId}/${tab.to}`}
            className={({ isActive }) =>
              cn(
                'flex flex-1 items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
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
    </div>
  );
}
