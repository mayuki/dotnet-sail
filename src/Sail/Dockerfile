# --------------------
# Build Stage
# --------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
RUN apt update && apt install -y --no-install-recommends \
		gcc \
		libc6-dev \
		zlib1g-dev

WORKDIR /work
COPY . .
RUN dotnet publish -o publish

# --------------------
# Final Stage
# --------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 as final

COPY --link --from=mcr.microsoft.com/dotnet/aspnet:6.0 /usr/share/dotnet/shared /usr/share/dotnet/shared
COPY --link --from=mcr.microsoft.com/dotnet/aspnet:7.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

#RUN apt update && apt install -y --no-install-recommends \
#		git \
#	&& apt-get clean \
#	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /work/publish/dotnet-sail /bin/dotnet-sail

#ENV DOTNET_ROLL_FORWARD=Major

ENTRYPOINT ["/bin/dotnet-sail"]