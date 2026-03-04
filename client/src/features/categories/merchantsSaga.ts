import { call, put, takeLatest } from "redux-saga/effects";
import type { PayloadAction } from "@reduxjs/toolkit";
import apiClient from "@/services/apiClient";
import type { ApiResponse } from "@/shared/types/api";
import type {
  MerchantRule,
  CreateMerchantRequest,
  UpdateMerchantRequest,
} from "./merchantTypes";
import {
  fetchMerchants,
  fetchMerchantsSuccess,
  fetchMerchantsFail,
  createMerchant,
  createMerchantSuccess,
  createMerchantFail,
  updateMerchant,
  updateMerchantSuccess,
  updateMerchantFail,
  deleteMerchant,
  deleteMerchantSuccess,
  deleteMerchantFail,
} from "./merchantsSlice";

function* handleFetchMerchants() {
  try {
    const response: { data: ApiResponse<MerchantRule[]> } = yield call(
      [apiClient, apiClient.get],
      "/merchants",
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchMerchantsSuccess(apiResponse.data));
    } else {
      yield put(
        fetchMerchantsFail(apiResponse.message ?? "Failed to fetch merchants."),
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : "Failed to fetch merchants.";
    yield put(fetchMerchantsFail(message));
  }
}

function* handleCreateMerchant(action: PayloadAction<CreateMerchantRequest>) {
  try {
    const response: { data: ApiResponse<MerchantRule> } = yield call(
      [apiClient, apiClient.post],
      "/merchants",
      action.payload,
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(createMerchantSuccess());
      yield put(fetchMerchants());
    } else {
      yield put(
        createMerchantFail(apiResponse.message ?? "Failed to create merchant."),
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : "Failed to create merchant.";
    yield put(createMerchantFail(message));
  }
}

function* handleUpdateMerchant(
  action: PayloadAction<{ id: number; data: UpdateMerchantRequest }>,
) {
  try {
    const { id, data } = action.payload;
    const response: { data: ApiResponse<MerchantRule> } = yield call(
      [apiClient, apiClient.put],
      `/merchants/${id}`,
      data,
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(updateMerchantSuccess());
      yield put(fetchMerchants());
    } else {
      yield put(
        updateMerchantFail(apiResponse.message ?? "Failed to update merchant."),
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : "Failed to update merchant.";
    yield put(updateMerchantFail(message));
  }
}

function* handleDeleteMerchant(action: PayloadAction<number>) {
  try {
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.delete],
      `/merchants/${action.payload}`,
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(deleteMerchantSuccess());
      yield put(fetchMerchants());
    } else {
      yield put(
        deleteMerchantFail(apiResponse.message ?? "Failed to delete merchant."),
      );
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : "Failed to delete merchant.";
    yield put(deleteMerchantFail(message));
  }
}

export function* merchantsSaga() {
  yield takeLatest(fetchMerchants.type, handleFetchMerchants);
  yield takeLatest(createMerchant.type, handleCreateMerchant);
  yield takeLatest(updateMerchant.type, handleUpdateMerchant);
  yield takeLatest(deleteMerchant.type, handleDeleteMerchant);
}
