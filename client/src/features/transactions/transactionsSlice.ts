import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { PagedTransactions, TransactionFilters } from './types';

const defaultFilters: TransactionFilters = {
  search: '',
  sortBy: 'date',
  sortDir: 'desc',
  page: 1,
  pageSize: 25,
  dateFrom: null,
  dateTo: null,
  categoryIds: null,
  amountMin: null,
  amountMax: null,
};

interface TransactionsState {
  data: PagedTransactions | null;
  filters: TransactionFilters;
  selectedIds: number[];
  loading: boolean;
  error: string | null;
  bulkLoading: boolean;
}

const initialState: TransactionsState = {
  data: null,
  filters: defaultFilters,
  selectedIds: [],
  loading: false,
  error: null,
  bulkLoading: false,
};

const transactionsSlice = createSlice({
  name: 'transactions',
  initialState,
  reducers: {
    fetchTransactions(state) {
      state.loading = true;
      state.error = null;
    },
    fetchTransactionsSuccess(state, action: PayloadAction<PagedTransactions>) {
      state.loading = false;
      state.data = action.payload;
    },
    fetchTransactionsFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    updateFilters(state, action: PayloadAction<Partial<TransactionFilters>>) {
      state.filters = { ...state.filters, ...action.payload, page: 1 };
    },
    setPage(state, action: PayloadAction<number>) {
      state.filters.page = action.payload;
    },
    toggleSelect(state, action: PayloadAction<number>) {
      const id = action.payload;
      const idx = state.selectedIds.indexOf(id);
      if (idx >= 0) {
        state.selectedIds.splice(idx, 1);
      } else {
        state.selectedIds.push(id);
      }
    },
    selectAll(state) {
      if (state.data) {
        state.selectedIds = state.data.items.map((t) => t.id);
      }
    },
    clearSelection(state) {
      state.selectedIds = [];
    },
    bulkCategorize(
      state,
      _action: PayloadAction<{ ids: number[]; categoryId: number }>
    ) {
      state.bulkLoading = true;
      state.error = null;
    },
    bulkCategorizeSuccess(state) {
      state.bulkLoading = false;
    },
    bulkCategorizeFail(state, action: PayloadAction<string>) {
      state.bulkLoading = false;
      state.error = action.payload;
    },
    bulkDelete(state, _action: PayloadAction<{ ids: number[] }>) {
      state.bulkLoading = true;
      state.error = null;
    },
    bulkDeleteSuccess(state) {
      state.bulkLoading = false;
      state.selectedIds = [];
    },
    bulkDeleteFail(state, action: PayloadAction<string>) {
      state.bulkLoading = false;
      state.error = action.payload;
    },
    reassignCategory(
      state,
      _action: PayloadAction<{ transactionId: number; categoryId: number }>
    ) {
      state.error = null;
    },
    reassignCategorySuccess(state) {
      void state;
    },
    reassignCategoryFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
  },
});

export const {
  fetchTransactions,
  fetchTransactionsSuccess,
  fetchTransactionsFail,
  updateFilters,
  setPage,
  toggleSelect,
  selectAll,
  clearSelection,
  bulkCategorize,
  bulkCategorizeSuccess,
  bulkCategorizeFail,
  bulkDelete,
  bulkDeleteSuccess,
  bulkDeleteFail,
  reassignCategory,
  reassignCategorySuccess,
  reassignCategoryFail,
} = transactionsSlice.actions;

export default transactionsSlice.reducer;