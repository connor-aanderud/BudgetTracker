import { combineReducers } from "@reduxjs/toolkit";
import statementsReducer from "@/features/upload/statementsSlice";
import categoriesReducer from "@/features/categories/categoriesSlice";
import merchantsReducer from "@/features/categories/merchantsSlice";
import transactionsReducer from "@/features/transactions/transactionsSlice";

export const rootReducer = combineReducers({
  statements: statementsReducer,
  categories: categoriesReducer,
  merchants: merchantsReducer,
  transactions: transactionsReducer,
});
