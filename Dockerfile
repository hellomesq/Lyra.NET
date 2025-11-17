# ======================
# 1. Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Lyra.csproj"
RUN dotnet publish "Lyra.csproj" -c Release -o /app

# ======================
# 2. Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
CMD ["dotnet", "Lyra.dll"]
