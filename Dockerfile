# v8.0
FROM mcr.microsoft.com/dotnet/aspnet@sha256:7882d3eee2527584cc9db7b2f26540c27dba4462ec565b8cb160a44874a65b97 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 80

# v8.0
FROM mcr.microsoft.com/dotnet/sdk@sha256:02fdc848bbda5d57d9211a72c99bd665b421206002d66b8bc2cc0b2297c227fa AS build
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
