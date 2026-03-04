export interface Transaction {
  id: number;
  transactionDate: string;
  postDate: string | null;
  rawDescription: string;
  amount: number;
  isCredit: boolean;
  categoryId: number | null;
  categoryName: string | null;
  merchantId: number | null;
  merchantName: string | null;
  statementId: number;
  statementFileName: string | null;
  manuallyRecategorized: boolean;
}

export interface TransactionFilters {
  dateFrom: string | null;
  dateTo: string | null;
  categoryIds: number[] | null;
  search: string;
  amountMin: number | null;
  amountMax: number | null;
  sortBy: string;
  sortDir: string;
  page: number;
  pageSize: number;
}

export interface PagedTransactions {
  items: Transaction[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}