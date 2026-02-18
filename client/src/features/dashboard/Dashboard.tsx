import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { CardSkeleton } from '../../components/ui/Skeleton';
import { financeApi } from '../../api/finance';
import { roomsApi } from '../../api/rooms';
import WelcomeSection from './WelcomeSection';
import QuickStats from './QuickStats';
import BudgetAlerts from './BudgetAlerts';
import MiniCalendar from './MiniCalendar';
import RecentActivities from './RecentActivities';
import QuickActions from './QuickActions';
import type {
  SummaryResponse,
  BudgetStatus,
  CalendarDay,
  Transaction,
  Room,
  RoomTask,
} from '../../types';

export default function Dashboard() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(true);
  const [summary, setSummary] = useState<SummaryResponse | null>(null);
  const [budgets, setBudgets] = useState<BudgetStatus[]>([]);
  const [calendarDays, setCalendarDays] = useState<CalendarDay[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [todayTaskCount, setTodayTaskCount] = useState(0);

  useEffect(() => {
    const now = new Date();
    const month = now.getMonth() + 1;
    const year = now.getFullYear();
    const todayStr = now.toISOString().split('T')[0];

    Promise.all([
      financeApi.getSummary(month, year).catch(() => null),
      financeApi.getBudgetStatus(month, year).catch(() => null),
      financeApi.getCalendar(month, year).catch(() => null),
      financeApi.getTransactions({ pageSize: 10, sortBy: 'CreatedAt', order: 'desc' }).catch(() => null),
      roomsApi.getMyRooms().catch(() => null),
    ])
      .then(async ([summaryRes, budgetRes, calendarRes, txRes, roomsRes]) => {
        setSummary(summaryRes?.data?.data || null);
        setBudgets(budgetRes?.data?.data || []);
        setCalendarDays(calendarRes?.data?.data?.days || []);

        // Handle paginated transaction response
        const txData = txRes?.data?.data;
        if (txData && 'items' in txData) {
          setTransactions(txData.items);
        } else {
          setTransactions([]);
        }

        // Count today's tasks from all rooms
        const rooms: Room[] = roomsRes?.data?.data || [];
        let taskCount = 0;
        for (const room of rooms.slice(0, 5)) {
          try {
            const tasksRes = await roomsApi.getTasks(room.id);
            const tasks: RoomTask[] = tasksRes.data.data || [];
            tasks.forEach((task) => {
              task.assignments.forEach((a) => {
                if (
                  a.dueDate.startsWith(todayStr) &&
                  (a.status === 'Pending' || a.status === 'InProgress')
                ) {
                  taskCount++;
                }
              });
            });
          } catch {
            // ignore
          }
        }
        setTodayTaskCount(taskCount);
      })
      .finally(() => setIsLoading(false));
  }, []);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <CardSkeleton />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <CardSkeleton key={i} />
          ))}
        </div>
        <CardSkeleton />
        <CardSkeleton />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <WelcomeSection />

      <QuickStats
        summary={summary}
        budgets={budgets}
        todayTaskCount={todayTaskCount}
        onTaskClick={() => navigate('/rooms')}
      />

      <BudgetAlerts budgets={budgets} />

      <MiniCalendar calendarDays={calendarDays} />

      <RecentActivities transactions={transactions} />

      <QuickActions />
    </div>
  );
}
