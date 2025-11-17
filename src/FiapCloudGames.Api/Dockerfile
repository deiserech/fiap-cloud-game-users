FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS base
WORKDIR /app
EXPOSE 80

# Install the agent
RUN apt-get update && apt-get install -y curl ca-certificates gnupg \
&& echo 'deb [signed-by=/etc/apt/keyrings/newrelic-apt.gpg] http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& curl -fsSL https://download.newrelic.com/NEWRELIC_APT_2DAD550E.public | gpg --dearmor > /etc/apt/keyrings/newrelic-apt.gpg \
&& apt-get update \
&& apt-get install -y newrelic-dotnet-agent

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_APP_NAME=FiapCloudGamesWebApp

FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FiapCloudGames.Api/FiapCloudGames.Api.csproj", "src/FiapCloudGames.Api/"]
COPY ["src/FiapCloudGames.Application/FiapCloudGames.Application.csproj", "src/FiapCloudGames.Application/"]
COPY ["src/FiapCloudGames.Domain/FiapCloudGames.Domain.csproj", "src/FiapCloudGames.Domain/"]
COPY ["src/FiapCloudGames.Infrastructure/FiapCloudGames.Infrastructure.csproj", "src/FiapCloudGames.Infrastructure/"]
RUN dotnet restore "src/FiapCloudGames.Api/FiapCloudGames.Api.csproj"
COPY . .
WORKDIR "/src/src/FiapCloudGames.Api"
RUN dotnet build "FiapCloudGames.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FiapCloudGames.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Garante que a aplicação escute na porta 80 em todas as interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:80

ENTRYPOINT ["dotnet", "FiapCloudGames.Api.dll"]