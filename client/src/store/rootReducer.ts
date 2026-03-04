import { combineReducers } from "@reduxjs/toolkit";
import statementsReducer from "@/features/upload/statementsSlice";
import categoriesReducer from "@/features/categories/categoriesSlice";
import merchantsReducer from "@/features/categories/merchantsSlice";

export const rootReducer = combineReducers({
  statements: statementsReducer,
  categories: categoriesReducer,
  merchants: merchantsReducer,
});
