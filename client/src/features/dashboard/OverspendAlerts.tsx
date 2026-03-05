import { AlertTriangle, CheckCircle } from 'lucide-react';
import { useAppSelector } from '@/store/hooks';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

const formatCurrency = (amount: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

export default function OverspendAlerts() {
  const budgetStatus = useAppSelector((state) => state.dashboard.budgetStatus);

  const overBudget = budgetStatus
    .filter((b) => b.overBudget)
    .sort((a, b) => b.percentUsed - a.percentUsed);

  const approaching = budgetStatus
    .filter((b) => !b.overBudget && b.percentUsed >= 80)
    .sort((a, b) => b.percentUsed - a.percentUsed);

  const hasAlerts = overBudget.length > 0 || approaching.length > 0;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Budget Alerts</CardTitle>
      </CardHeader>
      <CardContent>
        {!hasAlerts ? (
          <div className="flex items-center gap-2 text-green-600 py-4">
            <CheckCircle className="h-5 w-5" />
            <span>All categories within budget</span>
          </div>
        ) : (
          <div className="space-y-3">
            {overBudget.map((item) => (
              <div
                key={item.categoryId}
                className="flex items-start gap-3 rounded-md border border-red-200 bg-red-50 p-4 dark:border-red-900 dark:bg-red-950"
              >
                <AlertTriangle className="h-5 w-5 text-red-600 shrink-0 mt-0.5" />
                <div>
                  <p className="font-medium text-red-800 dark:text-red-200">
                    {item.categoryName} - Over Budget
                  </p>
                  <p className="text-sm text-red-600 dark:text-red-400">
                    Budget: {formatCurrency(item.limitAmount)} | Spent:{' '}
                    {formatCurrency(item.actualAmount)} | Over by:{' '}
                    {formatCurrency(Math.abs(item.remainingAmount))} (
                    {item.percentUsed.toFixed(0)}%)
                  </p>
                </div>
              </div>
            ))}

            {approaching.map((item) => (
              <div
                key={item.categoryId}
                className="flex items-start gap-3 rounded-md border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900 dark:bg-yellow-950"
              >
                <AlertTriangle className="h-5 w-5 text-yellow-600 shrink-0 mt-0.5" />
                <div>
                  <p className="font-medium text-yellow-800 dark:text-yellow-200">
                    {item.categoryName} - Approaching Limit
                  </p>
                  <p className="text-sm text-yellow-600 dark:text-yellow-400">
                    Budget: {formatCurrency(item.limitAmount)} | Spent:{' '}
                    {formatCurrency(item.actualAmount)} | Remaining:{' '}
                    {formatCurrency(item.remainingAmount)} (
                    {item.percentUsed.toFixed(0)}% used)
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
