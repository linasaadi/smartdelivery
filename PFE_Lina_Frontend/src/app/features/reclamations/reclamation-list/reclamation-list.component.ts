import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ReclamationService } from '../../../core/services/reclamation.service';
import { AuthService } from '../../../core/services/auth.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { ReclamationDto, ModifierReclamationDto } from '../../../shared/models';
import { ReclamationFormComponent } from '../reclamation-form/reclamation-form.component';

@Component({
  selector: 'app-reclamation-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatDialogModule, MatTooltipModule
  ],
  templateUrl: './reclamation-list.component.html',
  styleUrl: './reclamation-list.component.css'
})
export class ReclamationListComponent implements OnInit {
  private svc    = inject(ReclamationService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  auth           = inject(AuthService);
  private cdr    = inject(ChangeDetectorRef);

  all:       ReclamationDto[] = [];
  displayed: ReclamationDto[] = [];
  filterStatut = '';
  search       = '';
  expandedId: number | null = null;

  readonly statuts = [
    { value: '',          label: 'Toutes',      icon: 'list'         },
    { value: 'EnAttente', label: 'En attente',  icon: 'hourglass_empty' },
    { value: 'EnCours',   label: 'En cours',    icon: 'pending'      },
    { value: 'Resolue',   label: 'Résolues',    icon: 'check_circle' },
  ];

  get total()    { return this.all.length; }
  get enAttente(){ return this.all.filter(r => r.statut === 'EnAttente').length; }
  get enCours()  { return this.all.filter(r => r.statut === 'EnCours').length; }
  get resolues() { return this.all.filter(r => r.statut === 'Resolue').length; }

  ngOnInit() { this.load(); }

  load() {
    this.svc.getAll().subscribe(data => {
      this.all = data.sort((a, b) =>
        new Date(b.dateCreation).getTime() - new Date(a.dateCreation).getTime()
      );
      this.applyFilters();
      this.cdr.markForCheck();
    });
  }

  applyFilters() {
    let res = [...this.all];
    if (this.filterStatut) res = res.filter(r => r.statut === this.filterStatut);
    if (this.search.trim()) {
      const q = this.search.toLowerCase();
      res = res.filter(r =>
        r.titre.toLowerCase().includes(q) ||
        r.description.toLowerCase().includes(q) ||
        (r.nomUtilisateur ?? '').toLowerCase().includes(q) ||
        (r.referenceLivraison ?? '').toLowerCase().includes(q)
      );
    }
    this.displayed = res;
  }

  setFilter(v: string) { this.filterStatut = v; this.applyFilters(); this.cdr.markForCheck(); }

  openForm(reclamation?: ReclamationDto) {
    this.dialog.open(ReclamationFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'dialog-no-padding',
      data: { reclamation }
    }).afterClosed().subscribe(saved => {
      if (saved) {
        this.load();
        this.swal.succes(reclamation ? 'Réclamation modifiée' : 'Réclamation soumise');
      }
    });
  }

  async repondre(r: ReclamationDto) {
    const reponse = await this.swal.demanderTexte(
      `Répondre — ${r.titre}`, 'Réponse', 'Votre réponse...'
    );
    if (reponse === null) return;
    const dto: ModifierReclamationDto = {
      nouveauStatut: r.statut === 'EnAttente' ? 'EnCours' : 'Resolue',
      reponseAdmin: reponse
    };
    this.svc.modifier(r.id, dto).subscribe({
      next: () => { this.swal.succes('Réponse enregistrée'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible d\'enregistrer la réponse.')
    });
  }

  async changerStatut(r: ReclamationDto, nouveauStatut: string) {
    this.svc.modifier(r.id, { nouveauStatut }).subscribe({
      next: () => { this.swal.succes('Statut mis à jour'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de mettre à jour.')
    });
  }

  async supprimer(r: ReclamationDto) {
    const ok = await this.swal.confirmerSuppression(`"${r.titre}"`);
    if (!ok) return;
    this.svc.delete(r.id).subscribe({
      next: () => { this.swal.succes('Réclamation supprimée'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer.')
    });
  }

  toggle(id: number) {
    this.expandedId = this.expandedId === id ? null : id;
    this.cdr.markForCheck();
  }

  isUrgence(titre: string) { return titre.startsWith('🚨'); }

  getType(titre: string): string {
    const m = titre.match(/^\[([^\]]+)\]/);
    return m ? m[1] : '';
  }

  typeConfig(r: ReclamationDto): { icon: string; color: string; bg: string } {
    if (this.isUrgence(r.titre)) return { icon: 'emergency', color: '#ef4444', bg: '#fee2e2' };
    const type = this.getType(r.titre);
    const map: Record<string, { icon: string; color: string; bg: string }> = {
      'Réclamation': { icon: 'report_problem', color: '#f59e0b', bg: '#fef3c7' },
      'Retard':      { icon: 'schedule',       color: '#3b82f6', bg: '#dbeafe' },
      'Marchandise': { icon: 'inventory_2',    color: '#8b5cf6', bg: '#ede9fe' },
      'Service':     { icon: 'star_half',      color: '#06b6d4', bg: '#e0f2fe' },
    };
    return map[type] ?? { icon: 'report_problem', color: '#f59e0b', bg: '#fef3c7' };
  }

  cardClass(r: ReclamationDto): string { return ''; }

  statutConfig(statut: string): { label: string; icon: string; cls: string } {
    const map: Record<string, { label: string; icon: string; cls: string }> = {
      EnAttente: { label: 'En attente',  icon: 'hourglass_empty', cls: 'st-attente' },
      EnCours:   { label: 'En cours',    icon: 'pending',         cls: 'st-encours' },
      Resolue:   { label: 'Résolue',     icon: 'check_circle',    cls: 'st-resolue' },
    };
    return map[statut] ?? { label: statut, icon: 'info', cls: '' };
  }

  roleIcon(role?: string): string {
    if (role === 'Admin')      return 'admin_panel_settings';
    if (role === 'Dispatcher') return 'support_agent';
    if (role === 'Chauffeur')  return 'local_shipping';
    return 'person';
  }
}
