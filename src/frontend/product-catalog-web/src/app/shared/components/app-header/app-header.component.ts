import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';
import { apiErrorMessage } from '../../../core/http/api-error';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
})
export class AppHeaderComponent {
  protected readonly auth = inject(AuthStore);
  private readonly service = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly pending = signal(false);
  protected readonly error = signal('');

  protected async logout(): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    try {
      await this.service.logout();
      await this.router.navigate(['/login']);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.pending.set(false);
    }
  }
}
