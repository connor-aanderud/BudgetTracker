import { useEffect } from 'react';
import { Loader2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { setMonth, fetchDashboard } from './dashboardSlice';
import { Input } from '@/components/ui/input';
import MonthlySummaryCard from './MonthlySummaryCard';
import CategoryBreakdownChart from './CategoryBreakdownChart';
import BudgetVsActualChart from './BudgetVsActualChart';
import SpendingTrendsChart from './SpendingTrendsChart';
import TopMerchantsTable from './TopMerchantsTable';
import MonthlyComparisonChart from './MonthlyComparisonChart';
import OverspendAlerts from './OverspendAlerts';

export default function Dashboard() {
  const dispatch = useAppDispatch();
  const { month, loading, error } = useAppSelector((state) => state.dashboard);

  useEffect(() => {
    dispatch(fetchDashboard(month));
  }, [dispatch, month]);

  const handleMonthChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(setMonth(e.target.value));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-1">Dashboard</h2>
          <p className="text-muted-foreground">
            Overview of your finances
          </p>
        </div>
        <Input
          type="month"
          value={month}
          onChange={handleMonthChange}
          className="w-48"
        />
      </div>

      {error && (
        <div className="rounded-md bg-destructive/10 border border-destructive/30 p-4 text-sm text-destructive">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <>
          <MonthlySummaryCard />

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <CategoryBreakdownChart />
            <BudgetVsActualChart />
          </div>

          <SpendingTrendsChart />

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <TopMerchantsTable />
            <MonthlyComparisonChart />
          </div>

          <OverspendAlerts />
        </>
      )}
    </div>
  );
}
