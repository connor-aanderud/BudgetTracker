import { call, put, takeLatest } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type { BudgetItem, SetBudgetRequest, CopyBudgetsRequest } from './types';
import {
  fetchBudgets,
  fetchBudgetsSuccess,
  fetchBudgetsFail,
  setBudgets,
  setBudgetsSuccess,
  setBudgetsFail,
  copyBudgets,
  copyBudgetsSuccess,
  copyBudgetsFail,
} from './budgetsSlice';

function* handleFetchBudgets(action: PayloadAction<string>) {
  try {
    const month = action.payload;
    const response: { data: ApiResponse<BudgetItem[]> } = yield call(
      [apiClient, apiClient.get],
      `/budgets?month=${month}`
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchBudgetsSuccess(apiResponse.data));
    } else {
      yield put(fetchBudgetsFail(apiResponse.message ?? 'Failed to fetch budgets.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to fetch budgets.';
    yield put(fetchBudgetsFail(message));
  }
}

function* handleSetBudgets(action: PayloadAction<SetBudgetRequest>) {
  try {
    const response: { data: ApiResponse<BudgetItem[]> } = yield call(
      [apiClient, apiClient.put],
      '/budgets',
      action.payload
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(setBudgetsSuccess());
      yield put(fetchBudgets(action.payload.month));
    } else {
      yield put(setBudgetsFail(apiResponse.message ?? 'Failed to save budgets.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to save budgets.';
    yield put(setBudgetsFail(message));
  }
}

function* handleCopyBudgets(action: PayloadAction<CopyBudgetsRequest>) {
  try {
    const response: { data: ApiResponse<boolean> } = yield call(
      [apiClient, apiClient.post],
      '/budgets/copy',
      action.payload
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(copyBudgetsSuccess());
      yield put(fetchBudgets(action.payload.toMonth));
    } else {
      yield put(copyBudgetsFail(apiResponse.message ?? 'Failed to copy budgets.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to copy budgets.';
    yield put(copyBudgetsFail(message));
  }
}

export function* budgetsSaga() {
  yield takeLatest(fetchBudgets.type, handleFetchBudgets);
  yield takeLatest(setBudgets.type, handleSetBudgets);
  yield takeLatest(copyBudgets.type, handleCopyBudgets);
}
