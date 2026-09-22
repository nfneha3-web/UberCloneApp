import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../config/app-config';

@Injectable({ providedIn: 'root' })
export class RatingsService {
  constructor(private readonly http: HttpClient) {}

  submitRating(rideId: string, stars: number, comment: string | null): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API_BASE_URL}/ratings`, { rideId, stars, comment }));
  }
}
