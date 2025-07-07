
# Etapa base de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos .csproj de todos los proyectos
COPY ["AltecSystem.Api/AltecSystem.Api.csproj", "AltecSystem.Api/"]
COPY ["AltecSystem.Application/AltecSystem.Application.csproj", "AltecSystem.Application/"]
COPY ["AltecSystem.Domain/AltecSystem.Domain.csproj", "AltecSystem.Domain/"]
COPY ["AltecSystem.Infrastructure/AltecSystem.Infrastructure.csproj", "AltecSystem.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "AltecSystem.Api/AltecSystem.Api.csproj"

# Copiar todo el código
COPY . .

# Compilar
WORKDIR "/src/AltecSystem.Api"
RUN dotnet build "AltecSystem.Api.csproj" -c Release -o /app/build

# Publicar la app
FROM build AS publish
RUN dotnet publish "AltecSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Iniciar la aplicación
ENTRYPOINT ["dotnet", "AltecSystem.Api.dll"]
