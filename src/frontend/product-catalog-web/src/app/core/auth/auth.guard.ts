import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = () =>
  inject(AuthStore).user() ? true : inject(Router).createUrlTree(['/login']);

export const anonymousGuard: CanActivateFn = () =>
  inject(AuthStore).user() ? inject(Router).createUrlTree(['/catalog']) : true;

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  return !auth.user()
    ? router.createUrlTree(['/login'])
    : auth.isAdmin()
      ? true
      : router.createUrlTree(['/catalog']);
};
