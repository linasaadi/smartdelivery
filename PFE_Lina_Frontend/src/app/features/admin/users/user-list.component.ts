import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';

import { UtilisateurService } from '../../../core/services/utilisateur.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { UtilisateurDto } from '../../../core/models/auth.models';
import { UserFormComponent } from './user-form.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatDialogModule,
    MatTooltipModule, MatChipsModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Gestion des utilisateurs</h1>
          <p class="page-subtitle">{{ users.length }} utilisateurs enregistrés</p>
        </div>
        <button mat-raised-button color="primary" (click)="openForm()">
          <mat-icon>person_add</mat-icon> Nouvel utilisateur
        </button>
      </div>

      <div class="search-bar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Rechercher par nom ou email</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="search" (ngModelChange)="filterUsers()" placeholder="Nom, prénom ou email...">
        </mat-form-field>
        <mat-form-field appearance="outline" style="width:200px">
          <mat-label>Rôle</mat-label>
          <mat-select [(ngModel)]="filterRole" (ngModelChange)="filterUsers()">
            <mat-option value="">Tous les rôles</mat-option>
            <mat-option value="Admin">Administrateur</mat-option>
            <mat-option value="Dispatcher">Dispatcher</mat-option>
            <mat-option value="Chauffeur">Chauffeur</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <table mat-table [dataSource]="displayed" class="mat-elevation-z2">
        <ng-container matColumnDef="nom">
          <th mat-header-cell *matHeaderCellDef>Nom complet</th>
          <td mat-cell *matCellDef="let u">{{ u.prenom }} {{ u.nom }}</td>
        </ng-container>
        <ng-container matColumnDef="email">
          <th mat-header-cell *matHeaderCellDef>Email</th>
          <td mat-cell *matCellDef="let u">{{ u.email }}</td>
        </ng-container>
        <ng-container matColumnDef="role">
          <th mat-header-cell *matHeaderCellDef>Rôle</th>
          <td mat-cell *matCellDef="let u">
            <span class="role-badge" [class]="'role-' + u.role.toLowerCase()">
              {{ roleLabel(u.role) }}
            </span>
          </td>
        </ng-container>
        <ng-container matColumnDef="lien">
          <th mat-header-cell *matHeaderCellDef>Lien chauffeur</th>
          <td mat-cell *matCellDef="let u">
            @if (u.role === 'Chauffeur') {
              @if (u.chauffeurId) {
                <span class="role-badge role-dispatcher">#{{ u.chauffeurId }}</span>
              } @else {
                <button mat-stroked-button color="warn" (click)="lierChauffeur(u)" style="font-size:12px;line-height:28px;height:28px;">
                  <mat-icon style="font-size:16px;height:16px;width:16px;">link</mat-icon> Lier
                </button>
              }
            } @else {
              <span style="color:#ccc">—</span>
            }
          </td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let u">
            <button mat-icon-button color="primary" (click)="changerRole(u)" matTooltip="Changer le rôle">
              <mat-icon>manage_accounts</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="supprimer(u)" matTooltip="Supprimer">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length" class="no-data">
            Aucun utilisateur trouvé.
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-title { font-size: 24px; font-weight: 600; margin: 0; }
    .page-subtitle { color: #666; margin: 4px 0 0; }
    .search-bar { display: flex; gap: 16px; margin-bottom: 16px; flex-wrap: wrap; }
    .search-field { flex: 1; min-width: 240px; }
    .role-badge { padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600; }
    .role-admin { background: #e8eaf6; color: #3f51b5; }
    .role-dispatcher { background: #e3f2fd; color: #1565c0; }
    .role-chauffeur { background: #e8f5e9; color: #2e7d32; }
    .no-data { text-align: center; padding: 32px; color: #999; }
    table { width: 100%; }
  `]
})
export class UserListComponent implements OnInit {
  private svc    = inject(UtilisateurService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  private cdr    = inject(ChangeDetectorRef);

  users: UtilisateurDto[]    = [];
  displayed: UtilisateurDto[] = [];
  search      = '';
  filterRole  = '';
  displayedColumns = ['nom', 'email', 'role', 'lien', 'actions'];

  ngOnInit() { this.load(); }

  load() {
    this.svc.getAll().subscribe(data => {
      this.users    = data;
      this.filterUsers();
      this.cdr.markForCheck();
    });
  }

  filterUsers() {
    this.displayed = this.users
      .filter(u => !this.filterRole || u.role === this.filterRole)
      .filter(u => {
        if (!this.search) return true;
        const s = this.search.toLowerCase();
        return u.nom.toLowerCase().includes(s) ||
               u.prenom.toLowerCase().includes(s) ||
               u.email.toLowerCase().includes(s);
      });
  }

  openForm() {
    this.dialog.open(UserFormComponent, { width: '520px', maxWidth: '95vw' })
      .afterClosed().subscribe(saved => {
        if (saved) { this.load(); this.swal.succes('Utilisateur créé avec succès'); }
      });
  }

  async changerRole(u: UtilisateurDto) {
    const roles = ['Admin', 'Dispatcher', 'Chauffeur'].filter(r => r !== u.role);
    const choix = await this.swal.choisirRole(u, roles);
    if (!choix) return;
    this.svc.changerRole(u.id, { nouveauRole: choix }).subscribe({
      next: () => { this.swal.succes('Rôle mis à jour'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de changer le rôle.')
    });
  }

  async lierChauffeur(u: UtilisateurDto) {
    const idStr = await this.swal.demanderTexte(
      `Lier ${u.prenom} ${u.nom} à un chauffeur`,
      'ID du chauffeur',
      'Entrez l\'identifiant numérique du chauffeur (ex: 1, 2...)'
    );
    if (idStr === null) return;
    const chauffeurId = parseInt(idStr ?? '', 10);
    if (isNaN(chauffeurId) || chauffeurId <= 0) {
      this.swal.erreur('ID invalide', 'Veuillez entrer un nombre entier positif.');
      return;
    }
    this.svc.lierChauffeur(u.id, chauffeurId).subscribe({
      next: () => {
        this.swal.succes('Compte lié !', 'L\'utilisateur doit se reconnecter pour que le lien soit actif.');
        this.load();
      },
      error: err => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible de lier le compte.')
    });
  }

  async supprimer(u: UtilisateurDto) {
    const ok = await this.swal.confirmerSuppression(`l'utilisateur ${u.prenom} ${u.nom}`);
    if (!ok) return;
    this.svc.supprimer(u.id).subscribe({
      next: () => { this.swal.succes('Utilisateur supprimé'); this.load(); },
      error: (err) => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible de supprimer.')
    });
  }

  roleLabel(role: string): string {
    const map: Record<string, string> = {
      Admin: 'Administrateur', Dispatcher: 'Dispatcher', Chauffeur: 'Chauffeur'
    };
    return map[role] ?? role;
  }
}
