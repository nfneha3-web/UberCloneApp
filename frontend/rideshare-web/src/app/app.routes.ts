import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },

  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register/rider',
    loadComponent: () =>
      import('./features/auth/register-rider/register-rider.component').then((m) => m.RegisterRiderComponent)
  },
  {
    path: 'register/driver',
    loadComponent: () =>
      import('./features/auth/register-driver/register-driver.component').then((m) => m.RegisterDriverComponent)
  },

  {
    path: 'rider/request',
    canActivate: [roleGuard('Rider')],
    loadComponent: () => import('./features/rider/request-ride/request-ride.component').then((m) => m.RequestRideComponent)
  },
  {
    path: 'rider/active',
    canActivate: [roleGuard('Rider')],
    loadComponent: () => import('./features/rider/active-ride/active-ride.component').then((m) => m.ActiveRideComponent)
  },
  {
    path: 'rider/history',
    canActivate: [roleGuard('Rider')],
    loadComponent: () => import('./features/rider/ride-history/ride-history.component').then((m) => m.RideHistoryComponent)
  },

  {
    path: 'driver',
    canActivate: [roleGuard('Driver')],
    loadComponent: () => import('./features/driver/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'driver/active',
    canActivate: [roleGuard('Driver')],
    loadComponent: () =>
      import('./features/driver/active-ride/active-ride.component').then((m) => m.DriverActiveRideComponent)
  },
  {
    path: 'driver/vehicle-setup',
    canActivate: [roleGuard('Driver')],
    loadComponent: () =>
      import('./features/driver/vehicle-setup/vehicle-setup.component').then((m) => m.VehicleSetupComponent)
  },
  {
    path: 'driver/history',
    canActivate: [roleGuard('Driver')],
    loadComponent: () =>
      import('./features/driver/ride-history/ride-history.component').then((m) => m.DriverRideHistoryComponent)
  },

  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () => import('./features/admin/overview/overview.component').then((m) => m.AdminOverviewComponent)
  },

  { path: '**', redirectTo: 'login' }
];
