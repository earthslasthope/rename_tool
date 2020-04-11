FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app

FROM build AS tools
RUN apt-get update && \
  apt-get -y install software-properties-common && \
  add-apt-repository non-free && \
  apt-get update && \
  apt-get -y install unrar

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.3-buster-slim AS final
WORKDIR /app
COPY --from=build /app .
COPY --from=tools /usr/bin/unrar /usr/bin/unrar
COPY --from=tools /usr/bin/unrar-nonfree /usr/bin/unrar-nonfree
VOLUME [ "/source_dir", "/dest_dir" ]
ENTRYPOINT [ "dotnet", "rename_tool.dll" ]