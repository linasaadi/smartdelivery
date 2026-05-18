import { Component, inject, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import * as L from 'leaflet';

import { RouteService } from '../../core/services/route.service';
import { RouteResult } from '../../shared/models';

@Component({
  selector: 'app-route-optimizer',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDividerModule, MatSnackBarModule],
  templateUrl: './route-optimizer.component.html',
  styleUrl: './route-optimizer.component.css'
})
export class RouteOptimizerComponent implements AfterViewInit, OnDestroy {
  private routeSvc = inject(RouteService);
  private snack = inject(MatSnackBar);

  map?: L.Map;
  private overlays: L.Layer[] = [];

  depart = { latitude: 36.8189, longitude: 10.1658 };
  destination = { latitude: 36.4074, longitude: 10.5729 };
  result?: RouteResult;
  loading = false;

  stops: Array<{ latitude: number; longitude: number; nom: string }> = [];
  newStop = { latitude: 0, longitude: 0, nom: '' };

  ngAfterViewInit() {
    this.map = L.map('route-map').setView([36.8189, 10.1658], 7);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);
  }

  shortest() { this.call('shortest'); }
  fastest() { this.call('fastest'); }

  private call(type: 'shortest' | 'fastest') {
    this.loading = true;
    const obs = type === 'shortest'
      ? this.routeSvc.shortest({ depart: this.depart, destination: this.destination })
      : this.routeSvc.fastest({ depart: this.depart, destination: this.destination });
    obs.subscribe({
      next: r => { this.result = r; this.loading = false; this.drawRoute(); },
      error: () => { this.snack.open('Erreur lors du calcul', 'OK', { duration: 4000 }); this.loading = false; }
    });
  }

  multiStop() {
    if (this.stops.length === 0) return;
    this.loading = true;
    this.routeSvc.multiStop({ depart: this.depart, arrets: this.stops }).subscribe({
      next: r => { this.result = r; this.loading = false; this.drawRoute(); },
      error: () => { this.snack.open('Erreur lors du calcul multi-arrêts', 'OK', { duration: 4000 }); this.loading = false; }
    });
  }

  addStop() {
    if (!this.newStop.latitude || !this.newStop.longitude) return;
    this.stops.push({ ...this.newStop });
    this.newStop = { latitude: 0, longitude: 0, nom: '' };
  }

  removeStop(i: number) { this.stops.splice(i, 1); }

  private drawRoute() {
    this.overlays.forEach(l => l.remove());
    this.overlays = [];
    if (!this.map) return;

    const pts: [number, number][] = [
      [this.depart.latitude, this.depart.longitude],
      ...this.stops.map(s => [s.latitude, s.longitude] as [number, number]),
      [this.destination.latitude, this.destination.longitude]
    ];

    const line = L.polyline(pts, { color: '#1d4ed8', weight: 4, dashArray: '8 4' }).addTo(this.map);
    this.overlays.push(line);

    pts.forEach((p, i) => {
      const icon = L.divIcon({ className: '', html: `<div class="route-marker">${i + 1}</div>`, iconSize: [28, 28] });
      const m = L.marker(p, { icon }).addTo(this.map!);
      this.overlays.push(m);
    });

    this.map.fitBounds(line.getBounds(), { padding: [40, 40] });
  }

  ngOnDestroy() { this.map?.remove(); }
}
