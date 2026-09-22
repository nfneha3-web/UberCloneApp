import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register-rider',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="card auth-card">
      <h1>Create your rider account</h1>

      <form (ngSubmit)="submit()">
        <div class="form-field">
          <label for="fullName">Full name</label>
          <input id="fullName" [(ngModel)]="fullName" name="fullName" required />
        </div>
        <div class="form-field">
          <label for="email">Email</label>
          <input id="email" type="email" [(ngModel)]="email" name="email" required autocomplete="email" />
        </div>
        <div class="form-field">
          <label for="phoneNumber">Phone number</label>
          <input id="phoneNumber" [(ngModel)]="phoneNumber" name="phoneNumber" required />
        </div>
        <div class="form-field">
          <label for="password">Password</label>
          <input id="password" type="password" [(ngModel)]="password" name="password" required autocomplete="new-password" />
          <small>At least 8 characters, one uppercase letter, one digit.</small>
        </div>

        <button type="submit" class="btn-primary" [disabled]="loading()">
          {{ loading() ? 'Creating account…' : 'Sign up' }}
        </button>
      </form>

      <p class="switch">Already have an account? <a routerLink="/login">Log in</a></p>
    </div>
  `,
  styles: [
    `
      .auth-card {
        max-width: 420px;
        margin: 3rem auto;
      }
      h1 {
        margin-top: 0;
        font-size: 1.4rem;
      }
      small {
        color: var(--rs-text-muted);
        font-size: 0.75rem;
      }
      .switch {
        margin-top: 1rem;
        font-size: 0.85rem;
        color: var(--rs-text-muted);
      }
    `
  ]
})
export class RegisterRiderComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected fullName = '';
  protected email = '';
  protected phoneNumber = '';
  protected password = '';
  protected readonly loading = signal(false);

  async submit(): Promise<void> {
    this.loading.set(true);
    try {
      await this.auth.registerRider({
        fullName: this.fullName,
        email: this.email,
        phoneNumber: this.phoneNumber,
        password: this.password
      });
      this.router.navigateByUrl('/rider/request');
    } finally {
      this.loading.set(false);
    }
  }
}
