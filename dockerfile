# Build stage: compile the API using the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching for NuGet restore.
COPY ["SecureDocs.sln", "./"]
COPY ["src/SecureDocs.Domain/SecureDocs.Domain.csproj", "src/SecureDocs.Domain/"]
COPY ["src/SecureDocs.Application/SecureDocs.Application.csproj", "src/SecureDocs.Application/"]
COPY ["src/SecureDocs.Infrastructure/SecureDocs.Infrastructure.csproj", "src/SecureDocs.Infrastructure/"]
COPY ["src/SecureDocs.API/SecureDocs.API.csproj", "src/SecureDocs.API/"]

RUN dotnet restore "src/SecureDocs.API/SecureDocs.API.csproj"

# Copy the rest of the source and publish.
COPY src/. src/
WORKDIR /src/src/SecureDocs.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage: only the ASP.NET runtime and the published artifacts.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SecureDocs.API.dll"]
