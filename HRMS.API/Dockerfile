# ====================================================================
# AlienHR Web API — Dockerfile for HRMS.API subdirectory context
# ====================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["HRMS.API.csproj", "./"]
RUN dotnet restore "HRMS.API.csproj"

COPY . .
RUN dotnet publish "HRMS.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 5000
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HRMS.API.dll"]
