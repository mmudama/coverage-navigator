import { useState } from 'react';
import { sendMessage } from '../services/chatService';
import './Chat.css';

export default function Chat() {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [sessionId, setSessionId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!input.trim()) return;

    const userMessage = input.trim();
    setInput('');
    setError(null);

    // Add user message to UI immediately
    setMessages(prev => [...prev, { role: 'user', content: userMessage, timestamp: new Date().toISOString() }]);

    setIsLoading(true);

    try {
      const response = await sendMessage(userMessage, sessionId);
      
      // Store session ID from first response
      if (!sessionId && response.sessionId) {
        setSessionId(response.sessionId);
      }

      // Add assistant response to messages
      setMessages(prev => [...prev, { 
        role: 'assistant', 
        content: response.message, 
        timestamp: new Date().toISOString(),
        provider: response.provider,
        model: response.model,
      }]);
    } catch (err) {
      setError(err.message);
      console.error('Chat error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleReset = () => {
    setMessages([]);
    setSessionId(null);
    setError(null);
  };

  return (
    <div className="chat-container">
      <div className="chat-header">
        <h1>Coverage Navigator</h1>
        {sessionId && (
          <div className="session-info">
            <span className="session-id">Session: {sessionId.substring(0, 8)}...</span>
            <button onClick={handleReset} className="reset-button">New Chat</button>
          </div>
        )}
      </div>

      <div className="chat-messages">
        {messages.length === 0 && (
          <div className="empty-state">
            <p>Start a conversation by typing a message below.</p>
          </div>
        )}
        
        {messages.map((msg, index) => (
          <div key={index} className={`message message-${msg.role}`}>
            <div className="message-header">
              <span className="message-role">{msg.role === 'user' ? 'You' : 'Assistant'}</span>
              {msg.model && <span className="message-model">{msg.model}</span>}
            </div>
            <div className="message-content">{msg.content}</div>
          </div>
        ))}
        
        {isLoading && (
          <div className="message message-assistant">
            <div className="message-header">
              <span className="message-role">Assistant</span>
            </div>
            <div className="message-content loading">Thinking...</div>
          </div>
        )}
      </div>

      {error && (
        <div className="error-banner">
          <strong>Error:</strong> {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="chat-input-form">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type your message..."
          disabled={isLoading}
          className="chat-input"
        />
        <button type="submit" disabled={isLoading || !input.trim()} className="send-button">
          Send
        </button>
      </form>
    </div>
  );
}
