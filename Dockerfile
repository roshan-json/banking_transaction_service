# Multi-stage build for optimized image size
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file
COPY ["banking_transaction_service.csproj", "."]

# Restore dependencies
RUN dotnet restore "banking_transaction_service.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "banking_transaction_service.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "banking_transaction_service.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "banking_transaction_service.dll"]
