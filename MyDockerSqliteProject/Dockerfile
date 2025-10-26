FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1
COPY . .
RUN dotnet --info \
 && dotnet restore "MyDockerSqliteProject.csproj" -v minimal \
 && dotnet build "MyDockerSqliteProject.csproj" -c $BUILD_CONFIGURATION -v minimal --no-restore

FROM build AS publish
WORKDIR /src
RUN dotnet publish "MyDockerSqliteProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -v minimal --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyDockerSqliteProject.dll"]