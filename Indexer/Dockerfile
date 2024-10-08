FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Indexer/Indexer.csproj", "Indexer/"]
RUN dotnet restore "Indexer/Indexer.csproj"
COPY . .
WORKDIR "/src/Indexer"
RUN dotnet build "Indexer.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "Indexer.dll"]
