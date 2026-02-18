import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../components/layout/MainLayout';
import ProtectedRoute from './ProtectedRoute';

// Lazy-loaded pages
const Login = lazy(() => import('../features/auth/Login'));
const Register = lazy(() => import('../features/auth/Register'));
const ForgotPassword = lazy(() => import('../features/auth/ForgotPassword'));
const Dashboard = lazy(() => import('../features/dashboard/Dashboard'));
const RoomList = lazy(() => import('../features/rooms/RoomList'));
const RoomDetail = lazy(() => import('../features/rooms/RoomDetail'));
const ExpenseList = lazy(() => import('../features/rooms/expenses/ExpenseList'));
const TaskList = lazy(() => import('../features/rooms/tasks/TaskList'));
const MemberList = lazy(() => import('../features/rooms/members/MemberList'));
const FinancePage = lazy(() => import('../features/finance/FinancePage'));
const TransactionList = lazy(() => import('../features/finance/transactions/TransactionList'));
const CalendarView = lazy(() => import('../features/finance/calendar/CalendarView'));
const BudgetList = lazy(() => import('../features/finance/budgets/BudgetList'));
const SummaryView = lazy(() => import('../features/finance/summary/SummaryView'));
const Reports = lazy(() => import('../features/reports/Reports'));
const Profile = lazy(() => import('../features/profile/Profile'));
const NotFoundPage = lazy(() => import('../features/NotFoundPage'));

function PageSpinner() {
  return (
    <div className="flex h-full items-center justify-center py-20">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-600 border-t-transparent" />
    </div>
  );
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Suspense fallback={<PageSpinner />}>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />

          {/* Protected routes */}
          <Route
            element={
              <ProtectedRoute>
                <MainLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />

            <Route path="/rooms" element={<RoomList />} />
            <Route path="/rooms/:roomId" element={<RoomDetail />}>
              <Route index element={<Navigate to="expenses" replace />} />
              <Route path="expenses" element={<ExpenseList />} />
              <Route path="tasks" element={<TaskList />} />
              <Route path="members" element={<MemberList room={null} />} />
            </Route>

            <Route path="/finance" element={<FinancePage />}>
              <Route index element={<Navigate to="calendar" replace />} />
              <Route path="calendar" element={<CalendarView />} />
              <Route path="transactions" element={<TransactionList />} />
              <Route path="budgets" element={<BudgetList />} />
              <Route path="summary" element={<SummaryView />} />
            </Route>

            <Route path="/reports" element={<Reports />} />
            <Route path="/profile" element={<Profile />} />
          </Route>

          {/* Redirect & 404 */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
