export interface ProductFields {
  name: string;
  description: string | null;
  imageUrl: string;
  price: number | null;
  currencyCode: string | null;
}

export interface Product extends ProductFields {
  id: number;
  sourceUrl: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}
