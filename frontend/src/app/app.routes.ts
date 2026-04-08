import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/pages/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
  },
  {
    path: 'policies',
    // TODO SPEC-6: canActivate: [AuthGuard]
    loadComponent: () =>
      import('./features/policies/container/policies-container.component').then(
        (m) => m.PoliciesContainerComponent
      ),
  },
  {
    path: 'policies/new',
    // TODO SPEC-6: canActivate: [AuthGuard], canMatch: [RoleGuard('admin', 'agent')]
    loadComponent: () =>
      import('./features/policies/ui/Pages/policy-create/policy-create.component').then(
        (m) => m.PolicyCreateComponent
      ),
  },
  {
    path: 'policies/:id',
    // TODO SPEC-6: canActivate: [AuthGuard]
    loadComponent: () =>
      import('./features/policies/ui/Pages/policy-detail/policy-detail.component').then(
        (m) => m.PolicyDetailComponent
      ),
  },
  {
    path: 'claims',
    loadComponent: () =>
      import('./features/claims/pages/claims-list.component').then(
        (m) => m.ClaimsListComponent
      ),
  },
  {
    path: 'claims/:id',
    loadComponent: () =>
      import('./features/claims/pages/claim-detail.component').then(
        (m) => m.ClaimDetailComponent
      ),
  },
  {
    path: 'notifications',
    loadComponent: () =>
      import('./features/notifications/pages/notifications.component').then(
        (m) => m.NotificationsComponent
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
];
