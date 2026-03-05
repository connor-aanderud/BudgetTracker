import { all } from "redux-saga/effects";
import { statementsSaga } from "@/features/upload/statementsSaga";
import { categoriesSaga } from "@/features/categories/categoriesSaga";
import { merchantsSaga } from "@/features/categories/merchantsSaga";
import { transactionsSaga } from "@/features/transactions/transactionsSaga";
import { incomeSaga } from "@/features/income/incomeSaga";
import { budgetsSaga } from "@/features/budgets/budgetsSaga";
import { dashboardSaga } from "@/features/dashboard/dashboardSaga";

export function* rootSaga() {
  yield all([
    statementsSaga(),
    categoriesSaga(),
    merchantsSaga(),
    transactionsSaga(),
    incomeSaga(),
    budgetsSaga(),
    dashboardSaga(),
  ]);
}
