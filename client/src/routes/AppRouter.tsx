import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../components/layout/MainLayout';
import ProtectedRoute from './ProtectedRoute';
import Login from '../features/auth/Login';
import Register from '../features/auth/Register';
import ForgotPassword from '../features/auth/ForgotPassword';
import Dashboard from '../features/dashboard/Dashboard';
import RoomList from '../features/rooms/RoomList';
import RoomDetail from '../features/rooms/RoomDetail';
import ExpenseList from '../features/rooms/expenses/ExpenseList';
import TaskList from '../features/rooms/tasks/TaskList';
import MemberList from '../features/rooms/members/MemberList';
import FinancePage from '../features/finance/FinancePage';
import TransactionList from '../features/finance/transactions/TransactionList';
import CalendarView from '../features/finance/calendar/CalendarView';
import BudgetList from '../features/finance/budgets/BudgetList';
import SummaryView from '../features/finance/summary/SummaryView';
import Reports from '../features/reports/Reports';
import Profile from '../features/profile/Profile';

export default function AppRouter() {
  return (
    <BrowserRouter>
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

        {/* Redirect */}
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
