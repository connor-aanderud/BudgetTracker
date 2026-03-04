import { call, put, select, takeLatest } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type { PagedTransactions, TransactionFilters } from './types';
import type { RootState } from '@/store/store';
import {
  fetchTransactions,
  fetchTransactionsSuccess,
  fetchTransactionsFail,
  updateFilters,
  setPage,
  bulkCategorize,
  bulkCategorizeSuccess,
  bulkCategorizeFail,
  bulkDelete,
  bulkDeleteSuccess,
  bulkDeleteFail,
  clearSelection,
  reassignCategory,
  reassignCategorySuccess,
  reassignCategoryFail,
} from './transactionsSlice';

function* handleFetchTransactions() {
  try {
    const filters: TransactionFilters = yield select(
      (state: RootState) => state.transactions.filters
    );

    const params: Record<string, string> = {
      page: String(filters.page),
      pageSize: String(filters.pageSize),
      sortBy: filters.sortBy,
      sortDir: filters.sortDir,
    };

    if (filters.search) params.search = filters.search;
    if (filters.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters.dateTo) params.dateTo = filters.dateTo;
    if (filters.amountMin !== null) params.amountMin = String(filters.amountMin);
    if (filters.amountMax !== null) params.amountMax = String(filters.amountMax);
    if (filters.categoryIds && filters.categoryIds.length > 0) {
      params.categoryIds = filters.categoryIds.join(',');
    }

    const response: { data: ApiResponse<PagedTransactions> } = yield call(
      [apiClient, apiClient.get],
      '/transactions',
      { params }
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchTransactionsSuccess(apiResponse.data));
    } else {
      yield put(
        fetchTransactionsFail(
          apiResponse.message ?? 'Failed to fetch transactions.'
        )
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to fetch transactions.';
    yield put(fetchTransactionsFail(message));
  }
}

function* handleBulkCategorize(
  action: PayloadAction<{ ids: number[]; categoryId: number }>
) {
  try {
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.put],
      '/transactions/bulk-categorize',
      action.payload
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(bulkCategorizeSuccess());
      yield put(fetchTransactions());
    } else {
      yield put(
        bulkCategorizeFail(
          apiResponse.message ?? 'Failed to bulk categorize.'
        )
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to bulk categorize.';
    yield put(bulkCategorizeFail(message));
  }
}

function* handleBulkDelete(action: PayloadAction<{ ids: number[] }>) {
  try {
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.delete],
      '/transactions/bulk-delete',
      { data: action.payload }
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(bulkDeleteSuccess());
      yield put(clearSelection());
      yield put(fetchTransactions());
    } else {
      yield put(
        bulkDeleteFail(apiResponse.message ?? 'Failed to bulk delete.')
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to bulk delete.';
    yield put(bulkDeleteFail(message));
  }
}

function* handleReassignCategory(
  action: PayloadAction<{ transactionId: number; categoryId: number }>
) {
  try {
    const { transactionId, categoryId } = action.payload;
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.put],
      `/transactions/${transactionId}/category`,
      { categoryId }
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(reassignCategorySuccess());
      yield put(fetchTransactions());
    } else {
      yield put(
        reassignCategoryFail(
          apiResponse.message ?? 'Failed to reassign category.'
        )
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to reassign category.';
    yield put(reassignCategoryFail(message));
  }
}

function* handleFiltersChanged() {
  yield put(fetchTransactions());
}

export function* transactionsSaga() {
  yield takeLatest(fetchTransactions.type, handleFetchTransactions);
  yield takeLatest(updateFilters.type, handleFiltersChanged);
  yield takeLatest(setPage.type, handleFiltersChanged);
  yield takeLatest(bulkCategorize.type, handleBulkCategorize);
  yield takeLatest(bulkDelete.type, handleBulkDelete);
  yield takeLatest(reassignCategory.type, handleReassignCategory);
}