# 🚀 Encurtador de URLs

> **Serviço de encurtamento de URLs construído com .NET 9, implementando Clean Architecture, CQRS e cache com Redis.**

[![.NET 9](htt---

## 🚧 **Status do Projeto**

⚠️ **Projeto em Desenvolvimento** - Este projeto ainda não está finalizado e está aberto para evoluções e melhorias.

### **Como Contribuir**
- 🐛 **Encontrou um bug?** Abra uma [issue](https://github.com/rrvasconcelos/UrlShortener/issues)
- 💡 **Tem uma ideia?** Compartilhe via [issues](https://github.com/rrvasconcelos/UrlShortener/issues)  
- 🔧 **Quer contribuir?** Pull requests são bem-vindos!

---

## 📝 **Licença**

Este projeto está licenciado sob a Licença MIT.

---

<div align="center">

**Construído com ❤️ por [rrvasconcelos](https://github.com/rrvasconcelos)**

*Demonstrando Clean Architecture e boas práticas de desenvolvimento .NET*

</div>s.io/badge/.NET-9-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Arquitetura-Clean-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![CQRS](https://img.shields.io/badge/Padrão-CQRS-blue)](https://docs.microsoft.com/pt-br/azure/architecture/patterns/cqrs)
[![Redis Cache](https://img.shields.io/badge/Cache-Redis-red?logo=redis)](https://redis.io/)

## 🎯 **Sobre o Projeto**

Um encurtador de URLs simples e funcional que demonstra **boas práticas de desenvolvimento .NET** com arquitetura limpa, separação de responsabilidades e testes abrangentes.

### ✨ **O que foi implementado**

- 🏗️ **Clean Architecture** com 4 camadas bem definidas
- ⚡ **Padrão CQRS** com Commands e Queries separados
- 🗄️ **Cache Redis** para otimizar consultas
- 🔐 **Hashids** para gerar códigos curtos seguros
- ✅ **51 testes unitários** com boa cobertura de código
- � **Logging estruturado** com Serilog

---

## 🏛️ **Arquitetura**

### **Clean Architecture - 4 Camadas**

```
┌─────────────────────────────────────┐
│            Api (Endpoints)          │ ← Controllers REST, Dependency Injection
├─────────────────────────────────────┤
│             Application             │ ← Handlers CQRS, Use Cases  
├─────────────────────────────────────┤
│              Domain                 │ ← Entities, Value Objects, Business Rules
├─────────────────────────────────────┤
│           Infrastructure            │ ← EF Core, Redis, External Services
└─────────────────────────────────────┘
```

### **CQRS Implementation**

- **CreateShortUrlCommand**: Cria URL curta com cache
- **GetUrlQuery**: Busca URL original com cache-first strategy
- **Result Pattern**: Tratamento de erros sem exceptions

---

## 🛠️ **Stack Tecnológico**

| Tecnologia | Uso |
|-----------|-----|
| **.NET 9** | Runtime e framework base |
| **ASP.NET Core** | Web API com minimal APIs |
| **PostgreSQL** | Banco de dados principal |
| **Entity Framework Core** | ORM para acesso aos dados |
| **Redis** | Cache para otimização |
| **Hashids.net** | Geração de códigos curtos |
| **xUnit** | Framework de testes |
| **NSubstitute** | Mocking para testes |
| **FluentAssertions** | Assertions mais legíveis |
| **Serilog** | Logging estruturado |

---

## �️ **Cache Strategy**

### **Redis Cache Implementation**

- **Cache Keys**: 
  - `long:{url}` → Armazena código curto
  - `short:{code}` → Armazena URL original
- **TTL**: 1 ano (URLs são imutáveis)
- **Fallback**: Funciona sem cache em caso de falha

---

## ✅ **Testes**

### **Cobertura de Testes - 51 testes total**

```
📊 Testes por Camada:
├── Domain (25 testes) - Value Objects, Entities, Business Rules
├── Application (12 testes) - CQRS Handlers, Use Cases  
└── Infrastructure (14 testes) - Services, Repositories
```

### **Frameworks utilizados**
- **xUnit**: Framework de testes principal
- **NSubstitute**: Mocks e stubs
- **FluentAssertions**: Assertions expressivas
- **Test Coverage**: Relatórios de cobertura

---

## 🚀 **Como Executar**

### **Pré-requisitos**
- .NET 9 SDK
- Docker (para PostgreSQL e Redis)

### **1. Clone e configure**

```bash
git clone https://github.com/rrvasconcelos/UrlShortener.git
cd UrlShortener

# Configure o salt do Hashids (obrigatório)
dotnet user-secrets init --project src/UrlShortener.Api
dotnet user-secrets set "Hashids:Salt" "seu-salt-aqui" --project src/UrlShortener.Api
```

### **2. Suba a infraestrutura**

```bash
# PostgreSQL + Redis
docker-compose up -d postgres redis
```

### **3. Execute a aplicação**

```bash
dotnet run --project src/UrlShortener.Api
```

### **4. Teste a API**

```bash
# Criar URL curta
curl -X POST http://localhost:5000 \
  -H "Content-Type: application/json" \
  -d '{"longUrl": "https://github.com/rrvasconcelos"}'

# Resposta: {"shortCode": "abc123", "longUrl": "https://github.com/rrvasconcelos"}

# Usar URL curta
curl -I http://localhost:5000/abc123
# Retorna: 302 Redirect para URL original
```

---

## 🧪 **Executando Testes**

```bash
# Todos os testes
dotnet test

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Projeto específico
dotnet test tests/UrlShortener.Application.Tests/

# Com output detalhado
dotnet test --logger "console;verbosity=detailed"
```

---

##  **Licença**

Este projeto está licenciado sob a Licença MIT.

---

<div align="center">

**Construído por [rrvasconcelos](https://github.com/rrvasconcelos)**

*Demonstrando Clean Architecture e boas práticas de desenvolvimento .NET*

</div>