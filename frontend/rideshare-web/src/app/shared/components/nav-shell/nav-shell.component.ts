import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-nav-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="nav">
      <a routerLink="/" class="brand">RideShare</a>

      <div class="links">
        @if (auth.role() === 'Rider') {
          <a routerLink="/rider/request" routerLinkActive="active">Request Ride</a>
          <a routerLink="/rider/history" routerLinkActive="active">History</a>
        }
        @if (auth.role() === 'Driver') {
          <a routerLink="/driver" routerLinkActive="active">Dashboard</a>
          <a routerLink="/driver/history" routerLinkActive="active">History</a>
        }
      </div>

      <div class="account">
        @if (auth.isAuthenticated()) {
          <span class="email">{{ auth.currentUser()?.email }}</span>
          <button type="button" (click)="auth.logout()">Log out</button>
        } @else {
          <a routerLink="/login">Log in</a>
        }
      </div>
    </nav>
  `,
  styles: [
    `
      .nav {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.75rem 1.5rem;
        padding: 0.9rem 1.25rem;
        background: var(--rs-surface, #fff);
        border-bottom: 1px solid var(--rs-border, #e4e7ec);
      }
      .brand {
        font-weight: 700;
        font-size: 1.15rem;
        color: var(--rs-primary, #1f6feb);
        text-decoration: none;
      }
      .links {
        display: flex;
        gap: 1.25rem;
        flex: 1;
      }
      .links a {
        color: var(--rs-text-muted, #667085);
        text-decoration: none;
        font-size: 0.92rem;
      }
      .links a.active {
        color: var(--rs-primary, #1f6feb);
        font-weight: 600;
      }
      .account {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        font-size: 0.9rem;
      }
      .email {
        color: var(--rs-text-muted, #667085);
        max-width: 160px;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
      button {
        border: 1px solid var(--rs-border, #e4e7ec);
        background: transparent;
        padding: 0.4rem 0.8rem;
        border-radius: 8px;
        cursor: pointer;
      }
    `
  ]
})
export class NavShellComponent {
  protected readonly auth = inject(AuthService);
}
