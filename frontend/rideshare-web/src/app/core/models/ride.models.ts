export type VehicleType = 'Economy' | 'Comfort' | 'Xl' | 'Premium';

export type RideStatus =
  | 'Requested'
  | 'DriverAssigned'
  | 'DriverArriving'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled';

export interface RideDetails {
  id: string;
  riderProfileId: string;
  riderName: string;
  driverProfileId: string | null;
  driverName: string | null;
  vehicleDescription: string | null;
  pickupLatitude: number;
  pickupLongitude: number;
  pickupAddress: string | null;
  dropoffLatitude: number;
  dropoffLongitude: number;
  dropoffAddress: string | null;
  status: RideStatus;
  estimatedFareAmount: number;
  finalFareAmount: number | null;
  currency: string;
  estimatedDistanceKm: number;
  requestedAtUtc: string;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
}

export interface RideHistoryItem {
  id: string;
  status: RideStatus;
  pickupLatitude: number;
  pickupLongitude: number;
  dropoffLatitude: number;
  dropoffLongitude: number;
  finalFareAmount: number | null;
  estimatedFareAmount: number;
  currency: string;
  requestedAtUtc: string;
  completedAtUtc: string | null;
}

export interface RequestRideRequest {
  pickupLatitude: number;
  pickupLongitude: number;
  pickupAddress: string | null;
  dropoffLatitude: number;
  dropoffLongitude: number;
  dropoffAddress: string | null;
  vehicleType: VehicleType;
}

export interface NearbyDriver {
  driverProfileId: string;
  fullName: string;
  averageRating: number;
  latitude: number;
  longitude: number;
  distanceKm: number;
  vehicleId: string;
  vehicleType: VehicleType;
  vehicleDescription: string;
}

export interface CompleteRideResult {
  ride: RideDetails;
  paymentIntentId: string;
  paymentClientSecret: string;
}

export interface PaymentInfo {
  paymentIntentId: string;
  clientSecret: string;
  status: string;
  amount: number;
  currency: string;
}

export interface RegisterVehicleRequest {
  make: string;
  model: string;
  year: number;
  color: string;
  plateNumber: string;
  vehicleType: VehicleType;
}
