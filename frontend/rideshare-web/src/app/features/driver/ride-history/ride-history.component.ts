import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RideHistoryItem } from '../../../core/models/ride.models';
import { RidesService } from '../../../core/services/rides.service';

@Component({
  selector: 'app-driver-ride-history',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  template: `
    <h1>Your completed trips</h1>

    @if (loading()) {
      <p>Loading…</p>
    } @else if (history().length === 0) {
      <p>No trips yet.</p>
    } @else {
      <div class="list">
        @for (r of history(); track r.id) {
          <div class="card row">
            <div>
              <span class="badge" [class]="'status-' + r.status">{{ r.status }}</span>
              <p class="date">{{ r.requestedAtUtc | date: 'medium' }}</p>
            </div>
            <p class="fare">{{ (r.finalFareAmount ?? r.estimatedFareAmount) | number: '1.2-2' }} {{ r.currency }}</p>
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      h1 {
        font-size: 1.3rem;
      }
      .list {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .row {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }
      .date {
        margin: 0.3rem 0 0;
        font-size: 0.8rem;
        color: var(--rs-text-muted);
      }
      .fare {
        font-weight: 700;
        margin: 0;
      }
    `
  ]
})
export class DriverRideHistoryComponent implements OnInit {
  private readonly ridesService = inject(RidesService);

  protected readonly loading = signal(true);
  protected readonly history = signal<RideHistoryItem[]>([]);

  async ngOnInit(): Promise<void> {
    try {
      this.history.set(await this.ridesService.getHistory(true));
    } finally {
      this.loading.set(false);
    }
  }
}
