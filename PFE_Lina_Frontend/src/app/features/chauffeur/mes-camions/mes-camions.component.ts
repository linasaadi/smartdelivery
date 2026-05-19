import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';

import { CamionService } from '../../../core/services/camion.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { CamionResumeDto } from '../../../shared/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { CamionFormComponent } from '../../camions/camion-form/camion-form.component';

@Component({
  selector: 'app-mes-camions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatDialogModule, MatTooltipModule,
    StatusBadgeComponent
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Mes camions</h1>
          <p class="page-subtitle">{{ all.length }} camion(s) enregistré(s)</p>
        </div>
        <button mat-raised-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon> Ajouter un camion
        </button>
      </div>

      <div class="search-bar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Rechercher</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="search" placeholder="Immatriculation, marque...">
        </mat-form-field>
      </div>

      <table mat-table [dataSource]="displayed" class="mat-elevation-z2">
        <ng-container matColumnDef="immatriculation">
          <th mat-header-cell *matHeaderCellDef>Immatriculation</th>
          <td mat-cell *matCellDef="let c"><strong>{{ c.immatriculation }}</strong></td>
        </ng-container>
        <ng-container matColumnDef="marque">
          <th mat-header-cell *matHeaderCellDef>Marque / Modèle</th>
          <td mat-cell *matCellDef="let c">{{ c.marque }} {{ c.modele }}</td>
        </ng-container>
        <ng-container matColumnDef="statut">
          <th mat-header-cell *matHeaderCellDef>Statut</th>
          <td mat-cell *matCellDef="let c">
            <app-status-badge [statut]="c.statut"></app-status-badge>
          </td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let c">
            <button mat-icon-button color="primary" (click)="openForm(c)" matTooltip="Modifier">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="supprimer(c)" matTooltip="Supprimer">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length" class="no-data">Aucun camion enregistré.</td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-title { font-size: 24px; font-weight: 600; margin: 0; }
    .page-subtitle { color: #666; margin: 4px 0 0; }
    .search-bar { margin-bottom: 16px; }
    .search-field { width: 100%; max-width: 400px; }
    .no-data { text-align: center; padding: 32px; color: #999; }
    table { width: 100%; }
  `]
})
export class MesCamionsComponent implements OnInit {
  private svc    = inject(CamionService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  private cdr    = inject(ChangeDetectorRef);

  all: CamionResumeDto[] = [];
  search = '';
  displayedColumns = ['immatriculation', 'marque', 'statut', 'actions'];

  get displayed() {
    if (!this.search) return this.all;
    const s = this.search.toLowerCase();
    return this.all.filter(c =>
      c.immatriculation.toLowerCase().includes(s) ||
      c.marque.toLowerCase().includes(s) ||
      c.modele.toLowerCase().includes(s)
    );
  }

  ngOnInit() { this.load(); }

  load() {
    this.svc.getMesCamions().subscribe(data => {
      this.all = data;
      this.cdr.markForCheck();
    });
  }

  openForm(camion?: CamionResumeDto) {
    this.dialog.open(CamionFormComponent, { width: '560px', maxWidth: '95vw', data: camion ?? null })
      .afterClosed().subscribe(saved => {
        if (saved) {
          this.load();
          this.swal.succes(camion ? 'Camion modifié' : 'Camion ajouté');
        }
      });
  }

  async supprimer(c: CamionResumeDto) {
    const ok = await this.swal.confirmerSuppression(`${c.marque} ${c.modele} (${c.immatriculation})`);
    if (!ok) return;
    this.svc.delete(c.id).subscribe({
      next: () => { this.swal.succes('Camion supprimé'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer ce camion.')
    });
  }
}
