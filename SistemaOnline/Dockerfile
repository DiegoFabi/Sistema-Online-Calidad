FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SistemaOnline/SistemaOnline.csproj", "SistemaOnline/"]
RUN dotnet restore "SistemaOnline/SistemaOnline.csproj"
COPY . .
WORKDIR /src/SistemaOnline
RUN dotnet build "SistemaOnline.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SistemaOnline.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# La app guarda el bloqueo por intentos fallidos en App_Data/login_lockouts.json: la carpeta
# debe poder escribirla el usuario sin privilegios ($APP_UID) con el que corre el contenedor.
RUN mkdir -p /app/App_Data && chown -R $APP_UID /app/App_Data
USER $APP_UID
ENTRYPOINT ["dotnet", "SistemaOnline.dll"]