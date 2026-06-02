# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy Shared project files for restore
COPY Shared/Shared.csproj Shared/

# Copy project file for restore
COPY CodexMcp/CodexMcp.csproj CodexMcp/

# Restore dependencies (cached layer)
RUN dotnet restore "CodexMcp/CodexMcp.csproj"

# Copy remaining source files
COPY Shared/ Shared/
COPY CodexMcp/ CodexMcp/

# Build and publish
RUN dotnet publish "CodexMcp/CodexMcp.csproj" -c Release -o /app/publish --runtime linux-x64 --self-contained false 

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY CodexMcp/appsettings.json ./
EXPOSE 4547
ENTRYPOINT ["dotnet", "CodexMcp.dll"]