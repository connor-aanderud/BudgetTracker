import { useEffect } from 'react';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { fetchStatements } from '../statementsSlice';

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

function formatPeriod(start: string | null, end: string | null): string {
  if (!start || !end) return '-';
  const [sy, sm, sd] = start.split('-');
  const [ey, em, ed] = end.split('-');
  return `${sm}/${sd}/${sy} - ${em}/${ed}/${ey}`;
}

export default function ImportHistory() {
  const dispatch = useAppDispatch();
  const { statements, loadingStatements } = useAppSelector(
    (state) => state.statements
  );

  useEffect(() => {
    dispatch(fetchStatements());
  }, [dispatch]);

  if (loadingStatements && statements.length === 0) {
    return (
      <Card className="p-6 text-center text-muted-foreground">
        Loading import history...
      </Card>
    );
  }

  if (statements.length === 0) {
    return (
      <Card className="p-6 text-center text-muted-foreground">
        No statements imported yet.
      </Card>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>File</TableHead>
            <TableHead>Source</TableHead>
            <TableHead>Period</TableHead>
            <TableHead className="text-right">Transactions</TableHead>
            <TableHead>Uploaded</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {statements.map((stmt) => (
            <TableRow key={stmt.id}>
              <TableCell className="font-medium">{stmt.fileName}</TableCell>
              <TableCell>
                <Badge variant="outline">{stmt.source}</Badge>
              </TableCell>
              <TableCell className="whitespace-nowrap">
                {formatPeriod(stmt.periodStart, stmt.periodEnd)}
              </TableCell>
              <TableCell className="text-right">
                {stmt.transactionCount}
              </TableCell>
              <TableCell className="whitespace-nowrap">
                {formatDate(stmt.uploadDate)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}