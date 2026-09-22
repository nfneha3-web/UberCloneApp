import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../config/app-config';
import {
  CompleteRideResult,
  NearbyDriver,
  RequestRideRequest,
  RideDetails,
  RideHistoryItem,
  VehicleType
} from '../models/ride.models';

@Injectable({ providedIn: 'root' })
export class RidesService {
  constructor(private readonly http: HttpClient) {}

  requestRide(request: RequestRideRequest): Promise<RideDetails> {
    return firstValueFrom(this.http.post<RideDetails>(`${API_BASE_URL}/rides`, request));
  }

  acceptRide(rideId: string): Promise<RideDetails> {
    return firstValueFrom(this.http.post<RideDetails>(`${API_BASE_URL}/rides/${rideId}/accept`, {}));
  }

  startRide(rideId: string): Promise<RideDetails> {
    return firstValueFrom(this.http.post<RideDetails>(`${API_BASE_URL}/rides/${rideId}/start`, {}));
  }

  completeRide(rideId: string): Promise<CompleteRideResult> {
    return firstValueFrom(this.http.post<CompleteRideResult>(`${API_BASE_URL}/rides/${rideId}/complete`, {}));
  }

  cancelRide(rideId: string, reason: string): Promise<RideDetails> {
    return firstValueFrom(this.http.post<RideDetails>(`${API_BASE_URL}/rides/${rideId}/cancel`, { reason }));
  }

  getById(rideId: string): Promise<RideDetails> {
    return firstValueFrom(this.http.get<RideDetails>(`${API_BASE_URL}/rides/${rideId}`));
  }

  getActive(asDriver: boolean): Promise<RideDetails | null> {
    return firstValueFrom(this.http.get<RideDetails | null>(`${API_BASE_URL}/rides/active`, { params: { asDriver } }));
  }

  getHistory(asDriver: boolean): Promise<RideHistoryItem[]> {
    return firstValueFrom(this.http.get<RideHistoryItem[]>(`${API_BASE_URL}/rides/history`, { params: { asDriver } }));
  }

  findNearbyDrivers(latitude: number, longitude: number, radiusKm = 8, vehicleType?: VehicleType): Promise<NearbyDriver[]> {
    const params: Record<string, string | number> = { latitude, longitude, radiusKm };
    if (vehicleType) params['vehicleType'] = vehicleType;
    return firstValueFrom(this.http.get<NearbyDriver[]>(`${API_BASE_URL}/rides/nearby-drivers`, { params }));
  }
}
