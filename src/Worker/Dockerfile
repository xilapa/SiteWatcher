FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
LABEL version="0.0.1" mantainer="xilapa"
WORKDIR /app

# install cultures on alpine
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZTION_INVARIANT=false

# Copy project files
COPY src/Worker ./src/Worker
COPY src/Application ./src/Application
COPY src/Domain ./src/Domain
COPY src/Infra ./src/Infra/
COPY Directory.Packages.props .

#https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/#pgo
# ENV DOTNET_ReadyToRun=0

# Build and publish a release
RUN dotnet restore "src/Worker/Worker.csproj"
RUN dotnet publish "src/Worker/Worker.csproj" -c Release -o out --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8000/tcp
ENV DOTNET_URLS http://*:8000;
ENTRYPOINT ["dotnet", "Worker.dll"]
