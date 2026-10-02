import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrentUser, LoginRequest } from './auth.models';
import { apiErrorMessage } from '../http/api-error';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  readonly user = signal<CurrentUser | null>(null);
  readonly startupError = signal('');
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);

  async restore(): Promise<void> {
    this.startupError.set('');
    try {
      this.user.set(await firstValueFrom(this.http.get<CurrentUser>('/api/auth/me')));
    } catch (error) {
      this.user.set(null);
      if (!(error instanceof HttpErrorResponse && error.status === 401)) {
        this.startupError.set(apiErrorMessage(error));
      }
    }
  }

  async login(request: LoginRequest): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/auth/login', request));
    this.user.set(await firstValueFrom(this.http.get<CurrentUser>('/api/auth/me')));
    this.startupError.set('');
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(this.http.post<void>('/api/auth/logout', {}));
    } catch (error) {
      if (!(error instanceof HttpErrorResponse && error.status === 401)) throw error;
    }
    this.user.set(null);
  }
}
