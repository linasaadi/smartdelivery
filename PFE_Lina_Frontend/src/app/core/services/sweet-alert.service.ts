import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

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
}
