import { call, put, takeLatest, all } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import apiClient from '@/services/apiClient';
import type { ApiResponse } from '@/shared/types/api';
import type {
  MonthlySummary,
  CategoryBreakdown,
  BudgetStatus,
  SpendingTrend,
  TopMerchant,
  MonthlyComparison,
} from './types';
import {
  fetchDashboard,
  fetchDashboardSuccess,
  fetchDashboardFail,
  fetchTrends,
  fetchTrendsSuccess,
  fetchTrendsFail,
} from './dashboardSlice';

function* handleFetchDashboard(action: PayloadAction<string>) {
  try {
    const month = action.payload;

    const [summaryRes, breakdownRes, budgetRes, trendsRes, merchantsRes, comparisonRes]: [
      { data: ApiResponse<MonthlySummary> },
      { data: ApiResponse<CategoryBreakdown[]> },
      { data: ApiResponse<BudgetStatus[]> },
      { data: ApiResponse<SpendingTrend[]> },
      { data: ApiResponse<TopMerchant[]> },
      { data: ApiResponse<MonthlyComparison[]> },
    ] = yield all([
      call([apiClient, apiClient.get], `/analytics/monthly-summary?month=${month}`),
      call([apiClient, apiClient.get], `/analytics/category-breakdown?month=${month}`),
      call([apiClient, apiClient.get], `/analytics/budget-status?month=${month}`),
      call([apiClient, apiClient.get], `/analytics/trends?months=6`),
      call([apiClient, apiClient.get], `/analytics/top-merchants?months=3&limit=10`),
      call([apiClient, apiClient.get], `/analytics/monthly-comparison?month=${month}`),
    ]);

    const summary = summaryRes.data;
    const breakdown = breakdownRes.data;
    const budget = budgetRes.data;
    const trends = trendsRes.data;
    const merchants = merchantsRes.data;
    const comparison = comparisonRes.data;

    if (
      summary.success && summary.data &&
      breakdown.success && breakdown.data &&
      budget.success && budget.data &&
      trends.success && trends.data &&
      merchants.success && merchants.data &&
      comparison.success && comparison.data
    ) {
      yield put(
        fetchDashboardSuccess({
          summary: summary.data,
          categoryBreakdown: breakdown.data,
          budgetStatus: budget.data,
          trends: trends.data,
          topMerchants: merchants.data,
          monthlyComparison: comparison.data,
        })
      );
    } else {
      const errorMessage =
        summary.message ??
        breakdown.message ??
        budget.message ??
        trends.message ??
        merchants.message ??
        comparison.message ??
        'Failed to load dashboard data.';
      yield put(fetchDashboardFail(errorMessage));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to load dashboard data.';
    yield put(fetchDashboardFail(message));
  }
}

function* handleFetchTrends(action: PayloadAction<{ months: number; categoryId?: number }>) {
  try {
    const { months, categoryId } = action.payload;
    let url = `/analytics/trends?months=${months}`;
    if (categoryId !== undefined) {
      url += `&categoryId=${categoryId}`;
    }

    const response: { data: ApiResponse<SpendingTrend[]> } = yield call(
      [apiClient, apiClient.get],
      url
    );

    const apiResponse = response.data;
    if (apiResponse.success && apiResponse.data) {
      yield put(fetchTrendsSuccess(apiResponse.data));
    } else {
      yield put(fetchTrendsFail(apiResponse.message ?? 'Failed to fetch trends.'));
    }
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : 'Failed to fetch trends.';
    yield put(fetchTrendsFail(message));
  }
}

export function* dashboardSaga() {
  yield takeLatest(fetchDashboard.type, handleFetchDashboard);
  yield takeLatest(fetchTrends.type, handleFetchTrends);
}
