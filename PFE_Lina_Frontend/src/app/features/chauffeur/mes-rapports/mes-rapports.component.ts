import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { LivraisonService } from '../../../core/services/livraison.service';
import { ReclamationService } from '../../../core/services/reclamation.service';
import { SweetAlertService } from '../../../core/services/sweet-alert.service';
import { LivraisonResumeDto, ReclamationDto } from '../../../shared/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-mes-rapports',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, RouterModule, FormsModule, ReactiveFormsModule,
    MatTabsModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatTooltipModule, MatProgressSpinnerModule,
    StatusBadgeComponent
  ],
  templateUrl: './mes-rapports.component.html',
  styleUrl: './mes-rapports.component.css'
})
export class MesRapportsComponent implements OnInit {
  private livraisonSvc = inject(LivraisonService);
  private reclamSvc    = inject(ReclamationService);
  private swal         = inject(SweetAlertService);
  private cdr          = inject(ChangeDetectorRef);
  private fb           = inject(FormBuilder);

  // ── données ────────────────────────────────────────────────────────────
  livraisonsEnCours:   LivraisonResumeDto[] = [];
  livraisonsTerminees: LivraisonResumeDto[] = [];
  rapportsEnvoyes:     ReclamationDto[]     = [];

  loading         = true;
  soumettreAlert  = false;
  soumettreRapport = false;

  readonly typesProbleme = [
    'Aucun problème',
    'Retard de livraison',
    'Marchandise endommagée',
    'Client absent',
    'Adresse introuvable',
    'Problème mécanique',
    'Autre',
  ];

  // ── formulaires ────────────────────────────────────────────────────────
  urgenceForm = this.fb.group({
    livraisonId:  [null as number | null, Validators.required],
    typeUrgence:  ['', Validators.required],
    description:  ['', [Validators.required, Validators.minLength(10)]],
  });

  rapportForm = this.fb.group({
    livraisonId:   [null as number | null, Validators.required],
    typeProbleme:  ['Aucun problème', Validators.required],
    notes:         ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly typesUrgence = [
    'Panne mécanique',
    'Accident de la route',
    'Retard important (> 1h)',
    'Route bloquée',
    'Problème de sécurité',
    'Autre urgence',
  ];

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    let done = 0;
    const check = () => { if (++done === 3) { this.loading = false; this.cdr.markForCheck(); } };

    this.livraisonSvc.getMesLivraisons({ page: 1, taillePage: 100, statut: 'EnCours' }).subscribe(p => {
      this.livraisonsEnCours = p.elements;
      check();
    });

    this.livraisonSvc.getMesLivraisons({ page: 1, taillePage: 100, statut: 'Livree' }).subscribe(p => {
      this.livraisonsTerminees = p.elements;
      check();
    });

    this.reclamSvc.getAll().subscribe(r => {
      this.rapportsEnvoyes = r.sort((a, b) =>
        new Date(b.dateCreation).getTime() - new Date(a.dateCreation).getTime()
      );
      check();
    });
  }

  // ── alerte urgence ─────────────────────────────────────────────────────
  async envoyerAlerte() {
    if (this.urgenceForm.invalid) { this.urgenceForm.markAllAsTouched(); return; }
    const { livraisonId, typeUrgence, description } = this.urgenceForm.value;
    const livraison = this.livraisonsEnCours.find(l => l.id === livraisonId);

    this.soumettreAlert = true;
    this.reclamSvc.create({
      titre: `🚨 URGENCE [${typeUrgence}] — ${livraison?.reference ?? 'Livraison'}`,
      description: description!,
      livraisonId: livraisonId!,
    }).subscribe({
      next: () => {
        this.swal.succes('Alerte envoyée !', 'Le dispatcher a été notifié immédiatement.');
        this.urgenceForm.reset();
        this.soumettreAlert = false;
        this.load();
      },
      error: err => {
        this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible d\'envoyer l\'alerte.');
        this.soumettreAlert = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ── rapport de fin de livraison ────────────────────────────────────────
  async envoyerRapport() {
    if (this.rapportForm.invalid) { this.rapportForm.markAllAsTouched(); return; }
    const { livraisonId, typeProbleme, notes } = this.rapportForm.value;
    const livraison = this.livraisonsTerminees.find(l => l.id === livraisonId);

    this.soumettreRapport = true;
    const description = `Type : ${typeProbleme}\n\n${notes}`;
    this.reclamSvc.create({
      titre: `Rapport livraison — ${livraison?.reference ?? 'Livraison'}`,
      description,
      livraisonId: livraisonId!,
    }).subscribe({
      next: () => {
        this.swal.succes('Rapport soumis !', 'Votre rapport a été transmis au dispatcher.');
        this.rapportForm.reset({ typeProbleme: 'Aucun problème' });
        this.soumettreRapport = false;
        this.load();
      },
      error: err => {
        this.swal.erreur('Erreur', err?.error?.message ?? 'Impossible de soumettre le rapport.');
        this.soumettreRapport = false;
        this.cdr.markForCheck();
      }
    });
  }

  isUrgence(titre: string): boolean {
    return titre.startsWith('🚨');
  }

  statutColor(statut: string): string {
    if (statut === 'Resolue') return '#22c55e';
    if (statut === 'EnCours') return '#3b82f6';
    return '#f59e0b';
  }

  statutLabel(statut: string): string {
    if (statut === 'Resolue') return 'Résolu';
    if (statut === 'EnCours') return 'En traitement';
    return 'En attente';
  }
}
