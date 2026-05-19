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

      // ── Tableau de bord (commun — logique interne par rôle) ────────────────
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // ── Admin & Dispatcher : liste complète des livraisons ─────────────────
      {
        path: 'livraisons',
        canActivate: [roleGuard('Admin', 'Dispatcher')],
        loadComponent: () => import('./features/livraisons/livraison-list/livraison-list.component').then(m => m.LivraisonListComponent)
      },
      {
        path: 'livraisons/:id',
        loadComponent: () => import('./features/livraisons/livraison-detail/livraison-detail.component').then(m => m.LivraisonDetailComponent)
      },

      // ── Chauffeur : ses livraisons assignées ───────────────────────────────
      {
        path: 'mes-livraisons',
        canActivate: [roleGuard('Chauffeur')],
        loadComponent: () => import('./features/chauffeur/mes-livraisons/mes-livraisons.component').then(m => m.MesLivraisonsComponent)
      },

      // ── Chauffeur : ses camions ────────────────────────────────────────────
      {
        path: 'mes-camions',
        canActivate: [roleGuard('Chauffeur')],
        loadComponent: () => import('./features/chauffeur/mes-camions/mes-camions.component').then(m => m.MesCamionsComponent)
      },

      // ── Chauffeur : rapports & alertes ─────────────────────────────────────
      {
        path: 'mes-rapports',
        canActivate: [roleGuard('Chauffeur')],
        loadComponent: () => import('./features/chauffeur/mes-rapports/mes-rapports.component').then(m => m.MesRapportsComponent)
      },

      // ── Admin & Dispatcher : gestion de la flotte ─────────────────────────
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

      // ── Admin : gestion des utilisateurs ──────────────────────────────────
      {
        path: 'admin/utilisateurs',
        canActivate: [roleGuard('Admin')],
        loadComponent: () => import('./features/admin/users/user-list.component').then(m => m.UserListComponent)
      },

      // ── Réclamations (Chauffeur + Dispatcher voient les leurs ; Admin voit tout) ─
      {
        path: 'reclamations',
        loadComponent: () => import('./features/reclamations/reclamation-list/reclamation-list.component').then(m => m.ReclamationListComponent)
      },

      // ── Tracking en temps réel (Dispatcher & Admin) ────────────────────────
      {
        path: 'tracking',
        canActivate: [roleGuard('Admin', 'Dispatcher')],
        loadComponent: () => import('./features/tracking/tracking-map.component').then(m => m.TrackingMapComponent)
      },

      // ── Optimisation de trajets (Dispatcher & Chauffeur) ──────────────────
      {
        path: 'routes',
        canActivate: [roleGuard('Admin', 'Chauffeur')],
        loadComponent: () => import('./features/routes/route-optimizer.component').then(m => m.RouteOptimizerComponent)
      },

      // ── Rapports : Admin voit tout, Dispatcher voit ses livraisons ─────────
      {
        path: 'rapports',
        canActivate: [roleGuard('Admin', 'Dispatcher')],
        loadComponent: () => import('./features/rapports/rapports.component').then(m => m.RapportsComponent)
      },
    ]
  },
  { path: '**', redirectTo: '' }
];
