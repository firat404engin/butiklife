# API için Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
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
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "ButikProjesi.API.dll"]


