export interface ParsedTransaction {
  transactionDate: string;
  postDate: string | null;
  rawDescription: string;
  amount: number;
  isCredit: boolean;
  duplicateHash: string;
  isDuplicate: boolean;
}

export interface ParseResult {
  fileName: string;
  source: string | null;
  parsedTransactions: ParsedTransaction[];
  warnings: string[];
  totalCount: number;
  duplicateCount: number;
}

export interface StatementSummary {
  id: number;
  fileName: string;
  uploadDate: string;
  source: string;
  periodStart: string | null;
  periodEnd: string | null;
  transactionCount: number;
}