# UrlShortener

Pequeno encurtador de URLs em .NET, com separação por camadas (Domain, Application, Infrastructure e API) e gerador de short codes baseado em Hashids.

## Visão geral

- Plataforma: .NET 9
- Testes: xUnit
- Persistência: Entity Framework Core (projeto Infrastructure já preparado)
- Short code: Hashids.net via `IShortCodeGenerator` com implementação `HashidsShortCodeGenerator`

## Estrutura da solução

- `src/UrlShortener.Api` — API ASP.NET Core (DI, configuração, endpoints de exemplo e OpenAPI em Desenvolvimento)
- `src/UrlShortener.Application` — Abstrações e orquestração de casos de uso (ex.: `IShortCodeGenerator`)
- `src/UrlShortener.Domain` — Entidades e Value Objects (ex.: `UrlMapping`, `LongUrl`, `ShortCode`)
- `src/UrlShortener.Infrastructure` — Implementações técnicas (EF Core, Hashids, etc.)
- `tests/UrlShortener.Domain.Tests` — Testes de domínio
- `tests/UrlShortener.Infrastructure.Tests` — Testes de infraestrutura (inclui “testes de contrato” para `IShortCodeGenerator`)

## Configuração do Hashids (segura)

A implementação `HashidsShortCodeGenerator` exige dois parâmetros:

- `Hashids:Salt` — segredo obrigatório (NÃO comitar em repositório)
- `Hashids:MinHashLength` — comprimento mínimo do código (default 7)

No `Program.cs` da API o registro é feito assim (via DI):

- Lê `Hashids:Salt` de configuração (User Secrets, variável de ambiente ou cofre) e falha com mensagem clara se não configurado
- Lê `Hashids:MinHashLength` (opcional; default 7)

Os arquivos `appsettings.json` e `appsettings.Development.json` contêm apenas `Hashids:MinHashLength` por padrão; o `Salt` deve vir de um provedor seguro.

### Desenvolvimento (User Secrets)

No Windows PowerShell, na raiz do repositório:

```powershell
# Inicializar User Secrets (já pode estar configurado)
dotnet user-secrets init --project .\src\UrlShortener.Api\UrlShortener.Api.csproj

# Definir o Salt de forma segura
# Use um valor longo e aleatório
dotnet user-secrets set "Hashids:Salt" "<seu-salt-bem-aleatorio>" --project .\src\UrlShortener.Api\UrlShortener.Api.csproj
```

### Variáveis de ambiente (produção/CI)

```powershell
# Em Windows PowerShell
$env:HASHIDS__SALT = "<valor>"
$env:HASHIDS__MINHASHLENGTH = "7"
```

> Observação: Em ASP.NET Core, chaves de configuração aninhadas usam `__` (duplo underscore) em variáveis de ambiente.

## Como executar

```powershell
# Restaurar e compilar
dotnet build .\UrlShortener.sln

# Executar a API (certifique-se de ter configurado o Hashids:Salt)
dotnet run --project .\src\UrlShortener.Api\UrlShortener.Api.csproj
```

- OpenAPI está habilitado em Ambiente de Desenvolvimento (template do .NET 9). A aplicação exibirá as rotas de documentação no console.

## Testes

```powershell
dotnet test
```

Tipos de testes incluídos:

- Domain: valida entidades e value objects (ex.: `UrlMapping`)
- Infrastructure: testes de contrato para `IShortCodeGenerator` e a implementação `HashidsShortCodeGenerator`
  - O “contrato” está em `tests/UrlShortener.Infrastructure.Tests/Services/CodeGeneration/ShortCodeGeneratorContractTests.cs`
  - A suíte específica da implementação está em `tests/UrlShortener.Infrastructure.Tests/Services/CodeGeneration/HashidsShortCodeGeneratorTests.cs`

### Adicionando outra implementação de short code

1. Implemente `IShortCodeGenerator` em `Infrastructure` (ou outro projeto)
2. Crie uma classe de teste no projeto de testes que herde do contrato e sobrescreva `CreateSut()` retornando a sua implementação

Exemplo:

```csharp
public class MyShortCodeGeneratorTests : ShortCodeGeneratorContractTests
{
		protected override IShortCodeGenerator CreateSut() => new MyShortCodeGenerator(/* params */);
}
```

## Solução de problemas

- Erro ao subir a API: `Hashids:Salt is not configured`
  - Configure via User Secrets (dev) ou variável de ambiente/cofre (produção)
- Teste falhando em `Decode(null)`
  - O contrato exige lançar alguma exceção para `null` (pode ser `ArgumentNullException` ou outra propagada pela lib Hashids). Se preferir padronizar, valide `null/whitespace` antes de chamar a lib e lance `InvalidShortCodeException`.

## Boas práticas de segurança

- Nunca comite segredos (como o `Hashids:Salt`) no repositório
- Prefira User Secrets em desenvolvimento, variáveis de ambiente ou cofres (Azure Key Vault, etc.) em produção
