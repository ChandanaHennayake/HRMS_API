# ====================================================================
# AlienHR Web API — Production Multi-Stage Dockerfile (.NET 9.0)
# Supports free cloud deployment on Render with Supabase PostgreSQL
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

# Run as official non-root user ($APP_UID = 1654 in .NET 9)
USER $APP_UID

# Copy compiled binaries from build stage
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

# Expose default HTTP port (Render dynamically sets PORT env var)
EXPOSE 8080

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "HRMS.API.dll"]
