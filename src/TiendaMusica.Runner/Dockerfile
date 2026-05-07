# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TiendaMusica.Runner/TiendaMusica.Runner.csproj", "src/TiendaMusica.Runner/"]
COPY ["src/TiendaMusica.Application/TiendaMusica.Application.csproj", "src/TiendaMusica.Application/"]
COPY ["src/TiendaMusica.Domain/TiendaMusica.Domain.csproj", "src/TiendaMusica.Domain/"]
COPY ["src/TiendaMusica.Infrastructure.Entrypoint/TiendaMusica.Infrastructure.Entrypoint.csproj", "src/TiendaMusica.Infrastructure.Entrypoint/"]
COPY ["src/TiendaMusica.Utilities/TiendaMusica.Utilities.csproj", "src/TiendaMusica.Utilities/"]
COPY ["src/TiendaMusica.Infrastructure.OutpointAdapter/TiendaMusica.Infrastructure.OutpointAdapter.csproj", "src/TiendaMusica.Infrastructure.OutpointAdapter/"]
RUN dotnet restore "./src/TiendaMusica.Runner/TiendaMusica.Runner.csproj"
COPY . .
WORKDIR "/src/src/TiendaMusica.Runner"
RUN dotnet build "./TiendaMusica.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TiendaMusica.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiendaMusica.Runner.dll"]