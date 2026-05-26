# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BootManager.Web/BootManager.Web.csproj", "BootManager.Web/"]
COPY ["BootManager.Application/BootManager.Application.csproj", "BootManager.Application/"]
COPY ["BootManager.Infrastructure/BootManager.Infrastructure.csproj", "BootManager.Infrastructure/"]
COPY ["BootManager.Core/BootManager.Core.csproj", "BootManager.Core/"]

# Restore dependencies
RUN dotnet restore "BootManager.Web/BootManager.Web.csproj" -r linux-arm64

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish "BootManager.Web/BootManager.Web.csproj" \
	-c Release \
	-r linux-arm64 \
	--self-contained false \
	--no-restore \
	-o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app

RUN apt-get update && \
	apt-get install -y --no-install-recommends curl && \
	rm -rf /var/lib/apt/lists/*

# Create data directories for volumes
RUN mkdir -p /var/lib/bootmanager && \
	mkdir -p /var/log/bootmanager && \
	mkdir -p /app/data/logbook-attachments

# Copy published application
COPY --from=build /app/publish .

# Set environment variables for runtime
ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
	ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
	CMD curl -fsS http://localhost:5000/health || exit 1

EXPOSE 5000

ENTRYPOINT ["dotnet", "BootManager.Web.dll"]
