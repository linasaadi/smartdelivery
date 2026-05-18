import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { KpiAdminDto, KpiDispatcherDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/dashboard';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  getKpiAdmin(): Observable<KpiAdminDto> {
    return this.http.get<KpiAdminDto>(`${BASE}/admin`);
  }

  getKpiDispatcher(): Observable<KpiDispatcherDto> {
    return this.http.get<KpiDispatcherDto>(`${BASE}/dispatcher`);
  }
}
