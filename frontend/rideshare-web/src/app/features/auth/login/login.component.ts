import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="card auth-card">
      <h1>Log in</h1>

      <form (ngSubmit)="submit()">
        <div class="form-field">
          <label for="email">Email</label>
          <input id="email" type="email" [(ngModel)]="email" name="email" required autocomplete="email" />
        </div>

        <div class="form-field">
          <label for="password">Password</label>
          <input id="password" type="password" [(ngModel)]="password" name="password" required autocomplete="current-password" />
        </div>

        <button type="submit" class="btn-primary" [disabled]="loading()">
          {{ loading() ? 'Logging in…' : 'Log in' }}
        </button>
      </form>

      <p class="switch">
        New here?
        <a routerLink="/register/rider">Sign up as a rider</a>
        or
        <a routerLink="/register/driver">as a driver</a>
      </p>
    </div>
  `,
  styles: [
    `
      .auth-card {
        max-width: 380px;
        margin: 3rem auto;
      }
      h1 {
        margin-top: 0;
        font-size: 1.4rem;
      }
      .switch {
        margin-top: 1rem;
        font-size: 0.85rem;
        color: var(--rs-text-muted);
      }
    `
  ]
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected readonly loading = signal(false);

  async submit(): Promise<void> {
    this.loading.set(true);
    try {
      await this.auth.login({ email: this.email, password: this.password });
      this.router.navigateByUrl(this.auth.role() === 'Driver' ? '/driver' : '/rider/request');
    } finally {
      this.loading.set(false);
    }
  }
}
