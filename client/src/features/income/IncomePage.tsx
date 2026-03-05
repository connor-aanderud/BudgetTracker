import { useEffect, useState, useCallback } from 'react';
import { Pencil, Trash2, Plus, Loader2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  fetchIncome,
  createIncome,
  updateIncome,
  deleteIncome,
} from './incomeSlice';
import type { Income, CreateIncomeRequest, UpdateIncomeRequest } from './types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface IncomeFormState {
  source: string;
  amount: string;
  date: string;
  isRecurring: boolean;
  frequency: string;
  notes: string;
}

const defaultFormState: IncomeFormState = {
  source: '',
  amount: '',
  date: '',
  isRecurring: false,
  frequency: '',
  notes: '',
};

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

export default function IncomePage() {
  const dispatch = useAppDispatch();
  const { incomes, loading, error } = useAppSelector((state) => state.income);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingIncome, setEditingIncome] = useState<Income | null>(null);
  const [form, setForm] = useState<IncomeFormState>(defaultFormState);
  const [deleteConfirmId, setDeleteConfirmId] = useState<number | null>(null);

  useEffect(() => {
    dispatch(fetchIncome());
  }, [dispatch]);

  const openCreateDialog = useCallback(() => {
    setEditingIncome(null);
    setForm(defaultFormState);
    setDialogOpen(true);
  }, []);

  const openEditDialog = useCallback((income: Income) => {
    setEditingIncome(income);
    setForm({
      source: income.source,
      amount: String(income.amount),
      date: income.date,
      isRecurring: income.isRecurring,
      frequency: income.frequency ?? '',
      notes: income.notes ?? '',
    });
    setDialogOpen(true);
  }, []);

  const handleSubmit = useCallback(() => {
    const notesValue = form.notes.trim() || null;
    const frequencyValue = form.isRecurring && form.frequency ? form.frequency : null;

    if (editingIncome) {
      const data: UpdateIncomeRequest = {
        source: form.source,
        amount: parseFloat(form.amount),
        date: form.date,
        isRecurring: form.isRecurring,
        frequency: frequencyValue,
        notes: notesValue,
      };
      dispatch(updateIncome({ id: editingIncome.id, data }));
    } else {
      const data: CreateIncomeRequest = {
        source: form.source,
        amount: parseFloat(form.amount),
        date: form.date,
        isRecurring: form.isRecurring,
        frequency: frequencyValue,
        notes: notesValue,
      };
      dispatch(createIncome(data));
    }

    setDialogOpen(false);
    setEditingIncome(null);
    setForm(defaultFormState);
  }, [dispatch, editingIncome, form]);

  const handleDelete = useCallback(
    (id: number) => {
      dispatch(deleteIncome(id));
      setDeleteConfirmId(null);
    },
    [dispatch]
  );

  const incomeToDelete = deleteConfirmId !== null
    ? incomes.find((i) => i.id === deleteConfirmId)
    : null;

  const isFormValid =
    form.source.trim() !== '' &&
    form.amount !== '' &&
    !isNaN(parseFloat(form.amount)) &&
    parseFloat(form.amount) > 0 &&
    form.date !== '' &&
    (!form.isRecurring || form.frequency !== '');

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-1">Income</h2>
          <p className="text-muted-foreground">
            Manage your income sources
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <Plus className="mr-2 h-4 w-4" />
          Add Income
        </Button>
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
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Source</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                <TableHead>Date</TableHead>
                <TableHead>Recurring</TableHead>
                <TableHead>Frequency</TableHead>
                <TableHead>Notes</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {incomes.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center text-muted-foreground py-8">
                    No income records found. Add one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                incomes.map((income) => (
                  <TableRow key={income.id}>
                    <TableCell className="font-medium">{income.source}</TableCell>
                    <TableCell className="text-right">{formatCurrency(income.amount)}</TableCell>
                    <TableCell>{income.date}</TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                          income.isRecurring
                            ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                            : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400'
                        }`}
                      >
                        {income.isRecurring ? 'Yes' : 'No'}
                      </span>
                    </TableCell>
                    <TableCell>{income.frequency ?? '-'}</TableCell>
                    <TableCell className="max-w-[200px] truncate">{income.notes ?? '-'}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEditDialog(income)}
                          title="Edit income"
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteConfirmId(income.id)}
                          title="Delete income"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingIncome ? 'Edit Income' : 'Add Income'}
            </DialogTitle>
            <DialogDescription className="sr-only">
              {editingIncome
                ? 'Edit an existing income record'
                : 'Create a new income record'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <label htmlFor="income-source" className="text-sm font-medium">
                Source
              </label>
              <Input
                id="income-source"
                value={form.source}
                onChange={(e) => setForm({ ...form, source: e.target.value })}
                placeholder="e.g. Salary"
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="income-amount" className="text-sm font-medium">
                Amount
              </label>
              <Input
                id="income-amount"
                type="number"
                min="0"
                step="0.01"
                value={form.amount}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
                placeholder="0.00"
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="income-date" className="text-sm font-medium">
                Date
              </label>
              <Input
                id="income-date"
                type="date"
                value={form.date}
                onChange={(e) => setForm({ ...form, date: e.target.value })}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="income-recurring"
                type="checkbox"
                checked={form.isRecurring}
                onChange={(e) =>
                  setForm({
                    ...form,
                    isRecurring: e.target.checked,
                    frequency: e.target.checked ? form.frequency : '',
                  })
                }
                className="h-4 w-4 rounded border-gray-300"
              />
              <label htmlFor="income-recurring" className="text-sm font-medium">
                Recurring
              </label>
            </div>
            {form.isRecurring && (
              <div className="space-y-2">
                <label htmlFor="income-frequency" className="text-sm font-medium">
                  Frequency
                </label>
                <Select
                  value={form.frequency}
                  onValueChange={(value) => setForm({ ...form, frequency: value })}
                >
                  <SelectTrigger id="income-frequency">
                    <SelectValue placeholder="Select frequency" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Weekly">Weekly</SelectItem>
                    <SelectItem value="Biweekly">Biweekly</SelectItem>
                    <SelectItem value="Monthly">Monthly</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            )}
            <div className="space-y-2">
              <label htmlFor="income-notes" className="text-sm font-medium">
                Notes (optional)
              </label>
              <Input
                id="income-notes"
                value={form.notes}
                onChange={(e) => setForm({ ...form, notes: e.target.value })}
                placeholder="Additional notes"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!isFormValid}>
              {editingIncome ? 'Save Changes' : 'Create'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteConfirmId !== null}
        onOpenChange={(open) => { if (!open) setDeleteConfirmId(null); }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Income</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete &quot;{incomeToDelete?.source}&quot;? This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteConfirmId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => deleteConfirmId !== null && handleDelete(deleteConfirmId)}
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
