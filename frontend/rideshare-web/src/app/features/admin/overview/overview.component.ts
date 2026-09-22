import { Component } from '@angular/core';

/**
 * Admin panel skeleton (docs/ROADMAP.md Phase 1 scope). The real dashboard — ride/driver
 * analytics, payouts, dispute handling — needs its own admin-facing API endpoints and role,
 * which are Phase 3. This page exists so the route/shell is in place to build against.
 */
@Component({
  selector: 'app-admin-overview',
  standalone: true,
  template: `
    <div class="card">
      <h1>Admin overview</h1>
      <p>
        This is a placeholder for the admin panel. Ride/driver analytics, payouts, and dispute
        handling are planned for Phase 3 (see <code>docs/ROADMAP.md</code>) once dedicated admin
        API endpoints and an Admin role exist.
      </p>
    </div>
  `,
  styles: [
    `
      h1 {
        font-size: 1.3rem;
        margin-top: 0;
      }
      code {
        background: var(--rs-bg-subtle);
        padding: 0.1rem 0.35rem;
        border-radius: 4px;
      }
    `
  ]
})
export class AdminOverviewComponent {}
