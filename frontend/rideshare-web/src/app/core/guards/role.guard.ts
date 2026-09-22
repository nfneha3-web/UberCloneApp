import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRole } from '../models/auth.models';
import { AuthService } from '../services/auth.service';

export const roleGuard = (role: UserRole): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      router.navigateByUrl('/login');
      return false;
    }

    if (auth.role() !== role) {
      router.navigateByUrl('/');
      return false;
    }

    return true;
  };
};
