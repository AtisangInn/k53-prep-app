# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["backend/K53PrepApp.Api.csproj", "backend/"]
RUN dotnet restore "backend/K53PrepApp.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/backend"
RUN dotnet publish "K53PrepApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Copy frontend files to wwwroot so UseStaticFiles() serves them
COPY --from=build /src/frontend ./wwwroot 
ENTRYPOINT ["dotnet", "K53PrepApp.Api.dll"]
