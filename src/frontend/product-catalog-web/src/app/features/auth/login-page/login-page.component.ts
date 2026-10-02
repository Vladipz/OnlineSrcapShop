import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage, fieldErrors } from '../../../core/http/api-error';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
  protected readonly pending = signal(false);
  protected readonly error = signal('');
  protected readonly errors = signal<Record<string, string[]>>({});

  protected async submit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.errors.set({});
    try {
      const values = this.form.getRawValue();
      await this.auth.login({ email: values.email.trim(), password: values.password });
      this.form.controls.password.reset();
      await this.router.navigate(['/catalog']);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.errors.set(fieldErrors(error));
      this.form.controls.password.reset();
    } finally {
      this.pending.set(false);
    }
  }

  protected async retry(): Promise<void> {
    this.pending.set(true);
    try {
      await this.auth.restore();
      if (this.auth.user()) await this.router.navigate(['/catalog']);
    } finally {
      this.pending.set(false);
    }
  }
}
