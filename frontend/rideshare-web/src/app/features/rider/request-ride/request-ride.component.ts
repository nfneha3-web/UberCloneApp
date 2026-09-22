import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MapComponent, MapMarker } from '../../../shared/components/map/map.component';
import { VehicleType } from '../../../core/models/ride.models';
import { RidesService } from '../../../core/services/rides.service';
import { NotificationService } from '../../../core/services/notification.service';

type LatLng = { lat: number; lng: number };

@Component({
  selector: 'app-request-ride',
  standalone: true,
  imports: [FormsModule, MapComponent],
  template: `
    <div class="layout">
      <div class="card map-card">
        <app-map [markers]="markers()" [clickable]="true" (mapClick)="onMapClick($event)" />
        <p class="instructions">
          @if (!pickup()) {
            Click the map to set your <strong>pickup</strong> point.
          } @else if (!dropoff()) {
            Now click to set your <strong>dropoff</strong> point.
          } @else {
            Ready to request — or click the map again to start over.
          }
        </p>
      </div>

      <div class="card form-card">
        <h1>Request a ride</h1>

        <div class="form-field">
          <label for="vehicleType">Vehicle type</label>
          <select id="vehicleType" [(ngModel)]="vehicleType" name="vehicleType">
            <option value="Economy">Economy</option>
            <option value="Comfort">Comfort</option>
            <option value="Xl">XL</option>
            <option value="Premium">Premium</option>
          </select>
        </div>

        @if (pickup() && dropoff()) {
          <p class="distance">Straight-line distance: {{ estimatedDistanceKm() }} km (fare estimated server-side)</p>
        }

        <button type="button" class="btn-primary" [disabled]="!canRequest() || requesting()" (click)="requestRide()">
          {{ requesting() ? 'Requesting…' : 'Request ride' }}
        </button>

        @if (pickup() || dropoff()) {
          <button type="button" class="btn-secondary" (click)="reset()">Start over</button>
        }
      </div>
    </div>
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
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      app-map {
        flex: 1;
      }
      .instructions {
        margin: 0;
        font-size: 0.85rem;
        color: var(--rs-text-muted);
      }
      .distance {
        font-size: 0.85rem;
        color: var(--rs-text-muted);
      }
      button {
        margin-top: 0.5rem;
        width: 100%;
      }
      @media (max-width: 800px) {
        .layout {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class RequestRideComponent {
  private readonly ridesService = inject(RidesService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly pickup = signal<LatLng | null>(null);
  protected readonly dropoff = signal<LatLng | null>(null);
  protected vehicleType: VehicleType = 'Economy';
  protected readonly requesting = signal(false);

  protected readonly canRequest = computed(() => this.pickup() !== null && this.dropoff() !== null);

  protected readonly estimatedDistanceKm = computed(() => {
    const p = this.pickup();
    const d = this.dropoff();
    if (!p || !d) return 0;
    return Math.round(haversineKm(p, d) * 10) / 10;
  });

  protected readonly markers = computed<MapMarker[]>(() => {
    const list: MapMarker[] = [];
    const p = this.pickup();
    const d = this.dropoff();
    if (p) list.push({ id: 'pickup', lat: p.lat, lng: p.lng, label: 'Pickup', color: 'green' });
    if (d) list.push({ id: 'dropoff', lat: d.lat, lng: d.lng, label: 'Dropoff', color: 'red' });
    return list;
  });

  onMapClick(point: LatLng): void {
    if (!this.pickup()) {
      this.pickup.set(point);
    } else if (!this.dropoff()) {
      this.dropoff.set(point);
    } else {
      this.pickup.set(point);
      this.dropoff.set(null);
    }
  }

  reset(): void {
    this.pickup.set(null);
    this.dropoff.set(null);
  }

  async requestRide(): Promise<void> {
    const p = this.pickup();
    const d = this.dropoff();
    if (!p || !d) return;

    this.requesting.set(true);
    try {
      await this.ridesService.requestRide({
        pickupLatitude: p.lat,
        pickupLongitude: p.lng,
        pickupAddress: null,
        dropoffLatitude: d.lat,
        dropoffLongitude: d.lng,
        dropoffAddress: null,
        vehicleType: this.vehicleType
      });
      this.notifications.success('Ride requested — looking for a nearby driver.');
      this.router.navigateByUrl('/rider/active');
    } finally {
      this.requesting.set(false);
    }
  }
}

function haversineKm(a: LatLng, b: LatLng): number {
  const R = 6371;
  const dLat = ((b.lat - a.lat) * Math.PI) / 180;
  const dLng = ((b.lng - a.lng) * Math.PI) / 180;
  const lat1 = (a.lat * Math.PI) / 180;
  const lat2 = (b.lat * Math.PI) / 180;
  const h = Math.sin(dLat / 2) ** 2 + Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLng / 2) ** 2;
  return 2 * R * Math.asin(Math.sqrt(h));
}
