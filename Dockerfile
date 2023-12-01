FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
LABEL version="0.0.1" mantainer="xilapa"
WORKDIR /app

# install cultures on alpine
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZTION_INVARIANT=false

# Copy project files
COPY src/Application ./src/Application
COPY src/Domain ./src/Domain
COPY src/Infra ./src/Infra/
COPY src/WebAPI ./src/WebAPI/
COPY Directory.Packages.props .
#COPY test/Benchmark ./test/Benchmark
#COPY test/UnitTests ./test/UnitTests
#COPY test/IntegrationTests ./test/IntegrationTests
#COPY *.sln .

#https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/#pgo
# ENV DOTNET_ReadyToRun=0

RUN dotnet restore "src/WebAPI/WebAPI.csproj"

# Build and publish a release
RUN dotnet publish "src/WebAPI/WebAPI.csproj" -c Release -o out --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .

ARG PORT
EXPOSE $PORT/tcp
ENV ASPNETCORE_URLS http://*:$PORT;
ENTRYPOINT ["dotnet", "WebAPI.dll"]
