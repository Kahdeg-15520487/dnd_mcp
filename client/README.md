# NetHack Chat Game - Web Client

A simple HTML/JavaScript client for testing the NetHack Chat Game.

## Features

- **User Authentication**: Register and login with username/password
- **SignalR Connection**: Real-time WebSocket communication with the game server
- **Chat Interface**: Terminal-style chat interface for game interaction
- **Conversation Management**: Automatically creates or resumes game sessions

## How to Use

### 1. Start the Backend Services

First, make sure all backend services are running. From the root directory:

```bash
docker-compose up
```

Or run services individually:

```bash
# Terminal 1 - PostgreSQL (using Docker)
docker run -p 5432:5432 -e POSTGRES_PASSWORD=nethack_pass -e POSTGRES_USER=nethack_user -e POSTGRES_DB=nethack_chat postgres:16

# Terminal 2 - Auth Service
cd src/NetHackChatGame.AuthService
dotnet run

# Terminal 3 - MCP Server
cd src/NetHackChatGame.McpServer
dotnet run

# Terminal 4 - LLM Proxy
cd src/NetHackChatGame.LlmProxy
dotnet run

# Terminal 5 - SignalR API
cd src/NetHackChatGame.SignalRApi
dotnet run
```

### 2. Run Database Migrations

Before first use, apply database migrations:

```bash
cd src/NetHackChatGame.Data
dotnet ef database update
```

### 3. Start LLM Server (Optional)

If you have Ollama installed:

```bash
ollama serve
ollama pull llama3.2
```

Or configure a different OpenAI-compatible endpoint in `src/NetHackChatGame.LlmProxy/appsettings.json`.

### 4. Open the Client

Simply open `client/index.html` in your web browser. No build step required!

You can use a local web server for better experience:

```bash
# Using Python
python -m http.server 8080

# Using Node.js http-server
npx http-server -p 8080
```

Then navigate to `http://localhost:8080/client/index.html`

## Usage

1. **Register**: Create a new account with username and password
2. **Login**: Sign in with your credentials
3. **Enter Character Name**: Choose a name for your in-game character
4. **Play**: Type commands like "look around", "move north", "attack goblin", etc.

## Configuration

The client connects to these services by default:

- **Auth Service**: `http://localhost:5003`
- **SignalR Hub**: `http://localhost:5000/chatHub`

To change these, edit the constants at the top of the `<script>` section in `index.html`:

```javascript
const AUTH_API_URL = 'http://localhost:5003/api/auth';
const CONVERSATIONS_API_URL = 'http://localhost:5003/api/conversations';
const SIGNALR_HUB_URL = 'http://localhost:5000/chatHub';
```

## Troubleshooting

### CORS Errors

If you see CORS errors in the browser console, make sure:
- All services are running
- CORS is enabled in each service's `Program.cs`
- You're accessing the client from `http://localhost` (not `file://`)

### Connection Failed

Check that:
1. PostgreSQL is running on port 5432
2. All .NET services are running and healthy
3. LLM server (Ollama or other) is accessible
4. Check service health endpoints:
   - Auth: `http://localhost:5003/health`
   - MCP Server: `http://localhost:5002/health`
   - LLM Proxy: `http://localhost:5001/health`
   - SignalR API: `http://localhost:5000/health`

### No Response from Game

The LLM Proxy requires an OpenAI-compatible API. Make sure:
- Ollama is running (`ollama serve`)
- Or configure another LLM endpoint in `src/NetHackChatGame.LlmProxy/appsettings.json`

## Technologies Used

- **HTML5** - Structure
- **CSS3** - Retro terminal styling
- **JavaScript** - Client logic
- **SignalR Client 8.0.0** - WebSocket communication via CDN
