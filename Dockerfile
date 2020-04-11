FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.3-buster-slim AS final
RUN echo deb http://archive.ubuntu.com/ubuntu precise universe multiverse >> /etc/apt/sources.list && \
  apt-get update && \
  apt-get -y install unrar
WORKDIR /app
COPY --from=build /app .
VOLUME [ "/source_dir", "/dest_dir" ]
ENTRYPOINT [ "dotnet", "run" ]