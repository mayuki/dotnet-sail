# dotnet-install.sh pinned to a specific dotnet/install-scripts commit and
# protected with its expected SHA-256 checksum. Do not switch to the mutable
# https://dot.net/v1/dotnet-install.sh URL without integrity pinning.
ARG DOTNET_INSTALL_SCRIPT_COMMIT=13cdf4607a097b262a91722458e860ea2162c838
ARG DOTNET_INSTALL_SCRIPT_SHA256=082f7685e156738a1b2e2ed8381a621870d4ce8e8c59278034556f05c186eb2e

# --------------------
# Build Stage (Sail itself, Native AOT)
# --------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot AS build
RUN apt update && apt install -y --no-install-recommends \
		clang \
		zlib1g-dev

WORKDIR /work
COPY . .
RUN dotnet publish -o publish

# --------------------
# Common final base: .NET 10 SDK + Sail binary
# --------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS final-base

COPY --from=build /work/publish/dotnet-sail /bin/dotnet-sail

ENV DOTNET_ROLL_FORWARD=Major

ENTRYPOINT ["/bin/dotnet-sail"]

# --------------------
# net10 flavor: .NET 10 SDK and runtime only (no additional runtimes installed)
# --------------------
FROM final-base AS net10

# --------------------
# net8 flavor: .NET 10 SDK + .NET 8 ASP.NET Core runtime
# --------------------
FROM final-base AS net8
ARG DOTNET_INSTALL_SCRIPT_COMMIT
ARG DOTNET_INSTALL_SCRIPT_SHA256
RUN curl -fsSL -o /tmp/dotnet-install.sh "https://raw.githubusercontent.com/dotnet/install-scripts/${DOTNET_INSTALL_SCRIPT_COMMIT}/src/dotnet-install.sh" \
	&& echo "${DOTNET_INSTALL_SCRIPT_SHA256}  /tmp/dotnet-install.sh" | sha256sum -c - \
	&& chmod +x /tmp/dotnet-install.sh \
	&& /tmp/dotnet-install.sh --channel 8.0 --runtime aspnetcore --install-dir /usr/share/dotnet --skip-non-versioned-files \
	&& rm -f /tmp/dotnet-install.sh

# --------------------
# net9 flavor: .NET 10 SDK + .NET 9 ASP.NET Core runtime
# --------------------
FROM final-base AS net9
ARG DOTNET_INSTALL_SCRIPT_COMMIT
ARG DOTNET_INSTALL_SCRIPT_SHA256
RUN curl -fsSL -o /tmp/dotnet-install.sh "https://raw.githubusercontent.com/dotnet/install-scripts/${DOTNET_INSTALL_SCRIPT_COMMIT}/src/dotnet-install.sh" \
	&& echo "${DOTNET_INSTALL_SCRIPT_SHA256}  /tmp/dotnet-install.sh" | sha256sum -c - \
	&& chmod +x /tmp/dotnet-install.sh \
	&& /tmp/dotnet-install.sh --channel 9.0 --runtime aspnetcore --install-dir /usr/share/dotnet --skip-non-versioned-files \
	&& rm -f /tmp/dotnet-install.sh

# --------------------
# all flavor: net9 (SDK 10 + runtime 9) plus .NET 8 ASP.NET Core runtime
# --------------------
FROM net9 AS all
ARG DOTNET_INSTALL_SCRIPT_COMMIT
ARG DOTNET_INSTALL_SCRIPT_SHA256
RUN curl -fsSL -o /tmp/dotnet-install.sh "https://raw.githubusercontent.com/dotnet/install-scripts/${DOTNET_INSTALL_SCRIPT_COMMIT}/src/dotnet-install.sh" \
	&& echo "${DOTNET_INSTALL_SCRIPT_SHA256}  /tmp/dotnet-install.sh" | sha256sum -c - \
	&& chmod +x /tmp/dotnet-install.sh \
	&& /tmp/dotnet-install.sh --channel 8.0 --runtime aspnetcore --install-dir /usr/share/dotnet --skip-non-versioned-files \
	&& rm -f /tmp/dotnet-install.sh