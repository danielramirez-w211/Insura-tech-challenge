import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/ui/pages/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
  },

  {
    path: 'policies',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/policies/container/policies-container.component').then(
        (m) => m.PoliciesContainerComponent
      ),
  },

  {
    path: 'policies/new',
    canActivate: [authGuard, roleGuard('Advisor', 'Admin')],
    loadComponent: () =>
      import('./features/policies/ui/Pages/policy-create/policy-create.component').then(
        (m) => m.PolicyCreateComponent
      ),
  },

  {
    path: 'policies/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/policies/ui/Pages/policy-detail/policy-detail.component').then(
        (m) => m.PolicyDetailComponent
      ),
  },

  {
    path: 'claims',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/claims/ui/pages/claim-list/claims-list.component').then(
        (m) => m.ClaimsListComponent
      ),
  },

  {
    path: 'claims/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/claims/ui/pages/claim-detail/claim-detail.component').then(
        (m) => m.ClaimDetailComponent
      ),
  },

  {
    path: 'notifications',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/notifications/ui/pages/notifications/notifications.component').then(
        (m) => m.NotificationsComponent
      ),
  },

  { path: '**', redirectTo: 'dashboard' },
];
