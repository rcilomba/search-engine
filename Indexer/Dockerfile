# Base image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Indexer/Indexer.csproj", "Indexer/"]
RUN dotnet restore "Indexer/Indexer.csproj"
COPY . .
WORKDIR "/src/Indexer"
RUN dotnet build "Indexer.csproj" -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "Indexer.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Indexer/enron enron
ENTRYPOINT ["dotnet", "Indexer.dll"]
