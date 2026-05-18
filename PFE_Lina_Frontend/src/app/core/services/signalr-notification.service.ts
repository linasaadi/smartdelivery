import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

export interface Notification {
  id: number;
  titre: string;
  message: string;
  type: 'info' | 'warning' | 'danger' | 'success';
  horodatage: Date;
  lue: boolean;
}

@Injectable({ providedIn: 'root' })
export class SignalRNotificationService {
  private auth = inject(AuthService);
  private hub!: signalR.HubConnection;

  readonly notifications = signal<Notification[]>([]);
  readonly nonLues       = signal(0);

  async connecter() {
    if (this.hub?.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7297/hubs/notifications', {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('NouvelleNotification', (data: Omit<Notification, 'id' | 'lue' | 'horodatage'>) => {
      const notif: Notification = { ...data, id: Date.now(), lue: false, horodatage: new Date() };
      this.notifications.update(list => [notif, ...list].slice(0, 50));
      this.nonLues.update(n => n + 1);
    });

    try {
      await this.hub.start();
    } catch (err) {
      console.error('NotificationHub:', err);
    }
  }

  marquerToutesLues() {
    this.notifications.update(list => list.map(n => ({ ...n, lue: true })));
    this.nonLues.set(0);
  }

  async deconnecter() {
    await this.hub?.stop();
  }
}
