import { Component, OnInit, inject, Inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { provideNativeDateAdapter } from '@angular/material/core';
import { forkJoin } from 'rxjs';

import { LivraisonService } from '../../../core/services/livraison.service';
import { CamionService } from '../../../core/services/camion.service';
import { DestinationService } from '../../../core/services/destination.service';
import { ChauffeurService } from '../../../core/services/chauffeur.service';
import { CamionResumeDto, Destination, ChauffeurResumeDto, CreerLivraisonDto } from '../../../shared/models';

@Component({
  selector: 'app-livraison-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, FormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatDatepickerModule,
    MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './livraison-form.component.html'
})
export class LivraisonFormComponent implements OnInit {
  private livraisonSvc = inject(LivraisonService);
  private camionSvc    = inject(CamionService);
  private destSvc      = inject(DestinationService);
  private chauffeurSvc = inject(ChauffeurService);
  private dialogRef    = inject(MatDialogRef<LivraisonFormComponent>);
  private cdr          = inject(ChangeDetectorRef);

  camions:    CamionResumeDto[]    = [];
  destinations: Destination[]     = [];
  chauffeurs: ChauffeurResumeDto[] = [];
  loading = false;
  error   = '';

  // Assignation
  camionId?: number;
  destinationId?: number;
  dateLivraisonPrevue: any = '';
  notes = '';

  // Produit inline
  nomProduit          = '';
  descriptionProduit  = '';
  poidsKg             = 0;
  volumeM3            = 0;
  quantite            = 1;
  prixUnitaire        = 0;

  // Filtre camions par chauffeur sélectionné
  chauffeurSelectionneId?: number;
  get camionsFiltres(): CamionResumeDto[] {
    if (!this.chauffeurSelectionneId) return this.camions;
    const nomChauffeur = this.chauffeurs.find(c => c.id === this.chauffeurSelectionneId)?.nomComplet;
    return this.camions.filter(c => c.nomChauffeur === nomChauffeur);
  }

  constructor(@Inject(MAT_DIALOG_DATA) public data: null) {}

  ngOnInit() {
    forkJoin({
      camions:      this.camionSvc.getAvailable(),
      destinations: this.destSvc.getAll(),
      chauffeurs:   this.chauffeurSvc.getAll(),
    }).subscribe(({ camions, destinations, chauffeurs }) => {
      this.camions      = camions;
      this.destinations = destinations;
      this.chauffeurs   = chauffeurs
        .filter((c: any) => c.estDisponible)
        .map((c: any) => ({ id: c.id, nomComplet: c.nomComplet ?? `${c.prenom} ${c.nom}`, telephone: c.telephone, estDisponible: c.estDisponible }));
      this.cdr.markForCheck();
    });
  }

  onChauffeurChange() {
    this.camionId = undefined;
    this.cdr.markForCheck();
  }

  save() {
    this.error = '';
    if (!this.camionId || !this.destinationId || !this.dateLivraisonPrevue) {
      this.error = 'Camion, destination et date sont obligatoires.';
      return;
    }
    if (!this.nomProduit.trim()) {
      this.error = 'Le nom du produit est obligatoire.';
      return;
    }
    if (this.quantite < 1) {
      this.error = 'La quantité doit être au moins 1.';
      return;
    }

    const date = this.dateLivraisonPrevue instanceof Date
      ? (this.dateLivraisonPrevue as Date).toISOString()
      : String(this.dateLivraisonPrevue);

    const dto: CreerLivraisonDto = {
      camionId:            this.camionId!,
      destinationId:       this.destinationId!,
      dateLivraisonPrevue: date,
      nomProduit:          this.nomProduit.trim(),
      descriptionProduit:  this.descriptionProduit || undefined,
      poidsKg:             this.poidsKg,
      volumeM3:            this.volumeM3,
      quantite:            this.quantite,
      prixUnitaire:        this.prixUnitaire,
      notes:               this.notes || undefined,
    };

    this.loading = true;
    this.cdr.markForCheck();
    this.livraisonSvc.create(dto).subscribe({
      next: () => { this.loading = false; this.dialogRef.close(true); },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.statusText ?? 'Une erreur est survenue.';
        this.cdr.markForCheck();
      }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
