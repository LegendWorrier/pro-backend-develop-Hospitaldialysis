#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.17 AS base
RUN echo http://dl-cdn.alpinelinux.org/alpine/v3.17/main >> /etc/apk/repositories \
  && echo http://dl-cdn.alpinelinux.org/alpine/v3.17/community >> /etc/apk/repositories

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --no-cache libgdiplus icu-libs icu-data-full \
# timezone
    && apk add --no-cache tzdata \
# ms fonts
    && apk add --no-cache msttcorefonts-installer fontconfig \
    && update-ms-fonts \
    && fc-cache -f \
# AWS s3fs
#    && apk add --no-cache s3fs-fuse \
# font for localize language
    && apk add --no-cache font-noto-thai \
    
    && rm -rf /var/lib/apt/lists/* \
    && fc-cache -f -v
    
ENV TZ Asia/Bangkok
ENV LANG=th_TH.UTF-8
ENV LANGUAGE=th_TH.UTF-8

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wasenshi.HemoDialysisPro.Report.Api/Wasenshi.HemoDialysisPro.Report.Api.csproj", "Wasenshi.HemoDialysisPro.Report.Api/"]
RUN dotnet restore "Wasenshi.HemoDialysisPro.Report.Api/Wasenshi.HemoDialysisPro.Report.Api.csproj"
COPY . .
WORKDIR "/src/Wasenshi.HemoDialysisPro.Report.Api"
RUN dotnet build "Wasenshi.HemoDialysisPro.Report.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wasenshi.HemoDialysisPro.Report.Api.csproj" -c Release -o /app/publish -r linux-musl-x64 --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wasenshi.HemoDialysisPro.Report.Api.dll"]