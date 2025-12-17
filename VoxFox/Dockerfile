FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["VoxFox/VoxFox.csproj", "VoxFox/"]
RUN dotnet restore "VoxFox/VoxFox.csproj"

COPY . .

RUN dotnet publish "VoxFox/VoxFox.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VoxFox.dll"]
