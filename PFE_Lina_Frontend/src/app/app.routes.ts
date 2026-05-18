import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'livraisons',
        loadComponent: () => import('./features/livraisons/livraison-list/livraison-list.component').then(m => m.LivraisonListComponent)
      },
      {
        path: 'livraisons/:id',
        loadComponent: () => import('./features/livraisons/livraison-detail/livraison-detail.component').then(m => m.LivraisonDetailComponent)
      },
      {
        path: 'tracking',
        loadComponent: () => import('./features/tracking/tracking-map.component').then(m => m.TrackingMapComponent)
      },
      {
        path: 'camions',
        canActivate: [roleGuard('Admin', 'Dispatcher')],
        loadComponent: () => import('./features/camions/camion-list/camion-list.component').then(m => m.CamionListComponent)
      },
      {
        path: 'chauffeurs',
        canActivate: [roleGuard('Admin', 'Dispatcher')],
        loadComponent: () => import('./features/chauffeurs/chauffeur-list/chauffeur-list.component').then(m => m.ChauffeurListComponent)
      },
      {
        path: 'routes',
        loadComponent: () => import('./features/routes/route-optimizer.component').then(m => m.RouteOptimizerComponent)
      },
      {
        path: 'rapports',
        canActivate: [roleGuard('Admin')],
        loadComponent: () => import('./features/rapports/rapports.component').then(m => m.RapportsComponent)
      },
    ]
  },
  { path: '**', redirectTo: '' }
];
