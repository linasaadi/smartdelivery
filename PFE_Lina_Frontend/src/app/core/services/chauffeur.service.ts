import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChauffeurDetailDto, ChauffeurFormDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/chauffeurs';

@Injectable({ providedIn: 'root' })
export class ChauffeurService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<ChauffeurDetailDto[]> {
    return this.http.get<ChauffeurDetailDto[]>(BASE);
  }

  getById(id: number): Observable<ChauffeurDetailDto> {
    return this.http.get<ChauffeurDetailDto>(`${BASE}/${id}`);
  }

  create(dto: ChauffeurFormDto): Observable<ChauffeurDetailDto> {
    return this.http.post<ChauffeurDetailDto>(BASE, dto);
  }

  update(id: number, dto: ChauffeurFormDto): Observable<ChauffeurDetailDto> {
    return this.http.put<ChauffeurDetailDto>(`${BASE}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/${id}`);
  }
}
