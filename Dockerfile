# API için Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Gerekli runtime bağımlılıkları (SQLite ve diğer native libs)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libsqlite3-0 \
        ca-certificates \
        libc6 \
        libgcc-s1 \
        libstdc++6 \
        zlib1g \
        && rm -rf /var/lib/apt/lists/* \
        && apt-get clean
# Render/Railway için 8080 portunu aç
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ButikProjesi.API/ButikProjesi.API.csproj", "ButikProjesi.API/"]
COPY ["ButikProjesi.Shared/ButikProjesi.Shared.csproj", "ButikProjesi.Shared/"]
RUN dotnet restore "ButikProjesi.API/ButikProjesi.API.csproj"
COPY . .
WORKDIR "/src/ButikProjesi.API"
RUN dotnet publish "ButikProjesi.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true
ENTRYPOINT ["dotnet", "ButikProjesi.API.dll"]


