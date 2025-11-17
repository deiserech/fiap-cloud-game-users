
# Executando a aplicação localmente com Docker e SQL Server

> ⚠️ **Atenção:** Para rodar a aplicação com monitoramento New Relic, você precisa de uma conta ativa no New Relic e de uma API Key (License Key) válida. Caso não possua, crie uma conta gratuita em [newrelic.com](https://newrelic.com/) e obtenha sua chave na área de administração da plataforma.

Para rodar a aplicação localmente utilizando Docker, siga os passos abaixo:


## 1. Baixe e rode um container SQL Server local

Se você ainda não possui a imagem do SQL Server, baixe-a com o comando:

```
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

Depois, crie e rode o container (ajuste a senha se desejar):

```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Fiap@2025!CloudGames#Db" -p 1433:1433 --name sqlserver-fiapcloudgames -d mcr.microsoft.com/mssql/server:2022-latest
```

## 2. Configure a string de conexão

No arquivo `src/FiapCloudGames.Api/appsettings.json`, utilize:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=host.docker.internal,1433;Database=fiapcloudgamesdb;User Id=sa;Password=Fiap@2025!CloudGames#Db;TrustServerCertificate=True;"
}
```

> **Nota:** `host.docker.internal` permite que o container .NET acesse o SQL Server rodando no host Windows.

## 3. Execute as migrações do banco de dados

Abra um terminal na pasta do projeto e execute:

```
dotnet tool install --global dotnet-ef # (caso não tenha o dotnet-ef)
dotnet ef database update --project src/FiapCloudGames.Api
```

## 4. Build e execute a aplicação via Docker


Na raiz do projeto, execute:

```
docker build -t fiapcloudgames-api -f src/FiapCloudGames.Api/Dockerfile .

# Execute o container com todas as variáveis de ambiente necessárias:
docker run -d \
  --name fiapcloudgames-api \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=fiapcloudgamesdb;User Id=sa;Password=Fiap@2025!CloudGames#Db;TrustServerCertificate=True;" \
  -e JwtSettings__Issuer="FiapCloudGames" \
  -e JwtSettings__Audience="FiapCloudGamesUsers" \
  -e JwtSettings__ExpiryInMinutes="60" \
  -e JwtSettings__SecretKey="FiapCloudGames@SuperSecretKey2025!" \
  -e NewRelic__LicenseKey="<YOUR_KEY>" \
  -p 5002:80 \
  fiapcloudgamesacr.azurecr.io/fiap-cloud-games-api:v2
```

Acesse a API em: [http://localhost:5002/swagger](http://localhost:5002/swagger)

---
