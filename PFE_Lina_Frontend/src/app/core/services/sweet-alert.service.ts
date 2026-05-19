import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';
import { UtilisateurDto } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class SweetAlertService {

  async confirmerSuppression(nom = 'cet élément'): Promise<boolean> {
    const result = await Swal.fire({
      title: 'Confirmer la suppression',
      html: `Voulez-vous vraiment supprimer <strong>${nom}</strong> ?<br><small style="color:#94a3b8">Cette action est irréversible.</small>`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Oui, supprimer',
      cancelButtonText: 'Annuler',
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#64748b',
      reverseButtons: true,
      customClass: { popup: 'swal-popup' }
    });
    return result.isConfirmed;
  }

  succes(titre: string, message = '') {
    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: titre,
      text: message || undefined,
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true,
      customClass: { popup: 'swal-toast' }
    });
  }

  erreur(titre: string, message = '') {
    Swal.fire({
      icon: 'error',
      title: titre,
      text: message || undefined,
      confirmButtonColor: '#2563eb',
      customClass: { popup: 'swal-popup' }
    });
  }

  info(titre: string, message = '') {
    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'info',
      title: titre,
      text: message || undefined,
      showConfirmButton: false,
      timer: 3500,
      timerProgressBar: true,
      customClass: { popup: 'swal-toast' }
    });
  }

  async choisirRole(utilisateur: UtilisateurDto, roles: string[]): Promise<string | null> {
    const labelMap: Record<string, string> = {
      Admin: 'Administrateur', Dispatcher: 'Dispatcher', Chauffeur: 'Chauffeur'
    };
    const options = roles.map(r => `<option value="${r}">${labelMap[r] ?? r}</option>`).join('');

    const result = await Swal.fire({
      title: 'Changer le rôle',
      html: `
        <p>Utilisateur : <strong>${utilisateur.prenom} ${utilisateur.nom}</strong></p>
        <p>Rôle actuel : <strong>${labelMap[utilisateur.role] ?? utilisateur.role}</strong></p>
        <select id="swal-role" class="swal2-select">${options}</select>
      `,
      showCancelButton:    true,
      confirmButtonText:   'Confirmer',
      cancelButtonText:    'Annuler',
      confirmButtonColor:  '#2563eb',
      cancelButtonColor:   '#64748b',
      preConfirm: () => {
        const sel = document.getElementById('swal-role') as HTMLSelectElement;
        return sel?.value ?? null;
      }
    });

    return result.isConfirmed ? (result.value as string) : null;
  }

  async demanderTexte(titre: string, label: string, placeholder = ''): Promise<string | null> {
    const result = await Swal.fire({
      title: titre,
      input: 'textarea',
      inputLabel: label,
      inputPlaceholder: placeholder,
      inputAttributes: { 'aria-label': label },
      showCancelButton:   true,
      confirmButtonText:  'Confirmer',
      cancelButtonText:   'Annuler',
      confirmButtonColor: '#2563eb',
      cancelButtonColor:  '#64748b'
    });
    return result.isConfirmed ? (result.value as string) : null;
  }
}
