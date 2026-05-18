import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';

export interface PositionGPS {
  camionId: number;
  lat: number;
  lng: number;
  vitesseKmh: number;
  horodatage: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRTrackingService {
  private auth   = inject(AuthService);
  private hub!: signalR.HubConnection;

  private _position$ = new Subject<PositionGPS>();
  readonly position$ = this._position$.asObservable();

  async connecter() {
    if (this.hub?.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7297/hubs/tracking', {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('PositionMisAJour', (data: PositionGPS) => {
      this._position$.next(data);
    });

    try {
      await this.hub.start();
      console.log('TrackingHub connecté');
    } catch (err) {
      console.error('TrackingHub erreur de connexion:', err);
    }
  }

  async deconnecter() {
    await this.hub?.stop();
  }

  /** Envoi de position par le chauffeur */
  async envoyerPosition(camionId: number, lat: number, lng: number, vitesse: number) {
    if (this.hub?.state === signalR.HubConnectionState.Connected)
      await this.hub.invoke('EnvoyerPosition', camionId, lat, lng, vitesse);
  }

  async suivreLivraison(livraisonId: number) {
    if (this.hub?.state === signalR.HubConnectionState.Connected)
      await this.hub.invoke('SuivreLivraison', livraisonId);
  }
}
