import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerService } from '../../../core/services/spinner.service';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (spinner.loading()) {
      <div class="spinner-overlay">
        <div class="loader-card">
          <div class="loader-ring"></div>
          <div class="loader-logo">SD</div>
        </div>
      </div>
    }
  `,
  styles: [`
    .loader-card {
      position: relative;
      width: 72px; height: 72px;
      display: flex; align-items: center; justify-content: center;
    }
    .loader-ring {
      position: absolute; inset: 0;
      border-radius: 50%;
      border: 3px solid rgba(255,255,255,.15);
      border-top-color: #60a5fa;
      animation: spin .8s linear infinite;
    }
    .loader-logo {
      font-size: 1.1rem; font-weight: 800;
      color: #fff; letter-spacing: -.03em;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class SpinnerComponent {
  spinner = inject(SpinnerService);
}
