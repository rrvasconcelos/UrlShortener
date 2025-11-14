# 🚀 Como rodar o projeto com Docker

## Pré-requisitos

- Docker Desktop instalado
- PostgreSQL rodando localmente (porta 5432)
- Banco de dados `urlshortener_db` criado

## 📝 Configuração

1. **O arquivo `.env` já está criado com o salt seguro**
   ```
   HASHIDS_SALT=vK9mP2xL8nQ4wE7rT6yU3iO5pA1sD0fG2hJ4kL7zX9cV8bN6mQ3wE5rT1yU4iO0p
   ```

## 🏃 Como rodar

### Opção 1: Build e start (primeira vez)
```powershell
docker-compose up --build
```

### Opção 2: Start (após build)
```powershell
docker-compose up -d
```

### Parar os containers
```powershell
docker-compose down
```

### Ver logs
```powershell
# Todos os serviços
docker-compose logs -f

# Apenas backend
docker-compose logs -f backend

# Apenas frontend
docker-compose logs -f frontend
```

## 🌐 Acesso

- **Frontend**: http://localhost (porta 80)
- **Backend (interno)**: http://localhost:8080 (não acesse diretamente)
- **Redis**: localhost:6379

## 📦 Serviços

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| nginx | 80 | Proxy reverso (ponto de entrada) |
| backend | 8080 | API .NET (interno) |
| frontend | 4200 | Angular (interno) |
| redis | 6379 | Cache |

## 🔐 Segurança

- O salt está no arquivo `.env` que **NÃO** é commitado no git
- O backend roda em HTTP interno (comunicação container-to-container)
- Nginx expõe apenas a porta 80 externamente
- Versão da API (`/v1/`) está oculta para o usuário final

## 🗄️ Banco de Dados

O PostgreSQL roda **localmente fora do Docker** na sua máquina host:
- Host: `localhost` (via `host.docker.internal` no Docker)
- Porta: `5432`
- Database: `urlshortener_db`
- User: `postgres`
- Password: `postgres`

Se quiser mudar as credenciais, edite o `docker-compose.yml` na seção `backend.environment.ConnectionStrings__Database`.

## 🔄 Rebuild após mudanças no código

```powershell
# Rebuild apenas o backend
docker-compose up --build backend

# Rebuild apenas o frontend
docker-compose up --build frontend

# Rebuild tudo
docker-compose up --build
```

## 🧹 Limpar tudo

```powershell
# Parar e remover containers, redes
docker-compose down

# Remover volumes também (cuidado: apaga dados do Redis)
docker-compose down -v

# Remover imagens
docker-compose down --rmi all
```
