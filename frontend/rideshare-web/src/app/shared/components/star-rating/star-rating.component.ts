import { Component, input, model } from '@angular/core';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  template: `
    <div class="stars" [class.readonly]="readonly()">
      @for (star of [1, 2, 3, 4, 5]; track star) {
        <button
          type="button"
          class="star"
          [class.filled]="star <= value()"
          [disabled]="readonly()"
          (click)="select(star)"
        >
          ★
        </button>
      }
    </div>
  `,
  styles: [
    `
      .stars {
        display: inline-flex;
        gap: 0.15rem;
      }
      .star {
        background: none;
        border: none;
        font-size: 1.6rem;
        line-height: 1;
        cursor: pointer;
        color: var(--rs-border, #d0d5dd);
        padding: 0;
      }
      .star.filled {
        color: var(--rs-warning, #f5a623);
      }
      .readonly .star {
        cursor: default;
      }
    `
  ]
})
export class StarRatingComponent {
  readonly value = model(0);
  readonly readonly = input(false);

  select(star: number): void {
    if (this.readonly()) return;
    this.value.set(star);
  }
}
