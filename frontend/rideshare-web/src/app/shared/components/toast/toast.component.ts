import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  template: `
    <div class="toast-stack">
      @for (n of notifications.notifications(); track n.id) {
        <div class="toast" [class]="n.kind" (click)="notifications.dismiss(n.id)">
          {{ n.message }}
        </div>
      }
    </div>
  `,
  styles: [
    `
      .toast-stack {
        position: fixed;
        top: 1rem;
        right: 1rem;
        z-index: 2000;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        max-width: 340px;
      }
      .toast {
        padding: 0.75rem 1rem;
        border-radius: 10px;
        color: white;
        font-size: 0.9rem;
        box-shadow: 0 4px 14px rgba(0, 0, 0, 0.15);
        cursor: pointer;
        animation: slide-in 0.2s ease-out;
      }
      .toast.error {
        background: #d64545;
      }
      .toast.success {
        background: #2f9e5b;
      }
      .toast.info {
        background: #2f6fed;
      }
      @keyframes slide-in {
        from {
          transform: translateX(20px);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }
    `
  ]
})
export class ToastComponent {
  protected readonly notifications = inject(NotificationService);
}
