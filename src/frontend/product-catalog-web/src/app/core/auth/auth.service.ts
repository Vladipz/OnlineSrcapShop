import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrentUser, LoginRequest } from './auth.models';
import { AuthStore } from './auth.store';
import { apiErrorMessage } from '../http/api-error';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);

  async restore(): Promise<void> {
    this.store.startupError.set('');
    try {
      this.store.user.set(await firstValueFrom(this.http.get<CurrentUser>('/api/auth/me')));
    } catch (error) {
      this.store.user.set(null);
      if (!(error instanceof HttpErrorResponse && error.status === 401)) {
        this.store.startupError.set(apiErrorMessage(error));
      }
    }
  }

  async login(request: LoginRequest): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/auth/login', request));
    this.store.user.set(await firstValueFrom(this.http.get<CurrentUser>('/api/auth/me')));
    this.store.startupError.set('');
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(this.http.post<void>('/api/auth/logout', {}));
    } catch (error) {
      if (!(error instanceof HttpErrorResponse && error.status === 401)) throw error;
    }
    this.store.user.set(null);
  }
}
