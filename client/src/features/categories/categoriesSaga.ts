import { call, put, takeLatest } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from './types';
import {
  fetchCategories,
  fetchCategoriesSuccess,
  fetchCategoriesFail,
  createCategory,
  createCategorySuccess,
  createCategoryFail,
  updateCategory,
  updateCategorySuccess,
  updateCategoryFail,
  deleteCategory,
  deleteCategorySuccess,
  deleteCategoryFail,
} from './categoriesSlice';

function* handleFetchCategories() {
  try {
    const response: { data: ApiResponse<Category[]> } = yield call(
      [apiClient, apiClient.get],
      '/categories'
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchCategoriesSuccess(apiResponse.data));
    } else {
      yield put(fetchCategoriesFail(apiResponse.message ?? 'Failed to fetch categories.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to fetch categories.';
    yield put(fetchCategoriesFail(message));
  }
}

function* handleCreateCategory(action: PayloadAction<CreateCategoryRequest>) {
  try {
    const response: { data: ApiResponse<Category> } = yield call(
      [apiClient, apiClient.post],
      '/categories',
      action.payload
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(createCategorySuccess());
      yield put(fetchCategories());
    } else {
      yield put(createCategoryFail(apiResponse.message ?? 'Failed to create category.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to create category.';
    yield put(createCategoryFail(message));
  }
}

function* handleUpdateCategory(
  action: PayloadAction<{ id: number; data: UpdateCategoryRequest }>
) {
  try {
    const { id, data } = action.payload;
    const response: { data: ApiResponse<Category> } = yield call(
      [apiClient, apiClient.put],
      `/categories/${id}`,
      data
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(updateCategorySuccess());
      yield put(fetchCategories());
    } else {
      yield put(updateCategoryFail(apiResponse.message ?? 'Failed to update category.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to update category.';
    yield put(updateCategoryFail(message));
  }
}

function* handleDeleteCategory(action: PayloadAction<number>) {
  try {
    const response: { data: ApiResponse<null> } = yield call(
      [apiClient, apiClient.delete],
      `/categories/${action.payload}`
    );

    const apiResponse = response.data;
    if (apiResponse.success) {
      yield put(deleteCategorySuccess());
      yield put(fetchCategories());
    } else {
      yield put(deleteCategoryFail(apiResponse.message ?? 'Failed to delete category.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to delete category.';
    yield put(deleteCategoryFail(message));
  }
}

export function* categoriesSaga() {
  yield takeLatest(fetchCategories.type, handleFetchCategories);
  yield takeLatest(createCategory.type, handleCreateCategory);
  yield takeLatest(updateCategory.type, handleUpdateCategory);
  yield takeLatest(deleteCategory.type, handleDeleteCategory);
}