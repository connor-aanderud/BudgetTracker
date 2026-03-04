import { Button } from '@/components/ui/button';
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
import type { ParseResult } from '../types';

interface ImportPreviewProps {
  parseResult: ParseResult;
  onConfirm: () => void;
  onCancel: () => void;
  confirming: boolean;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

function formatDate(dateStr: string): string {
  const [year, month, day] = dateStr.split('-');
  return `${month}/${day}/${year}`;
}

export default function ImportPreview({
  parseResult,
  onConfirm,
  onCancel,
  confirming,
}: ImportPreviewProps) {
  const newCount = parseResult.totalCount - parseResult.duplicateCount;

  return (
    <div className="space-y-4">
      <Card className="p-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-semibold">{parseResult.fileName}</h3>
            <p className="text-sm text-muted-foreground">
              Source: {parseResult.source ?? 'Unknown'} | {parseResult.totalCount}{' '}
              transactions found
            </p>
          </div>
          <div className="flex items-center gap-2">
            {parseResult.duplicateCount > 0 && (
              <Badge variant="secondary">
                {parseResult.duplicateCount} duplicates
              </Badge>
            )}
            <Badge variant="default">{newCount} new</Badge>
          </div>
        </div>
      </Card>

      {parseResult.warnings.length > 0 && (
        <Card className="p-4 border-yellow-500/50 bg-yellow-500/5">
          <p className="text-sm font-medium mb-2">Warnings</p>
          <ul className="text-sm text-muted-foreground space-y-1">
            {parseResult.warnings.map((w, i) => (
              <li key={i}>{w}</li>
            ))}
          </ul>
        </Card>
      )}

      <div className="rounded-md border max-h-96 overflow-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Date</TableHead>
              <TableHead>Description</TableHead>
              <TableHead className="text-right">Amount</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {parseResult.parsedTransactions.map((tx, i) => (
              <TableRow
                key={i}
                className={tx.isDuplicate ? 'opacity-50' : undefined}
              >
                <TableCell className="whitespace-nowrap">
                  {formatDate(tx.transactionDate)}
                </TableCell>
                <TableCell className="max-w-xs truncate">
                  {tx.rawDescription}
                </TableCell>
                <TableCell className="text-right whitespace-nowrap">
                  {formatCurrency(tx.amount)}
                </TableCell>
                <TableCell>
                  <Badge variant={tx.isCredit ? 'default' : 'secondary'}>
                    {tx.isCredit ? 'Credit' : 'Debit'}
                  </Badge>
                </TableCell>
                <TableCell>
                  {tx.isDuplicate ? (
                    <Badge variant="outline">Duplicate</Badge>
                  ) : (
                    <Badge variant="default">New</Badge>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="flex justify-end gap-2">
        <Button variant="outline" onClick={onCancel} disabled={confirming}>
          Cancel
        </Button>
        <Button onClick={onConfirm} disabled={confirming || newCount === 0}>
          {confirming
            ? 'Importing...'
            : `Import ${newCount} Transaction${newCount !== 1 ? 's' : ''}`}
        </Button>
      </div>
    </div>
  );
}