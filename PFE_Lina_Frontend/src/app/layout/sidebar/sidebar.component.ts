import { Component, inject, computed } from '@angular/core';
import { RouterModule, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
  dividerBefore?: boolean;
}

const ALL_ITEMS: NavItem[] = [
  // ── Commun ─────────────────────────────────────────────────────────────────
  { label: 'Tableau de bord',     icon: 'dashboard',        route: '/dashboard' },

  // ── Admin uniquement ───────────────────────────────────────────────────────
  { label: 'Utilisateurs',        icon: 'group',            route: '/admin/utilisateurs', roles: ['Admin'], dividerBefore: true },
  { label: 'Réclamations',        icon: 'report_problem',   route: '/reclamations',       roles: ['Admin'] },
  { label: 'Rapports',            icon: 'bar_chart',        route: '/rapports',           roles: ['Admin'] },

  // ── Dispatcher uniquement ──────────────────────────────────────────────────
  { label: 'Livraisons',          icon: 'local_shipping',   route: '/livraisons',         roles: ['Admin', 'Dispatcher'], dividerBefore: true },
  { label: 'Suivi en temps réel', icon: 'map',              route: '/tracking',           roles: ['Admin', 'Dispatcher'] },
  { label: 'Camions',             icon: 'directions_car',   route: '/camions',            roles: ['Admin'] },
  { label: 'Chauffeurs',          icon: 'people',           route: '/chauffeurs',         roles: ['Admin', 'Dispatcher'] },
  { label: 'Réclamations',        icon: 'report_problem',   route: '/reclamations',       roles: ['Dispatcher'] },
  { label: 'Rapports',            icon: 'bar_chart',        route: '/rapports',           roles: ['Dispatcher'] },

  // ── Chauffeur uniquement ───────────────────────────────────────────────────
  { label: 'Mes livraisons',      icon: 'local_shipping',   route: '/mes-livraisons',     roles: ['Chauffeur'], dividerBefore: true },
  { label: 'Réclamations',        icon: 'report_problem',   route: '/reclamations',       roles: ['Chauffeur'] },
  { label: 'Rapports',            icon: 'assignment',       route: '/mes-rapports',       roles: ['Chauffeur'] },
  { label: 'Optimisation trajet', icon: 'route',            route: '/routes',             roles: ['Chauffeur'] },
  { label: 'Mes camions',         icon: 'directions_car',   route: '/mes-camions',        roles: ['Chauffeur'], dividerBefore: true },
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterLinkActive, MatListModule, MatIconModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  auth = inject(AuthService);

  navItems = computed(() => {
    const role = this.auth.role();
    return ALL_ITEMS.filter(item =>
      !item.roles || (role ? item.roles.includes(role) : false)
    );
  });
}
