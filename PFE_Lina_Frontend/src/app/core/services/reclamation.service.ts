import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReclamationDto, ReclamationFormDto, ModifierReclamationDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/reclamations';

@Injectable({ providedIn: 'root' })
export class ReclamationService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<ReclamationDto[]> {
    return this.http.get<ReclamationDto[]>(BASE);
  }

  getById(id: number): Observable<ReclamationDto> {
    return this.http.get<ReclamationDto>(`${BASE}/${id}`);
  }

  create(dto: ReclamationFormDto): Observable<void> {
    return this.http.post<void>(BASE, dto);
  }

  modifier(id: number, dto: ModifierReclamationDto): Observable<void> {
    return this.http.put<void>(`${BASE}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/${id}`);
  }
}
