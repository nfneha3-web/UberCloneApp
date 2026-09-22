import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../config/app-config';
import { CopilotMessage, CopilotReply } from '../models/copilot.models';

@Injectable({ providedIn: 'root' })
export class CopilotService {
  constructor(private readonly http: HttpClient) {}

  sendMessage(message: string, conversationId: string | null): Promise<CopilotReply> {
    return firstValueFrom(this.http.post<CopilotReply>(`${API_BASE_URL}/copilot/messages`, { conversationId, message }));
  }

  getConversation(conversationId: string | null): Promise<CopilotMessage[]> {
    const path = conversationId ? `${API_BASE_URL}/copilot/conversations/${conversationId}` : `${API_BASE_URL}/copilot/conversations`;
    return firstValueFrom(this.http.get<CopilotMessage[]>(path));
  }
}
