import { DecimalPipe } from '@angular/common';
import { Component, input, signal } from '@angular/core';
import { ProductFields } from '../data-access/product.models';

@Component({
  selector: 'app-product-card',
  imports: [DecimalPipe],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
})
export class ProductCardComponent {
  readonly product = input.required<ProductFields & { sourceUrl: string }>();
  protected readonly failedImageUrl = signal('');
}
