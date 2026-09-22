import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL, STRIPE_PUBLISHABLE_KEY } from '../config/app-config';
import { PaymentInfo } from '../models/ride.models';

@Injectable({ providedIn: 'root' })
export class PaymentsService {
  private stripePromise: Promise<Stripe | null> | null = null;

  constructor(private readonly http: HttpClient) {}

  getStripe(): Promise<Stripe | null> {
    this.stripePromise ??= loadStripe(STRIPE_PUBLISHABLE_KEY);
    return this.stripePromise;
  }

  getForRide(rideId: string): Promise<PaymentInfo> {
    return firstValueFrom(this.http.get<PaymentInfo>(`${API_BASE_URL}/payments/ride/${rideId}`));
  }

  confirmPayment(paymentIntentId: string): Promise<boolean> {
    return firstValueFrom(this.http.post<boolean>(`${API_BASE_URL}/payments/confirm`, { paymentIntentId }));
  }
}
