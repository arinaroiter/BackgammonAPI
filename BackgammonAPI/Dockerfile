# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, as its own layer (cached unless the csproj changes)
COPY BackgammonAPI.csproj ./
RUN dotnet restore BackgammonAPI.csproj

# Copy the rest of the source and publish
COPY . ./
RUN dotnet publish BackgammonAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BackgammonAPI.dll"]