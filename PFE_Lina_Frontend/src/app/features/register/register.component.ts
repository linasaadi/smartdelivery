import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/services/auth.service';
import { TokenResponse } from '../../core/models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSelectModule, MatProgressSpinnerModule, MatCardModule
  ],
  templateUrl: './register.component.html',
  styleUrl:    './register.component.css'
})
export class RegisterComponent {
  prenom     = '';
  nom        = '';
  email      = '';
  motDePasse = '';
  confirm    = '';
  role: 'Dispatcher' | 'Chauffeur' = 'Dispatcher';

  hidePass    = signal(true);
  hideConfirm = signal(true);
  loading     = signal(false);
  error       = signal('');
  // Affiché après inscription pour montrer l'ID attribué
  idAttribue  = signal<{ role: string; id: number } | null>(null);

  constructor(private auth: AuthService, private router: Router) {}

  private readonly passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$/;

  get passStrength(): number {
    const p = this.motDePasse;
    if (!p) return 0;
    let s = 0;
    if (p.length >= 8)          s++;
    if (/[A-Z]/.test(p))        s++;
    if (/[0-9]/.test(p))        s++;
    if (/[^a-zA-Z0-9]/.test(p)) s++;
    return s;
  }
  get passStrengthLabel(): string {
    return ['', 'Faible', 'Moyen', 'Bon', 'Fort'][this.passStrength] ?? '';
  }
  get passStrengthColor(): string {
    return ['', '#ef4444', '#f59e0b', '#10b981', '#6366f1'][this.passStrength] ?? '';
  }

  onSubmit(form: NgForm) {
    if (form.invalid) return;

    if (this.motDePasse !== this.confirm) {
      this.error.set('Les mots de passe ne correspondent pas.');
      return;
    }

    if (!this.passwordRegex.test(this.motDePasse)) {
      this.error.set('Le mot de passe doit contenir au moins 8 caractères, une majuscule, un chiffre et un caractère spécial.');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.auth.register({
      prenom:     this.prenom,
      nom:        this.nom,
      email:      this.email,
      motDePasse: this.motDePasse,
      role:       this.role
    }).subscribe({
      next: (res: TokenResponse) => {
        this.loading.set(false);

        // Afficher l'ID attribué automatiquement
        if (this.role === 'Chauffeur' && res.chauffeurId) {
          this.idAttribue.set({ role: 'Chauffeur', id: res.chauffeurId });
        } else if (this.role === 'Dispatcher' && res.dispatcherId) {
          this.idAttribue.set({ role: 'Dispatcher', id: res.dispatcherId });
        }

        // Redirection automatique vers le tableau de bord du rôle
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 2000);
      },
      error: (err) => {
        this.loading.set(false);
        const data = err.error;
        if (data?.erreurs?.length) {
          this.error.set(data.erreurs[0]);
        } else if (data?.message) {
          this.error.set(data.message);
        } else {
          this.error.set('Une erreur est survenue. Veuillez réessayer.');
        }
      }
    });
  }
}
