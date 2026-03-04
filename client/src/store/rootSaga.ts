import { all } from "redux-saga/effects";
import { statementsSaga } from "@/features/upload/statementsSaga";
import { categoriesSaga } from "@/features/categories/categoriesSaga";
import { merchantsSaga } from "@/features/categories/merchantsSaga";
import { transactionsSaga } from "@/features/transactions/transactionsSaga";

export function* rootSaga() {
  yield all([
    statementsSaga(),
    categoriesSaga(),
    merchantsSaga(),
    transactionsSaga(),
  ]);
}
