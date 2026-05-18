import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SpinnerService {
  private count = 0;
  readonly loading = signal(false);

  show() {
    this.count++;
    // Defer to avoid ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => this.loading.set(true), 0);
  }

  hide() {
    if (--this.count <= 0) {
      this.count = 0;
      setTimeout(() => this.loading.set(false), 0);
    }
  }
}
