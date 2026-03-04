import { combineReducers } from '@reduxjs/toolkit';
import statementsReducer from '@/features/upload/statementsSlice';

export const rootReducer = combineReducers({
  statements: statementsReducer,
});