import { DecimalPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { RideDetails } from '../../../core/models/ride.models';
import { DriversService } from '../../../core/services/drivers.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RidesService } from '../../../core/services/rides.service';
import { SignalrService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-driver-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  template: `
    <div class="card status-card">
      <div class="row">
        <div>
          <h1>Driver dashboard</h1>
          <p class="hint">Go online to start receiving ride offers nearby.</p>
        </div>
        <button type="button" [class]="online() ? 'btn-danger' : 'btn-primary'" [disabled]="toggling()" (click)="toggleOnline()">
          {{ online() ? 'Go offline' : 'Go online' }}
        </button>
      </div>
    </div>

    @if (offer(); as o) {
      <div class="card offer-card">
        <h2>New ride request!</h2>
        <p>Pickup: {{ o.pickupLatitude | number: '1.4-4' }}, {{ o.pickupLongitude | number: '1.4-4' }}</p>
        <p>Estimated fare: {{ o.estimatedFareAmount | number: '1.2-2' }} {{ o.currency }} · {{ o.estimatedDistanceKm | number: '1.1-1' }} km</p>
        <div class="offer-actions">
          <button type="button" class="btn-primary" [disabled]="accepting()" (click)="accept(o)">Accept</button>
          <button type="button" class="btn-secondary" (click)="dismiss()">Dismiss</button>
        </div>
      </div>
    }

    <p class="link"><a routerLink="/driver/vehicle-setup">Manage your vehicle</a></p>
  `,
  styles: [
    `
      h1 {
        font-size: 1.3rem;
        margin: 0 0 0.25rem;
      }
      .hint {
        margin: 0;
        color: var(--rs-text-muted);
        font-size: 0.85rem;
      }
      .row {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }
      .offer-card {
        margin-top: 1rem;
        border-color: var(--rs-primary);
      }
      .offer-actions {
        display: flex;
        gap: 0.6rem;
        margin-top: 0.75rem;
      }
      .link {
        margin-top: 1rem;
        font-size: 0.85rem;
      }
    `
  ]
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly driversService = inject(DriversService);
  private readonly ridesService = inject(RidesService);
  private readonly signalr = inject(SignalrService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly online = signal(false);
  protected readonly toggling = signal(false);
  protected readonly accepting = signal(false);
  protected readonly offer = signal<RideDetails | null>(null);

  private watchId: number | null = null;

  constructor() {
    effect(() => {
      const offer = this.signalr.rideOffer();
      if (offer && offer.status === 'Requested') this.offer.set(offer);
    });
  }

  async ngOnInit(): Promise<void> {
    await this.signalr.connect();

    const active = await this.ridesService.getActive(true);
    if (active && active.status !== 'Completed' && active.status !== 'Cancelled') {
      this.router.navigateByUrl('/driver/active');
    }
  }

  ngOnDestroy(): void {
    if (this.watchId !== null) navigator.geolocation.clearWatch(this.watchId);
  }

  async toggleOnline(): Promise<void> {
    this.toggling.set(true);
    try {
      if (this.online()) {
        await this.driversService.goOffline();
        this.online.set(false);
        if (this.watchId !== null) navigator.geolocation.clearWatch(this.watchId);
      } else {
        await this.driversService.goOnline();
        this.online.set(true);
        this.startLocationTracking();
      }
    } finally {
      this.toggling.set(false);
    }
  }

  private startLocationTracking(): void {
    if (!navigator.geolocation) return;

    this.watchId = navigator.geolocation.watchPosition(
      (position) => {
        this.driversService.updateLocation(position.coords.latitude, position.coords.longitude, null).catch(() => {});
      },
      () => this.notifications.error('Could not read your location — enable location access to receive rides.'),
      { enableHighAccuracy: true, maximumAge: 5000 }
    );
  }

  async accept(ride: RideDetails): Promise<void> {
    this.accepting.set(true);
    try {
      await this.ridesService.acceptRide(ride.id);
      this.offer.set(null);
      this.router.navigateByUrl('/driver/active');
    } finally {
      this.accepting.set(false);
    }
  }

  dismiss(): void {
    this.offer.set(null);
  }
}
