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
  private dialogRef    = inject(MatDialogRef<CamionFormComponent>);
  private cdr          = inject(ChangeDetectorRef);

  chauffeurs: ChauffeurDetailDto[] = [];
  isEdit  = false;
  loading = false;
  error   = '';

  form: CamionFormDto = {
    immatriculation: '', marque: '', modele: '',
    capaciteKg: 0, capaciteM3: 0, chauffeurId: undefined
  };

  constructor(@Inject(MAT_DIALOG_DATA) public data: CamionResumeDto | null) {}

  ngOnInit() {
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

  save() {
    this.error = '';
    this.loading = true;
    this.cdr.markForCheck();
    const obs = this.isEdit
      ? this.svc.update(this.data!.id, this.form)
      : this.svc.create(this.form);
    obs.subscribe({
      next: () => { this.loading = false; this.dialogRef.close(true); },
      error: (err) => {
        this.loading = false;
        this.error = err?.statusText ?? 'Une erreur est survenue. Vérifiez les données.';
        this.cdr.markForCheck();
      }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
