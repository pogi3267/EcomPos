##See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
# ------------------------
# Stage 1: Build the app
# ------------------------
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ApplicationWeb/ApplicationWeb.csproj ./ApplicationWeb/
COPY ApplicationCore/ApplicationCore.csproj ./ApplicationCore/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
RUN dotnet restore ./ApplicationWeb/ApplicationWeb.csproj --runtime linux-x64

# Copy everything else and publish
COPY . .
RUN dotnet publish ./ApplicationWeb/ApplicationWeb.csproj -c Release -o /app/publish --runtime linux-x64 --self-contained false

# ----------------------------
# Stage 2: Build the runtime
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /App
COPY --from=build /app/publish .
EXPOSE 8085
ENTRYPOINT ["dotnet", "ApplicationWeb.dll"]




