FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/FCG.Notifications.Worker/FCG.Notifications.Worker.csproj", "src/FCG.Notifications.Worker/"]
COPY ["src/FCG.Notifications.Application/FCG.Notifications.Application.csproj", "src/FCG.Notifications.Application/"]
RUN dotnet restore "src/FCG.Notifications.Worker/FCG.Notifications.Worker.csproj"

COPY . .
WORKDIR "/src/src/FCG.Notifications.Worker"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Executa sem privilégios de root (usuário 'app' das imagens oficiais .NET 8+)
USER $APP_UID
ENTRYPOINT ["dotnet", "FCG.Notifications.Worker.dll"]
