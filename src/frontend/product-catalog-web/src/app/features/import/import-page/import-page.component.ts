import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { apiErrorMessage, fieldErrors } from '../../../core/http/api-error';
import { httpUrlValidator } from '../../../core/http/url-validator';
import { ImportApiService } from '../data-access/import-api.service';
import { ImportResult } from '../data-access/import.models';

@Component({
  selector: 'app-import-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './import-page.component.html',
  styleUrl: './import-page.component.scss',
})
export class ImportPageComponent {
  private readonly api = inject(ImportApiService);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    sourceUrl: [
      'https://books.toscrape.com/',
      [Validators.required, httpUrlValidator, Validators.maxLength(2048)],
    ],
  });
  protected readonly result = signal<ImportResult | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal('');
  protected readonly errors = signal<Record<string, string[]>>({});

  protected async submit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.errors.set({});
    this.result.set(null);
    try {
      this.result.set(await this.api.import(this.form.getRawValue().sourceUrl));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.errors.set(fieldErrors(error));
    } finally {
      this.pending.set(false);
    }
  }
}
