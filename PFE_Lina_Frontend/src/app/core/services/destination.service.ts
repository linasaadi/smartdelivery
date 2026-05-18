import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Destination, DestinationFormDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/destinations';

@Injectable({ providedIn: 'root' })
export class DestinationService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Destination[]> { return this.http.get<Destination[]>(BASE); }
  getById(id: number): Observable<Destination> { return this.http.get<Destination>(`${BASE}/${id}`); }
  create(dto: DestinationFormDto): Observable<Destination> { return this.http.post<Destination>(BASE, dto); }
  update(id: number, dto: DestinationFormDto): Observable<Destination> { return this.http.put<Destination>(`${BASE}/${id}`, dto); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${BASE}/${id}`); }
}
