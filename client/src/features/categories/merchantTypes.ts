export interface MerchantRule {
  id: number;
  normalizedName: string;
  matchPattern: string;
  matchType: string;
  categoryId: number;
  categoryName: string | null;
}

export interface CreateMerchantRequest {
  normalizedName: string;
  matchPattern: string;
  matchType: string;
  categoryId: number;
}

export interface UpdateMerchantRequest {
  normalizedName: string;
  matchPattern: string;
  matchType: string;
  categoryId: number;
}