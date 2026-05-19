import { Component, OnInit, inject, signal, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';

import { LivraisonService } from '../../../core/services/livraison.service';
import { ReclamationService } from '../../../core/services/reclamation.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { LivraisonResumeDto, StatutLivraison } from '../../../shared/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

export interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  livraisons: LivraisonResumeDto[];
}

@Component({
  selector: 'app-mes-livraisons',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule, MatSelectModule,
    MatFormFieldModule, MatPaginatorModule, MatTooltipModule, MatBadgeModule,
    StatusBadgeComponent
  ],
  templateUrl: './mes-livraisons.component.html',
  styleUrl: './mes-livraisons.component.css'
})
export class MesLivraisonsComponent implements OnInit {
  private svc           = inject(LivraisonService);
  private reclamSvc     = inject(ReclamationService);
  private swal          = inject(SweetAlertService);
  private cdr           = inject(ChangeDetectorRef);

  // ── liste ──────────────────────────────────────────────────────────────
  elements: LivraisonResumeDto[] = [];
  allElements: LivraisonResumeDto[] = [];   // toutes les livraisons pour le calendrier
  totalElements = 0;
  filteredStatut: StatutLivraison | '' = '';
  pageIndex = 0;
  pageSize  = 10;
  erreur = signal('');

  readonly statuts: Array<{ value: StatutLivraison | ''; label: string }> = [
    { value: '',          label: 'Tous' },
    { value: 'EnAttente', label: 'En attente' },
    { value: 'EnCours',   label: 'En cours' },
    { value: 'Livree',    label: 'Livrée' },
    { value: 'Refusee',   label: 'Refusée' },
    { value: 'EnRetard',  label: 'En retard' },
  ];

  displayedColumns = ['reference', 'statut', 'destination', 'datePrevue', 'actions'];

  // ── vue ────────────────────────────────────────────────────────────────
  vue: 'liste' | 'calendrier' = 'liste';

  // ── calendrier ─────────────────────────────────────────────────────────
  calendarDate  = new Date();
  calendarDays: CalendarDay[] = [];
  joursLabels   = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];
  selectedDay: CalendarDay | null = null;

  get calendarTitle(): string {
    return this.calendarDate.toLocaleDateString('fr-FR', { month: 'long', year: 'numeric' });
  }

  ngOnInit() { this.load(); this.loadAll(); }

  // ── chargement paginé (liste) ──────────────────────────────────────────
  load() {
    this.erreur.set('');
    this.svc.getMesLivraisons({
      page: this.pageIndex + 1, taillePage: this.pageSize,
      statut: this.filteredStatut || undefined
    }).subscribe({
      next: page => {
        this.elements      = page.elements;
        this.totalElements = page.totalElements;
        this.cdr.markForCheck();
      },
      error: err => {
        this.erreur.set(err?.error?.message ?? 'Impossible de charger vos livraisons.');
        this.cdr.markForCheck();
      }
    });
  }

  // ── chargement complet pour calendrier ────────────────────────────────
  loadAll() {
    this.svc.getMesLivraisons({ page: 1, taillePage: 200 }).subscribe(page => {
      this.allElements = page.elements;
      this.buildCalendar();
      this.cdr.markForCheck();
    });
  }

  // ── calendrier ─────────────────────────────────────────────────────────
  buildCalendar() {
    const year  = this.calendarDate.getFullYear();
    const month = this.calendarDate.getMonth();
    const today = new Date(); today.setHours(0, 0, 0, 0);

    const firstDay  = new Date(year, month, 1);
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - ((startDate.getDay() + 6) % 7));

    this.calendarDays = [];
    for (let i = 0; i < 42; i++) {
      const d = new Date(startDate);
      d.setDate(startDate.getDate() + i);
      d.setHours(0, 0, 0, 0);
      const dateStr = d.toISOString().split('T')[0];
      this.calendarDays.push({
        date: d,
        isCurrentMonth: d.getMonth() === month,
        isToday: d.getTime() === today.getTime(),
        livraisons: this.allElements.filter(l =>
          l.dateLivraisonPrevue?.startsWith(dateStr)
        )
      });
    }
    this.selectedDay = null;
  }

  switchVue(v: 'liste' | 'calendrier') {
    this.vue = v;
    if (v === 'calendrier') this.buildCalendar();
    this.cdr.markForCheck();
  }

  prevMonth() {
    this.calendarDate = new Date(this.calendarDate.getFullYear(), this.calendarDate.getMonth() - 1, 1);
    this.buildCalendar();
    this.cdr.markForCheck();
  }

  nextMonth() {
    this.calendarDate = new Date(this.calendarDate.getFullYear(), this.calendarDate.getMonth() + 1, 1);
    this.buildCalendar();
    this.cdr.markForCheck();
  }

  selectDay(day: CalendarDay) {
    this.selectedDay = this.selectedDay?.date.getTime() === day.date.getTime() ? null : day;
    this.cdr.markForCheck();
  }

  statColor(statut: StatutLivraison): string {
    const map: Record<StatutLivraison, string> = {
      EnAttente: '#f59e0b', EnCours: '#3b82f6',
      Livree: '#22c55e', EnRetard: '#ef4444',
      Annulee: '#94a3b8', Refusee: '#f43f5e'
    };
    return map[statut] ?? '#94a3b8';
  }

  // ── pagination / filtre ────────────────────────────────────────────────
  onPage(e: PageEvent) { this.pageIndex = e.pageIndex; this.pageSize = e.pageSize; this.load(); }
  onStatut(v: StatutLivraison | '') { this.filteredStatut = v; this.pageIndex = 0; this.load(); }

  // ── actions livraison ──────────────────────────────────────────────────
  async accepter(l: LivraisonResumeDto) {
    const ok = await this.swal.confirmerSuppression(`Accepter la livraison ${l.reference} ?`);
    if (!ok) return;
    this.svc.accepter(l.id).subscribe({
      next: () => { this.swal.succes('Livraison acceptée !', 'Bonne route !'); this.load(); this.loadAll(); },
      error: (err) => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible d\'accepter.')
    });
  }

  async refuser(l: LivraisonResumeDto) {
    const raison = await this.swal.demanderTexte(
      `Refuser la livraison ${l.reference}`, 'Motif du refus',
      'Expliquez pourquoi vous refusez cette livraison...'
    );
    if (raison === null) return;
    this.svc.refuser(l.id, raison || 'Refus sans motif').subscribe({
      next: () => { this.swal.succes('Livraison refusée', 'Le dispatcher a été notifié.'); this.load(); this.loadAll(); },
      error: (err) => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible de refuser.')
    });
  }

  async signalerUrgence(l: LivraisonResumeDto) {
    const desc = await this.swal.demanderTexte(
      `Signaler une urgence — ${l.reference}`,
      'Description du problème',
      'Ex : panne mécanique, accident, route bloquée...'
    );
    if (desc === null) return;
    this.reclamSvc.create({
      titre: `🚨 URGENCE — ${l.reference}`,
      description: desc || 'Urgence signalée par le chauffeur.',
      livraisonId: l.id
    }).subscribe({
      next: () => this.swal.succes('Alerte envoyée !', 'Le dispatcher a été notifié immédiatement.'),
      error: (err) => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible d\'envoyer l\'alerte.')
    });
  }

  async soumettreRapport(l: LivraisonResumeDto) {
    const notes = await this.swal.demanderTexte(
      `Rapport de livraison — ${l.reference}`,
      'Notes de fin de mission',
      'Décrivez le déroulement de la livraison, problèmes rencontrés, état de la marchandise...'
    );
    if (notes === null) return;
    this.reclamSvc.create({
      titre: `Rapport livraison — ${l.reference}`,
      description: notes || 'Livraison effectuée sans remarque particulière.',
      livraisonId: l.id
    }).subscribe({
      next: () => this.swal.succes('Rapport envoyé !', 'Votre rapport a été transmis au dispatcher.'),
      error: (err) => this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible de soumettre le rapport.')
    });
  }
}
