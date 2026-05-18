import { Component, OnInit, OnDestroy, inject, AfterViewInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import * as L from 'leaflet';

import { LivraisonService } from '../../core/services/livraison.service';
import { LivraisonResumeDto, LivraisonDetailDto, PointTrackingDto } from '../../shared/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-tracking-map',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatSelectModule,
    MatFormFieldModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatDividerModule, StatusBadgeComponent
  ],
  templateUrl: './tracking-map.component.html',
  styleUrl: './tracking-map.component.css'
})
export class TrackingMapComponent implements AfterViewInit, OnDestroy {
  private livraisonSvc = inject(LivraisonService);
  private cdr = inject(ChangeDetectorRef);

  map?: L.Map;
  livraisons: LivraisonResumeDto[] = [];
  selectedLivraisonId?: number;
  selectedLivraison?: LivraisonDetailDto;
  tracking: PointTrackingDto[] = [];
  private markers: L.Marker[] = [];
  private polyline?: L.Polyline;

  ngAfterViewInit() {
    this.initMap();
    this.livraisonSvc.getAll({ statut: 'EnCours', taillePage: 100 }).subscribe(page => {
      const enRetard = page.elements.filter(lv => lv.statut === 'EnRetard');
      this.livraisons = [...page.elements, ...enRetard.filter(l => !page.elements.includes(l))];
      this.cdr.markForCheck();
    });
  }

  private initMap() {
    this.map = L.map('tracking-map').setView([36.8189, 10.1658], 7);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);
  }

  loadTracking() {
    if (!this.selectedLivraisonId) return;
    this.clearOverlays();

    this.livraisonSvc.getById(this.selectedLivraisonId).subscribe(detail => {
      this.selectedLivraison = detail;
      this.cdr.markForCheck();
    });

    this.livraisonSvc.getTracking(this.selectedLivraisonId).subscribe(points => {
      this.tracking = points;
      this.cdr.markForCheck();
      if (points.length === 0) return;

      const coords: [number, number][] = points.map(p => [p.latitude, p.longitude]);
      this.polyline = L.polyline(coords, { color: '#1d4ed8', weight: 3 }).addTo(this.map!);

      const startIcon = L.divIcon({ className: '', html: '<div class="track-point start">▶</div>', iconSize: [24, 24] });
      const endIcon   = L.divIcon({ className: '', html: '<div class="track-point end">⬛</div>', iconSize: [24, 24] });
      L.marker(coords[0], { icon: startIcon }).addTo(this.map!);
      L.marker(coords[coords.length - 1], { icon: endIcon }).addTo(this.map!);

      if (this.selectedLivraison?.destination) {
        const d = this.selectedLivraison.destination;
        const destIcon = L.divIcon({ className: '', html: '<div class="track-point dest">📍</div>', iconSize: [24, 24] });
        L.marker([d.latitude, d.longitude], { icon: destIcon })
          .bindPopup(`<b>Destination</b><br>${d.adresse}<br>${d.ville}`)
          .addTo(this.map!);
      }
      this.map!.fitBounds(this.polyline.getBounds(), { padding: [40, 40] });
    });
  }

  private clearOverlays() {
    this.polyline?.remove();
    this.markers.forEach(m => m.remove());
    this.markers = [];
  }

  ngOnDestroy() { this.map?.remove(); }
}
