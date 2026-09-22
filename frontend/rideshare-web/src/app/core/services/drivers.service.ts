import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../config/app-config';
import { RegisterVehicleRequest } from '../models/ride.models';

@Injectable({ providedIn: 'root' })
export class DriversService {
  constructor(private readonly http: HttpClient) {}

  registerVehicle(request: RegisterVehicleRequest): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${API_BASE_URL}/drivers/vehicles`, request));
  }

  goOnline(): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API_BASE_URL}/drivers/online`, {}));
  }

  goOffline(): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API_BASE_URL}/drivers/offline`, {}));
  }

  updateLocation(latitude: number, longitude: number, activeRideId: string | null): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API_BASE_URL}/drivers/location`, { latitude, longitude, activeRideId }));
  }
}
