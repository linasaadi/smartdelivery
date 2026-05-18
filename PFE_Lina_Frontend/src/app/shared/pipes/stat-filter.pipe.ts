import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'statFilter', standalone: true, pure: false })
export class StatFilterPipe implements PipeTransform {
  transform(items: { statut: string }[], statut: string): number {
    return items.filter(l => l.statut === statut).length;
  }
}
