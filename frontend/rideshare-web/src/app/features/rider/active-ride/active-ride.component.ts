import { DecimalPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapComponent, MapMarker } from '../../../shared/components/map/map.component';
import { PaymentPanelComponent } from '../../../shared/components/payment-panel/payment-panel.component';
import { RatingPanelComponent } from '../../../shared/components/rating-panel/rating-panel.component';
import { PaymentInfo, RideDetails } from '../../../core/models/ride.models';
import { NotificationService } from '../../../core/services/notification.service';
import { PaymentsService } from '../../../core/services/payments.service';
import { RidesService } from '../../../core/services/rides.service';
import { SignalrService } from '../../../core/services/signalr.service';

const ACTIVE_STATUSES = new Set(['Requested', 'DriverAssigned', 'DriverArriving', 'InProgress']);

@Component({
  selector: 'app-active-ride',
  standalone: true,
  imports: [RouterLink, DecimalPipe, MapComponent, PaymentPanelComponent, RatingPanelComponent],
  template: `
    @if (loading()) {
      <p>Loading…</p>
    } @else if (!ride()) {
      <div class="card">
        <p>You don't have an active ride right now.</p>
        <a class="btn-primary" routerLink="/rider/request">Request a ride</a>
      </div>
    } @else {
      <div class="layout">
        <div class="card map-card">
          <app-map [markers]="markers()" />
        </div>

        <div class="card info-card">
          <span class="badge" [class]="'status-' + ride()!.status">{{ ride()!.status }}</span>

          @if (ride()!.driverName) {
            <p class="driver">{{ ride()!.driverName }} — {{ ride()!.vehicleDescription }}</p>
          } @else {
            <p class="driver">Looking for a nearby driver…</p>
          }

          <p class="fare">
            {{ ride()!.finalFareAmount ?? ride()!.estimatedFareAmount | number: '1.2-2' }} {{ ride()!.currency }}
            <span class="fare-label">{{ ride()!.finalFareAmount ? 'final fare' : 'estimated' }}</span>
          </p>

          @if (isCancellable()) {
            <button type="button" class="btn-danger" (click)="cancel()">Cancel ride</button>
          }

          @if (ride()!.status === 'Completed' && !paid()) {
            @if (paymentInfo(); as info) {
              <app-payment-panel
                [clientSecret]="info.clientSecret"
                [amount]="info.amount"
                [currency]="info.currency"
                (paid)="onPaid()"
              />
            }
          }

          @if (paid() && !rated()) {
            <app-rating-panel [rideId]="ride()!.id" (rated)="onRated()" />
          }

          @if (rated()) {
            <p class="thanks">Thanks for riding with us! 🎉</p>
            <a class="btn-primary" routerLink="/rider/request">Request another ride</a>
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
        gap: 0.75rem;
      }
      .driver {
        font-weight: 600;
        margin: 0;
      }
      .fare {
        font-size: 1.3rem;
        font-weight: 700;
        margin: 0;
      }
      .fare-label {
        font-size: 0.75rem;
        font-weight: 400;
        color: var(--rs-text-muted);
      }
      .thanks {
        font-weight: 600;
      }
      @media (max-width: 800px) {
        .layout {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class ActiveRideComponent implements OnInit, OnDestroy {
  private readonly ridesService = inject(RidesService);
  private readonly paymentsService = inject(PaymentsService);
  private readonly signalr = inject(SignalrService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly loading = signal(true);
  protected readonly ride = signal<RideDetails | null>(null);
  protected readonly paymentInfo = signal<PaymentInfo | null>(null);
  protected readonly paid = signal(false);
  protected readonly rated = signal(false);

  protected readonly isCancellable = computed(() => {
    const r = this.ride();
    return r ? r.status === 'Requested' || r.status === 'DriverAssigned' || r.status === 'DriverArriving' : false;
  });

  protected readonly markers = computed<MapMarker[]>(() => {
    const r = this.ride();
    if (!r) return [];
    const list: MapMarker[] = [
      { id: 'pickup', lat: r.pickupLatitude, lng: r.pickupLongitude, label: 'Pickup', color: 'green' },
      { id: 'dropoff', lat: r.dropoffLatitude, lng: r.dropoffLongitude, label: 'Dropoff', color: 'red' }
    ];

    const loc = this.signalr.driverLocationUpdated();
    if (loc && loc.rideId === r.id) {
      list.push({ id: 'driver', lat: loc.latitude, lng: loc.longitude, label: r.driverName ?? 'Driver', color: 'blue' });
    }

    return list;
  });

  constructor() {
    effect(() => {
      const updated = this.signalr.rideStatusChanged();
      if (updated && updated.id === this.ride()?.id) {
        this.ride.set(updated);
        if (updated.status === 'Completed') this.loadPaymentInfo(updated.id);
        if (updated.status === 'Cancelled') this.notifications.info('This ride was cancelled.');
      }
    });
  }

  async ngOnInit(): Promise<void> {
    try {
      const active = await this.ridesService.getActive(false);
      this.ride.set(active);

      if (active) {
        await this.signalr.connect();
        await this.signalr.joinRideGroup(active.id);
        if (active.status === 'Completed') await this.loadPaymentInfo(active.id);
      }
    } finally {
      this.loading.set(false);
    }
  }

  async ngOnDestroy(): Promise<void> {
    const id = this.ride()?.id;
    if (id) await this.signalr.leaveRideGroup(id);
  }

  async cancel(): Promise<void> {
    const r = this.ride();
    if (!r) return;
    const updated = await this.ridesService.cancelRide(r.id, 'Cancelled by rider');
    this.ride.set(updated);
    this.notifications.info('Ride cancelled.');
    this.router.navigateByUrl('/rider/request');
  }

  private async loadPaymentInfo(rideId: string): Promise<void> {
    try {
      this.paymentInfo.set(await this.paymentsService.getForRide(rideId));
    } catch {
      // Payment record not written yet (race with the SignalR push) — the button just won't show until refresh.
    }
  }

  onPaid(): void {
    this.paid.set(true);
  }

  onRated(): void {
    this.rated.set(true);
  }
}
