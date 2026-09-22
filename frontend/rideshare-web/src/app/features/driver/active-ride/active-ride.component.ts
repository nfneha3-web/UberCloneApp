import { DecimalPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapComponent, MapMarker } from '../../../shared/components/map/map.component';
import { RideDetails } from '../../../core/models/ride.models';
import { DriversService } from '../../../core/services/drivers.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RidesService } from '../../../core/services/rides.service';
import { SignalrService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-driver-active-ride',
  standalone: true,
  imports: [RouterLink, DecimalPipe, MapComponent],
  template: `
    @if (loading()) {
      <p>Loading…</p>
    } @else if (!ride()) {
      <div class="card">
        <p>No active ride right now.</p>
        <a class="btn-primary" routerLink="/driver">Back to dashboard</a>
      </div>
    } @else {
      <div class="layout">
        <div class="card map-card">
          <app-map [markers]="markers()" />
        </div>

        <div class="card info-card">
          <span class="badge" [class]="'status-' + ride()!.status">{{ ride()!.status }}</span>
          <p class="rider">Rider: {{ ride()!.riderName }}</p>
          <p class="fare">{{ ride()!.estimatedFareAmount | number: '1.2-2' }} {{ ride()!.currency }} estimated</p>

          @if (ride()!.status === 'DriverAssigned' || ride()!.status === 'DriverArriving') {
            <button type="button" class="btn-primary" [disabled]="acting()" (click)="start()">Start ride</button>
          }
          @if (ride()!.status === 'InProgress') {
            <button type="button" class="btn-primary" [disabled]="acting()" (click)="complete()">Complete ride</button>
          }
          @if (ride()!.status === 'Completed') {
            <p class="done">Ride completed — waiting on rider payment.</p>
            <a class="btn-secondary" routerLink="/driver">Back to dashboard</a>
          }
        </div>
      </div>
    }
  `,
  styles: [
    `
      .layout {
        display: grid;
        grid-template-columns: 1.4fr 1fr;
        gap: 1.25rem;
        align-items: start;
      }
      .map-card {
        height: 420px;
      }
      .info-card {
        display: flex;
        flex-direction: column;
        gap: 0.6rem;
      }
      .rider {
        font-weight: 600;
        margin: 0;
      }
      .fare {
        margin: 0;
        color: var(--rs-text-muted);
      }
      .done {
        font-weight: 600;
        color: var(--rs-success);
      }
      @media (max-width: 800px) {
        .layout {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class DriverActiveRideComponent implements OnInit, OnDestroy {
  private readonly ridesService = inject(RidesService);
  private readonly driversService = inject(DriversService);
  private readonly signalr = inject(SignalrService);
  private readonly notifications = inject(NotificationService);

  protected readonly loading = signal(true);
  protected readonly ride = signal<RideDetails | null>(null);
  protected readonly acting = signal(false);

  private watchId: number | null = null;

  protected readonly markers = computed<MapMarker[]>(() => {
    const r = this.ride();
    if (!r) return [];
    return [
      { id: 'pickup', lat: r.pickupLatitude, lng: r.pickupLongitude, label: 'Pickup', color: 'green' },
      { id: 'dropoff', lat: r.dropoffLatitude, lng: r.dropoffLongitude, label: 'Dropoff', color: 'red' }
    ];
  });

  constructor() {
    effect(() => {
      const updated = this.signalr.rideStatusChanged();
      if (updated && updated.id === this.ride()?.id) this.ride.set(updated);
    });
  }

  async ngOnInit(): Promise<void> {
    try {
      const active = await this.ridesService.getActive(true);
      this.ride.set(active);

      if (active) {
        await this.signalr.connect();
        await this.signalr.joinRideGroup(active.id);
        this.startLocationBroadcast(active.id);
      }
    } finally {
      this.loading.set(false);
    }
  }

  async ngOnDestroy(): Promise<void> {
    if (this.watchId !== null) navigator.geolocation.clearWatch(this.watchId);
    const id = this.ride()?.id;
    if (id) await this.signalr.leaveRideGroup(id);
  }

  private startLocationBroadcast(rideId: string): void {
    if (!navigator.geolocation) return;

    this.watchId = navigator.geolocation.watchPosition(
      (position) => {
        // Passing the active ride id makes the backend also broadcast this update to the
        // rider currently watching this specific ride (see RideHub's "ride-{id}" group).
        this.driversService.updateLocation(position.coords.latitude, position.coords.longitude, rideId).catch(() => {});
      },
      undefined,
      { enableHighAccuracy: true, maximumAge: 5000 }
    );
  }

  async start(): Promise<void> {
    const r = this.ride();
    if (!r) return;
    this.acting.set(true);
    try {
      this.ride.set(await this.ridesService.startRide(r.id));
    } finally {
      this.acting.set(false);
    }
  }

  async complete(): Promise<void> {
    const r = this.ride();
    if (!r) return;
    this.acting.set(true);
    try {
      const result = await this.ridesService.completeRide(r.id);
      this.ride.set(result.ride);
      this.notifications.success('Ride marked complete.');
    } finally {
      this.acting.set(false);
    }
  }
}
