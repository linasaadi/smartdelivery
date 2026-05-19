import { Component, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { UtilisateurService } from '../../../core/services/utilisateur.service';
import { RegisterDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-user-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule
  ],
  template: `
    <div class="dialog-header">
      <mat-icon>person_add</mat-icon>
      <h2>Nouvel utilisateur</h2>
    </div>
    <div mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Prénom</mat-label>
        <input matInput [(ngModel)]="prenom" required placeholder="Prénom">
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Nom</mat-label>
        <input matInput [(ngModel)]="nom" required placeholder="Nom de famille">
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Adresse e-mail</mat-label>
        <input matInput type="email" [(ngModel)]="email" required placeholder="exemple@domaine.com">
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Mot de passe</mat-label>
        <input matInput [type]="showPwd ? 'text' : 'password'" [(ngModel)]="motDePasse" required>
        <button mat-icon-button matSuffix (click)="showPwd = !showPwd" type="button">
          <mat-icon>{{ showPwd ? 'visibility_off' : 'visibility' }}</mat-icon>
        </button>
        <mat-hint>Minimum 8 caractères, 1 majuscule, 1 chiffre, 1 caractère spécial</mat-hint>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Rôle</mat-label>
        <mat-select [(ngModel)]="role" required>
          <mat-option value="Admin">Administrateur</mat-option>
          <mat-option value="Dispatcher">Dispatcher</mat-option>
          <mat-option value="Chauffeur">Chauffeur</mat-option>
        </mat-select>
      </mat-form-field>
      @if (error) {
        <p class="error-msg">{{ error }}</p>
      }
    </div>
    <div mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Annuler</button>
      <button mat-raised-button color="primary" (click)="save()" [disabled]="loading">
        @if (loading) { <mat-icon class="spin">sync</mat-icon> } @else { <mat-icon>save</mat-icon> }
        Créer l'utilisateur
      </button>
    </div>
  `,
  styles: [`
    .dialog-header { display: flex; align-items: center; gap: 12px; padding: 20px 24px 8px;
      background: linear-gradient(135deg, #4f46e5, #2563eb); color: white; }
    .dialog-header h2 { margin: 0; font-size: 18px; font-weight: 600; }
    .full-width { width: 100%; margin-bottom: 8px; }
    .error-msg { color: #ef4444; font-size: 13px; margin: 4px 0; }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
  `]
})
export class UserFormComponent {
  private svc      = inject(UtilisateurService);
  private dialogRef = inject(MatDialogRef<UserFormComponent>);
  private cdr      = inject(ChangeDetectorRef);

  prenom    = '';
  nom       = '';
  email     = '';
  motDePasse = '';
  role: 'Admin' | 'Dispatcher' | 'Chauffeur' = 'Dispatcher';
  showPwd   = false;
  loading   = false;
  error     = '';

  save() {
    this.error = '';
    if (!this.prenom || !this.nom || !this.email || !this.motDePasse || !this.role) {
      this.error = 'Tous les champs sont obligatoires.';
      return;
    }
    const dto: RegisterDto = {
      prenom: this.prenom,
      nom:    this.nom,
      email:  this.email,
      motDePasse: this.motDePasse,
      role:   this.role
    };
    this.loading = true;
    this.cdr.markForCheck();
    this.svc.creer(dto).subscribe({
      next: () => { this.loading = false; this.dialogRef.close(true); },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Erreur lors de la création.';
        this.cdr.markForCheck();
      }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
