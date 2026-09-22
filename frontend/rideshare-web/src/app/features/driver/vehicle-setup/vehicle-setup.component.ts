import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { VehicleType } from '../../../core/models/ride.models';
import { DriversService } from '../../../core/services/drivers.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-vehicle-setup',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="card form-card">
      <h1>Register your vehicle</h1>
      <p class="hint">You need an active vehicle on file before you can go online and accept rides.</p>

      <form (ngSubmit)="submit()">
        <div class="form-field">
          <label for="make">Make</label>
          <input id="make" [(ngModel)]="make" name="make" required />
        </div>
        <div class="form-field">
          <label for="model">Model</label>
          <input id="model" [(ngModel)]="model" name="model" required />
        </div>
        <div class="form-field">
          <label for="year">Year</label>
          <input id="year" type="number" [(ngModel)]="year" name="year" required />
        </div>
        <div class="form-field">
          <label for="color">Color</label>
          <input id="color" [(ngModel)]="color" name="color" required />
        </div>
        <div class="form-field">
          <label for="plateNumber">Plate number</label>
          <input id="plateNumber" [(ngModel)]="plateNumber" name="plateNumber" required />
        </div>
        <div class="form-field">
          <label for="vehicleType">Vehicle type</label>
          <select id="vehicleType" [(ngModel)]="vehicleType" name="vehicleType">
            <option value="Economy">Economy</option>
            <option value="Comfort">Comfort</option>
            <option value="Xl">XL</option>
            <option value="Premium">Premium</option>
          </select>
        </div>

        <button type="submit" class="btn-primary" [disabled]="saving()">
          {{ saving() ? 'Saving…' : 'Save vehicle' }}
        </button>
      </form>
    </div>
  `,
  styles: [
    `
      .form-card {
        max-width: 420px;
      }
      h1 {
        font-size: 1.3rem;
        margin-top: 0;
      }
      .hint {
        color: var(--rs-text-muted);
        font-size: 0.85rem;
      }
      button {
        width: 100%;
      }
    `
  ]
})
export class VehicleSetupComponent {
  private readonly driversService = inject(DriversService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected make = '';
  protected model = '';
  protected year = new Date().getFullYear();
  protected color = '';
  protected plateNumber = '';
  protected vehicleType: VehicleType = 'Economy';
  protected readonly saving = signal(false);

  async submit(): Promise<void> {
    this.saving.set(true);
    try {
      await this.driversService.registerVehicle({
        make: this.make,
        model: this.model,
        year: this.year,
        color: this.color,
        plateNumber: this.plateNumber,
        vehicleType: this.vehicleType
      });
      this.notifications.success('Vehicle registered — you can go online now.');
      this.router.navigateByUrl('/driver');
    } finally {
      this.saving.set(false);
    }
  }
}
