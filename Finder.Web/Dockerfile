FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /home/app/.aspnet

USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Finder.Web/Finder.Web.csproj", "Finder.Web/"]
COPY ["Finder.Shared/Finder.Shared.csproj", "Finder.Shared/"]
COPY ["Finder.Db/Finder.Db.csproj", "Finder.Db/"]
RUN dotnet restore "Finder.Web/Finder.Web.csproj"
COPY . .
WORKDIR "/src/Finder.Web"
RUN dotnet build "./Finder.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Finder.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finder.Web.dll"]
