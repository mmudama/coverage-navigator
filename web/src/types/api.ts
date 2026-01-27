/**
 * Auto-generated TypeScript types for CoverageNavigator.Contracts
 * Mirrors C# models from: CoverageNavigator.Contracts/Models
 */

export interface ChatRequest {
  sessionId?: string;
  message: string;
  model?: string;
  systemPromptIdentifier?: string;
}

export interface ChatResponse {
  sessionId: string;
  message: string;
  provider: string;
  model: string;
  messageCount: number;
}

export interface ConversationMessage {
  role: string;
  content: string;
  timestamp: string; // ISO 8601 format (DateTime from C#)
}

export interface ConversationSession {
  sessionId: string;
  messages: ConversationMessage[];
  createdAt: string; // ISO 8601 format (DateTime from C#)
  lastAccessedAt: string; // ISO 8601 format (DateTime from C#)
}
