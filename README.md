# 🔗 URL Shortener

> **Encurtador de URLs full-stack construído com .NET 9, Angular 20 e Docker. Demonstra Clean Architecture, CQRS e boas práticas de desenvolvimento.**

![Stack](https://img.shields.io/badge/.NET-10.0-purple) ![Angular](https://img.shields.io/badge/Angular-20.3-red) ![Docker](https://img.shields.io/badge/Docker-Ready-blue) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue) ![Redis](https://img.shields.io/badge/Redis-7-red)

---

## 🚀 **Execução Rápida (Docker)**

### **Pré-requisitos**

- [Docker](https://www.docker.com/) e [Docker Compose](https://docs.docker.com/compose/)

### **1. Clone o projeto**

```bash
git clone https://github.com/rrvasconcelos/UrlShortener.git
cd UrlShortener
```

### **2. Configure o Salt (OBRIGATÓRIO)**

Crie o arquivo `.env` na raiz do projeto:

```bash
# Windows (PowerShell)
@"
# Hashids Salt - NUNCA COMITE ESTE ARQUIVO NO GIT!
HASHIDS_SALT=vK9mP2xL8nQ4wE7rT6yU3iO5pA1sD0fG2hJ4kL7zX9cV8bN6mQ3wE5rT1yU4iO0p
HASHIDS_MIN_LENGTH=7

# PostgreSQL Settings
POSTGRES_DB=urlshortener_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# Backend Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://*:8080
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=urlshortener_db;Username=postgres;Password=postgres;Include Error Detail=true
REDIS_CONNECTION_STRING=redis:6379
"@ | Out-File -FilePath ".env" -Encoding utf8

# Linux/macOS
cat > .env << EOF
# Hashids Salt - NUNCA COMITE ESTE ARQUIVO NO GIT!
HASHIDS_SALT=vK9mP2xL8nQ4wE7rT6yU3iO5pA1sD0fG2hJ4kL7zX9cV8bN6mQ3wE5rT1yU4iO0p
HASHIDS_MIN_LENGTH=7

# PostgreSQL Settings
POSTGRES_DB=urlshortener_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# Backend Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://*:8080
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=urlshortener_db;Username=postgres;Password=postgres;Include Error Detail=true
REDIS_CONNECTION_STRING=redis:6379
EOF
```

### **3. Execute a aplicação**

```bash
docker-compose up -d
```

### **4. Acesse a aplicação**

- **Frontend**: http://localhost:8080
- **API Direta**: http://localhost:5000
- **PostgreSQL**: localhost:5434

🎉 **Pronto!** A aplicação estará rodando completamente via Docker.

---

## 🧪 **Testando a API**

### **Criar URL curta**

```bash
curl -X POST http://localhost:8080/api/shorten \
  -H "Content-Type: application/json" \
  -d '{"LongUrl": "https://github.com/rrvasconcelos/UrlShortener"}'
```

**Resposta:**

```json
{"shortCode":"R5AZNAz","longUrl":null}
```

### **Usar URL curta**

```bash
curl -I http://localhost:8080/R5AZNAz
```

**Resposta:**

```
HTTP/1.1 301 Moved Permanently
Location: https://github.com/rrvasconcelos/UrlShortener
```

---

## 🏗️ **Arquitetura do Sistema**

### **Stack Tecnológico**

- **Backend**: .NET 10, ASP.NET Core, Entity Framework Core
- **Frontend**: Angular 20, Material Design, TypeScript
- **Database**: PostgreSQL 16 (dados), Redis 7 (cache)
- **Proxy**: Nginx (reverse proxy)
- **Containerização**: Docker + Docker Compose

### **Clean Architecture (Backend)**

```
┌─────────────────────────────────────┐
│     Api (Endpoints & Controllers)   │ ← REST API, Dependency Injection
├─────────────────────────────────────┤
│         Application (CQRS)          │ ← Commands, Queries, Handlers  
├─────────────────────────────────────┤
│    Domain (Business Logic)         │ ← Entities, Value Objects, Rules
├─────────────────────────────────────┤
│   Infrastructure (Data & External)  │ ← EF Core, Redis, External APIs
└─────────────────────────────────────┘
```

### **Containerização**

- **backend-dev**: .NET container com hot reload
- **frontend-dev**: Angular container com live reload
- **nginx**: Reverse proxy (Frontend + API)
- **postgres**: PostgreSQL database
- **redis**: Cache Redis

---

## 🛠️ **Comandos Úteis**

### **Docker**

```bash
# Parar todos os containers
docker-compose down

# Ver logs de um serviço específico
docker-compose logs backend-dev
docker-compose logs frontend-dev

# Reiniciar um serviço
docker-compose restart nginx

# Limpar cache do Redis
docker-compose exec redis redis-cli FLUSHALL

# Conectar no PostgreSQL
docker-compose exec postgres psql -U postgres -d urlshortener_db
```

### **Desenvolvimento Local (sem Docker)**

```bash
# Instalar dependências
dotnet restore

# Rodar testes
dotnet test

# Rodar backend apenas
dotnet run --project src/UrlShortener.Api

# Rodar frontend apenas
cd frontend/UrlShortener && npm install && npm start
```

---

## 🎯 **Recursos Implementados**

### **Backend (.NET)**

- ✅ **Clean Architecture** com 4 camadas
- ✅ **CQRS Pattern** (Commands & Queries)
- ✅ **Repository Pattern** com EF Core
- ✅ **Cache Strategy** com Redis
- ✅ **Logging** estruturado com Serilog
- ✅ **Validação** com FluentValidation
- ✅ **Migrations** automáticas
- ✅ **Health Checks**
- ✅ **CORS** configurado

### **Frontend (Angular)**

- ✅ **Angular 20** com Standalone Components
- ✅ **Material Design** moderno
- ✅ **Reactive Forms** com validação
- ✅ **HTTP Interceptors**
- ✅ **Dark/Light Theme** toggle
- ✅ **Responsive Design**
- ✅ **Copy to Clipboard**
- ✅ **Error Handling**

### **DevOps & Infrastructure**

- ✅ **Docker Compose** multi-container
- ✅ **Nginx** como reverse proxy
- ✅ **PostgreSQL** com volumes persistentes
- ✅ **Redis** com persistência
- ✅ **Hot Reload** em desenvolvimento
- ✅ **Environment Variables** configuráveis

---

## 🧪 **Executando Testes**

```bash
# Via Docker
docker-compose exec backend-dev dotnet test

# Localmente
dotnet test --logger "console;verbosity=detailed"

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

**Cobertura**: Domain (85%+), Application (80%+), Infrastructure (75%+)

---

## 🔧 **Configuração Avançada**

### **Variáveis de Ambiente (.env)**

| Variável                      | Descrição                      | Obrigatório      |
| ------------------------------ | -------------------------------- | ----------------- |
| `HASHIDS_SALT`               | Salt para geração de códigos  | ✅ Sim            |
| `HASHIDS_MIN_LENGTH`         | Tamanho mínimo do código       | Não (padrão: 7) |
| `DATABASE_CONNECTION_STRING` | String de conexão do PostgreSQL | Não              |
| `REDIS_CONNECTION_STRING`    | String de conexão do Redis      | Não              |

### **Portas Utilizadas**

| Serviço                   | Porta Host | Porta Container |
| -------------------------- | ---------- | --------------- |
| Nginx (Frontend + API)     | 8080       | 80              |
| Backend (desenvolvimento)  | 5000       | 8080            |
| Frontend (desenvolvimento) | 4200       | 4200            |
| PostgreSQL                 | 5434       | 5432            |
| Redis                      | -          | 6379            |

---

## 📝 **Licença**

MIT License - veja [LICENSE](LICENSE) para detalhes.

---

<div align="center">

**Desenvolvido por [rrvasconcelos](https://github.com/rrvasconcelos)**

*Demonstração de Clean Architecture e boas práticas Full-Stack*

</div>
