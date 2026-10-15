# ====================================================================
# AlienHR Web API — Production Multi-Stage Dockerfile (.NET 9.0)
# Supports cloud deployment (Render, Railway, Docker Hub, Fly.io, etc.)
# ====================================================================

# Stage 1: Build & Publish using official Microsoft .NET 9.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project definition first for optimal Docker layer caching
COPY ["HRMS.API/HRMS.API.csproj", "HRMS.API/"]

# Restore NuGet dependencies
RUN dotnet restore "HRMS.API/HRMS.API.csproj"

# Copy API source code
COPY HRMS.API/ HRMS.API/

WORKDIR "/src/HRMS.API"

# Publish production binaries
RUN dotnet publish "HRMS.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 2: Runtime using official Microsoft ASP.NET 9.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy compiled binaries from build stage
COPY --from=build /app/publish .

# Expose default HTTP ports (Render/Railway dynamically bind PORT env var)
EXPOSE 8080
EXPOSE 5000
EXPOSE 80

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HRMS.API.dll"]
