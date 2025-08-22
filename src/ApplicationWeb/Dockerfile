# === Base Image: ASP.NET Core Runtime ===
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Ensure the Images folder exists inside wwwroot
RUN mkdir -p /app/wwwroot/Images

# === Build Image: .NET SDK ===
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/ApplicationWeb/ApplicationWeb.csproj", "src/ApplicationWeb/"]
COPY ["src/ApplicationCore/ApplicationCore.csproj", "src/ApplicationCore/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/ApplicationWeb/ApplicationWeb.csproj"

# Copy the full source and build the project
COPY . .
WORKDIR "/src/src/ApplicationWeb"
RUN dotnet build "ApplicationWeb.csproj" -c Release -o /app/build

# === Publish Image ===
FROM build AS publish
RUN dotnet publish "ApplicationWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# === Final Runtime Image ===
FROM base AS final
WORKDIR /app

# Copy published app to final image
COPY --from=publish /app/publish .

# Optional: explicitly ensure wwwroot/Images exists in runtime container
RUN mkdir -p /app/wwwroot/Images

# Entry point for container
ENTRYPOINT ["dotnet", "ApplicationWeb.dll"]
