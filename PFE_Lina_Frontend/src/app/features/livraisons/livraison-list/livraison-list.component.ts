import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';

import { LivraisonService } from '../../../core/services/livraison.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { LivraisonResumeDto, StatutLivraison } from '../../../shared/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LivraisonFormComponent } from '../livraison-form/livraison-form.component';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  livraisons: LivraisonResumeDto[];
}

@Component({
  selector: 'app-livraison-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatFormFieldModule, MatInputModule,
    MatDialogModule, MatPaginatorModule, MatTooltipModule,
    StatusBadgeComponent
  ],
  templateUrl: './livraison-list.component.html',
  styleUrl: './livraison-list.component.css'
})
export class LivraisonListComponent implements OnInit {
  private svc    = inject(LivraisonService);
  private dialog = inject(MatDialog);
  private swal   = inject(SweetAlertService);
  private cdr    = inject(ChangeDetectorRef);

  // ── liste ──────────────────────────────────────────────────────────────
  elements: LivraisonResumeDto[] = [];
  allElements: LivraisonResumeDto[] = [];
  totalElements = 0;
  filteredStatut: StatutLivraison | '' = '';
  searchRef = '';
  pageIndex = 0;
  pageSize  = 20;

  statuts: Array<{ value: StatutLivraison | '', label: string }> = [
    { value: '',          label: 'Tous les statuts' },
    { value: 'EnAttente', label: 'En attente' },
    { value: 'EnCours',   label: 'En cours' },
    { value: 'Livree',    label: 'Livrée' },
    { value: 'EnRetard',  label: 'En retard' },
    { value: 'Annulee',   label: 'Annulée' },
    { value: 'Refusee',   label: 'Refusée' },
  ];

  displayedColumns = ['reference', 'statut', 'destination', 'camion', 'datePrevue', 'cout', 'actions'];

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

  // ── chargement paginé ──────────────────────────────────────────────────
  load() {
    this.svc.getAll({
      page: this.pageIndex + 1, taillePage: this.pageSize,
      statut: this.filteredStatut || undefined,
      recherche: this.searchRef || undefined,
    }).subscribe(page => {
      this.elements      = page.elements;
      this.totalElements = page.totalElements;
      this.cdr.markForCheck();
    });
  }

  // ── chargement complet pour calendrier ────────────────────────────────
  loadAll() {
    this.svc.getAll({ page: 1, taillePage: 500 }).subscribe(page => {
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
    const firstDay = new Date(year, month, 1);
    const start    = new Date(firstDay);
    start.setDate(start.getDate() - ((start.getDay() + 6) % 7));
    this.calendarDays = [];
    for (let i = 0; i < 42; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      d.setHours(0, 0, 0, 0);
      const ds = d.toISOString().split('T')[0];
      this.calendarDays.push({
        date: d,
        isCurrentMonth: d.getMonth() === month,
        isToday: d.getTime() === today.getTime(),
        livraisons: this.allElements.filter(l => l.dateLivraisonPrevue?.startsWith(ds))
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
    this.buildCalendar(); this.cdr.markForCheck();
  }

  nextMonth() {
    this.calendarDate = new Date(this.calendarDate.getFullYear(), this.calendarDate.getMonth() + 1, 1);
    this.buildCalendar(); this.cdr.markForCheck();
  }

  selectDay(day: CalendarDay) {
    this.selectedDay = this.selectedDay?.date.getTime() === day.date.getTime() ? null : day;
    this.cdr.markForCheck();
  }

  statColor(statut: StatutLivraison): string {
    const m: Record<StatutLivraison, string> = {
      EnAttente: '#f59e0b', EnCours: '#3b82f6', Livree: '#22c55e',
      EnRetard: '#ef4444', Annulee: '#94a3b8', Refusee: '#f43f5e'
    };
    return m[statut] ?? '#94a3b8';
  }

  // ── filtres / pagination ───────────────────────────────────────────────
  onStatutChange(v: StatutLivraison | '') { this.filteredStatut = v; this.pageIndex = 0; this.load(); }
  onSearch() { this.pageIndex = 0; this.load(); }
  clearSearch() { this.searchRef = ''; this.pageIndex = 0; this.load(); }
  onPage(e: PageEvent) { this.pageIndex = e.pageIndex; this.pageSize = e.pageSize; this.load(); }

  openForm() {
    this.dialog.open(LivraisonFormComponent, { width: '640px', maxWidth: '95vw', data: null })
      .afterClosed().subscribe(saved => {
        if (saved) { this.load(); this.loadAll(); this.swal.succes('Livraison créée', ''); }
      });
  }

  async delete(l: LivraisonResumeDto) {
    const ok = await this.swal.confirmerSuppression(`la livraison ${l.reference}`);
    if (!ok) return;
    this.svc.delete(l.id).subscribe({
      next: () => { this.swal.succes('Livraison supprimée'); this.load(); this.loadAll(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer cette livraison.')
    });
  }
}
