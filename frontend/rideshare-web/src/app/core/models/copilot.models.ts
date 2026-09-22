export interface CopilotMessage {
  id?: string;
  role: 'User' | 'Assistant' | 'System';
  content: string;
  createdAtUtc?: string;
  sources?: string[];
}

export interface CopilotReply {
  conversationId: string;
  reply: string;
  toolExecuted: string | null;
  sources: string[];
}
