FROM mcr.microsoft.com/dotnet/aspnet@sha256:516d18718444d5af9cd5a55dc10095c55bfcf96606a15e3d03476d4e67d9139a AS base@sha256:516d18718444d5af9cd5a55dc10095c55bfcf96606a15e3d03476d4e67d9139a # v8.0
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk@sha256:935902ef9eee58a9226b906e3d6ff1b2abaca240c9d5b4ac8dca9943b26c8f33 AS build # v8.0
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

FROM build@sha256:516d18718444d5af9cd5a55dc10095c55bfcf96606a15e3d03476d4e67d9139a AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base@sha256:516d18718444d5af9cd5a55dc10095c55bfcf96606a15e3d03476d4e67d9139a AS final
WORKDIR /app
# ENV ASPNETCORE_URLS=https://+:443;http://+:80 # uncomment when we have certs for HTTPS
# ENV ASPNETCORE_HTTPS_PORT=443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
