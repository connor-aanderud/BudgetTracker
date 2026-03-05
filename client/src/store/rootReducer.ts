import { combineReducers } from "@reduxjs/toolkit";
import statementsReducer from "@/features/upload/statementsSlice";
import categoriesReducer from "@/features/categories/categoriesSlice";
import merchantsReducer from "@/features/categories/merchantsSlice";
import transactionsReducer from "@/features/transactions/transactionsSlice";
import incomeReducer from "@/features/income/incomeSlice";
import budgetsReducer from "@/features/budgets/budgetsSlice";
import dashboardReducer from "@/features/dashboard/dashboardSlice";

export const rootReducer = combineReducers({
  statements: statementsReducer,
  categories: categoriesReducer,
  merchants: merchantsReducer,
  transactions: transactionsReducer,
  income: incomeReducer,
  budgets: budgetsReducer,
  dashboard: dashboardReducer,
});
