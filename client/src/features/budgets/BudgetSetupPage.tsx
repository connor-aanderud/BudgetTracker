import { useEffect, useState, useCallback, useMemo } from 'react';
import { Save, Copy, Loader2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { fetchCategories } from '@/features/categories/categoriesSlice';
import { fetchBudgets, setBudgets, copyBudgets } from './budgetsSlice';
import type { SetBudgetRequest } from './types';
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

function getCurrentMonth(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  return `${year}-${month}`;
}

function getPreviousMonth(month: string): string {
  const [year, mon] = month.split('-').map(Number);
  const date = new Date(year, mon - 2, 1);
  const prevYear = date.getFullYear();
  const prevMonth = String(date.getMonth() + 1).padStart(2, '0');
  return `${prevYear}-${prevMonth}`;
}

function formatMonthLabel(month: string): string {
  const [year, mon] = month.split('-').map(Number);
  const date = new Date(year, mon - 1, 1);
  return date.toLocaleDateString('en-US', { year: 'numeric', month: 'long' });
}

export default function BudgetSetupPage() {
  const dispatch = useAppDispatch();
  const { categories, loading: categoriesLoading } = useAppSelector(
    (state) => state.categories
  );
  const { budgets, loading, error, saving } = useAppSelector(
    (state) => state.budgets
  );

  const [selectedMonth, setSelectedMonth] = useState(getCurrentMonth);
  const [limits, setLimits] = useState<Record<number, string>>({});
  const [copyDialogOpen, setCopyDialogOpen] = useState(false);

  useEffect(() => {
    dispatch(fetchCategories());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchBudgets(selectedMonth));
  }, [dispatch, selectedMonth]);

  useEffect(() => {
    const newLimits: Record<number, string> = {};
    for (const category of categories) {
      const budget = budgets.find((b) => b.categoryId === category.id);
      newLimits[category.id] = budget ? budget.limitAmount.toFixed(2) : '0.00';
    }
    setLimits(newLimits);
  }, [budgets, categories]);

  const handleLimitChange = useCallback(
    (categoryId: number, value: string) => {
      setLimits((prev) => ({ ...prev, [categoryId]: value }));
    },
    []
  );

  const handleSave = useCallback(() => {
    const budgetLines = Object.entries(limits)
      .filter(([, value]) => {
        const num = parseFloat(value);
        return !isNaN(num) && num > 0;
      })
      .map(([categoryId, value]) => ({
        categoryId: parseInt(categoryId, 10),
        limitAmount: parseFloat(parseFloat(value).toFixed(2)),
      }));

    const request: SetBudgetRequest = {
      month: selectedMonth,
      budgets: budgetLines,
    };
    dispatch(setBudgets(request));
  }, [dispatch, limits, selectedMonth]);

  const handleCopyFromPrevious = useCallback(() => {
    const fromMonth = getPreviousMonth(selectedMonth);
    dispatch(copyBudgets({ fromMonth, toMonth: selectedMonth }));
    setCopyDialogOpen(false);
  }, [dispatch, selectedMonth]);

  const previousMonthLabel = useMemo(
    () => formatMonthLabel(getPreviousMonth(selectedMonth)),
    [selectedMonth]
  );

  const selectedMonthLabel = useMemo(
    () => formatMonthLabel(selectedMonth),
    [selectedMonth]
  );

  const isLoading = loading || categoriesLoading;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-1">Budget Setup</h2>
          <p className="text-muted-foreground">
            Set spending limits for each category
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Input
            type="month"
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(e.target.value)}
            className="w-48"
          />
          <Button
            variant="outline"
            onClick={() => setCopyDialogOpen(true)}
            disabled={saving}
          >
            <Copy className="mr-2 h-4 w-4" />
            Copy Previous
          </Button>
        </div>
      </div>

      {error && (
        <div className="rounded-md bg-destructive/10 border border-destructive/30 p-4 text-sm text-destructive">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : categories.length === 0 ? (
        <div className="rounded-md border p-8 text-center text-muted-foreground">
          No categories found. Create categories first before setting budgets.
        </div>
      ) : (
        <div className="space-y-3">
          {categories.map((category) => (
            <div
              key={category.id}
              className="flex items-center justify-between rounded-md border p-4"
            >
              <div className="flex items-center gap-3">
                <div
                  className="h-4 w-4 rounded-full shrink-0"
                  style={{ backgroundColor: category.color }}
                />
                <span className="font-medium">{category.name}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-sm text-muted-foreground">$</span>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  value={limits[category.id] ?? '0.00'}
                  onChange={(e) =>
                    handleLimitChange(category.id, e.target.value)
                  }
                  className="w-32 text-right"
                />
              </div>
            </div>
          ))}

          <div className="flex justify-end pt-4">
            <Button onClick={handleSave} disabled={saving}>
              {saving ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Save className="mr-2 h-4 w-4" />
              )}
              Save Budgets
            </Button>
          </div>
        </div>
      )}

      {/* Copy From Previous Month Dialog */}
      <Dialog open={copyDialogOpen} onOpenChange={setCopyDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Copy Budgets</DialogTitle>
            <DialogDescription>
              Copy all budget limits from {previousMonthLabel} to{' '}
              {selectedMonthLabel}? This will overwrite any existing budgets for{' '}
              {selectedMonthLabel}.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setCopyDialogOpen(false)}
            >
              Cancel
            </Button>
            <Button onClick={handleCopyFromPrevious}>
              <Copy className="mr-2 h-4 w-4" />
              Copy Budgets
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
