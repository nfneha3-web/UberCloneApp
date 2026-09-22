import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HUB_BASE_URL } from '../config/app-config';
import { RideDetails } from '../models/ride.models';
import { AuthService } from './auth.service';

export interface DriverLocationUpdate {
  rideId: string;
  latitude: number;
  longitude: number;
}

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private connection: signalR.HubConnection | null = null;

  readonly rideStatusChanged = signal<RideDetails | null>(null);
  readonly rideOffer = signal<RideDetails | null>(null);
  readonly driverLocationUpdated = signal<DriverLocationUpdate | null>(null);

  constructor(private readonly auth: AuthService) {}

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_BASE_URL, { accessTokenFactory: () => this.auth.token ?? '' })
      .withAutomaticReconnect()
      .build();

    this.connection.on('RideStatusChanged', (ride: RideDetails) => this.rideStatusChanged.set(ride));
    this.connection.on('RideOffer', (ride: RideDetails) => this.rideOffer.set(ride));
    this.connection.on('DriverLocationUpdated', (update: DriverLocationUpdate) => this.driverLocationUpdated.set(update));

    await this.connection.start();
  }

  async joinRideGroup(rideId: string): Promise<void> {
    await this.connection?.invoke('JoinRideGroup', rideId);
  }

  async leaveRideGroup(rideId: string): Promise<void> {
    await this.connection?.invoke('LeaveRideGroup', rideId);
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
  }
}
