# v8.0
FROM mcr.microsoft.com/dotnet/aspnet@sha256:30d8619d9a4f68508d9b17fc2088e857e629d3f9ceaaf57c22d6747f7326d89e AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 80

# v8.0
FROM mcr.microsoft.com/dotnet/sdk@sha256:1e8c6666f3e31f96d22a5c77054589f85078a4850595421c6999395f26e79b9a AS build
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
