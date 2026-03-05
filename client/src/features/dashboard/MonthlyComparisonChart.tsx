import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { useAppSelector } from '@/store/hooks';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

const formatCurrency = (value: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

export default function MonthlyComparisonChart() {
  const monthlyComparison = useAppSelector((state) => state.dashboard.monthlyComparison);

  if (monthlyComparison.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Monthly Comparison</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-8">
            No comparison data available.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Monthly Comparison</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={monthlyComparison}>
            <XAxis dataKey="categoryName" />
            <YAxis tickFormatter={(v: number) => formatCurrency(v)} />
            <Tooltip formatter={(value: number) => formatCurrency(value)} />
            <Legend />
            <Bar dataKey="currentMonth" name="Current Month" fill="#6366f1" />
            <Bar dataKey="previousMonth" name="Previous Month" fill="#a5b4fc" />
            <Bar dataKey="threeMonthAverage" name="3-Month Average" fill="#d1d5db" />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
