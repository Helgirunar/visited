# Step 1: Use the .NET runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 80
EXPOSE 443

# Step 2: Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "./backend/backend.csproj"

# Copy the rest of the code
COPY . . 
WORKDIR "/src/backend"

# Build the application
RUN dotnet build "./backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Step 3: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Step 4: Install EF Core tools in the final image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Copy published app from build stage
COPY --from=publish /app/publish .

# Copy the migration file to the container. Remember to update it with "dotnet ef migrations script -o migration.sql" command!!
COPY ["backend/migration.sql", "/app/"]
RUN mkdir -p /app/certificates
COPY backend/certs/visitCounter.pfx /app/certificates/
RUN chown -R app:app /app

# Step 6: Run migrations and then start the application
ENTRYPOINT ["dotnet", "backend.dll"]
