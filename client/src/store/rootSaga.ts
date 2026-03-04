import { all } from "redux-saga/effects";
import { statementsSaga } from "@/features/upload/statementsSaga";
import { categoriesSaga } from "@/features/categories/categoriesSaga";

export function* rootSaga() {
  yield all([statementsSaga(), categoriesSaga()]);
}
