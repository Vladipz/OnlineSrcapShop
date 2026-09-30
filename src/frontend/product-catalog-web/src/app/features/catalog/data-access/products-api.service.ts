import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Product, ProductFields } from './product.models';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  list(): Promise<Product[]> {
    return firstValueFrom(this.http.get<Product[]>('/api/products'));
  }
  get(id: number): Promise<Product> {
    return firstValueFrom(this.http.get<Product>(`/api/products/${id}`));
  }
  update(id: number, fields: ProductFields): Promise<Product> {
    return firstValueFrom(this.http.put<Product>(`/api/products/${id}`, fields));
  }
  delete(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/products/${id}`));
  }
}
