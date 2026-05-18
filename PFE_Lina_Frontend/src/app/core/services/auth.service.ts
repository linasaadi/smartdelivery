import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { LoginDto, RegisterDto, TokenResponse, UserRole } from '../models/auth.models';

const BASE      = 'https://localhost:7297/api/auth';
const TOKEN_KEY = 'sd_token';
const USER_KEY  = 'sd_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _user = signal<TokenResponse | null>(this.loadUser());

  readonly user     = this._user.asReadonly();
  readonly isLogged = computed(() => !!this._user());
  readonly role     = computed(() => this._user()?.role ?? null);
  readonly nomComplet = computed(() => {
    const u = this._user();
    return u ? `${u.prenom} ${u.nom}` : '';
  });

  constructor(private http: HttpClient, private router: Router) {}

  login(dto: LoginDto) {
    return this.http.post<TokenResponse>(`${BASE}/login`, dto).pipe(
      tap(res => {
        localStorage.setItem(TOKEN_KEY, res.token);
        localStorage.setItem(USER_KEY, JSON.stringify(res));
        this._user.set(res);
      })
    );
  }

  register(dto: RegisterDto) {
    return this.http.post<void>(`${BASE}/register-public`, dto);
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(...roles: UserRole[]): boolean {
    const r = this.role();
    return r ? roles.includes(r) : false;
  }

  isAdmin()      { return this.hasRole('Admin'); }
  isDispatcher() { return this.hasRole('Admin', 'Dispatcher'); }
  isChauffeur()  { return this.hasRole('Chauffeur'); }

  private loadUser(): TokenResponse | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      if (!raw) return null;
      const u: TokenResponse = JSON.parse(raw);
      // Vérifier l'expiration
      if (new Date(u.expiration) < new Date()) {
        localStorage.clear();
        return null;
      }
      return u;
    } catch { return null; }
  }
}
