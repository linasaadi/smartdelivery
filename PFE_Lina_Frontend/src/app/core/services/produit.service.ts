import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produit, ProduitFormDto } from '../../shared/models';

const BASE = 'https://localhost:7297/api/produits';

@Injectable({ providedIn: 'root' })
export class ProduitService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Produit[]> { return this.http.get<Produit[]>(BASE); }
  getById(id: number): Observable<Produit> { return this.http.get<Produit>(`${BASE}/${id}`); }
  create(dto: ProduitFormDto): Observable<Produit> { return this.http.post<Produit>(BASE, dto); }
  update(id: number, dto: ProduitFormDto): Observable<Produit> { return this.http.put<Produit>(`${BASE}/${id}`, dto); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${BASE}/${id}`); }
}
