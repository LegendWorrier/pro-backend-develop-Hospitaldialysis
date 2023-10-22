#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.16 AS base
# timezone
RUN apk add --no-cache tzdata icu-libs
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wasenshi.HemoDialysisPro.JobsServer/Wasenshi.HemoDialysisPro.JobsServer.csproj", "Wasenshi.HemoDialysisPro.JobsServer/"]
RUN dotnet restore "Wasenshi.HemoDialysisPro.JobsServer/Wasenshi.HemoDialysisPro.JobsServer.csproj"
COPY . .
WORKDIR "/src/Wasenshi.HemoDialysisPro.JobsServer"
RUN dotnet build "Wasenshi.HemoDialysisPro.JobsServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wasenshi.HemoDialysisPro.JobsServer.csproj" -c Release -o /app/publish -r linux-musl-x64 --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "Wasenshi.HemoDialysisPro.JobsServer.dll"]