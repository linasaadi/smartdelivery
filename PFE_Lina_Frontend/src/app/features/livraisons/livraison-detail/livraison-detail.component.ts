import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { LivraisonService } from '../../../core/services/livraison.service';
import { LivraisonDetailDto, PointTrackingDto, StatutLivraison } from '../../../shared/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-livraison-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatFormFieldModule, MatTableModule,
    MatDividerModule, MatSnackBarModule, StatusBadgeComponent
  ],
  templateUrl: './livraison-detail.component.html',
  styleUrl: './livraison-detail.component.css'
})
export class LivraisonDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private svc   = inject(LivraisonService);
  private snack = inject(MatSnackBar);

  livraison?: LivraisonDetailDto;
  tracking: PointTrackingDto[] = [];
  anomalies: any[] = [];
  newStatut: StatutLivraison | '' = '';

  statuts: Array<{ value: StatutLivraison; label: string }> = [
    { value: 'EnAttente', label: 'En attente' },
    { value: 'EnCours',   label: 'En cours' },
    { value: 'Livree',    label: 'Livrée' },
    { value: 'EnRetard',  label: 'En retard' },
    { value: 'Annulee',   label: 'Annulée' },
  ];

  trackingColumns  = ['horodatage', 'latitude', 'longitude', 'vitesse', 'adresse'];
  anomaliesColumns = ['type', 'description', 'date', 'retard', 'resolu'];

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    forkJoin({
      livraison: this.svc.getById(id),
      tracking:  this.svc.getTracking(id),
      anomalies: this.svc.getAnomalies(id),
    }).subscribe(({ livraison, tracking, anomalies }) => {
      this.livraison = livraison;
      this.tracking  = tracking ?? [];
      this.anomalies = Array.isArray(anomalies) ? anomalies : (anomalies ? [anomalies] : []);
      this.newStatut = livraison.statut;
    });
  }

  updateStatut() {
    if (!this.livraison || !this.newStatut) return;
    this.svc.updateStatut(this.livraison.id, { nouveauStatut: this.newStatut as StatutLivraison }).subscribe({
      next: () => {
        this.livraison!.statut = this.newStatut as StatutLivraison;
        this.snack.open('Statut mis à jour', 'OK', { duration: 3000 });
      },
      error: () => this.snack.open('Erreur lors de la mise à jour', 'OK', { duration: 4000 })
    });
  }
}
