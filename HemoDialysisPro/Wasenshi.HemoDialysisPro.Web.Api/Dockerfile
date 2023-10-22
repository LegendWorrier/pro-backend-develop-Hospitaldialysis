#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.17 AS base
RUN echo http://dl-cdn.alpinelinux.org/alpine/v3.17/main >> /etc/apk/repositories \
  && echo http://dl-cdn.alpinelinux.org/alpine/v3.17/community >> /etc/apk/repositories

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --no-cache libgdiplus icu-libs \
# timezone
    && apk add --no-cache tzdata \
# ms fonts
    && apk add --no-cache msttcorefonts-installer fontconfig \
    && update-ms-fonts \
    && fc-cache -f \
# AWS s3fs
#    && apk add --no-cache s3fs-fuse \
# Clean
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wasenshi.HemoDialysisPro.Web.Api/Wasenshi.HemoDialysisPro.Web.Api.csproj", "Wasenshi.HemoDialysisPro.Web.Api/"]
RUN dotnet restore "Wasenshi.HemoDialysisPro.Web.Api/Wasenshi.HemoDialysisPro.Web.Api.csproj"
COPY . .
WORKDIR "/src/Wasenshi.HemoDialysisPro.Web.Api"
RUN dotnet build "Wasenshi.HemoDialysisPro.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wasenshi.HemoDialysisPro.Web.Api.csproj" -c Release -o /app/publish -r linux-musl-x64 --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Wasenshi.HemoDialysisPro.Web.Api.dll"]
