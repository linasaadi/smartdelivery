import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { normalizeStatut } from '../../models/statut-map';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="cssClass">{{ label }}</span>`
})
export class StatusBadgeComponent {
  @Input() statut!: string | number;

  private get key(): string {
    return normalizeStatut(this.statut);
  }

  get cssClass(): string {
    const map: Record<string, string> = {
      EnAttente:     'badge-en-attente',
      EnCours:       'badge-en-cours',
      Livree:        'badge-livree',
      EnRetard:      'badge-en-retard',
      Annulee:       'badge-annulee',
      Refusee:       'badge-annulee',
      Disponible:    'badge-disponible',
      EnRoute:       'badge-en-route',
      EnMaintenance: 'badge-en-maintenance',
      HorsService:   'badge-hors-service',
    };
    return map[this.key] ?? 'badge-en-attente';
  }

  get label(): string {
    const map: Record<string, string> = {
      EnAttente:     'En attente',
      EnCours:       'En cours',
      Livree:        'Livrée',
      EnRetard:      'En retard',
      Annulee:       'Annulée',
      Refusee:       'Refusée',
      Disponible:    'Disponible',
      EnRoute:       'En route',
      EnMaintenance: 'En maintenance',
      HorsService:   'Hors service',
    };
    return map[this.key] ?? this.key;
  }
}
