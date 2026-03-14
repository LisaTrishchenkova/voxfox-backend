FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# ============ VERSIONING ============
ARG APP_VERSION
ARG GIT_COMMIT
ARG BUILD_DATE

ENV APP_VERSION=${APP_VERSION:-unknown}
ENV GIT_COMMIT=${GIT_COMMIT:-unknown}
ENV BUILD_DATE=${BUILD_DATE:-unknown}
# ========================================

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

ARG APP_VERSION
ARG GIT_COMMIT
ARG BUILD_DATE

ENV APP_VERSION=${APP_VERSION:-unknown}
ENV GIT_COMMIT=${GIT_COMMIT:-unknown}
ENV BUILD_DATE=${BUILD_DATE:-unknown}

WORKDIR /src
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_WATCH=1
ENTRYPOINT ["dotnet", "watch", "run", "--project", "VoxFox", "--no-launch-profile"]

# ============ TESTING ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS testing

ARG APP_VERSION
ARG GIT_COMMIT
ARG BUILD_DATE

ENV APP_VERSION=${APP_VERSION:-unknown}
ENV GIT_COMMIT=${GIT_COMMIT:-unknown}
ENV BUILD_DATE=${BUILD_DATE:-unknown}

WORKDIR /src
COPY . .
RUN dotnet test --configuration Release --verbosity normal --no-restore

# ============ STAGING ============
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS staging

ARG APP_VERSION
ARG GIT_COMMIT
ARG BUILD_DATE

ENV APP_VERSION=${APP_VERSION:-unknown}
ENV GIT_COMMIT=${GIT_COMMIT:-unknown}
ENV BUILD_DATE=${BUILD_DATE:-unknown}

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
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS production

ARG APP_VERSION
ARG GIT_COMMIT
ARG BUILD_DATE

ENV APP_VERSION=${APP_VERSION:-unknown}
ENV GIT_COMMIT=${GIT_COMMIT:-unknown}
ENV BUILD_DATE=${BUILD_DATE:-unknown}

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

USER $APP_UID

EXPOSE 80
ENTRYPOINT ["dotnet", "VoxFox.dll"]
