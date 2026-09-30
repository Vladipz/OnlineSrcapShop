import { Routes } from '@angular/router';
import { authGuard, anonymousGuard, adminGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'catalog' },
  {
    path: 'login',
    canActivate: [anonymousGuard],
    loadComponent: () =>
      import('./features/auth/login-page/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: 'catalog',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/catalog/catalog-page/catalog-page.component').then(
        (m) => m.CatalogPageComponent,
      ),
  },
  {
    path: 'import',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/import/import-page/import-page.component').then(
        (m) => m.ImportPageComponent,
      ),
  },
  {
    path: 'products/:id/edit',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/product-edit/product-edit-page/product-edit-page.component').then(
        (m) => m.ProductEditPageComponent,
      ),
  },
  { path: '**', redirectTo: 'catalog' },
];
