import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { useAppSelector } from '@/store/hooks';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

const formatCurrency = (value: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

function getBarColor(percentUsed: number): string {
  if (percentUsed > 100) return '#ef4444';
  if (percentUsed >= 80) return '#eab308';
  return '#22c55e';
}

export default function BudgetVsActualChart() {
  const budgetStatus = useAppSelector((state) => state.dashboard.budgetStatus);

  if (budgetStatus.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Budget vs Actual</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-8">
            No budget data available for this month.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Budget vs Actual</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={budgetStatus} layout="vertical">
            <XAxis type="number" tickFormatter={(v: number) => formatCurrency(v)} />
            <YAxis dataKey="categoryName" type="category" width={120} />
            <Tooltip formatter={(value: number) => formatCurrency(value)} />
            <Bar dataKey="limitAmount" name="Budget" fill="#d1d5db" barSize={16} />
            <Bar dataKey="actualAmount" name="Actual" barSize={16}>
              {budgetStatus.map((entry) => (
                <Cell key={entry.categoryId} fill={getBarColor(entry.percentUsed)} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
