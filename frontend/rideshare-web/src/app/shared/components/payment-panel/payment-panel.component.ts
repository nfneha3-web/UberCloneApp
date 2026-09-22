import { AfterViewInit, Component, ElementRef, OnDestroy, effect, inject, input, output, signal, viewChild } from '@angular/core';
import type { Stripe, StripeElements, StripePaymentElement } from '@stripe/stripe-js';
import { NotificationService } from '../../../core/services/notification.service';
import { PaymentsService } from '../../../core/services/payments.service';

/** Stripe TEST MODE payment form — card 4242 4242 4242 4242, any future date/CVC, never a real charge. */
@Component({
  selector: 'app-payment-panel',
  standalone: true,
  template: `
    <div class="panel">
      <p class="amount">Amount due: {{ amount() }} {{ currency() }}</p>
      <p class="test-hint">Test mode — use card 4242 4242 4242 4242, any future expiry, any CVC.</p>
      <div #elementHost class="element-host"></div>
      <button type="button" class="btn-primary" [disabled]="paying() || !ready()" (click)="pay()">
        {{ paying() ? 'Processing…' : 'Pay now' }}
      </button>
    </div>
  `,
  styles: [
    `
      .panel {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .amount {
        font-size: 1.1rem;
        font-weight: 700;
        margin: 0;
      }
      .test-hint {
        margin: 0;
        font-size: 0.78rem;
        color: var(--rs-text-muted);
      }
      .element-host {
        min-height: 220px;
      }
    `
  ]
})
export class PaymentPanelComponent implements AfterViewInit, OnDestroy {
  readonly clientSecret = input.required<string>();
  readonly amount = input<number>(0);
  readonly currency = input<string>('USD');
  readonly paid = output<void>();

  private readonly paymentsService = inject(PaymentsService);
  private readonly notifications = inject(NotificationService);

  private readonly elementHost = viewChild.required<ElementRef<HTMLDivElement>>('elementHost');
  protected readonly ready = signal(false);
  protected readonly paying = signal(false);

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private paymentElement: StripePaymentElement | null = null;

  constructor() {
    effect(() => {
      this.clientSecret();
      this.mountElement();
    });
  }

  async ngAfterViewInit(): Promise<void> {
    await this.mountElement();
  }

  private async mountElement(): Promise<void> {
    if (!this.elementHost() || this.paymentElement) return;

    this.stripe = await this.paymentsService.getStripe();
    if (!this.stripe) return;

    this.elements = this.stripe.elements({ clientSecret: this.clientSecret() });
    this.paymentElement = this.elements.create('payment');
    this.paymentElement.mount(this.elementHost().nativeElement);
    this.paymentElement.on('ready', () => this.ready.set(true));
  }

  async pay(): Promise<void> {
    if (!this.stripe || !this.elements) return;

    this.paying.set(true);
    try {
      const { error, paymentIntent } = await this.stripe.confirmPayment({
        elements: this.elements,
        redirect: 'if_required'
      });

      if (error) {
        this.notifications.error(error.message ?? 'Payment failed.');
        return;
      }

      if (paymentIntent?.status === 'succeeded') {
        await this.paymentsService.confirmPayment(paymentIntent.id);
        this.notifications.success('Payment successful!');
        this.paid.emit();
      }
    } finally {
      this.paying.set(false);
    }
  }

  ngOnDestroy(): void {
    this.paymentElement?.unmount();
  }
}
