import { Component, OnInit, inject, Inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { provideNativeDateAdapter } from '@angular/material/core';

import { ChauffeurService } from '../../../core/services/chauffeur.service';
import { ChauffeurDetailDto, ChauffeurFormDto } from '../../../shared/models';

@Component({
  selector: 'app-chauffeur-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, FormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatDatepickerModule,
    MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './chauffeur-form.component.html'
})
export class ChauffeurFormComponent implements OnInit {
  private svc       = inject(ChauffeurService);
  private dialogRef = inject(MatDialogRef<ChauffeurFormComponent>);
  private cdr       = inject(ChangeDetectorRef);

  isEdit  = false;
  loading = false;
  error   = '';

  form: ChauffeurFormDto = {
    nom: '', prenom: '', numeroPermis: '',
    telephone: '', email: '', dateEmbauche: ''
  };

  constructor(@Inject(MAT_DIALOG_DATA) public data: ChauffeurDetailDto | null) {}

  ngOnInit() {
    if (this.data) {
      this.isEdit = true;
      this.form = {
        nom:          this.data.nom,
        prenom:       this.data.prenom,
        numeroPermis: this.data.numeroPermis,
        telephone:    this.data.telephone,
        email:        this.data.email ?? '',
        dateEmbauche: this.data.dateEmbauche,
      };
    }
  }

  save() {
    this.error = '';
    this.loading = true;
    this.cdr.markForCheck();
    const rawDate = this.form.dateEmbauche as unknown;
    const payload: ChauffeurFormDto = {
      ...this.form,
      dateEmbauche: rawDate instanceof Date
        ? (rawDate as Date).toISOString()
        : String(rawDate),
    };
    const obs = this.isEdit
      ? this.svc.update(this.data!.id, payload)
      : this.svc.create(payload);
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
