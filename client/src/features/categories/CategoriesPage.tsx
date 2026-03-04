import { useEffect, useState, useCallback } from 'react';
import { Pencil, Trash2, Plus, Loader2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  fetchCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from './categoriesSlice';
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from './types';
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

interface CategoryFormState {
  name: string;
  color: string;
  icon: string;
}

const defaultFormState: CategoryFormState = {
  name: '',
  color: '#6366f1',
  icon: '',
};

export default function CategoriesPage() {
  const dispatch = useAppDispatch();
  const { categories, loading, error } = useAppSelector((state) => state.categories);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [form, setForm] = useState<CategoryFormState>(defaultFormState);
  const [deleteConfirmId, setDeleteConfirmId] = useState<number | null>(null);

  useEffect(() => {
    dispatch(fetchCategories());
  }, [dispatch]);

  const openCreateDialog = useCallback(() => {
    setEditingCategory(null);
    setForm(defaultFormState);
    setDialogOpen(true);
  }, []);

  const openEditDialog = useCallback((category: Category) => {
    setEditingCategory(category);
    setForm({
      name: category.name,
      color: category.color,
      icon: category.icon ?? '',
    });
    setDialogOpen(true);
  }, []);

  const handleSubmit = useCallback(() => {
    const iconValue = form.icon.trim() || null;

    if (editingCategory) {
      const data: UpdateCategoryRequest = {
        name: form.name,
        color: form.color,
        icon: iconValue,
      };
      dispatch(updateCategory({ id: editingCategory.id, data }));
    } else {
      const data: CreateCategoryRequest = {
        name: form.name,
        color: form.color,
        icon: iconValue,
      };
      dispatch(createCategory(data));
    }

    setDialogOpen(false);
    setEditingCategory(null);
    setForm(defaultFormState);
  }, [dispatch, editingCategory, form]);

  const handleDelete = useCallback(
    (id: number) => {
      dispatch(deleteCategory(id));
      setDeleteConfirmId(null);
    },
    [dispatch]
  );

  const categoryToDelete = deleteConfirmId !== null
    ? categories.find((c) => c.id === deleteConfirmId)
    : null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-1">Categories</h2>
          <p className="text-muted-foreground">
            Manage transaction categories
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <Plus className="mr-2 h-4 w-4" />
          Add Category
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
                <TableHead className="text-right">Transactions</TableHead>
                <TableHead className="text-right">Merchants</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {categories.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={4} className="text-center text-muted-foreground py-8">
                    No categories found. Add one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                categories.map((category) => (
                  <TableRow key={category.id}>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <div
                          className="h-4 w-4 rounded-full shrink-0"
                          style={{ backgroundColor: category.color }}
                        />
                        <span className="font-medium">{category.name}</span>
                        {category.isDefault && (
                          <span className="text-xs text-muted-foreground">(default)</span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-right">{category.transactionCount}</TableCell>
                    <TableCell className="text-right">{category.merchantCount}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEditDialog(category)}
                          title="Edit category"
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteConfirmId(category.id)}
                          disabled={category.transactionCount > 0}
                          title={
                            category.transactionCount > 0
                              ? 'Cannot delete category with transactions'
                              : 'Delete category'
                          }
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
              {editingCategory ? 'Edit Category' : 'Add Category'}
            </DialogTitle>
            <DialogDescription className="sr-only">
              {editingCategory
                ? 'Edit an existing category'
                : 'Create a new category'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <label htmlFor="category-name" className="text-sm font-medium">
                Name
              </label>
              <Input
                id="category-name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="e.g. Groceries"
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="category-color" className="text-sm font-medium">
                Color
              </label>
              <div className="flex items-center gap-3">
                <Input
                  id="category-color"
                  type="color"
                  value={form.color}
                  onChange={(e) => setForm({ ...form, color: e.target.value })}
                  className="w-16 h-10 p-1 cursor-pointer"
                />
                <Input
                  value={form.color}
                  onChange={(e) => setForm({ ...form, color: e.target.value })}
                  placeholder="#6366f1"
                  className="flex-1"
                />
              </div>
            </div>
            <div className="space-y-2">
              <label htmlFor="category-icon" className="text-sm font-medium">
                Icon (optional)
              </label>
              <Input
                id="category-icon"
                value={form.icon}
                onChange={(e) => setForm({ ...form, icon: e.target.value })}
                placeholder="e.g. shopping-cart"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!form.name.trim()}>
              {editingCategory ? 'Save Changes' : 'Create'}
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
            <DialogTitle>Delete Category</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete &quot;{categoryToDelete?.name}&quot;? This action
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