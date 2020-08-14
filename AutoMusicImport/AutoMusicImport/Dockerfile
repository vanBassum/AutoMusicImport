#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AutoMusicImport/AutoMusicImport.csproj", "AutoMusicImport/"]
COPY ["Lib/STDLib/STDLib.csproj", "Lib/STDLib/"]
RUN dotnet restore "AutoMusicImport/AutoMusicImport.csproj"
COPY . .
WORKDIR "/src/AutoMusicImport"
RUN dotnet build "AutoMusicImport.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoMusicImport.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoMusicImport.dll"]