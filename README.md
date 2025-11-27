# 🎮 FIAP Cloud Games - Users Service

## 📝 Visão Geral

Projeto backend para o microserviço de gestão de usuários do sistema FIAP Cloud Games. Contém API REST, serviços de background para integração com corretores de mensagens, persistência via Entity Framework Core e instrumentação (OpenTelemetry).

## 🏗️ Tecnologias Principais

- **Plataforma**: .NET 8 (TargetFramework: `net8.0`)
- **ORM**: Entity Framework Core
- **Autenticação**: JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Observabilidade**: OpenTelemetry
- **Contêiner**: Docker (`Dockerfile` presente em `src/FiapCloudGames.Api`)
- **CI/CD**: Azure Pipelines (`pipeline/azure-pipelines.yml`)

## 📁 Estrutura da Solution

- `FiapCloudGames.Users.sln` — Solution raiz
- `src/FiapCloudGames.Api/` — Projeto Web API (entrypoint). Contém controllers, middlewares, background services e configuração do Docker.
- `src/FiapCloudGames.Application/` — Camada de aplicação: DTOs, interfaces e serviços de negócio.
- `src/FiapCloudGames.Domain/` — Entidades de domínio, enums e eventos.
- `src/FiapCloudGames.Infrastructure/` — Persistência (EF Core), repositórios, migrations e integração com service bus.
- `src/FiapCloudGames.Shared/` — Utilitários compartilhados (logging, tracing helpers).
- `tests/FiapCloudGames.Tests/` — Testes unitários e de integração.

## 🔍 Principais Componentes

- **Controllers**: `AuthController`, `UserController`, `LibraryController` — rotas de autenticação, gestão de usuários e biblioteca.
- **BackgroundServices**: consumidores que processam mensagens (ex.: `GameConsumer.cs`, `PurchaseCompletedConsumer.cs`).
- **ServiceBus**: integração para envio/recebimento de mensagens (pasta `ServiceBus` em `Infrastructure`).
- **Middlewares**: tratamento de erros e enriquecimento de tracing (`ErrorHandlingMiddleware`, `TracingEnrichmentMiddleware`).
- **Extensions**: configuração de JWT, Swagger e OpenTelemetry em `Extensions/`.

## 🚀 Pré-requisitos

- .NET SDK 8.0
- Docker (opcional, para execução em contêiner)
- Ferramentas opcionais: `dotnet-ef` para migrations

Confirme a versão do SDK com:

```powershell
dotnet --version
```

## 🧭 Como Rodar Localmente

1. Restaurar pacotes e compilar:

```powershell
dotnet restore
dotnet build
```

2. Executar a API (modo desenvolvimento):

```powershell
dotnet run --project src\FiapCloudGames.Api\FiapCloudGames.Users.Api.csproj
```

3. A API expõe Swagger (quando em `Development`) — acesse `https://localhost:{PORT}/swagger`.

## 🐳 Executando com Docker

Gerar imagem localmente (executar na raiz do repositório):

```powershell
docker build -f src\FiapCloudGames.Api\Dockerfile -t fiapcloudgames.users:local .
```

Rodar contêiner com variáveis de ambiente essenciais:

```powershell
docker run -e ASPNETCORE_ENVIRONMENT=Production -e ConnectionStrings__DefaultConnection="<CONN_STRING>" -e Jwt__Secret="<SECRET>" -p 5000:80 fiapcloudgames.users:local
```

Observação: Use `__` (dois underscores) para nomes hierárquicos de configuração do .NET (ex.: `ConnectionStrings__DefaultConnection`).

## 🔧 Variáveis de Ambiente / Configurações Importantes

- `ConnectionStrings__DefaultConnection` — string de conexão com o banco de dados
- `ASPNETCORE_ENVIRONMENT` — `Development` | `Production`
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Secret` — configuração do JWT
- Service bus / Kafka: `ServiceBus__ConnectionString` ou variáveis equivalentes usadas na infra

Verifique `appsettings.json` e `appsettings.Development.json` em `src/FiapCloudGames.Api` para chaves e exemplos.

## 🗄️ Migrations e Banco de Dados

Para aplicar migrations (exemplo):

```powershell
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database update --project src\FiapCloudGames.Infrastructure\FiapCloudGames.Users.Infrastructure.csproj --startup-project src\FiapCloudGames.Api\FiapCloudGames.Users.Api.csproj
```

## 🔁 Mensageria e Background Services

O projeto contém consumidores que processam eventos de compra e atualizam a biblioteca do usuário. Verifique `BackgroundServices/` e `Infrastructure/ServiceBus` para fluxos e tópicos/filas configuráveis.

## 🧪 Testes

Executar testes unitários:

```powershell
dotnet test tests\FiapCloudGames.Tests\FiapCloudGames.Tests.csproj
```

## 📦 CI / CD

Pipeline de CI está definido em `pipeline/azure-pipelines.yml`. Ele contém etapas típicas de build, test e publicação de artefatos. Ajuste conforme sua organização (variáveis secretas, feeds de pacote, etc.).

## 📞 Contato

- **Mantenedora**: `@deiserech` — rech.deise@gmail.com

## 🧾 Exemplos de Endpoints (rotas principais e payloads)

- POST `/api/auth/register` — registra um novo usuário

	Request JSON:

	```json
	{
		"name": "Maria Silva",
		"email": "maria@example.com",
		"password": "P@ssw0rd!",
		"role": "Player"
	}
	```

	Response (201 Created):

	```json
	{
		"id": "guid",
		"name": "Maria Silva",
		"email": "maria@example.com"
	}
	```

- POST `/api/auth/login` — autentica e retorna token JWT

	Request JSON:

	```json
	{
		"email": "maria@example.com",
		"password": "P@ssw0rd!"
	}
	```

	Response (200 OK):

	```json
	{
		"token": "eyJhbGci...",
		"expiresIn": 3600,
		"user": { "id": "guid", "name": "Maria Silva", "email": "maria@example.com" }
	}
	```

- GET `/api/users/profile` — obtém dados do usuário (autenticado)

	Response (200 OK):

	```json
	{
		"code": "int",
		"name": "Maria Silva",
		"email": "maria@example.com",
		"createdAt": "2025-11-01T12:00:00Z"
	}
	```

- GET `/api/library/{userCode}` — lista jogos na biblioteca do usuário

	Response (200 OK):

	```json
	[
		{ "gameCode": "int", "title": "Space Adventure", "purchasedAt": "2025-10-05T10:00:00Z" }
	]
	```
