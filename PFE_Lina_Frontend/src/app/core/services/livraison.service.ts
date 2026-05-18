import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LivraisonResumeDto, LivraisonDetailDto, CreerLivraisonDto,
  ModifierStatutDto, PointTrackingDto, PageDonnees, StatutLivraison
} from '../../shared/models';

const BASE = 'https://localhost:7297/api/livraisons';

export interface ParametresLivraisons {
  page?: number;
  taillePage?: number;
  statut?: StatutLivraison | '';
  recherche?: string;
}

@Injectable({ providedIn: 'root' })
export class LivraisonService {
  constructor(private http: HttpClient) {}

  getAll(params: ParametresLivraisons = {}): Observable<PageDonnees<LivraisonResumeDto>> {
    let qp = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('taillePage', String(params.taillePage ?? 20));
    if (params.statut)   qp = qp.set('statut', params.statut);
    if (params.recherche) qp = qp.set('recherche', params.recherche);
    return this.http.get<PageDonnees<LivraisonResumeDto>>(BASE, { params: qp });
  }

  getById(id: number): Observable<LivraisonDetailDto> {
    return this.http.get<LivraisonDetailDto>(`${BASE}/${id}`);
  }

  create(dto: CreerLivraisonDto): Observable<LivraisonResumeDto> {
    return this.http.post<LivraisonResumeDto>(BASE, dto);
  }

  updateStatut(id: number, dto: ModifierStatutDto): Observable<void> {
    return this.http.put<void>(`${BASE}/${id}/status`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/${id}`);
  }

  getDelayed(): Observable<LivraisonResumeDto[]> {
    return this.http.get<LivraisonResumeDto[]>(`${BASE}/delayed`);
  }

  getTracking(id: number): Observable<PointTrackingDto[]> {
    return this.http.get<PointTrackingDto[]>(`${BASE}/${id}/tracking`);
  }

  getAnomalies(id: number): Observable<any> {
    return this.http.get<any>(`${BASE}/${id}/anomalies`);
  }
}
