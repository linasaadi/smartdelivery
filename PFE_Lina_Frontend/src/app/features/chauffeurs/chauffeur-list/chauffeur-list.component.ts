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

import { ChauffeurService } from '../../../core/services/chauffeur.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { ChauffeurDetailDto } from '../../../shared/models';
import { ChauffeurFormComponent } from '../chauffeur-form/chauffeur-form.component';

@Component({
  selector: 'app-chauffeur-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatDialogModule, MatTooltipModule
  ],
  templateUrl: './chauffeur-list.component.html',
  styleUrl: './chauffeur-list.component.css'
})
export class ChauffeurListComponent implements OnInit {
  private svc    = inject(ChauffeurService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  private cdr    = inject(ChangeDetectorRef);

  chauffeurs: ChauffeurDetailDto[] = [];
  search = '';
  displayedColumns = ['nom', 'permis', 'telephone', 'email', 'embauche', 'dispo', 'actions'];

  get displayed() {
    return this.chauffeurs.filter(c =>
      !this.search ||
      c.nomComplet.toLowerCase().includes(this.search.toLowerCase()) ||
      c.telephone.includes(this.search)
    );
  }

  get disponibles() { return this.chauffeurs.filter(c => c.estDisponible).length; }

  ngOnInit() { this.load(); }

  load() {
    this.svc.getAll().subscribe(d => { this.chauffeurs = d; this.cdr.markForCheck(); });
  }

  openForm(c?: ChauffeurDetailDto) {
    this.dialog.open(ChauffeurFormComponent, { width: '560px', maxWidth: '95vw', data: c ?? null })
      .afterClosed().subscribe(saved => {
        if (saved) {
          this.load();
          this.swal.succes(c ? 'Chauffeur modifié' : 'Chauffeur ajouté', '');
        }
      });
  }

  async delete(c: ChauffeurDetailDto) {
    const ok = await this.swal.confirmerSuppression(c.nomComplet);
    if (!ok) return;
    this.svc.delete(c.id).subscribe({
      next: () => { this.swal.succes('Chauffeur supprimé'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer ce chauffeur.')
    });
  }
}
