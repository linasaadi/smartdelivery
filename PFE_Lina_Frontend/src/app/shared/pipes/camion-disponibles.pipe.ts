import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'camionDisponibles', standalone: true, pure: false })
export class CamionDisponiblesPipe implements PipeTransform {
  transform(camions: { statut: string }[]): number {
    return camions.filter(c => c.statut === 'Disponible').length;
  }
}
