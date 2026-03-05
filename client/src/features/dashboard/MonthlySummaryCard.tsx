import { TrendingUp, TrendingDown } from 'lucide-react';
import { useAppSelector } from '@/store/hooks';
import { Card, CardContent } from '@/components/ui/card';

const formatCurrency = (amount: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

export default function MonthlySummaryCard() {
  const summary = useAppSelector((state) => state.dashboard.summary);

  if (!summary) {
    return null;
  }

  const { totalIncome, totalExpenses, netSavings, percentChangeExpenses } = summary;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
      <Card>
        <CardContent>
          <p className="text-sm text-muted-foreground">Total Income</p>
          <p className="text-2xl font-bold text-green-600">
            {formatCurrency(totalIncome)}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <p className="text-sm text-muted-foreground">Total Expenses</p>
          <p className="text-2xl font-bold text-red-600">
            {formatCurrency(totalExpenses)}
          </p>
          {percentChangeExpenses !== null && (
            <div className="flex items-center gap-1 mt-1">
              {percentChangeExpenses > 0 ? (
                <TrendingUp className="h-4 w-4 text-red-500" />
              ) : (
                <TrendingDown className="h-4 w-4 text-green-500" />
              )}
              <span
                className={`text-xs ${
                  percentChangeExpenses > 0 ? 'text-red-500' : 'text-green-500'
                }`}
              >
                {Math.abs(percentChangeExpenses).toFixed(1)}% vs last month
              </span>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <p className="text-sm text-muted-foreground">Net Savings</p>
          <p
            className={`text-2xl font-bold ${
              netSavings >= 0 ? 'text-green-600' : 'text-red-600'
            }`}
          >
            {formatCurrency(netSavings)}
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
