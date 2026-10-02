import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage, fieldErrors } from '../../../core/http/api-error';
import {
  finitePriceValidator,
  httpUrlValidator,
  nonBlankValidator,
} from '../../../core/http/url-validator';
import { Product } from '../../catalog/data-access/product.models';
import { ProductsApiService } from '../../catalog/data-access/products-api.service';

@Component({
  selector: 'app-product-edit-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-edit-page.component.html',
  styleUrl: './product-edit-page.component.scss',
})
export class ProductEditPageComponent {
  private readonly api = inject(ProductsApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private loadVersion = 0;
  protected readonly product = signal<Product | null>(null);
  protected readonly loading = signal(true);
  protected readonly pending = signal(false);
  protected readonly error = signal('');
  protected readonly errors = signal<Record<string, string[]>>({});
  protected readonly form = inject(FormBuilder).group({
    name: ['', [nonBlankValidator, Validators.maxLength(300)]],
    description: ['', Validators.maxLength(5000)],
    imageUrl: ['', [Validators.required, httpUrlValidator, Validators.maxLength(2048)]],
    price: [null as number | null, [Validators.min(0), finitePriceValidator]],
    currencyCode: ['', Validators.maxLength(3)],
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe(() => void this.load());
  }

  protected async load(): Promise<void> {
    const version = ++this.loadVersion;
    this.loading.set(true);
    this.error.set('');
    this.errors.set({});
    this.product.set(null);
    try {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      if (!Number.isSafeInteger(id) || id <= 0) {
        this.error.set('The requested product was not found.');
        return;
      }
      const product = await this.api.get(id);
      if (version !== this.loadVersion) return;
      this.product.set(product);
      this.form.reset(product);
    } catch (error) {
      if (version === this.loadVersion) this.error.set(apiErrorMessage(error));
    } finally {
      if (version === this.loadVersion) this.loading.set(false);
    }
  }

  protected async submit(): Promise<void> {
    this.form.markAllAsTouched();
    const product = this.product();
    if (!product || this.form.invalid || this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.errors.set({});
    try {
      const fields = this.form.getRawValue();
      await this.api.update(product.id, {
        name: (fields.name ?? '').trim(),
        description: fields.description || null,
        imageUrl: fields.imageUrl ?? '',
        price: fields.price,
        currencyCode: fields.currencyCode || null,
      });
      await this.router.navigate(['/catalog']);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.errors.set(fieldErrors(error));
    } finally {
      this.pending.set(false);
    }
  }

  protected fieldError(name: keyof typeof this.form.controls): string {
    const server = this.errors()[name.toLowerCase()]?.join(' ');
    if (server) return server;
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return '';
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('maxlength'))
      return `Maximum ${control.getError('maxlength').requiredLength} characters.`;
    if (control.hasError('httpUrl')) return 'Enter an absolute HTTP/HTTPS URL without credentials.';
    return 'Enter a nonnegative price.';
  }
}
