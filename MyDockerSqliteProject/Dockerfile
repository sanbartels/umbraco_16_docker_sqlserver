FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyDockerSqliteProject/MyDockerSqliteProject.csproj", "MyDockerSqliteProject/"]
RUN dotnet restore "MyDockerSqliteProject/MyDockerSqliteProject.csproj"
COPY . .
WORKDIR "/src/MyDockerSqliteProject"
RUN dotnet build "MyDockerSqliteProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "MyDockerSqliteProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyDockerSqliteProject.dll"]