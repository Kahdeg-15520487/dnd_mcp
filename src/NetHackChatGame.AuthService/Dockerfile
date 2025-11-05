# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/NetHackChatGame.AuthService/NetHackChatGame.AuthService.csproj", "src/NetHackChatGame.AuthService/"]
COPY ["src/NetHackChatGame.Core/NetHackChatGame.Core.csproj", "src/NetHackChatGame.Core/"]
COPY ["src/NetHackChatGame.Data/NetHackChatGame.Data.csproj", "src/NetHackChatGame.Data/"]
RUN dotnet restore "src/NetHackChatGame.AuthService/NetHackChatGame.AuthService.csproj"

# Copy source files and build
COPY src/NetHackChatGame.AuthService/ src/NetHackChatGame.AuthService/
COPY src/NetHackChatGame.Core/ src/NetHackChatGame.Core/
COPY src/NetHackChatGame.Data/ src/NetHackChatGame.Data/
WORKDIR /src/src/NetHackChatGame.AuthService
RUN dotnet build "NetHackChatGame.AuthService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NetHackChatGame.AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "NetHackChatGame.AuthService.dll"]
