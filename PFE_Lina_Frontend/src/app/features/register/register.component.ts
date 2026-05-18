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
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSelectModule, MatProgressSpinnerModule
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
  success     = signal('');

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(form: NgForm) {
    if (form.invalid) return;

    if (this.motDePasse !== this.confirm) {
      this.error.set('Les mots de passe ne correspondent pas.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.success.set('');

    this.auth.register({
      prenom:     this.prenom,
      nom:        this.nom,
      email:      this.email,
      motDePasse: this.motDePasse,
      role:       this.role
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set('Compte créé avec succès. Vous pouvez maintenant vous connecter.');
        setTimeout(() => this.router.navigate(['/login']), 2000);
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
