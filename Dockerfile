# Use Alpine for smaller image size in base image
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER app
WORKDIR /app

# Use Alpine SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChannelsDVR Log Monitor.csproj", "."]
# Combine the restore with the copy
RUN dotnet restore "ChannelsDVR Log Monitor.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ChannelsDVR Log Monitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChannelsDVR Log Monitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Define volumes
VOLUME ["/app/logs", "/app/appsettings.json"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChannelsDVR Log Monitor.dll"]