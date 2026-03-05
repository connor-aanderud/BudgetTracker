import { call, put, takeLatest } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type { Income, CreateIncomeRequest, UpdateIncomeRequest } from './types';
import {
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
} from './incomeSlice';

function* handleFetchIncome() {
  try {
    const response: { data: ApiResponse<Income[]> } = yield call(
      [apiClient, apiClient.get],
      '/income'
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchIncomeSuccess(apiResponse.data));
    } else {
      yield put(fetchIncomeFail(apiResponse.message ?? 'Failed to fetch income.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to fetch income.';
    yield put(fetchIncomeFail(message));
  }
}

function* handleCreateIncome(action: PayloadAction<CreateIncomeRequest>) {
  try {
    const response: { data: ApiResponse<Income> } = yield call(
      [apiClient, apiClient.post],
      '/income',
      action.payload
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(createIncomeSuccess());
      yield put(fetchIncome());
    } else {
      yield put(createIncomeFail(apiResponse.message ?? 'Failed to create income.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to create income.';
    yield put(createIncomeFail(message));
  }
}

function* handleUpdateIncome(
  action: PayloadAction<{ id: number; data: UpdateIncomeRequest }>
) {
  try {
    const { id, data } = action.payload;
    const response: { data: ApiResponse<Income> } = yield call(
      [apiClient, apiClient.put],
      `/income/${id}`,
      data
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(updateIncomeSuccess());
      yield put(fetchIncome());
    } else {
      yield put(updateIncomeFail(apiResponse.message ?? 'Failed to update income.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to update income.';
    yield put(updateIncomeFail(message));
  }
}

function* handleDeleteIncome(action: PayloadAction<number>) {
  try {
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.delete],
      `/income/${action.payload}`
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(deleteIncomeSuccess());
      yield put(fetchIncome());
    } else {
      yield put(deleteIncomeFail(apiResponse.message ?? 'Failed to delete income.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to delete income.';
    yield put(deleteIncomeFail(message));
  }
}

export function* incomeSaga() {
  yield takeLatest(fetchIncome.type, handleFetchIncome);
  yield takeLatest(createIncome.type, handleCreateIncome);
  yield takeLatest(updateIncome.type, handleUpdateIncome);
  yield takeLatest(deleteIncome.type, handleDeleteIncome);
}
