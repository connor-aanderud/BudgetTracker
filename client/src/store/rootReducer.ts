import { combineReducers } from "@reduxjs/toolkit";
import statementsReducer from "@/features/upload/statementsSlice";
import categoriesReducer from "@/features/categories/categoriesSlice";

export const rootReducer = combineReducers({
  statements: statementsReducer,
  categories: categoriesReducer,
});
