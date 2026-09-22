export type UserRole = 'Rider' | 'Driver';

export interface AuthResult {
  token: string;
  applicationUserId: string;
  email: string;
  roles: UserRole[];
}

export interface RegisterRiderRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber: string;
}

export interface RegisterDriverRequest extends RegisterRiderRequest {
  licenseNumber: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
