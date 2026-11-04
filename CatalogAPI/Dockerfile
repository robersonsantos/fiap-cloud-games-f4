FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/FCG.Catalog.API/FCG.Catalog.API.csproj", "src/FCG.Catalog.API/"]
COPY ["src/FCG.Catalog.Application/FCG.Catalog.Application.csproj", "src/FCG.Catalog.Application/"]
COPY ["src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "src/FCG.Catalog.Domain/"]
COPY ["src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "src/FCG.Catalog.Infrastructure/"]
RUN dotnet restore "src/FCG.Catalog.API/FCG.Catalog.API.csproj"
COPY . .
WORKDIR "/src/src/FCG.Catalog.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
COPY --from=build /app/publish .
# Executa sem privilégios de root (usuário 'app' das imagens oficiais .NET 8+)
USER $APP_UID
ENTRYPOINT ["dotnet", "FCG.Catalog.API.dll"]
