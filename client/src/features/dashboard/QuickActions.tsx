import { useNavigate } from 'react-router-dom';
import { Plus, Home, Camera } from 'lucide-react';
import { ROUTES } from '../../constants';

export default function QuickActions() {
  const navigate = useNavigate();

  const actions = [
    {
      icon: Plus,
      label: 'Thêm chi tiêu',
      color: 'bg-primary-600 text-white hover:bg-primary-700',
      onClick: () => navigate(ROUTES.TRANSACTIONS),
    },
    {
      icon: Home,
      label: 'Thêm tiền phòng',
      color: 'bg-blue-600 text-white hover:bg-blue-700',
      onClick: () => navigate(ROUTES.ROOMS),
    },
    {
      icon: Camera,
      label: 'Chụp bill nhanh',
      color: 'bg-amber-600 text-white hover:bg-amber-700',
      onClick: () => navigate(ROUTES.TRANSACTIONS),
    },
  ];

  return (
    <div className="flex gap-3">
      {actions.map((action) => (
        <button
          key={action.label}
          onClick={action.onClick}
          className={`flex flex-1 items-center justify-center gap-2 rounded-xl px-4 py-3 text-sm font-medium transition-colors ${action.color}`}
        >
          <action.icon className="h-4 w-4" />
          <span className="hidden sm:inline">{action.label}</span>
          <span className="sm:hidden">{action.label.split(' ').slice(-1)}</span>
        </button>
      ))}
    </div>
  );
}
