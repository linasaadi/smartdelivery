import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';

import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { LivraisonService } from '../../core/services/livraison.service';
import { KpiAdminDto, KpiDispatcherDto, LivraisonResumeDto, CamionResumeDto } from '../../shared/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

interface KpiCard {
  title: string;
  value: number | string;
  icon: string;
  trend: string;
  style: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, RouterModule, MatCardModule, MatIconModule,
    MatTableModule, MatButtonModule, StatusBadgeComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private dashboardSvc  = inject(DashboardService);
  private authSvc       = inject(AuthService);
  private livraisonSvc  = inject(LivraisonService);
  private cdr           = inject(ChangeDetectorRef);

  isChauffeur = false;
  isAdmin     = false;

  stats: KpiCard[] = [];
  dernieresLivraisons: LivraisonResumeDto[] = [];
  camionsResume: CamionResumeDto[] = [];
  totalLivraisons = 0;
  enRetard  = 0;
  enCours   = 0;
  livrees   = 0;
  enAttente = 0;
  displayedColumns        = ['reference', 'statut', 'destination', 'datePrevue'];
  displayedColumnsChauffeur = ['reference', 'statut', 'destination', 'datePrevue'];

  ngOnInit() {
    this.isChauffeur = this.authSvc.isChauffeur();
    this.isAdmin     = this.authSvc.isAdmin();

    if (this.isChauffeur) {
      this.loadChauffeurDashboard();
    } else if (this.isAdmin) {
      this.dashboardSvc.getKpiAdmin().subscribe(kpi => {
        this.buildStatsAdmin(kpi);
        this.dernieresLivraisons = kpi.livraisonsEnRetard.slice(0, 8);
        this.totalLivraisons = kpi.totalLivraisons;
        this.enRetard  = kpi.enRetard;
        this.enCours   = kpi.enCours;
        this.livrees   = kpi.livrees;
        this.enAttente = kpi.enAttente;
        this.cdr.markForCheck();
      });
    } else {
      this.dashboardSvc.getKpiDispatcher().subscribe(kpi => {
        this.buildStatsDispatcher(kpi);
        this.dernieresLivraisons = kpi.urgentesEnRetard.slice(0, 8);
        this.enAttente = kpi.livraisonsEnAttente;
        this.enCours   = kpi.livraisonsEnCours;
        this.enRetard  = kpi.urgentesEnRetard.length;
        this.cdr.markForCheck();
      });
    }
  }

  private loadChauffeurDashboard() {
    this.livraisonSvc.getMesLivraisons({ page: 1, taillePage: 200 }).subscribe(page => {
      const all = page.elements;
      this.totalLivraisons = page.totalElements;
      this.enCours   = all.filter(l => l.statut === 'EnCours').length;
      this.livrees   = all.filter(l => l.statut === 'Livree').length;
      this.enRetard  = all.filter(l => l.statut === 'EnRetard').length;
      this.enAttente = all.filter(l => l.statut === 'EnAttente').length;

      this.stats = [
        {
          title: 'En cours', value: this.enCours, icon: 'local_shipping',
          trend: 'Livraisons actives',
          style: '--kpi-from:#2563eb;--kpi-to:#1d4ed8'
        },
        {
          title: 'En attente', value: this.enAttente, icon: 'schedule',
          trend: 'À accepter / confirmer',
          style: '--kpi-from:#f59e0b;--kpi-to:#d97706'
        },
        {
          title: 'Livrées', value: this.livrees, icon: 'task_alt',
          trend: 'Missions accomplies',
          style: '--kpi-from:#059669;--kpi-to:#047857'
        },
        {
          title: 'En retard', value: this.enRetard, icon: 'warning_amber',
          trend: this.enRetard > 0 ? 'Action requise !' : 'Aucun retard',
          style: '--kpi-from:#dc2626;--kpi-to:#b91c1c'
        },
      ];

      // Show active (EnCours + EnAttente) livraisons as the upcoming list
      this.dernieresLivraisons = all
        .filter(l => l.statut === 'EnCours' || l.statut === 'EnAttente')
        .slice(0, 6);
      this.cdr.markForCheck();
    });
  }

  private buildStatsAdmin(kpi: KpiAdminDto) {
    this.stats = [
      {
        title: 'Total livraisons', value: kpi.totalLivraisons, icon: 'local_shipping',
        trend: `${kpi.livrees} livrées · ${kpi.enRetard} en retard`,
        style: '--kpi-from:#2563eb;--kpi-to:#1d4ed8'
      },
      {
        title: 'Camions disponibles', value: kpi.camionsDisponibles, icon: 'directions_car',
        trend: `${kpi.totalCamions} dans la flotte`,
        style: '--kpi-from:#059669;--kpi-to:#047857'
      },
      {
        title: 'En retard', value: kpi.enRetard, icon: 'warning_amber',
        trend: kpi.enRetard > 0 ? 'Attention requise' : 'Tout est à jour',
        style: '--kpi-from:#dc2626;--kpi-to:#b91c1c'
      },
      {
        title: 'Taux de réussite', value: kpi.tauxReussite.toFixed(1) + ' %', icon: 'task_alt',
        trend: `${kpi.chauffeursDisponibles} chauffeur(s) disponible(s)`,
        style: '--kpi-from:#7c3aed;--kpi-to:#6d28d9'
      },
    ];
  }

  private buildStatsDispatcher(kpi: KpiDispatcherDto) {
    this.stats = [
      {
        title: 'En attente', value: kpi.livraisonsEnAttente, icon: 'schedule',
        trend: 'Livraisons à dispatcher',
        style: '--kpi-from:#f59e0b;--kpi-to:#d97706'
      },
      {
        title: 'En cours', value: kpi.livraisonsEnCours, icon: 'local_shipping',
        trend: 'Livraisons en transit',
        style: '--kpi-from:#2563eb;--kpi-to:#1d4ed8'
      },
      {
        title: 'Camions disponibles', value: kpi.camionsDisponibles, icon: 'directions_car',
        trend: 'Prêts à être assignés',
        style: '--kpi-from:#059669;--kpi-to:#047857'
      },
      {
        title: 'Chauffeurs disponibles', value: kpi.chauffeursDisponibles, icon: 'person',
        trend: `${kpi.urgentesEnRetard.length} livraison(s) urgente(s)`,
        style: '--kpi-from:#7c3aed;--kpi-to:#6d28d9'
      },
    ];
  }
}
