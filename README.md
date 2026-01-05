# Fiap Cloud Games - Users Service

Serviço de **Usuários** da plataforma *Fiap Cloud Games*, responsável por autenticação, gerenciamento de usuários, biblioteca de jogos do usuário e sugestões baseadas em histórico de compras.

Este projeto foi pensado como parte de um ecossistema de microsserviços (user, game, payments), com foco em **boas práticas de arquitetura**, **observabilidade** e **comunicação assíncrona**.

---

## Visão Arquitetural

### Solução e projetos

A solução é composta por múltiplos projetos .NET 8 organizados em camadas:

- **src/FiapCloudGames.Api**  
  API HTTP (ASP.NET Core) que expõe os endpoints públicos do serviço de usuários.
- **src/FiapCloudGames.Application**  
  Camada de aplicação (DTOs, services, orquestração de casos de uso, interfaces de infraestrutura).
- **src/FiapCloudGames.Domain**  
  Núcleo de domínio (entidades, enums, eventos, interfaces de repositório e contratos de domínio).
- **src/FiapCloudGames.Infrastructure**  
  Implementações de repositórios (EF Core), integração com Azure Service Bus, Elasticsearch, publishers de eventos.
- **src/FiapCloudGames.Shared**  
  Componentes cross-cutting (tracing, background service de logging, utilitários compartilhados).
- **tests/FiapCloudGames.Tests**  
  Testes automatizados (xUnit, Moq, Shouldly, coverlet para cobertura).

Essa divisão segue princípios de **separação de responsabilidades** e facilita evolução independente das camadas.

### Principais responsabilidades do serviço `user`

- Autenticação de usuários (login) e emissão de **JWT**.
- Registro de novos usuários.
- Consulta e gerenciamento de perfis de usuário.
- Exposição da biblioteca de jogos do usuário.
- Consumo de eventos de compra para atualizar biblioteca e histórico.
- Exposição de sugestões de jogos baseadas em dados indexados no **Elasticsearch**.

### Integrações externas

- **SQL Server**: banco transacional principal do serviço de usuários, acessado via **Entity Framework Core**.
- **Azure Service Bus**: usado para consumir eventos de outros serviços, como `purchases.completed`.
- **Elasticsearch** (via **NEST**): utilizado como **read model** otimizado para histórico de compras e sugestões.
- **OpenTelemetry**: coleta de métricas e traces distribuídos.

Detalhes adicionais sobre a arquitetura de autenticação/gateway e uso de Elasticsearch estão em:

- docs/arquitetura-autenticacao-gateway.md
- docs/elasticsearch-arquitetura.md
- docs/message-contracts.md

---

## API HTTP

A API é construída em **ASP.NET Core 8** e configurada em `src/FiapCloudGames.Api/Program.cs`.

### Controllers principais

- **AuthController** (`/api/auth`)
  - `POST /login` – autentica usuário via email/senha e retorna JWT + dados do usuário.
  - `POST /register` – registra novo usuário e, em caso de sucesso, retorna JWT + dados do usuário.

- **UserController** (`/api/user`)
  - `GET /{code}` – obtém usuário por código numérico (requer autenticação e roles `Admin` ou `User`).
  - `GET /profile` – retorna o perfil do usuário autenticado com base no `NameIdentifier` do token.
  - `POST /` – cria usuário (registro) de forma anônima, retornando o recurso criado.

- **LibraryController** (`/api/library`)
  - `GET /user/{userCode}` – obtém a biblioteca completa de jogos de um usuário (requer autenticação e roles `Admin` ou `User`).

- **SuggestionsController** (`/api/suggestions`)
  - `GET /user/{userCode}` – retorna sugestões de jogos com base no histórico do usuário (requer autenticação e roles `Admin` ou `User`).

### Middlewares e extensões

- **ErrorHandlingMiddleware** – tratamento centralizado de erros, retornando `ProblemDetails` padronizados.
- **TracingEnrichmentMiddleware** – enriquece contexto de tracing/log com informações da requisição.
- **JwtAuthenticationServiceCollectionExtensions** – configura autenticação JWT com base em `JwtSettings` do `appsettings`.
- **OpenTelemetryServiceCollectionExtensions** – registra exporters e instrumentações de OpenTelemetry.
- **SwaggerServiceCollectionExtensions** – configura **Swagger / OpenAPI** para documentação da API.

---

## Background Services e Mensageria

O projeto usa **Hosted Services** para processar eventos assíncronos e tarefas em background:

- **PurchaseCompletedConsumer**  
  Consome mensagens de `purchases.completed` via **Azure Service Bus** e atualiza a biblioteca de jogos do usuário.

- **GameConsumer** / **PurchaseHistoryConsumer**  
  Consumidores adicionais que integram informações de jogos e histórico de compras, alimentando repositórios e Elasticsearch.

- **ResourceLoggingService** (em Shared)  
  Serviço de background que coleta métricas de recursos e envia para observabilidade.

Os publishers/consumers utilizam:

- `ServiceBusClient` e um wrapper (`IServiceBusClientWrapper`) para publicar/consumir eventos.
- Contratos documentados em docs/message-contracts.md.

---

## Elasticsearch

O serviço configura um `IElasticClient` em `Program.cs` lendo as configurações de `Elasticsearch` no `appsettings`:

- `Elasticsearch:Uri`
- `Elasticsearch:Username`
- `Elasticsearch:Password`

A partir desse client, a camada de infraestrutura implementa:

- Indexação e leitura de histórico de compras do usuário.
- Endpoints de sugestões (`ISuggestionService`) que utilizam índices otimizados conforme definido em docs/elasticsearch-arquitetura.md.

Para ambiente de estudo, a validação de certificado está desabilitada (`ServerCertificateValidationCallback` sempre `true`), o que **não deve ser usado em produção**.

---

## Autenticação e Autorização

- Autenticação baseada em **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`).
- Configuração em `appsettings.json` (seção `JwtSettings`):
  - `Issuer`
  - `Audience`
  - `ExpiryInMinutes`
- As rotas sensíveis usam `[Authorize]` com restrição de roles (`Admin`, `User`).
- O serviço `user` é o emissor de tokens e deve ser o IdP dentro do ecossistema, conforme docs/arquitetura-autenticacao-gateway.md.

A arquitetura considera um **API Gateway** externo como única porta pública, que pode:

- Apenas rotear chamadas (cada serviço valida JWT), ou
- Validar JWT e repassar contexto via headers internos.

---

## Observabilidade

- Uso de **OpenTelemetry** para traces e métricas:
  - Pacotes `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Instrumentation.SqlClient` (beta, usada apenas por ser projeto de estudo).
- Integração opcional com **NewRelic** via `NewRelic:LicenseKey` (configurado em `appsettings.json`).

---

## Configuração

### appsettings.json (padrão)

Principais seções:

- `ConnectionStrings:DefaultConnection` – string de conexão com SQL Server.
- `JwtSettings` – configuração de JWT.
- `Elasticsearch` – endpoint e credenciais.
- `NewRelic` – license key para APM.

### appsettings.Development.json

- Sobrescreve `ConnectionStrings` para ambiente local.
- Configurações de `ServiceBus`:
  - `ServiceBus:ConnectionString` – pode apontar para **Azure Service Bus** ou emulador local.
  - `ServiceBus:UseWebSockets` – flag de uso de websockets no transporte.

### Variáveis de ambiente recomendadas

Para ambiente real, **não** deixar segredos em `appsettings.json`. Configurar via variáveis de ambiente:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__ExpiryInMinutes`
- `Elasticsearch__Uri`
- `Elasticsearch__Username`
- `Elasticsearch__Password`
- `ServiceBus__ConnectionString`
- `NewRelic__LicenseKey`

---

## Requisitos de Ambiente

- **.NET SDK 8.0**
- **SQL Server** (local ou em container Docker)
- **Elasticsearch** (idealmente 7.x compatível com NEST 7.17.5)
- Acesso a um **Azure Service Bus** ou emulador/endpoint equivalente

Exemplo (opcional) de subir SQL Server via Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Admin347856" -p 1433:1433 --name sql-users -d mcr.microsoft.com/mssql/server:2022-latest
```

---

## Como executar localmente

Na raiz do repositório:

1. Restaurar pacotes e compilar:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Garantir que SQL Server, Service Bus e Elasticsearch estejam acessíveis conforme `appsettings.Development.json`.

3. Rodar a API:

   ```bash
   dotnet run --project src/FiapCloudGames.Api/FiapCloudGames.Users.Api.csproj
   ```

4. Acessar Swagger (em desenvolvimento):

   - `https://localhost:5001/swagger` ou `http://localhost:5000/swagger` (conforme profile/launchSettings).

---

## Testes

Os testes estão em `tests/FiapCloudGames.Tests` e utilizam xUnit.

Para executar:

```bash
dotnet test
```

Para gerar cobertura em formato Cobertura (integrado via coverlet):

```bash
cd tests/FiapCloudGames.Tests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

Há scripts auxiliares em `others/` (por exemplo, `coverage.ps1`) que podem automatizar geração de relatórios.

---

## Migrações de Banco de Dados

Scripts auxiliares em `others/` podem ajudar a gerar e aplicar migrações:

- `others/generate-migration.ps1`
- `others/apply-migration.ps1`

A camada de infraestrutura utiliza **EF Core** com `AppDbContext` em `FiapCloudGames.Infrastructure`.

---

## Decisões de Arquitetura (Resumo)

- **Camadas separadas** (Api, Application, Domain, Infrastructure, Shared) para manter baixo acoplamento e alta coesão.
- **Mensageria** com Azure Service Bus para integração entre serviços (compras, jogos, usuários).
- **Elasticsearch** tratado como read model (busca/analytics), mantendo banco relacional como fonte de verdade.
- **JWT** como mecanismo de autenticação central, com possibilidade de validação no Gateway e/ou em cada serviço.
- **Observabilidade** desde o início com OpenTelemetry e integração opcional com NewRelic.

Para detalhes aprofundados, consulte a pasta `docs/`, que contém documentos de arquitetura para autenticação/gateway, Elasticsearch e contratos de mensagens.