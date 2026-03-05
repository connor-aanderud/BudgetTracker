export interface MonthlySummary {
  month: string;
  totalIncome: number;
  totalExpenses: number;
  netSavings: number;
  previousMonthExpenses: number | null;
  percentChangeExpenses: number | null;
}

export interface CategoryBreakdown {
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  totalAmount: number;
  transactionCount: number;
  percentOfTotal: number;
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

export interface SpendingTrend {
  month: string;
  totalAmount: number;
  categoryId: number | null;
  categoryName: string | null;
}

export interface TopMerchant {
  merchantId: number;
  merchantName: string;
  totalAmount: number;
  transactionCount: number;
}

export interface MonthlyComparison {
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  currentMonth: number;
  previousMonth: number;
  threeMonthAverage: number;
}
