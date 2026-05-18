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

  elements: LivraisonResumeDto[] = [];
  totalElements = 0;
  filteredStatut: StatutLivraison | '' = '';
  searchRef = '';
  pageIndex = 0;
  pageSize  = 20;

  statuts: Array<{ value: StatutLivraison | '', label: string }> = [
    { value: '', label: 'Tous les statuts' },
    { value: 'EnAttente', label: 'En attente' },
    { value: 'EnCours', label: 'En cours' },
    { value: 'Livree', label: 'Livrée' },
    { value: 'EnRetard', label: 'En retard' },
    { value: 'Annulee', label: 'Annulée' },
  ];

  displayedColumns = ['reference', 'statut', 'destination', 'camion', 'datePrevue', 'cout', 'actions'];

  ngOnInit() { this.load(); }

  load() {
    this.svc.getAll({
      page: this.pageIndex + 1,
      taillePage: this.pageSize,
      statut: this.filteredStatut || undefined,
      recherche: this.searchRef || undefined,
    }).subscribe(page => {
      this.elements      = page.elements;
      this.totalElements = page.totalElements;
      this.cdr.markForCheck();
    });
  }

  onStatutChange(v: StatutLivraison | '') {
    this.filteredStatut = v;
    this.pageIndex = 0;
    this.load();
  }

  onSearch() {
    this.pageIndex = 0;
    this.load();
  }

  clearSearch() {
    this.searchRef = '';
    this.pageIndex = 0;
    this.load();
  }

  onPage(e: PageEvent) {
    this.pageIndex = e.pageIndex;
    this.pageSize  = e.pageSize;
    this.load();
  }

  openForm() {
    this.dialog.open(LivraisonFormComponent, { width: '640px', maxWidth: '95vw', data: null })
      .afterClosed().subscribe(saved => {
        if (saved) { this.load(); this.swal.succes('Livraison créée', ''); }
      });
  }

  async delete(l: LivraisonResumeDto) {
    const ok = await this.swal.confirmerSuppression(`la livraison ${l.reference}`);
    if (!ok) return;
    this.svc.delete(l.id).subscribe({
      next: () => { this.swal.succes('Livraison supprimée'); this.load(); },
      error: () => this.swal.erreur('Erreur', 'Impossible de supprimer cette livraison.')
    });
  }
}
