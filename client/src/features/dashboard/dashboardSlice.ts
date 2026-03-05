import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type {
  MonthlySummary,
  CategoryBreakdown,
  BudgetStatus,
  SpendingTrend,
  TopMerchant,
  MonthlyComparison,
} from './types';

export interface DashboardState {
  month: string;
  summary: MonthlySummary | null;
  categoryBreakdown: CategoryBreakdown[];
  budgetStatus: BudgetStatus[];
  trends: SpendingTrend[];
  topMerchants: TopMerchant[];
  monthlyComparison: MonthlyComparison[];
  loading: boolean;
  error: string | null;
}

function getCurrentMonth(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  return `${year}-${month}`;
}

const initialState: DashboardState = {
  month: getCurrentMonth(),
  summary: null,
  categoryBreakdown: [],
  budgetStatus: [],
  trends: [],
  topMerchants: [],
  monthlyComparison: [],
  loading: false,
  error: null,
};

interface FetchDashboardSuccessPayload {
  summary: MonthlySummary;
  categoryBreakdown: CategoryBreakdown[];
  budgetStatus: BudgetStatus[];
  trends: SpendingTrend[];
  topMerchants: TopMerchant[];
  monthlyComparison: MonthlyComparison[];
}

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState,
  reducers: {
    setMonth(state, action: PayloadAction<string>) {
      state.month = action.payload;
    },
    fetchDashboard(state, _action: PayloadAction<string>) {
      state.loading = true;
      state.error = null;
    },
    fetchDashboardSuccess(state, action: PayloadAction<FetchDashboardSuccessPayload>) {
      state.loading = false;
      state.summary = action.payload.summary;
      state.categoryBreakdown = action.payload.categoryBreakdown;
      state.budgetStatus = action.payload.budgetStatus;
      state.trends = action.payload.trends;
      state.topMerchants = action.payload.topMerchants;
      state.monthlyComparison = action.payload.monthlyComparison;
    },
    fetchDashboardFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    fetchTrends(state, _action: PayloadAction<{ months: number; categoryId?: number }>) {
      state.error = null;
      void state;
    },
    fetchTrendsSuccess(state, action: PayloadAction<SpendingTrend[]>) {
      state.trends = action.payload;
    },
    fetchTrendsFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
  },
});

export const {
  setMonth,
  fetchDashboard,
  fetchDashboardSuccess,
  fetchDashboardFail,
  fetchTrends,
  fetchTrendsSuccess,
  fetchTrendsFail,
} = dashboardSlice.actions;

export default dashboardSlice.reducer;
