# Stage 1: Build the app
# We use the .NET 8.0 SDK image (which has the build tools)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies first
# This copies MiniProjectManager.csproj
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the source code and build
COPY . .
RUN dotnet publish "MiniProjectManager.csproj" -c Release -o /app/publish

# Stage 2: Create the final, smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render will connect to this port
EXPOSE 8080

# Entrypoint to run the app
ENTRYPOINT ["dotnet", "MiniProjectManager.dll"]