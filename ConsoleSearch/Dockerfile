# Base image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ConsoleSearch.csproj ./ConsoleSearch/
RUN dotnet restore "ConsoleSearch/ConsoleSearch.csproj"
COPY . .
WORKDIR "/src/ConsoleSearch"
RUN dotnet build "ConsoleSearch.csproj" -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "ConsoleSearch.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleSearch.dll"]
