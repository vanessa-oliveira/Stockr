import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.model';

export const tenantAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUserValue;

  if (!user) {
    router.navigate(['/login']);
    return false;
  }

  const isTenantAdmin = user.role === UserRole.TenantAdmin || user.role === UserRole.SystemAdmin;

  if (!isTenantAdmin) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
