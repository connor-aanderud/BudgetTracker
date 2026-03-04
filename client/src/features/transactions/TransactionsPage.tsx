import { useEffect, useRef, useState, useCallback } from 'react';
import { Search, Loader2, ChevronUp, ChevronDown, Trash2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  fetchTransactions,
  updateFilters,
  setPage,
  toggleSelect,
  selectAll,
  clearSelection,
  bulkCategorize,
  bulkDelete,
  reassignCategory,
} from './transactionsSlice';
import { fetchCategories } from '@/features/categories/categoriesSlice';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  const year = d.getFullYear();
  return `${month}/${day}/${year}`;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

function truncate(str: string, maxLen: number): string {
  if (str.length <= maxLen) return str;
  return str.slice(0, maxLen) + '...';
}

export default function TransactionsPage() {
  const dispatch = useAppDispatch();
  const { data, filters, selectedIds, loading, error, bulkLoading } =
    useAppSelector((state) => state.transactions);
  const { categories } = useAppSelector((state) => state.categories);

  const [searchInput, setSearchInput] = useState(filters.search);
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [bulkCategoryId, setBulkCategoryId] = useState<string>('');

  useEffect(() => {
    dispatch(fetchTransactions());
    dispatch(fetchCategories());
  }, [dispatch]);

  const handleSearchChange = useCallback(
    (value: string) => {
      setSearchInput(value);
      if (searchTimerRef.current) {
        clearTimeout(searchTimerRef.current);
      }
      searchTimerRef.current = setTimeout(() => {
        dispatch(updateFilters({ search: value }));
      }, 300);
    },
    [dispatch]
  );

  useEffect(() => {
    return () => {
      if (searchTimerRef.current) {
        clearTimeout(searchTimerRef.current);
      }
    };
  }, []);

  const handleSort = useCallback(
    (column: string) => {
      if (filters.sortBy === column) {
        dispatch(
          updateFilters({
            sortDir: filters.sortDir === 'asc' ? 'desc' : 'asc',
          })
        );
      } else {
        dispatch(updateFilters({ sortBy: column, sortDir: 'asc' }));
      }
    },
    [dispatch, filters.sortBy, filters.sortDir]
  );

  const handleInlineCategory = useCallback(
    (transactionId: number, categoryId: string) => {
      if (categoryId === 'none') return;
      dispatch(
        reassignCategory({
          transactionId,
          categoryId: Number(categoryId),
        })
      );
    },
    [dispatch]
  );

  const handleBulkCategorize = useCallback(() => {
    if (!bulkCategoryId || selectedIds.length === 0) return;
    dispatch(
      bulkCategorize({
        ids: selectedIds,
        categoryId: Number(bulkCategoryId),
      })
    );
    setBulkCategoryId('');
  }, [dispatch, bulkCategoryId, selectedIds]);

  const handleBulkDelete = useCallback(() => {
    if (selectedIds.length === 0) return;
    dispatch(bulkDelete({ ids: selectedIds }));
    setDeleteConfirmOpen(false);
  }, [dispatch, selectedIds]);

  const handleSelectAllToggle = useCallback(() => {
    if (!data) return;
    const allSelected =
      data.items.length > 0 &&
      data.items.every((t) => selectedIds.includes(t.id));
    if (allSelected) {
      dispatch(clearSelection());
    } else {
      dispatch(selectAll());
    }
  }, [dispatch, data, selectedIds]);

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 1;
  const currentPage = filters.page;
  const pageSize = filters.pageSize;
  const startItem = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  const allSelected =
    items.length > 0 && items.every((t) => selectedIds.includes(t.id));

  const SortIcon = ({ column }: { column: string }) => {
    if (filters.sortBy !== column)
      return <span className="inline-block w-4" />;
    return filters.sortDir === 'asc' ? (
      <ChevronUp className="inline h-4 w-4" />
    ) : (
      <ChevronDown className="inline h-4 w-4" />
    );
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold mb-1">Transactions</h2>
        <p className="text-muted-foreground">
          View and manage your transactions
        </p>
      </div>

      {/* Search bar */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search transactions..."
          value={searchInput}
          onChange={(e) => handleSearchChange(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Filter row */}
      <div className="flex flex-wrap items-end gap-4">
        <div className="space-y-1">
          <label className="text-sm font-medium">Date From</label>
          <Input
            type="date"
            value={filters.dateFrom ?? ''}
            onChange={(e) =>
              dispatch(
                updateFilters({
                  dateFrom: e.target.value || null,
                })
              )
            }
            className="w-40"
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium">Date To</label>
          <Input
            type="date"
            value={filters.dateTo ?? ''}
            onChange={(e) =>
              dispatch(
                updateFilters({
                  dateTo: e.target.value || null,
                })
              )
            }
            className="w-40"
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium">Category</label>
          <Select
            value={
              filters.categoryIds && filters.categoryIds.length === 1
                ? String(filters.categoryIds[0])
                : 'all'
            }
            onValueChange={(value) =>
              dispatch(
                updateFilters({
                  categoryIds:
                    value === 'all' ? null : [Number(value)],
                })
              )
            }
          >
            <SelectTrigger className="w-44">
              <SelectValue placeholder="All categories" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All categories</SelectItem>
              {categories.map((cat) => (
                <SelectItem key={cat.id} value={String(cat.id)}>
                  {cat.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium">Min Amount</label>
          <Input
            type="number"
            placeholder="0.00"
            value={filters.amountMin ?? ''}
            onChange={(e) =>
              dispatch(
                updateFilters({
                  amountMin: e.target.value ? Number(e.target.value) : null,
                })
              )
            }
            className="w-28"
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium">Max Amount</label>
          <Input
            type="number"
            placeholder="0.00"
            value={filters.amountMax ?? ''}
            onChange={(e) =>
              dispatch(
                updateFilters({
                  amountMax: e.target.value ? Number(e.target.value) : null,
                })
              )
            }
            className="w-28"
          />
        </div>
      </div>

      {/* Bulk action bar */}
      {selectedIds.length > 0 && (
        <div className="flex items-center gap-4 rounded-md border bg-muted/50 p-3">
          <span className="text-sm font-medium">
            {selectedIds.length} selected
          </span>
          <div className="flex items-center gap-2">
            <Select
              value={bulkCategoryId}
              onValueChange={setBulkCategoryId}
            >
              <SelectTrigger className="w-44">
                <SelectValue placeholder="Assign category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((cat) => (
                  <SelectItem key={cat.id} value={String(cat.id)}>
                    {cat.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              size="sm"
              onClick={handleBulkCategorize}
              disabled={!bulkCategoryId || bulkLoading}
            >
              Apply
            </Button>
          </div>
          <Button
            variant="destructive"
            size="sm"
            onClick={() => setDeleteConfirmOpen(true)}
            disabled={bulkLoading}
          >
            <Trash2 className="mr-1 h-4 w-4" />
            Delete
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => dispatch(clearSelection())}
          >
            Clear selection
          </Button>
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="rounded-md bg-destructive/10 border border-destructive/30 p-4 text-sm text-destructive">
          {error}
        </div>
      )}

      {/* Data table */}
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-10">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={handleSelectAllToggle}
                    className="h-4 w-4 cursor-pointer"
                  />
                </TableHead>
                <TableHead
                  className="cursor-pointer select-none"
                  onClick={() => handleSort('date')}
                >
                  Date <SortIcon column="date" />
                </TableHead>
                <TableHead>Description</TableHead>
                <TableHead
                  className="cursor-pointer select-none text-right"
                  onClick={() => handleSort('amount')}
                >
                  Amount <SortIcon column="amount" />
                </TableHead>
                <TableHead>Category</TableHead>
                <TableHead>Statement</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={6}
                    className="text-center text-muted-foreground py-8"
                  >
                    No transactions found.
                  </TableCell>
                </TableRow>
              ) : (
                items.map((txn) => (
                  <TableRow key={txn.id}>
                    <TableCell>
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(txn.id)}
                        onChange={() => dispatch(toggleSelect(txn.id))}
                        className="h-4 w-4 cursor-pointer"
                      />
                    </TableCell>
                    <TableCell className="whitespace-nowrap">
                      {formatDate(txn.transactionDate)}
                    </TableCell>
                    <TableCell
                      className="max-w-[250px] truncate"
                      title={txn.rawDescription}
                    >
                      {truncate(txn.rawDescription, 50)}
                    </TableCell>
                    <TableCell
                      className={`text-right whitespace-nowrap font-medium ${
                        txn.isCredit
                          ? 'text-green-600 dark:text-green-400'
                          : ''
                      }`}
                    >
                      {txn.isCredit ? '+' : '-'}
                      {formatCurrency(txn.amount)}
                    </TableCell>
                    <TableCell>
                      <Select
                        value={
                          txn.categoryId !== null
                            ? String(txn.categoryId)
                            : 'none'
                        }
                        onValueChange={(value) =>
                          handleInlineCategory(txn.id, value)
                        }
                      >
                        <SelectTrigger className="w-36" size="sm">
                          <SelectValue placeholder="Uncategorized" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="none">Uncategorized</SelectItem>
                          {categories.map((cat) => (
                            <SelectItem
                              key={cat.id}
                              value={String(cat.id)}
                            >
                              {cat.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </TableCell>
                    <TableCell className="text-muted-foreground text-sm">
                      {txn.statementFileName ?? '-'}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Pagination */}
      {totalCount > 0 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Showing {startItem}-{endItem} of {totalCount}
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage <= 1}
              onClick={() => dispatch(setPage(currentPage - 1))}
            >
              Previous
            </Button>
            <span className="text-sm">
              Page {currentPage} of {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage >= totalPages}
              onClick={() => dispatch(setPage(currentPage + 1))}
            >
              Next
            </Button>
          </div>
        </div>
      )}

      {/* Bulk Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Transactions</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete {selectedIds.length} selected
              transaction{selectedIds.length !== 1 ? 's' : ''}? This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteConfirmOpen(false)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleBulkDelete}
              disabled={bulkLoading}
            >
              {bulkLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : null}
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}