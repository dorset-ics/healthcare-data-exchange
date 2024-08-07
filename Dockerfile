﻿# v8.0
FROM mcr.microsoft.com/dotnet/aspnet@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 80

# v8.0
FROM mcr.microsoft.com/dotnet/sdk@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build
ARG BUILD_CONFIGURATION=Release
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/Api/Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# ENV ASPNETCORE_URLS=https://+:443;http://+:80 # uncomment when we have certs for HTTPS
# ENV ASPNETCORE_HTTPS_PORT=443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
