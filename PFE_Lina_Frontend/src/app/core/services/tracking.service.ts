import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PointTracking, FacteurTrafic } from '../../shared/models';

const BASE = 'https://localhost:7297/api/tracking';

@Injectable({ providedIn: 'root' })
export class TrackingService {
  constructor(private http: HttpClient) {}

  enregistrerPosition(dto: Partial<PointTracking>): Observable<PointTracking> {
    return this.http.post<PointTracking>(BASE, dto);
  }

  getFacteurTrafic(heure?: number, zone?: string): Observable<FacteurTrafic> {
    const params: Record<string, string> = {};
    if (heure !== undefined) params['heure'] = heure.toString();
    if (zone) params['zone'] = zone;
    return this.http.get<FacteurTrafic>(`${BASE}/facteur-trafic`, { params });
  }
}
