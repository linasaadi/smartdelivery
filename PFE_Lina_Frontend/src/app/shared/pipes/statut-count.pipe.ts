import { Pipe, PipeTransform } from '@angular/core';
import { StatutLivraison } from '../models';

@Pipe({ name: 'statutCount', standalone: true, pure: false })
export class StatutCountPipe implements PipeTransform {
  transform(livraisons: { statut: string }[], statut: StatutLivraison): number {
    return livraisons.filter(l => l.statut === statut).length;
  }
}
