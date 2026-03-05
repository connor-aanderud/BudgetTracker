import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { BudgetItem, SetBudgetRequest, CopyBudgetsRequest } from './types';

interface BudgetsState {
  budgets: BudgetItem[];
  loading: boolean;
  error: string | null;
  saving: boolean;
}

const initialState: BudgetsState = {
  budgets: [],
  loading: false,
  error: null,
  saving: false,
};

const budgetsSlice = createSlice({
  name: 'budgets',
  initialState,
  reducers: {
    fetchBudgets(state, _action: PayloadAction<string>) {
      state.loading = true;
      state.error = null;
    },
    fetchBudgetsSuccess(state, action: PayloadAction<BudgetItem[]>) {
      state.loading = false;
      state.budgets = action.payload;
    },
    fetchBudgetsFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    setBudgets(state, _action: PayloadAction<SetBudgetRequest>) {
      state.saving = true;
      state.error = null;
    },
    setBudgetsSuccess(state) {
      state.saving = false;
      state.error = null;
    },
    setBudgetsFail(state, action: PayloadAction<string>) {
      state.saving = false;
      state.error = action.payload;
    },
    copyBudgets(state, _action: PayloadAction<CopyBudgetsRequest>) {
      state.saving = true;
      state.error = null;
    },
    copyBudgetsSuccess(state) {
      state.saving = false;
      state.error = null;
    },
    copyBudgetsFail(state, action: PayloadAction<string>) {
      state.saving = false;
      state.error = action.payload;
    },
  },
});

export const {
  fetchBudgets,
  fetchBudgetsSuccess,
  fetchBudgetsFail,
  setBudgets,
  setBudgetsSuccess,
  setBudgetsFail,
  copyBudgets,
  copyBudgetsSuccess,
  copyBudgetsFail,
} = budgetsSlice.actions;

export default budgetsSlice.reducer;
