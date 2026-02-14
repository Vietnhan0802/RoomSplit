import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { financeApi } from '../../api/finance';
import type { MonthSummary } from '../../types';

interface FinanceContextType {
  month: number;
  year: number;
  setMonth: (m: number) => void;
  setYear: (y: number) => void;
  setMonthYear: (m: number, y: number) => void;
  summary: MonthSummary | null;
  refreshKey: number;
  triggerRefresh: () => void;
}

const FinanceContext = createContext<FinanceContextType | null>(null);

export function FinanceProvider({ children }: { children: React.ReactNode }) {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [summary, setSummary] = useState<MonthSummary | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const setMonthYear = useCallback((m: number, y: number) => {
    setMonth(m);
    setYear(y);
  }, []);

  const triggerRefresh = useCallback(() => {
    setRefreshKey((k) => k + 1);
  }, []);

  useEffect(() => {
    financeApi.getCalendar(month, year).then((res) => {
      if (res.data.data) {
        setSummary(res.data.data.monthSummary);
      }
    }).catch(() => setSummary(null));
  }, [month, year, refreshKey]);

  return (
    <FinanceContext.Provider
      value={{ month, year, setMonth, setYear, setMonthYear, summary, refreshKey, triggerRefresh }}
    >
      {children}
    </FinanceContext.Provider>
  );
}

export function useFinance() {
  const ctx = useContext(FinanceContext);
  if (!ctx) throw new Error('useFinance must be used within FinanceProvider');
  return ctx;
}
