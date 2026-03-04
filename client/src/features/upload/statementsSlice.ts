import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { ParseResult, StatementSummary } from './types';

interface StatementsState {
  parseResult: ParseResult | null;
  uploading: boolean;
  uploadError: string | null;
  confirming: boolean;
  confirmError: string | null;
  confirmMessage: string | null;
  statements: StatementSummary[];
  loadingStatements: boolean;
}

const initialState: StatementsState = {
  parseResult: null,
  uploading: false,
  uploadError: null,
  confirming: false,
  confirmError: null,
  confirmMessage: null,
  statements: [],
  loadingStatements: false,
};

const statementsSlice = createSlice({
  name: 'statements',
  initialState,
  reducers: {
    uploadFile(state, _action: PayloadAction<File>) {
      state.uploading = true;
      state.uploadError = null;
      state.parseResult = null;
      state.confirmMessage = null;
    },
    uploadFileSuccess(state, action: PayloadAction<ParseResult>) {
      state.uploading = false;
      state.parseResult = action.payload;
    },
    uploadFileFail(state, action: PayloadAction<string>) {
      state.uploading = false;
      state.uploadError = action.payload;
    },
    confirmImport(state, _action: PayloadAction<ParseResult>) {
      state.confirming = true;
      state.confirmError = null;
      state.confirmMessage = null;
    },
    confirmImportSuccess(state, action: PayloadAction<{ message: string }>) {
      state.confirming = false;
      state.confirmMessage = action.payload.message;
      state.parseResult = null;
    },
    confirmImportFail(state, action: PayloadAction<string>) {
      state.confirming = false;
      state.confirmError = action.payload;
    },
    fetchStatements(state) {
      state.loadingStatements = true;
    },
    fetchStatementsSuccess(state, action: PayloadAction<StatementSummary[]>) {
      state.loadingStatements = false;
      state.statements = action.payload;
    },
    fetchStatementsFail(state) {
      state.loadingStatements = false;
    },
    clearParseResult(state) {
      state.parseResult = null;
      state.uploadError = null;
      state.confirmError = null;
      state.confirmMessage = null;
    },
  },
});

export const {
  uploadFile,
  uploadFileSuccess,
  uploadFileFail,
  confirmImport,
  confirmImportSuccess,
  confirmImportFail,
  fetchStatements,
  fetchStatementsSuccess,
  fetchStatementsFail,
  clearParseResult,
} = statementsSlice.actions;

export default statementsSlice.reducer;