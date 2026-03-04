import { useEffect, useState, useCallback } from 'react';
import { Pencil, Trash2, Plus, Loader2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  fetchMerchants,
  createMerchant,
  updateMerchant,
  deleteMerchant,
} from './merchantsSlice';
import { fetchCategories } from './categoriesSlice';
import type { MerchantRule, CreateMerchantRequest, UpdateMerchantRequest } from './merchantTypes';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
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

interface MerchantFormState {
  normalizedName: string;
  matchPattern: string;
  matchType: string;
  categoryId: string;
}

const defaultFormState: MerchantFormState = {
  normalizedName: '',
  matchPattern: '',
  matchType: 'Exact',
  categoryId: '',
};

const matchTypeOptions = ['Exact', 'Contains', 'Regex'];

export default function MerchantRulesPage() {
  const dispatch = useAppDispatch();
  const { merchants, loading, error } = useAppSelector((state) => state.merchants);
  const { categories } = useAppSelector((state) => state.categories);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingMerchant, setEditingMerchant] = useState<MerchantRule | null>(null);
  const [form, setForm] = useState<MerchantFormState>(defaultFormState);
  const [deleteConfirmId, setDeleteConfirmId] = useState<number | null>(null);

  useEffect(() => {
    dispatch(fetchMerchants());
    dispatch(fetchCategories());
  }, [dispatch]);

  const openCreateDialog = useCallback(() => {
    setEditingMerchant(null);
    setForm(defaultFormState);
    setDialogOpen(true);
  }, []);

  const openEditDialog = useCallback((merchant: MerchantRule) => {
    setEditingMerchant(merchant);
    setForm({
      normalizedName: merchant.normalizedName,
      matchPattern: merchant.matchPattern,
      matchType: merchant.matchType,
      categoryId: String(merchant.categoryId),
    });
    setDialogOpen(true);
  }, []);

  const handleSubmit = useCallback(() => {
    const categoryId = Number(form.categoryId);
    if (!categoryId) return;

    if (editingMerchant) {
      const data: UpdateMerchantRequest = {
        normalizedName: form.normalizedName,
        matchPattern: form.matchPattern,
        matchType: form.matchType,
        categoryId,
      };
      dispatch(updateMerchant({ id: editingMerchant.id, data }));
    } else {
      const data: CreateMerchantRequest = {
        normalizedName: form.normalizedName,
        matchPattern: form.matchPattern,
        matchType: form.matchType,
        categoryId,
      };
      dispatch(createMerchant(data));
    }

    setDialogOpen(false);
    setEditingMerchant(null);
    setForm(defaultFormState);
  }, [dispatch, editingMerchant, form]);

  const handleDelete = useCallback(
    (id: number) => {
      dispatch(deleteMerchant(id));
      setDeleteConfirmId(null);
    },
    [dispatch]
  );

  const isFormValid =
    form.normalizedName.trim() !== '' &&
    form.matchPattern.trim() !== '' &&
    form.matchType !== '' &&
    form.categoryId !== '';

  const merchantToDelete = deleteConfirmId !== null
    ? merchants.find((m) => m.id === deleteConfirmId)
    : null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-1">Merchant Rules</h2>
          <p className="text-muted-foreground">
            Manage merchant matching rules for automatic categorization
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <Plus className="mr-2 h-4 w-4" />
          Add Rule
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
                <TableHead>Name</TableHead>
                <TableHead>Pattern</TableHead>
                <TableHead>Match Type</TableHead>
                <TableHead>Category</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {merchants.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                    No merchant rules found. Add one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                merchants.map((merchant) => (
                  <TableRow key={merchant.id}>
                    <TableCell className="font-medium">
                      {merchant.normalizedName}
                    </TableCell>
                    <TableCell>
                      <code className="text-sm bg-muted px-1.5 py-0.5 rounded">
                        {merchant.matchPattern}
                      </code>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{merchant.matchType}</Badge>
                    </TableCell>
                    <TableCell>
                      {merchant.categoryName ?? 'Unknown'}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEditDialog(merchant)}
                          title="Edit rule"
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteConfirmId(merchant.id)}
                          title="Delete rule"
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
              {editingMerchant ? 'Edit Merchant Rule' : 'Add Merchant Rule'}
            </DialogTitle>
            <DialogDescription className="sr-only">
              {editingMerchant
                ? 'Edit an existing merchant rule'
                : 'Create a new merchant rule'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <label htmlFor="merchant-name" className="text-sm font-medium">
                Name
              </label>
              <Input
                id="merchant-name"
                value={form.normalizedName}
                onChange={(e) => setForm({ ...form, normalizedName: e.target.value })}
                placeholder="e.g. Walmart"
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="merchant-pattern" className="text-sm font-medium">
                Pattern
              </label>
              <Input
                id="merchant-pattern"
                value={form.matchPattern}
                onChange={(e) => setForm({ ...form, matchPattern: e.target.value })}
                placeholder="e.g. WAL-MART"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">
                Match Type
              </label>
              <Select
                value={form.matchType}
                onValueChange={(value) => setForm({ ...form, matchType: value })}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select match type" />
                </SelectTrigger>
                <SelectContent>
                  {matchTypeOptions.map((option) => (
                    <SelectItem key={option} value={option}>
                      {option}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">
                Category
              </label>
              <Select
                value={form.categoryId}
                onValueChange={(value) => setForm({ ...form, categoryId: value })}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a category" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={String(category.id)}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!isFormValid}>
              {editingMerchant ? 'Save Changes' : 'Create'}
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
            <DialogTitle>Delete Merchant Rule</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the rule for &quot;{merchantToDelete?.normalizedName}&quot;?
              This action cannot be undone.
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