import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () =>
  inject(AuthService).user() ? true : inject(Router).createUrlTree(['/login']);

export const anonymousGuard: CanActivateFn = () =>
  inject(AuthService).user() ? inject(Router).createUrlTree(['/catalog']) : true;

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return !auth.user()
    ? router.createUrlTree(['/login'])
    : auth.isAdmin()
      ? true
      : router.createUrlTree(['/catalog']);
};
