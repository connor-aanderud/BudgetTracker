import { all } from 'redux-saga/effects';
import { statementsSaga } from '@/features/upload/statementsSaga';

export function* rootSaga() {
  yield all([statementsSaga()]);
}