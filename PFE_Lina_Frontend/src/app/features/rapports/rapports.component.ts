import { Component, OnInit, AfterViewInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';

import { LivraisonService } from '../../core/services/livraison.service';
import { TrackingService } from '../../core/services/tracking.service';
import { LivraisonResumeDto, FacteurTrafic } from '../../shared/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-rapports',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTableModule,
    MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatDividerModule, MatTabsModule,
    StatusBadgeComponent
  ],
  templateUrl: './rapports.component.html',
  styleUrl: './rapports.component.css'
})
export class RapportsComponent implements OnInit, AfterViewInit {
  private livraisonSvc = inject(LivraisonService);
  private trackingSvc  = inject(TrackingService);
  private cdr          = inject(ChangeDetectorRef);

  delayed: LivraisonResumeDto[] = [];
  facteurTrafic?: FacteurTrafic;
  heureQuery = new Date().getHours();

  delayedCols = ['reference', 'statut', 'destination', 'datePrevue', 'anomalies'];

  get totalAnomaliesOuvertes() {
    return this.delayed.reduce((sum, l) => sum + (l.nombreAnomaliesOuvertes ?? 0), 0);
  }

  ngOnInit() {
    this.livraisonSvc.getDelayed().subscribe(d => { this.delayed = d; this.cdr.markForCheck(); });
  }

  ngAfterViewInit() {
    this.loadTrafic();
  }

  loadTrafic() {
    this.trackingSvc.getFacteurTrafic(this.heureQuery).subscribe(f => {
      this.facteurTrafic = f;
      this.cdr.markForCheck();
    });
  }
}
