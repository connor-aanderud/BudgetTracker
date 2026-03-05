export interface BudgetItem {
  id: number;
  categoryId: number;
  categoryName: string | null;
  categoryColor: string | null;
  month: string;
  limitAmount: number;
}

export interface BudgetLineItem {
  categoryId: number;
  limitAmount: number;
}

export interface SetBudgetRequest {
  month: string;
  budgets: BudgetLineItem[];
}

export interface CopyBudgetsRequest {
  fromMonth: string;
  toMonth: string;
}

export interface BudgetStatus {
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  limitAmount: number;
  actualAmount: number;
  remainingAmount: number;
  overBudget: boolean;
  percentUsed: number;
}
