import { call, put, takeLatest } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type { ParseResult, StatementSummary } from './types';
import {
  uploadFile,
  uploadFileSuccess,
  uploadFileFail,
  confirmImport,
  confirmImportSuccess,
  confirmImportFail,
  fetchStatements,
  fetchStatementsSuccess,
  fetchStatementsFail,
} from './statementsSlice';

function* handleUpload(action: PayloadAction<File>) {
  try {
    const formData = new FormData();
    formData.append('file', action.payload);

    const response: { data: ApiResponse<ParseResult> } = yield call(
      [apiClient, apiClient.post],
      '/statements/upload',
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(uploadFileSuccess(apiResponse.data));
    } else {
      yield put(uploadFileFail(apiResponse.message ?? 'Upload failed.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Upload failed unexpectedly.';
    yield put(uploadFileFail(message));
  }
}

function* handleConfirm(action: PayloadAction<ParseResult>) {
  try {
    const response: { data: ApiResponse<{ transactionCount: number }> } =
      yield call([apiClient, apiClient.post], '/statements/confirm', action.payload);

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(
        confirmImportSuccess({
          message: apiResponse.message ?? 'Import complete.',
        })
      );
      yield put(fetchStatements());
    } else {
      yield put(confirmImportFail(apiResponse.message ?? 'Confirm failed.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Confirm failed unexpectedly.';
    yield put(confirmImportFail(message));
  }
}

function* handleFetchStatements() {
  try {
    const response: { data: ApiResponse<StatementSummary[]> } = yield call(
      [apiClient, apiClient.get],
      '/statements'
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchStatementsSuccess(apiResponse.data));
    } else {
      yield put(fetchStatementsFail());
    }
  } catch {
    yield put(fetchStatementsFail());
  }
}

export function* statementsSaga() {
  yield takeLatest(uploadFile.type, handleUpload);
  yield takeLatest(confirmImport.type, handleConfirm);
  yield takeLatest(fetchStatements.type, handleFetchStatements);
}