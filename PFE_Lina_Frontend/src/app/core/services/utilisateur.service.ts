import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UtilisateurDto, ChangerRoleDto, RegisterDto } from '../models/auth.models';

const BASE = 'https://localhost:7297/api/auth';

@Injectable({ providedIn: 'root' })
export class UtilisateurService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<UtilisateurDto[]> {
    return this.http.get<UtilisateurDto[]>(`${BASE}/utilisateurs`);
  }

  creer(dto: RegisterDto): Observable<void> {
    return this.http.post<void>(`${BASE}/register`, dto);
  }

  changerRole(id: string, dto: ChangerRoleDto): Observable<void> {
    return this.http.put<void>(`${BASE}/utilisateurs/${id}/role`, dto);
  }

  supprimer(id: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/utilisateurs/${id}`);
  }

  lierChauffeur(userId: string, chauffeurId: number): Observable<void> {
    return this.http.put<void>(`${BASE}/utilisateurs/${userId}/lier-chauffeur/${chauffeurId}`, {});
  }
}
