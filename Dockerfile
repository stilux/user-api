FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.11 AS build
WORKDIR /source
COPY *.sln .
COPY UserAPI/*.csproj ./UserAPI/
RUN dotnet restore -r linux-musl-x64

COPY UserAPI/. ./UserAPI/
WORKDIR /source/UserAPI
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine3.11
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["./UserAPI"]