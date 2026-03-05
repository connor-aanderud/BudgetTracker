import { useAppSelector } from '@/store/hooks';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from '@/components/ui/table';

const formatCurrency = (amount: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

export default function TopMerchantsTable() {
  const topMerchants = useAppSelector((state) => state.dashboard.topMerchants);

  if (topMerchants.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Top Merchants</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-8">
            No merchant data available.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Top Merchants</CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-12">#</TableHead>
              <TableHead>Merchant</TableHead>
              <TableHead className="text-right">Total Spent</TableHead>
              <TableHead className="text-right">Transactions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {topMerchants.map((merchant, index) => (
              <TableRow key={merchant.merchantId}>
                <TableCell className="font-medium">{index + 1}</TableCell>
                <TableCell>{merchant.merchantName}</TableCell>
                <TableCell className="text-right">
                  {formatCurrency(merchant.totalAmount)}
                </TableCell>
                <TableCell className="text-right">
                  {merchant.transactionCount}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
