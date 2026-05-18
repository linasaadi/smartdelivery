import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { SignalRNotificationService } from '../../core/services/signalr-notification.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule, RouterModule, MatIconModule, MatButtonModule,
    MatMenuModule, MatBadgeModule, MatTooltipModule, MatDividerModule
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  auth          = inject(AuthService);
  notifService  = inject(SignalRNotificationService);
  now = new Date();

  ngOnInit() {
    if (this.auth.isLogged()) {
      this.notifService.connecter();
    }
    setInterval(() => this.now = new Date(), 60_000);
  }

  get roleLabel(): string {
    const map: Record<string, string> = {
      Admin: '🔑 Administrateur',
      Dispatcher: '📋 Dispatcher',
      Chauffeur: '🚛 Chauffeur'
    };
    return map[this.auth.role() ?? ''] ?? '';
  }
}
