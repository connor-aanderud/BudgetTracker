import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { Income, CreateIncomeRequest, UpdateIncomeRequest } from './types';

interface IncomeState {
  incomes: Income[];
  loading: boolean;
  error: string | null;
}

const initialState: IncomeState = {
  incomes: [],
  loading: false,
  error: null,
};

const incomeSlice = createSlice({
  name: 'income',
  initialState,
  reducers: {
    fetchIncome(state) {
      state.loading = true;
      state.error = null;
    },
    fetchIncomeSuccess(state, action: PayloadAction<Income[]>) {
      state.loading = false;
      state.incomes = action.payload;
    },
    fetchIncomeFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    createIncome(state, _action: PayloadAction<CreateIncomeRequest>) {
      state.error = null;
    },
    createIncomeSuccess(state) {
      state.error = null;
      void state;
    },
    createIncomeFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    updateIncome(
      state,
      _action: PayloadAction<{ id: number; data: UpdateIncomeRequest }>
    ) {
      state.error = null;
    },
    updateIncomeSuccess(state) {
      state.error = null;
      void state;
    },
    updateIncomeFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    deleteIncome(state, _action: PayloadAction<number>) {
      state.error = null;
    },
    deleteIncomeSuccess(state) {
      state.error = null;
      void state;
    },
    deleteIncomeFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
  },
});

export const {
  fetchIncome,
  fetchIncomeSuccess,
  fetchIncomeFail,
  createIncome,
  createIncomeSuccess,
  createIncomeFail,
  updateIncome,
  updateIncomeSuccess,
  updateIncomeFail,
  deleteIncome,
  deleteIncomeSuccess,
  deleteIncomeFail,
} = incomeSlice.actions;

export default incomeSlice.reducer;
