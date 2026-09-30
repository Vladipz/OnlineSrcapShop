import { computed, Injectable, signal } from '@angular/core';
import { CurrentUser } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  readonly user = signal<CurrentUser | null>(null);
  readonly startupError = signal('');
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);
}
