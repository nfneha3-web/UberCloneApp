import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CopilotMessage } from '../../../core/models/copilot.models';
import { AuthService } from '../../../core/services/auth.service';
import { CopilotService } from '../../../core/services/copilot.service';

/**
 * Floating chat widget available on every authenticated screen. Handles both jobs the roadmap
 * calls for: it can DO things (book a ride, cancel one, go online/offline — via tool calls the
 * backend executes against the same commands a button click would trigger) and it can just
 * ANSWER questions (fare, ETA, status) conversationally.
 */
@Component({
  selector: 'app-copilot-chat',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (auth.isAuthenticated()) {
      <button type="button" class="fab" (click)="toggleOpen()" [class.open]="open()">
        {{ open() ? '✕' : '💬' }}
      </button>

      @if (open()) {
        <div class="panel">
          <header>
            <strong>RideShare Copilot</strong>
            <span class="hint">Ask me to book a ride, or ask "where's my driver?"</span>
          </header>

          <div class="messages" #scrollAnchor>
            @if (messages().length === 0) {
              <p class="empty">Hi! I can book rides, check status, or answer questions — try me.</p>
            }
            @for (m of messages(); track $index) {
              <div class="bubble" [class.user]="m.role === 'User'" [class.assistant]="m.role !== 'User'">
                {{ m.content }}
                @if (m.sources && m.sources.length > 0) {
                  <div class="sources">Grounded in: {{ m.sources.join(', ') }}</div>
                }
              </div>
            }
            @if (sending()) {
              <div class="bubble assistant typing">…</div>
            }
          </div>

          <form class="composer" (ngSubmit)="send()">
            <input
              type="text"
              placeholder="Type a message…"
              [(ngModel)]="draft"
              name="draft"
              [disabled]="sending()"
              autocomplete="off"
            />
            <button type="submit" [disabled]="sending() || !draft().trim()">Send</button>
          </form>
        </div>
      }
    }
  `,
  styles: [
    `
      :host {
        position: fixed;
        right: 1.25rem;
        bottom: 1.25rem;
        z-index: 1500;
      }
      .fab {
        width: 56px;
        height: 56px;
        border-radius: 50%;
        border: none;
        background: var(--rs-primary, #1f6feb);
        color: white;
        font-size: 1.4rem;
        cursor: pointer;
        box-shadow: 0 6px 18px rgba(31, 111, 235, 0.35);
      }
      .panel {
        position: absolute;
        right: 0;
        bottom: 68px;
        width: 320px;
        max-height: 440px;
        display: flex;
        flex-direction: column;
        background: var(--rs-surface, #fff);
        border-radius: 14px;
        box-shadow: 0 12px 32px rgba(16, 24, 40, 0.18);
        overflow: hidden;
        border: 1px solid var(--rs-border, #e4e7ec);
      }
      header {
        padding: 0.85rem 1rem;
        background: var(--rs-primary, #1f6feb);
        color: white;
      }
      header .hint {
        display: block;
        font-size: 0.72rem;
        opacity: 0.85;
        margin-top: 0.15rem;
      }
      .messages {
        flex: 1;
        overflow-y: auto;
        padding: 0.75rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        min-height: 180px;
      }
      .empty {
        color: var(--rs-text-muted, #667085);
        font-size: 0.85rem;
      }
      .bubble {
        max-width: 85%;
        padding: 0.5rem 0.7rem;
        border-radius: 10px;
        font-size: 0.85rem;
        line-height: 1.35;
      }
      .bubble.user {
        align-self: flex-end;
        background: var(--rs-primary, #1f6feb);
        color: white;
      }
      .bubble.assistant {
        align-self: flex-start;
        background: var(--rs-bg-subtle, #f2f4f7);
        color: var(--rs-text, #1d2939);
      }
      .sources {
        margin-top: 0.35rem;
        font-size: 0.68rem;
        opacity: 0.65;
        font-style: italic;
      }
      .composer {
        display: flex;
        gap: 0.5rem;
        padding: 0.6rem;
        border-top: 1px solid var(--rs-border, #e4e7ec);
      }
      .composer input {
        flex: 1;
        border: 1px solid var(--rs-border, #e4e7ec);
        border-radius: 8px;
        padding: 0.5rem 0.6rem;
        font-size: 0.85rem;
      }
      .composer button {
        border: none;
        background: var(--rs-primary, #1f6feb);
        color: white;
        border-radius: 8px;
        padding: 0.5rem 0.9rem;
        cursor: pointer;
      }
      .composer button:disabled {
        opacity: 0.5;
        cursor: default;
      }
    `
  ]
})
export class CopilotChatComponent {
  protected readonly auth = inject(AuthService);
  private readonly copilot = inject(CopilotService);

  protected readonly open = signal(false);
  protected readonly messages = signal<CopilotMessage[]>([]);
  protected readonly sending = signal(false);
  protected readonly draft = signal('');
  private conversationId: string | null = null;

  async toggleOpen(): Promise<void> {
    this.open.update((v) => !v);
    if (this.open() && this.messages().length === 0) {
      try {
        const history = await this.copilot.getConversation(null);
        this.messages.set(history);
      } catch {
        // No prior conversation yet — that's fine, start fresh.
      }
    }
  }

  async send(): Promise<void> {
    const text = this.draft().trim();
    if (!text) return;

    this.messages.update((list) => [...list, { role: 'User', content: text }]);
    this.draft.set('');
    this.sending.set(true);

    try {
      const reply = await this.copilot.sendMessage(text, this.conversationId);
      this.conversationId = reply.conversationId;
      this.messages.update((list) => [...list, { role: 'Assistant', content: reply.reply, sources: reply.sources }]);
    } finally {
      this.sending.set(false);
    }
  }
}
