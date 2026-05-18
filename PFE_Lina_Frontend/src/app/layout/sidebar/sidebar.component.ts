import { Component, inject, computed } from '@angular/core';
import { RouterModule, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';

interface NavItem { label: string; icon: string; route: string; roles?: string[]; }

const ALL_ITEMS: NavItem[] = [
  { label: 'Tableau de bord', icon: 'dashboard',      route: '/dashboard' },
  { label: 'Livraisons',      icon: 'local_shipping', route: '/livraisons' },
  { label: 'Tracking',        icon: 'map',             route: '/tracking' },
  { label: 'Camions',         icon: 'directions_car', route: '/camions',    roles: ['Admin','Dispatcher'] },
  { label: 'Chauffeurs',      icon: 'person',         route: '/chauffeurs', roles: ['Admin','Dispatcher'] },
  { label: 'Optimisation',    icon: 'route',          route: '/routes' },
  { label: 'Rapports',        icon: 'bar_chart',      route: '/rapports',   roles: ['Admin'] },
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
