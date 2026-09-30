import { ProductFields } from '../../catalog/data-access/product.models';

export interface PreviewProduct extends ProductFields {
  sourceUrl: string;
  alreadyImported: boolean;
}
export interface ImportPreview {
  sourceUrl: string;
  found: number;
  newCount: number;
  duplicateCount: number;
  failed: number;
  products: PreviewProduct[];
}
export interface ImportResult {
  found: number;
  created: number;
  skipped: number;
  failed: number;
}
