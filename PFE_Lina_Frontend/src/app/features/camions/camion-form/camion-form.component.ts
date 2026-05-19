import { Component, OnInit, inject, Inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin } from 'rxjs';

import { CamionService } from '../../../core/services/camion.service';
import { ChauffeurService } from '../../../core/services/chauffeur.service';
import { AuthService } from '../../../core/services/auth.service';
import { CamionResumeDto, CamionFormDto, ChauffeurDetailDto } from '../../../shared/models';

@Component({
  selector: 'app-camion-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './camion-form.component.html'
})
export class CamionFormComponent implements OnInit {
  private svc          = inject(CamionService);
  private chauffeurSvc = inject(ChauffeurService);
  private auth         = inject(AuthService);
  private dialogRef    = inject(MatDialogRef<CamionFormComponent>);
  private cdr          = inject(ChangeDetectorRef);

  chauffeurs: ChauffeurDetailDto[] = [];
  isEdit      = false;
  loading     = false;
  error       = '';

  // Vrai si l'utilisateur connecté est un Chauffeur — affectation automatique dans ce cas
  readonly estChauffeur = this.auth.isChauffeur();

  form: CamionFormDto = {
    immatriculation: '', marque: '', modele: '',
    capaciteKg: 0, capaciteM3: 0, chauffeurId: undefined
  };

  constructor(@Inject(MAT_DIALOG_DATA) public data: CamionResumeDto | null) {}

  ngOnInit() {
    // Si c'est un Chauffeur, on associe directement au chauffeur connecté
    if (this.estChauffeur) {
      const id = this.auth.chauffeurId();
      if (id) this.form.chauffeurId = id;

      if (this.data) {
        this.isEdit = true;
        this.svc.getById(this.data.id).subscribe(detail => {
          this.form = {
            immatriculation: detail.immatriculation,
            marque:          detail.marque,
            modele:          detail.modele,
            capaciteKg:      detail.capaciteKg,
            capaciteM3:      detail.capaciteM3,
            chauffeurId:     id ?? detail.chauffeur?.id,
          };
          this.cdr.markForCheck();
        });
      }
      return;
    }

    // Admin / Dispatcher : charger la liste des chauffeurs
    const chauffeurs$ = this.chauffeurSvc.getAll();

    if (this.data) {
      this.isEdit = true;
      forkJoin({ chauffeurs: chauffeurs$, detail: this.svc.getById(this.data.id) })
        .subscribe(({ chauffeurs, detail }) => {
          this.chauffeurs = chauffeurs;
          this.form = {
            immatriculation: detail.immatriculation,
            marque:          detail.marque,
            modele:          detail.modele,
            capaciteKg:      detail.capaciteKg,
            capaciteM3:      detail.capaciteM3,
            chauffeurId:     detail.chauffeur?.id,
          };
          this.cdr.markForCheck();
        });
    } else {
      chauffeurs$.subscribe(c => { this.chauffeurs = c; this.cdr.markForCheck(); });
    }
  }

  majusculesImmat() {
    this.form = { ...this.form, immatriculation: this.form.immatriculation.toUpperCase() };
  }

  save() {
    this.error = '';

    // Validation frontend avant envoi
    if (!this.form.immatriculation.trim()) { this.error = "L'immatriculation est obligatoire."; return; }
    if (!this.form.marque.trim())          { this.error = "La marque est obligatoire."; return; }
    if (!this.form.modele.trim())          { this.error = "Le modèle est obligatoire."; return; }
    if (this.form.capaciteKg <= 0)         { this.error = "La capacité en kg doit être supérieure à 0."; return; }
    if (this.form.capaciteM3 <= 0)         { this.error = "La capacité en m³ doit être supérieure à 0."; return; }

    this.loading = true;
    this.cdr.markForCheck();
    const obs = this.isEdit
      ? this.svc.update(this.data!.id, this.form)
      : this.svc.create(this.form);
    obs.subscribe({
      next: () => { this.loading = false; this.dialogRef.close(true); },
      error: (err) => {
        this.loading = false;
        const data = err?.error;
        if (data?.erreurs?.length)  this.error = data.erreurs[0];
        else if (data?.message)     this.error = data.message;
        else                        this.error = 'Une erreur est survenue. Vérifiez les données.';
        this.cdr.markForCheck();
      }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
