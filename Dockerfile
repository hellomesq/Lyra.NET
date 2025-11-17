# ======================
# 1. Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os arquivos para dentro do container
COPY . .

# Restaura pacotes
RUN dotnet restore "Lyra.csproj"

# Publica o projeto em Release dentro da pasta /app
RUN dotnet publish "Lyra.csproj" -c Release -o /app

# ======================
# 2. Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia a pasta publicada do build stage
COPY --from=build /app .

# Expõe a porta que o Render vai usar
EXPOSE 8080

# Comando para iniciar o app
CMD ["dotnet", "Lyra.dll"]
