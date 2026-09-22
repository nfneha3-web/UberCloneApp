import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RatingsService } from '../../../core/services/ratings.service';
import { StarRatingComponent } from '../star-rating/star-rating.component';

@Component({
  selector: 'app-rating-panel',
  standalone: true,
  imports: [FormsModule, StarRatingComponent],
  template: `
    <div class="panel">
      <p class="title">How was your ride?</p>
      <app-star-rating [(value)]="stars" />
      <textarea placeholder="Optional comment…" [(ngModel)]="comment" rows="2"></textarea>
      <button type="button" class="btn-primary" [disabled]="stars() === 0 || submitting()" (click)="submit()">
        {{ submitting() ? 'Submitting…' : 'Submit rating' }}
      </button>
    </div>
  `,
  styles: [
    `
      .panel {
        display: flex;
        flex-direction: column;
        gap: 0.6rem;
      }
      .title {
        margin: 0;
        font-weight: 600;
      }
      textarea {
        border: 1px solid var(--rs-border);
        border-radius: 8px;
        padding: 0.5rem;
        font-family: inherit;
        resize: vertical;
      }
    `
  ]
})
export class RatingPanelComponent {
  readonly rideId = input.required<string>();
  readonly rated = output<void>();

  private readonly ratingsService = inject(RatingsService);

  protected readonly stars = signal(0);
  protected comment = '';
  protected readonly submitting = signal(false);

  async submit(): Promise<void> {
    this.submitting.set(true);
    try {
      await this.ratingsService.submitRating(this.rideId(), this.stars(), this.comment || null);
      this.rated.emit();
    } finally {
      this.submitting.set(false);
    }
  }
}
