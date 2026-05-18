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
import { ProduitService } from '../../../core/services/produit.service';
import { CamionResumeDto, Destination, Produit, CreerLivraisonDto } from '../../../shared/models';

interface LigneProduit { produitId: number; quantite: number; }

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
  private produitSvc   = inject(ProduitService);
  private dialogRef    = inject(MatDialogRef<LivraisonFormComponent>);
  private cdr          = inject(ChangeDetectorRef);

  camions: CamionResumeDto[] = [];
  destinations: Destination[] = [];
  produits: Produit[] = [];
  loading = false;
  error   = '';

  camionId?: number;
  destinationId?: number;
  dateLivraisonPrevue: any = '';
  notes = '';
  lignes: LigneProduit[] = [{ produitId: 0, quantite: 1 }];

  constructor(@Inject(MAT_DIALOG_DATA) public data: null) {}

  ngOnInit() {
    forkJoin({
      camions:      this.camionSvc.getAll(),
      destinations: this.destSvc.getAll(),
      produits:     this.produitSvc.getAll(),
    }).subscribe(({ camions, destinations, produits }) => {
      this.camions      = camions;
      this.destinations = destinations;
      this.produits     = produits;
      this.cdr.markForCheck();
    });
  }

  ajouterLigne() {
    this.lignes = [...this.lignes, { produitId: 0, quantite: 1 }];
  }

  retirerLigne(i: number) {
    if (this.lignes.length > 1) this.lignes = this.lignes.filter((_, idx) => idx !== i);
  }

  save() {
    this.error = '';
    if (!this.camionId || !this.destinationId || !this.dateLivraisonPrevue) {
      this.error = 'Camion, destination et date sont obligatoires.';
      return;
    }
    const lignesValides = this.lignes.filter(l => l.produitId > 0 && l.quantite > 0);
    if (lignesValides.length === 0) {
      this.error = 'Ajoutez au moins un produit.';
      return;
    }

    const date = this.dateLivraisonPrevue instanceof Date
      ? (this.dateLivraisonPrevue as Date).toISOString()
      : String(this.dateLivraisonPrevue);

    const dto: CreerLivraisonDto = {
      camionId:            this.camionId!,
      destinationId:       this.destinationId!,
      dateLivraisonPrevue: date,
      notes:               this.notes || undefined,
      produits:            lignesValides.map(l => ({ produitId: l.produitId, quantite: l.quantite, nomProduit: null, prixTotal: 0 })),
    };

    this.loading = true;
    this.cdr.markForCheck();
    this.livraisonSvc.create(dto).subscribe({
      next: () => { this.loading = false; this.dialogRef.close(true); },
      error: (err) => {
        this.loading = false;
        this.error = err?.statusText ?? 'Une erreur est survenue.';
        this.cdr.markForCheck();
      }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
