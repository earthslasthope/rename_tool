FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.3-buster-slim AS final
WORKDIR /app
COPY --from=build /app .
VOLUME [ "/source_dir", "/dest_dir" ]
ENTRYPOINT [ "dotnet", "run" ]