FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL version="0.0.1" mantainer="xilapa"
WORKDIR /app

# Copy project files
COPY src/Core/SiteWatcher.Application ./src/Core/SiteWatcher.Application
COPY src/Core/SiteWatcher.Domain ./src/Core/SiteWatcher.Domain
COPY src/Infra/SiteWatcher.Data ./src/Infra/SiteWatcher.Data
COPY src/WebAPI/SiteWatcher.WebAPI ./src/WebAPI/SiteWatcher.WebAPI
COPY *.sln .
RUN dotnet restore

# Build and publish a release
RUN dotnet publish -c Release -o out --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Entrypoint do heroku
CMD ASPNETCORE_URLS=http://*:$PORT dotnet SiteWatcher.WebAPI.dll

# ENV ASPNETCORE_URLS http://*:7126
# ENTRYPOINT ["dotnet", "SiteWatcher.WebAPI.dll"]