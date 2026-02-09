import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Home, Wallet, TrendingUp, TrendingDown } from 'lucide-react';
import Card from '../../components/ui/Card';
import { CardSkeleton } from '../../components/ui/Skeleton';
import { useAuth } from '../../hooks/useAuth';
import { roomsApi } from '../../api/rooms';
import { financeApi } from '../../api/finance';
import { formatCurrency } from '../../utils/formatCurrency';
import { ROUTES } from '../../constants';
import type { Room, Transaction } from '../../types';

export default function Dashboard() {
  const { user } = useAuth();
  const [rooms, setRooms] = useState<Room[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const now = new Date();
    Promise.all([
      roomsApi.getMyRooms(),
      financeApi.getTransactions(now.getMonth() + 1, now.getFullYear()),
    ])
      .then(([roomsRes, txRes]) => {
        setRooms(roomsRes.data.data || []);
        setTransactions(txRes.data.data || []);
      })
      .finally(() => setIsLoading(false));
  }, []);

  const totalIncome = transactions.filter((t) => t.type === 'Income').reduce((s, t) => s + t.amount, 0);
  const totalExpense = transactions.filter((t) => t.type === 'Expense').reduce((s, t) => s + t.amount, 0);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <CardSkeleton />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => <CardSkeleton key={i} />)}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Xin chào, {user?.fullName}!</h1>
        <p className="text-gray-500">Tổng quan tháng {new Date().getMonth() + 1}/{new Date().getFullYear()}</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-blue-100 p-2 dark:bg-blue-900/30">
              <Home className="h-5 w-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Phòng trọ</p>
              <p className="text-xl font-bold">{rooms.length}</p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-green-100 p-2 dark:bg-green-900/30">
              <TrendingUp className="h-5 w-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Thu nhập</p>
              <p className="text-xl font-bold text-green-600">{formatCurrency(totalIncome)}</p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-red-100 p-2 dark:bg-red-900/30">
              <TrendingDown className="h-5 w-5 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Chi tiêu</p>
              <p className="text-xl font-bold text-red-600">{formatCurrency(totalExpense)}</p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-purple-100 p-2 dark:bg-purple-900/30">
              <Wallet className="h-5 w-5 text-purple-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Còn lại</p>
              <p className="text-xl font-bold">{formatCurrency(totalIncome - totalExpense)}</p>
            </div>
          </div>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <div className="mb-4 flex items-center justify-between">
            <h2 className="font-semibold">Phòng trọ của tôi</h2>
            <Link to={ROUTES.ROOMS} className="text-sm text-primary-600 hover:underline">Xem tất cả</Link>
          </div>
          {rooms.length === 0 ? (
            <p className="text-center text-sm text-gray-400 py-8">Bạn chưa tham gia phòng nào</p>
          ) : (
            <div className="space-y-2">
              {rooms.slice(0, 3).map((room) => (
                <Link key={room.id} to={`/rooms/${room.id}`} className="flex items-center gap-3 rounded-lg p-2 hover:bg-gray-50 dark:hover:bg-gray-700">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100 text-primary-700 font-medium">
                    {room.name.charAt(0)}
                  </div>
                  <div>
                    <p className="font-medium">{room.name}</p>
                    <p className="text-xs text-gray-500">{room.memberCount} thành viên</p>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </Card>

        <Card>
          <div className="mb-4 flex items-center justify-between">
            <h2 className="font-semibold">Giao dịch gần đây</h2>
            <Link to={ROUTES.TRANSACTIONS} className="text-sm text-primary-600 hover:underline">Xem tất cả</Link>
          </div>
          {transactions.length === 0 ? (
            <p className="text-center text-sm text-gray-400 py-8">Chưa có giao dịch</p>
          ) : (
            <div className="space-y-2">
              {transactions.slice(0, 5).map((tx) => (
                <div key={tx.id} className="flex items-center justify-between rounded-lg p-2">
                  <div>
                    <p className="text-sm font-medium">{tx.description || tx.category}</p>
                    <p className="text-xs text-gray-500">{tx.category}</p>
                  </div>
                  <span className={tx.type === 'Income' ? 'font-medium text-green-600' : 'font-medium text-red-600'}>
                    {tx.type === 'Income' ? '+' : '-'}{formatCurrency(tx.amount)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
