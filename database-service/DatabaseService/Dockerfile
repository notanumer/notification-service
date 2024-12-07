FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DatabaseService/DatabaseService.csproj", "DatabaseService/"]
COPY ["DatabaseService.DataAccess/DatabaseService.DataAccess.csproj", "DatabaseService.DataAccess/"]
COPY ["DatabaseService.Models/DatabaseService.Models.csproj", "DatabaseService.Models/"]
COPY ["DatabaseService.Services/DatabaseService.Services.csproj", "DatabaseService.Services/"]
RUN dotnet restore "DatabaseService/DatabaseService.csproj"
COPY . .
WORKDIR "/src/DatabaseService"
RUN dotnet build "DatabaseService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DatabaseService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseService.dll"]
