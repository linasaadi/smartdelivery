import { Component, inject, Inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ReclamationService } from '../../../core/services/reclamation.service';
import { ReclamationDto, ReclamationFormDto, LivraisonResumeDto } from '../../../shared/models';

export interface ReclamationFormData {
  reclamation?: ReclamationDto;
  livraisons?:  LivraisonResumeDto[];
}

const TYPES = [
  { value: 'Réclamation',      label: 'Réclamation générale', icon: 'report_problem',    color: '#f59e0b' },
  { value: 'Retard',           label: 'Signalement retard',   icon: 'schedule',          color: '#3b82f6' },
  { value: 'Marchandise',      label: 'Problème marchandise', icon: 'inventory_2',       color: '#8b5cf6' },
  { value: 'Service',          label: 'Qualité de service',   icon: 'star_half',         color: '#06b6d4' },
];

@Component({
  selector: 'app-reclamation-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatTooltipModule
  ],
  templateUrl: './reclamation-form.component.html',
  styleUrl:    './reclamation-form.component.css'
})
export class ReclamationFormComponent {
  private svc       = inject(ReclamationService);
  private dialogRef = inject(MatDialogRef<ReclamationFormComponent>);
  private cdr       = inject(ChangeDetectorRef);

  editMode    = false;
  titre       = '';
  description = '';
  livraisonId: number | null = null;
  typeSelec   = TYPES[0].value;
  loading     = false;
  error       = '';

  readonly types = TYPES;

  get typeActif() { return TYPES.find(t => t.value === this.typeSelec) ?? TYPES[0]; }
  get titreComplet() {
    return this.editMode
      ? this.titre
      : (this.titre ? `[${this.typeSelec}] ${this.titre}` : '');
  }

  constructor(@Inject(MAT_DIALOG_DATA) public data: ReclamationFormData) {
    if (data?.reclamation) {
      this.editMode    = true;
      this.titre       = data.reclamation.titre;
      this.description = data.reclamation.description;
      this.livraisonId = data.reclamation.livraisonId ?? null;
    }
  }

  save() {
    this.error = '';
    if (!this.titre.trim())       { this.error = 'Le titre est obligatoire.'; return; }
    if (!this.description.trim()) { this.error = 'La description est obligatoire.'; return; }
    if (this.description.trim().length < 10) { this.error = 'La description doit contenir au moins 10 caractères.'; return; }

    this.loading = true;
    this.cdr.markForCheck();

    const finalTitre = this.editMode
      ? this.titre
      : `[${this.typeSelec}] ${this.titre}`;

    if (this.editMode && this.data.reclamation) {
      this.svc.modifier(this.data.reclamation.id, {
        titre: this.titre, description: this.description
      }).subscribe({
        next:  () => { this.loading = false; this.dialogRef.close(true); },
        error: (err) => { this.loading = false; this.error = err?.error?.message ?? 'Une erreur est survenue.'; this.cdr.markForCheck(); }
      });
    } else {
      const dto: ReclamationFormDto = {
        titre: finalTitre, description: this.description,
        livraisonId: this.livraisonId ?? undefined
      };
      this.svc.create(dto).subscribe({
        next:  () => { this.loading = false; this.dialogRef.close(true); },
        error: (err) => { this.loading = false; this.error = err?.error?.message ?? 'Une erreur est survenue.'; this.cdr.markForCheck(); }
      });
    }
  }

  cancel() { this.dialogRef.close(false); }
}
