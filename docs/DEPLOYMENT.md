# Deployment Guide

This guide covers how to deploy and run the NetHack Chat Game in various environments.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development with Docker Compose](#local-development-with-docker-compose)
3. [Local Development without Docker](#local-development-without-docker)
4. [Production Deployment](#production-deployment)
5. [Environment Configuration](#environment-configuration)
6. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| Docker | 20+ | Container runtime |
| Docker Compose | 2.0+ | Multi-container orchestration |
| .NET SDK | 8.0+ | Local development (optional) |
| Git | Any | Clone repository |

### Optional Tools

- **Visual Studio 2022** or **VS Code** - IDE
- **pgAdmin** or **DBeaver** - Database management
- **Postman** - API testing

---

## Local Development with Docker Compose

### Quick Start

**1. Clone the repository:**

```bash
git clone <repository-url>
cd NetHackChatGame
```

**2. Create environment file:**

```bash
cp .env.example .env
```

**3. Edit `.env` and add your OpenAI API key:**

```env
OPENAI_API_ENDPOINT=https://api.openai.com/v1
OPENAI_API_KEY=sk-your-api-key-here
OPENAI_MODEL=gpt-4
```

See [.env.example](#environment-file-example) for all variables.

**4. Start all services:**

```bash
docker-compose up -d
```

**5. Check service health:**

```bash
# Check if all containers are running
docker-compose ps

# Check logs
docker-compose logs -f
```

**6. Open the client:**

```
http://localhost:8080
```

### Development Workflow

**Watch logs:**

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f signalr-api
docker-compose logs -f llm-proxy
docker-compose logs -f mcp-server
docker-compose logs -f postgres
```

**Rebuild services after code changes:**

```bash
# Rebuild all services
docker-compose build

# Rebuild specific service
docker-compose build signalr-api

# Rebuild and restart
docker-compose up -d --build
```

**Stop services:**

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (deletes database data)
docker-compose down -v
```

**Database access:**

```bash
# Connect to PostgreSQL
docker exec -it nethack-postgres psql -U nethack_user -d nethack_chat

# Run SQL commands
\dt                          # List tables
SELECT * FROM conversations; # Query data
\q                           # Exit
```

**Access services:**

| Service | URL |
|---------|-----|
| SignalR API | http://localhost:5000 |
| LLM Proxy | http://localhost:5001 |
| MCP Server | http://localhost:5002 |
| PostgreSQL | localhost:5432 |
| Web Client | http://localhost:8080 |

---

## Local Development without Docker

For development without Docker, run each service separately.

### 1. Setup PostgreSQL

**Install PostgreSQL 16:**

- **Windows**: Download from [postgresql.org](https://www.postgresql.org/download/windows/)
- **macOS**: `brew install postgresql@16`
- **Linux**: `sudo apt install postgresql-16`

**Create database:**

```sql
CREATE DATABASE nethack_chat;
CREATE USER nethack_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE nethack_chat TO nethack_user;
```

### 2. Configure Connection Strings

Update `appsettings.Development.json` in each project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nethack_chat;Username=nethack_user;Password=your_password"
  }
}
```

### 3. Run Database Migrations

```bash
cd src/NetHackChatGame.Data
dotnet ef database update
```

### 4. Start MCP Server

```bash
cd src/NetHackChatGame.McpServer
dotnet run
```

**Runs on**: http://localhost:5002

### 5. Start LLM Proxy

**Set environment variables:**

```bash
# Windows PowerShell
$env:OPENAI_API_KEY="sk-your-key"
$env:OPENAI_API_ENDPOINT="https://api.openai.com/v1"
$env:OPENAI_MODEL="gpt-4"

# macOS/Linux
export OPENAI_API_KEY="sk-your-key"
export OPENAI_API_ENDPOINT="https://api.openai.com/v1"
export OPENAI_MODEL="gpt-4"
```

**Run service:**

```bash
cd src/NetHackChatGame.LlmProxy
dotnet run
```

**Runs on**: http://localhost:5001

### 6. Start SignalR API

```bash
cd src/NetHackChatGame.SignalRApi
dotnet run
```

**Runs on**: http://localhost:5000

### 7. Open Client

Open `client/index.html` in your browser, or serve with:

```bash
# Python
python -m http.server 8080

# Node.js
npx serve client -p 8080
```

---

## Production Deployment

### Docker Compose (Production)

**1. Create production environment file:**

```bash
cp .env.example .env.production
```

**2. Update production settings:**

```env
# Use production database credentials
POSTGRES_PASSWORD=strong_random_password_here

# Use production OpenAI endpoint
OPENAI_API_KEY=sk-prod-key

# Restrict CORS
ALLOWED_ORIGINS=https://yourdomain.com

# Disable detailed errors
ASPNETCORE_ENVIRONMENT=Production
```

**3. Deploy with production compose file:**

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

**4. Setup SSL/TLS:**

Use a reverse proxy (nginx or Caddy) for HTTPS:

```yaml
# docker-compose.prod.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - signalr-api
```

**Example nginx.conf:**

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location /chatHub {
        proxy_pass http://signalr-api:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location / {
        proxy_pass http://signalr-api:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Azure Deployment

**Option 1: Azure Container Instances**

```bash
# Login to Azure
az login

# Create resource group
az group create --name nethack-rg --location eastus

# Create container group
az container create \
  --resource-group nethack-rg \
  --name nethack-chat \
  --image your-registry/signalr-api:latest \
  --dns-name-label nethack-chat \
  --ports 80 443
```

**Option 2: Azure Container Apps**

```bash
# Create Container Apps environment
az containerapp env create \
  --name nethack-env \
  --resource-group nethack-rg \
  --location eastus

# Deploy each service
az containerapp create \
  --name signalr-api \
  --resource-group nethack-rg \
  --environment nethack-env \
  --image your-registry/signalr-api:latest \
  --target-port 5000 \
  --ingress external
```

**Option 3: Azure Kubernetes Service (AKS)**

See [Kubernetes deployment guide](#kubernetes-deployment) below.

### AWS Deployment

**Option 1: AWS ECS (Elastic Container Service)**

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name nethack-cluster

# Create task definition
aws ecs register-task-definition --cli-input-json file://task-definition.json

# Create service
aws ecs create-service \
  --cluster nethack-cluster \
  --service-name signalr-api \
  --task-definition signalr-api:1 \
  --desired-count 1
```

**Option 2: AWS Fargate**

Serverless container deployment with ECS.

### Kubernetes Deployment

**1. Create namespace:**

```bash
kubectl create namespace nethack
```

**2. Create secrets:**

```bash
kubectl create secret generic nethack-secrets \
  --from-literal=postgres-password=your_password \
  --from-literal=openai-api-key=sk-your-key \
  -n nethack
```

**3. Apply deployments:**

```bash
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/mcp-server.yaml
kubectl apply -f k8s/llm-proxy.yaml
kubectl apply -f k8s/signalr-api.yaml
```

**Example Kubernetes deployment (signalr-api.yaml):**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: signalr-api
  namespace: nethack
spec:
  replicas: 2
  selector:
    matchLabels:
      app: signalr-api
  template:
    metadata:
      labels:
        app: signalr-api
    spec:
      containers:
      - name: signalr-api
        image: your-registry/signalr-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: LlmProxy__Url
          value: "http://llm-proxy:5001"
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
---
apiVersion: v1
kind: Service
metadata:
  name: signalr-api
  namespace: nethack
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
  selector:
    app: signalr-api
```

---

## Environment Configuration

### Environment File Example

**.env.example:**

```env
# ========================================
# Database Configuration
# ========================================
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=nethack_chat
POSTGRES_USER=nethack_user
POSTGRES_PASSWORD=changeme_in_production

# ========================================
# OpenAI Configuration
# ========================================
OPENAI_API_ENDPOINT=https://api.openai.com/v1
OPENAI_API_KEY=sk-your-api-key-here
OPENAI_MODEL=gpt-4
OPENAI_MAX_TOKENS=2000
OPENAI_TEMPERATURE=0.7

# Alternative: Azure OpenAI
# OPENAI_API_ENDPOINT=https://your-resource.openai.azure.com
# OPENAI_API_KEY=your-azure-key
# OPENAI_MODEL=gpt-4-deployment-name

# Alternative: Local LLM (Ollama, LM Studio)
# OPENAI_API_ENDPOINT=http://localhost:11434/v1
# OPENAI_API_KEY=not-needed
# OPENAI_MODEL=llama2

# ========================================
# Service URLs (Docker internal network)
# ========================================
SIGNALR_API_URL=http://signalr-api:5000
LLM_PROXY_URL=http://llm-proxy:5001
MCP_SERVER_URL=http://mcp-server:5002

# ========================================
# CORS Configuration
# ========================================
ALLOWED_ORIGINS=http://localhost:8080,http://localhost:3000

# Production: Use your domain
# ALLOWED_ORIGINS=https://yourdomain.com

# ========================================
# Application Settings
# ========================================
ASPNETCORE_ENVIRONMENT=Development
# Production: Set to Production

# Logging Level
LOG_LEVEL=Information
# Options: Trace, Debug, Information, Warning, Error, Critical

# ========================================
# Feature Flags
# ========================================
ENABLE_STREAMING=false
ENABLE_DEBUG_LOGGING=true
ENABLE_SWAGGER=true

# ========================================
# Security (Production)
# ========================================
# JWT_SECRET=your-secret-key-here
# ENCRYPTION_KEY=your-encryption-key
```

### Configuration Files

Each service has `appsettings.json` and `appsettings.Development.json`:

**appsettings.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=${POSTGRES_HOST:-localhost};Port=${POSTGRES_PORT:-5432};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
  },
  "OpenAI": {
    "Endpoint": "${OPENAI_API_ENDPOINT}",
    "ApiKey": "${OPENAI_API_KEY}",
    "Model": "${OPENAI_MODEL:-gpt-4}",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "McpServer": {
    "Url": "${MCP_SERVER_URL:-http://localhost:5002}"
  }
}
```

### Configuration Priority

1. **Environment variables** (highest priority)
2. **appsettings.{Environment}.json**
3. **appsettings.json**
4. **Default values** (lowest priority)

---

## Monitoring and Logging

### Logs with Docker Compose

```bash
# All logs
docker-compose logs -f

# Last 100 lines
docker-compose logs --tail=100

# Specific service
docker-compose logs -f signalr-api

# Follow logs from specific time
docker-compose logs --since 2025-11-04T10:00:00
```

### Structured Logging with Serilog

All services use Serilog for structured logging:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { 
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Health Checks

**Check service health:**

```bash
# SignalR API
curl http://localhost:5000/health

# LLM Proxy
curl http://localhost:5001/health

# MCP Server
curl http://localhost:5002/health
```

**Health check response:**

```json
{
  "status": "healthy",
  "service": "signalr-api",
  "dependencies": {
    "database": "healthy",
    "llmProxy": "healthy"
  },
  "timestamp": "2025-11-04T10:00:00Z"
}
```

### Monitoring with Prometheus (Future)

Add Prometheus metrics exporter to each service:

```csharp
builder.Services.AddOpenTelemetryMetrics(builder =>
{
    builder.AddPrometheusExporter();
    builder.AddMeter("nethack.*");
});
```

---

## Database Backups

### Automated Backups with Docker

**Backup script:**

```bash
#!/bin/bash
# backup.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="./backups"
mkdir -p $BACKUP_DIR

docker exec postgres pg_dump -U nethack_user nethack_chat \
  > $BACKUP_DIR/nethack_backup_$DATE.sql

echo "Backup completed: $BACKUP_DIR/nethack_backup_$DATE.sql"
```

**Schedule with cron:**

```bash
# Backup every day at 2 AM
0 2 * * * /path/to/backup.sh
```

### Restore from Backup

```bash
# Stop services
docker-compose down

# Start only database
docker-compose up -d postgres

# Restore backup
docker exec -i postgres psql -U nethack_user nethack_chat \
  < backups/nethack_backup_20251104.sql

# Start all services
docker-compose up -d
```

---

## Troubleshooting

### Common Issues

**1. Services won't start**

```bash
# Check logs
docker-compose logs

# Check if ports are in use
netstat -ano | findstr :5000
netstat -ano | findstr :5001
netstat -ano | findstr :5432

# Kill processes using ports (Windows PowerShell)
Stop-Process -Id <PID> -Force
```

**2. Database connection errors**

```bash
# Verify database is running
docker-compose ps postgres

# Check database logs
docker-compose logs postgres

# Test connection manually
docker exec -it postgres psql -U nethack_user -d nethack_chat

# Recreate database
docker-compose down -v
docker-compose up -d postgres
```

**3. OpenAI API errors**

```bash
# Verify API key is set
docker-compose exec llm-proxy printenv OPENAI_API_KEY

# Test API key manually
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer sk-your-key"

# Check LLM Proxy logs
docker-compose logs llm-proxy | grep -i openai
```

**4. SignalR connection failures**

```bash
# Check CORS settings
docker-compose logs signalr-api | grep -i cors

# Verify SignalR endpoint
curl http://localhost:5000/chatHub

# Check client console for errors (browser DevTools)
```

**5. MCP Server not responding**

```bash
# Check MCP Server logs
docker-compose logs mcp-server

# Test MCP endpoint
curl http://localhost:5002/health

# Verify LLM Proxy can reach MCP Server
docker-compose exec llm-proxy curl http://mcp-server:5002/health
```

### Debug Mode

Enable detailed logging:

```env
# .env
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL=Debug
ENABLE_DEBUG_LOGGING=true
```

Rebuild and restart:

```bash
docker-compose down
docker-compose up --build
```

### Clean Slate

Start fresh with clean state:

```bash
# Stop and remove everything
docker-compose down -v

# Remove all images
docker-compose down --rmi all

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up -d
```

---

## Performance Tuning

### PostgreSQL Configuration

**Increase shared memory:**

```yaml
# docker-compose.yml
services:
  postgres:
    shm_size: 256mb
    environment:
      - POSTGRES_SHARED_BUFFERS=256MB
```

### .NET Configuration

**Increase connection pool:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;...;MinPoolSize=5;MaxPoolSize=20"
  }
}
```

### Docker Resources

Allocate more resources to Docker:

- **Memory**: 4 GB minimum, 8 GB recommended
- **CPU**: 2 cores minimum, 4 cores recommended

---

## Scaling

### Horizontal Scaling

**Scale SignalR API:**

```bash
docker-compose up -d --scale signalr-api=3
```

**Requires**:
- Redis backplane for SignalR
- Load balancer (nginx)

**Add Redis to docker-compose.yml:**

```yaml
services:
  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
```

**Configure SignalR to use Redis:**

```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis("redis:6379");
```

---

## Security Checklist

**Production deployment checklist:**

- [ ] Change default database password
- [ ] Use HTTPS/TLS with valid certificates
- [ ] Restrict CORS to specific domains
- [ ] Enable authentication (JWT/OAuth)
- [ ] Use secrets management (Azure Key Vault, AWS Secrets Manager)
- [ ] Enable rate limiting
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Disable Swagger in production
- [ ] Enable firewall rules
- [ ] Regular security updates
- [ ] Database backups configured
- [ ] Monitoring and alerting set up

---

## Conclusion

This deployment guide covers:
- ✅ Local development with Docker Compose
- ✅ Local development without Docker
- ✅ Production deployment options
- ✅ Environment configuration
- ✅ Monitoring and logging
- ✅ Backups and recovery
- ✅ Troubleshooting common issues
- ✅ Performance tuning
- ✅ Security best practices

For questions or issues, open a GitHub issue or check the logs for error details.
