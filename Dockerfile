FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VoxFox/*.csproj ./VoxFox/
RUN dotnet restore ./VoxFox/VoxFox.csproj

COPY . .
WORKDIR /src/VoxFox

RUN dotnet clean && \
    dotnet publish -c Release -o /app/publish \
    --no-restore && \
    rm -rf /root/.nuget/packages/*

# ============ DEVELOPMENT ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
WORKDIR /src
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_WATCH=1
ENTRYPOINT ["dotnet", "watch", "run", "--project", "VoxFox", "--no-launch-profile"]

# ============ TESTING ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS testing
WORKDIR /src
COPY . .
RUN dotnet test --configuration Release --verbosity normal --no-restore

# ============ STAGING ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS staging
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Staging

RUN apk add --no-cache \
    curl \
    vim \
    htop \
    netcat-openbsd \
    postgresql-client \
    redis \
    bash \
    && rm -rf /var/cache/apk/* \
    && echo "PS1='\u@\h:\w\$ '" >> /etc/profile

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "VoxFox.dll"]

# ============ PRODUCTION ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS production
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

USER $APP_UID

EXPOSE 80
ENTRYPOINT ["dotnet", "VoxFox.dll"]
