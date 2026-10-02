import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage } from '../../../core/http/api-error';
import { Product } from '../data-access/product.models';
import { ProductsApiService } from '../data-access/products-api.service';
import { ProductCardComponent } from '../product-card/product-card.component';

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink, ProductCardComponent],
  templateUrl: './catalog-page.component.html',
})
export class CatalogPageComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly api = inject(ProductsApiService);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly deleting = signal<number | null>(null);
  protected readonly error = signal('');

  ngOnInit(): void {
    void this.load();
  }

  protected async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.products.set(await this.api.list());
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async delete(product: Product): Promise<void> {
    if (
      this.deleting() !== null ||
      !window.confirm(`Delete “${product.name}”? This cannot be undone.`)
    )
      return;
    this.deleting.set(product.id);
    this.error.set('');
    try {
      await this.api.delete(product.id);
      this.products.update((products) => products.filter((item) => item.id !== product.id));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.deleting.set(null);
    }
  }
}
