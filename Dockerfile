FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL version="0.0.1" mantainer="xilapa"
WORKDIR /app

# Copy project files
COPY src/Application ./src/Application
COPY src/Domain ./src/Domain
COPY src/Infra ./src/Infra/
COPY src/WebAPI ./src/WebAPI/
#COPY test/Benchmark ./test/Benchmark
#COPY test/UnitTests ./test/UnitTests
#COPY test/IntegrationTests ./test/IntegrationTests
#COPY *.sln .
WORKDIR app/src/WebAPI
RUN dotnet restore "WebAPI.csproj"

# Build and publish a release
RUN dotnet publish "WebAPI.csproj" -c Release -o out --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Heroku entrypoint
CMD ASPNETCORE_URLS=http://*:$PORT dotnet WebAPI.dll

# ENV ASPNETCORE_URLS http://*:7126
# ENTRYPOINT ["dotnet", "SiteWatcher.WebAPI.dll"]
