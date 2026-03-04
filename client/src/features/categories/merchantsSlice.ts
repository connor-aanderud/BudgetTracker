import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { MerchantRule, CreateMerchantRequest, UpdateMerchantRequest } from './merchantTypes';

interface MerchantsState {
  merchants: MerchantRule[];
  loading: boolean;
  error: string | null;
}

const initialState: MerchantsState = {
  merchants: [],
  loading: false,
  error: null,
};

const merchantsSlice = createSlice({
  name: 'merchants',
  initialState,
  reducers: {
    fetchMerchants(state) {
      state.loading = true;
      state.error = null;
    },
    fetchMerchantsSuccess(state, action: PayloadAction<MerchantRule[]>) {
      state.loading = false;
      state.merchants = action.payload;
    },
    fetchMerchantsFail(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
    },
    createMerchant(state, _action: PayloadAction<CreateMerchantRequest>) {
      state.error = null;
    },
    createMerchantSuccess(state) {
      state.error = null;
      void state;
    },
    createMerchantFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    updateMerchant(
      state,
      _action: PayloadAction<{ id: number; data: UpdateMerchantRequest }>
    ) {
      state.error = null;
    },
    updateMerchantSuccess(state) {
      state.error = null;
      void state;
    },
    updateMerchantFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
    deleteMerchant(state, _action: PayloadAction<number>) {
      state.error = null;
    },
    deleteMerchantSuccess(state) {
      state.error = null;
      void state;
    },
    deleteMerchantFail(state, action: PayloadAction<string>) {
      state.error = action.payload;
    },
  },
});

export const {
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
} = merchantsSlice.actions;

export default merchantsSlice.reducer;