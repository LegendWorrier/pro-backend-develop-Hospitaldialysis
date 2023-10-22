FROM docker:23-git

RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib \
    && cd /opt && wget https://dot.net/v1/dotnet-install.sh \
    && chmod +x ./dotnet-install.sh \
    && ./dotnet-install.sh --channel 6.0 --install-dir /usr/share/dotnet \
    && export PATH="$PATH:/usr/share/dotnet/" \
    && dotnet tool install --tool-path /opt/t4 dotnet-t4

ENTRYPOINT sh