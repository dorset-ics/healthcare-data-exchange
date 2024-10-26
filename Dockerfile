# v8.0
FROM mcr.microsoft.com/dotnet/aspnet@sha256:3ded9ccf06f222ec934311be4f9facda83d144331c028340e3a694733cad7d4b AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 80

# v8.0
FROM mcr.microsoft.com/dotnet/sdk@sha256:cab0284cce7bc26d41055d0ac5859a69a8b75d9a201cd226999f4f00cc983f13 AS build
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
