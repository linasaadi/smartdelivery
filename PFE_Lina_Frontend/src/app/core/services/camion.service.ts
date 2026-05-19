import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CamionResumeDto, CamionDetailDto, CamionFormDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/camions';

@Injectable({ providedIn: 'root' })
export class CamionService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<CamionResumeDto[]> {
    return this.http.get<CamionResumeDto[]>(BASE);
  }

  getMesCamions(): Observable<CamionResumeDto[]> {
    return this.http.get<CamionResumeDto[]>(`${BASE}/mes-camions`);
  }

  getAvailable(): Observable<CamionResumeDto[]> {
    return this.http.get<CamionResumeDto[]>(`${BASE}/available`);
  }

  getById(id: number): Observable<CamionDetailDto> {
    return this.http.get<CamionDetailDto>(`${BASE}/${id}`);
  }

  create(dto: CamionFormDto): Observable<CamionResumeDto> {
    return this.http.post<CamionResumeDto>(BASE, dto);
  }

  update(id: number, dto: CamionFormDto): Observable<CamionResumeDto> {
    return this.http.put<CamionResumeDto>(`${BASE}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/${id}`);
  }
}
