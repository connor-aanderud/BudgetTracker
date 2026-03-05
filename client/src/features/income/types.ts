export interface Income {
  id: number;
  source: string;
  amount: number;
  date: string;
  isRecurring: boolean;
  frequency: string | null;
  transactionId: number | null;
  notes: string | null;
}

export interface CreateIncomeRequest {
  source: string;
  amount: number;
  date: string;
  isRecurring: boolean;
  frequency: string | null;
  notes: string | null;
}

export interface UpdateIncomeRequest {
  source: string;
  amount: number;
  date: string;
  isRecurring: boolean;
  frequency: string | null;
  notes: string | null;
}
