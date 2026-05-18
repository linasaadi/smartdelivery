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
import { CamionFormComponent } from '../camion-form/camion-form.component';

@Component({
  selector: 'app-camion-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatDialogModule, MatTooltipModule,
    StatusBadgeComponent
  ],
  templateUrl: './camion-list.component.html',
  styleUrl: './camion-list.component.css'
})
export class CamionListComponent implements OnInit {
  private svc    = inject(CamionService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  private cdr    = inject(ChangeDetectorRef);

  all: CamionResumeDto[] = [];
  search = '';
  filterStatut = '';
  displayedColumns = ['immatriculation', 'statut', 'chauffeur', 'actions'];

  get displayed() {
    return this.all
      .filter(c => !this.filterStatut || c.statut === this.filterStatut)
      .filter(c => !this.search ||
        c.immatriculation.toLowerCase().includes(this.search.toLowerCase()) ||
        c.marque.toLowerCase().includes(this.search.toLowerCase()));
  }

  get disponibles() { return this.all.filter(c => c.statut === 'Disponible').length; }
  get enRoute()     { return this.all.filter(c => c.statut === 'EnRoute').length; }

  ngOnInit() { this.load(); }

  load() {
    this.svc.getAll().subscribe(d => { this.all = d; this.cdr.markForCheck(); });
  }

  openForm(camion?: CamionResumeDto) {
    this.dialog.open(CamionFormComponent, { width: '560px', maxWidth: '95vw', data: camion ?? null })
      .afterClosed().subscribe(saved => {
        if (saved) {
          this.load();
          this.swal.succes(camion ? 'Camion modifié' : 'Camion ajouté', '');
        }
      });
  }

  async delete(c: CamionResumeDto) {
    const ok = await this.swal.confirmerSuppression(`${c.marque} ${c.modele} (${c.immatriculation})`);
    if (!ok) return;
    this.svc.delete(c.id).subscribe({
      next: () => { this.swal.succes('Camion supprimé'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer ce camion.')
    });
  }
}
