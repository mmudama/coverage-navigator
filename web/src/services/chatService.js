/**
 * Chat API Service
 * Handles communication with the backend chat endpoint
 */

const API_URL = import.meta.env.VITE_API_URL;

if (!API_URL) {
  console.error('VITE_API_URL environment variable is not set');
}

/**
 * Send a message to the chat API
 * @param {string} message - The user's message
 * @param {string|null} sessionId - Optional session ID for continuing a conversation
 * @param {string|null} model - Optional model to use
 * @param {string|null} systemPromptIdentifier - Optional system prompt identifier
 * @returns {Promise<ChatResponse>}
 */
export async function sendMessage(
  message,
  sessionId = null,
  model = null,
  systemPromptIdentifier = null
) {
  /** @type {ChatRequest} */
  const request = {
    message,
    sessionId,
    model,
    systemPromptIdentifier,
  };

  const response = await fetch(`${API_URL}/api/chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(`Chat API error: ${response.status} ${response.statusText}`);
  }

  /** @type {ChatResponse} */
  const data = await response.json();
  return data;
}
