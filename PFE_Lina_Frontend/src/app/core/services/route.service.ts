import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RouteRequest, MultiStopRequest, RouteResult } from '../../shared/models';

const BASE = 'https://localhost:7297/api/routes';

@Injectable({ providedIn: 'root' })
export class RouteService {
  constructor(private http: HttpClient) {}

  shortest(req: RouteRequest): Observable<RouteResult> {
    return this.http.post<RouteResult>(`${BASE}/shortest`, req);
  }

  fastest(req: RouteRequest): Observable<RouteResult> {
    return this.http.post<RouteResult>(`${BASE}/fastest`, req);
  }

  multiStop(req: MultiStopRequest): Observable<RouteResult> {
    return this.http.post<RouteResult>(`${BASE}/multi-stop`, req);
  }
}
