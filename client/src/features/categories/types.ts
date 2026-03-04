export interface Category {
  id: number;
  name: string;
  color: string;
  icon: string | null;
  isDefault: boolean;
  transactionCount: number;
  merchantCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  color: string;
  icon: string | null;
}

export interface UpdateCategoryRequest {
  name: string;
  color: string;
  icon: string | null;
}