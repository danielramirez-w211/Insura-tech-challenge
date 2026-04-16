import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export type AppRole = 'Admin' | 'Leader' | 'Advisor';

/** Factory: devuelve un guard que verifica que el usuario tenga alguno de los roles indicados. */
export function roleGuard(...allowedRoles: AppRole[]): CanActivateFn {
  return () => {
    const auth   = inject(AuthService);
    const router = inject(Router);

    const role = auth.role();
    if (role && allowedRoles.includes(role as AppRole)) return true;

    router.navigate(['/dashboard']);
    return false;
  };
}
