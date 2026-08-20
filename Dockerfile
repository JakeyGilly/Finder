FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Finder.Bot/Finder.Bot.csproj", "Finder.Bot/"]
RUN dotnet restore "Finder.Bot/Finder.Bot.csproj"
COPY . .
WORKDIR "/src/Finder.Bot"
RUN dotnet build "./Finder.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Finder.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finder.Bot.dll"]
