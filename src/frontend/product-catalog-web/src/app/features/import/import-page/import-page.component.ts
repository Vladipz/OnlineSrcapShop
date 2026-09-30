import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { apiErrorMessage, fieldErrors } from '../../../core/http/api-error';
import { httpUrlValidator } from '../../../core/http/url-validator';
import { ProductCardComponent } from '../../catalog/product-card/product-card.component';
import { ImportApiService } from '../data-access/import-api.service';
import { ImportPreview } from '../data-access/import.models';

@Component({
  selector: 'app-import-page',
  imports: [ReactiveFormsModule, ProductCardComponent],
  templateUrl: './import-page.component.html',
  styleUrl: './import-page.component.scss',
})
export class ImportPageComponent {
  private readonly api = inject(ImportApiService);
  private readonly router = inject(Router);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    sourceUrl: [
      'https://books.toscrape.com/',
      [Validators.required, httpUrlValidator, Validators.maxLength(2048)],
    ],
  });
  protected readonly preview = signal<ImportPreview | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal('');
  protected readonly errors = signal<Record<string, string[]>>({});

  protected async parse(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.errors.set({});
    try {
      this.preview.set(await this.api.preview(this.form.getRawValue().sourceUrl));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.errors.set(fieldErrors(error));
    } finally {
      this.pending.set(false);
    }
  }

  protected back(): void {
    if (this.pending()) return;
    this.preview.set(null);
    this.error.set('');
    this.errors.set({});
  }

  protected async confirm(): Promise<void> {
    const preview = this.preview();
    if (!preview || this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    try {
      // Only the parsed source URL is submitted; the server re-parses and checks duplicates.
      const result = await this.api.confirm(preview.sourceUrl);
      await this.router.navigate(['/catalog'], {
        state: {
          importSummary: `Import complete: ${result.found} found, ${result.created} created, ${result.skipped} skipped, ${result.failed} failed.`,
        },
      });
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.pending.set(false);
    }
  }
}
