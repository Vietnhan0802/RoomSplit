import { useState, useEffect } from 'react';
import MonthNavigator from '../../components/shared/MonthNavigator';
import { CardSkeleton } from '../../components/ui/Skeleton';
import { financeApi } from '../../api/finance';
import OverviewSection from './OverviewSection';
import CategoryBreakdownSection from './CategoryBreakdownSection';
import TrendSection from './TrendSection';
import DailySpendingSection from './DailySpendingSection';
import TopExpensesSection from './TopExpensesSection';
import ComparisonSection from './ComparisonSection';
import type {
  ReportOverview,
  CategoryBreakdownItem,
  MonthlyTrendItem,
  DailySpendingResponse,
  CategoryComparisonItem,
} from '../../types';

export default function Reports() {
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [isLoading, setIsLoading] = useState(true);

  const [overview, setOverview] = useState<ReportOverview | null>(null);
  const [breakdown, setBreakdown] = useState<CategoryBreakdownItem[]>([]);
  const [trend, setTrend] = useState<MonthlyTrendItem[]>([]);
  const [dailySpending, setDailySpending] = useState<DailySpendingResponse | null>(null);
  const [comparison, setComparison] = useState<CategoryComparisonItem[]>([]);

  useEffect(() => {
    let cancelled = false;
    Promise.all([
      financeApi.getReportOverview(month, year).catch(() => null),
      financeApi.getCategoryBreakdown(month, year).catch(() => null),
      financeApi.getMonthlyTrend(6).catch(() => null),
      financeApi.getDailySpending(month, year).catch(() => null),
      financeApi.getCategoryComparison(month, year).catch(() => null),
    ])
      .then(([overviewRes, breakdownRes, trendRes, dailyRes, compRes]) => {
        if (cancelled) return;
        setOverview(overviewRes?.data?.data || null);
        setBreakdown(breakdownRes?.data?.data || []);
        setTrend(trendRes?.data?.data || []);
        setDailySpending(dailyRes?.data?.data || null);
        setComparison(compRes?.data?.data || []);
      })
      .finally(() => { if (!cancelled) setIsLoading(false); });
    return () => { cancelled = true; };
  }, [month, year]);

  const handleMonthChange = (m: number, y: number) => {
    setMonth(m);
    setYear(y);
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <CardSkeleton />
        <div className="grid gap-4 lg:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
        <CardSkeleton />
        <CardSkeleton />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <MonthNavigator month={month} year={year} onChange={handleMonthChange} />

      {overview && <OverviewSection data={overview} />}

      <CategoryBreakdownSection data={breakdown} />

      {trend.length > 0 && <TrendSection data={trend} />}

      {dailySpending && <DailySpendingSection data={dailySpending} />}

      {overview && <TopExpensesSection expenses={overview.topExpenses} />}

      <ComparisonSection data={comparison} month={month} year={year} />
    </div>
  );
}
