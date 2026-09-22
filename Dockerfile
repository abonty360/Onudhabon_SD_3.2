# =========================================================
# Stage 1: Build & Publish
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first for Docker layer caching
COPY ["Onudhabon.csproj", "./"]
RUN dotnet restore "Onudhabon.csproj"

# Copy source code and build in Release configuration
COPY . .
RUN dotnet publish "Onudhabon.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================================
# Stage 2: Final Runtime Image
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Expose default HTTP container port
EXPOSE 8080

# Configure container defaults (Render injects PORT dynamically at runtime)
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

# Copy published application from build stage
COPY --from=build /app/publish .

# Start the ASP.NET Core application
ENTRYPOINT ["dotnet", "Onudhabon.dll"]
