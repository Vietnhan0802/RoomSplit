import { useAuth } from '../../hooks/useAuth';

const DAYS_VI = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];

export default function WelcomeSection() {
  const { user } = useAuth();
  const now = new Date();
  const dayName = DAYS_VI[now.getDay()];
  const dateStr = `${dayName}, ${now.getDate().toString().padStart(2, '0')}/${(now.getMonth() + 1).toString().padStart(2, '0')}/${now.getFullYear()}`;

  return (
    <div>
      <h1 className="text-2xl font-bold">
        Chào {user?.fullName}!
      </h1>
      <p className="text-gray-500 dark:text-gray-400">
        Hôm nay {dateStr}
      </p>
    </div>
  );
}
