# Imagen base para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Imagen para construir el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "ApiReconocimientoVoz/ApiReconocimientoVoz.csproj"
RUN dotnet publish "ApiReconocimientoVoz/ApiReconocimientoVoz.csproj" -c Release -o /app/publish

# Imagen final con solo los archivos necesarios
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiReconocimientoVoz.dll"]
