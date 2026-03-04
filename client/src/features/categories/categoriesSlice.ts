import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from './types';

interface CategoriesState {
  categories: Category[];
  loading: boolean;
  error: string | null;
}

const initialState: CategoriesState = {
  categories: [],
  loading: false,
  error: null,
};

const categoriesSlice = createSlice({
  name: 'categories',
  initialState,
  reducers: {
    fetchCategories(state) {
      state.loading = true;
      state.error = null;
    },
    fetchCategoriesSuccess(state, action: PayloadAction<Category[]>) {
      state.loading = false;
      state.categories = action.payload;
    },
    fetchCategoriesFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    createCategory(state, _action: PayloadAction<CreateCategoryRequest>) {
      state.error = null;
    },
    createCategorySuccess(state) {
      state.error = null;
      void state;
    },
    createCategoryFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    updateCategory(
      state,
      _action: PayloadAction<{ id: number; data: UpdateCategoryRequest }>
    ) {
      state.error = null;
    },
    updateCategorySuccess(state) {
      state.error = null;
      void state;
    },
    updateCategoryFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    deleteCategory(state, _action: PayloadAction<number>) {
      state.error = null;
    },
    deleteCategorySuccess(state) {
      state.error = null;
      void state;
    },
    deleteCategoryFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
  },
});

export const {
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
} = categoriesSlice.actions;

export default categoriesSlice.reducer;