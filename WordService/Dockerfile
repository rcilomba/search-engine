FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY WordService.csproj ./WordService/

RUN dotnet restore "./WordService/WordService.csproj"
COPY . . 
WORKDIR "/src/WordService"
RUN dotnet build "./WordService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./WordService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WordService.dll"]
