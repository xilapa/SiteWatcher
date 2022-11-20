FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL version="0.0.1" mantainer="xilapa"
WORKDIR /app

# Copy project files
COPY src/Worker ./src/Worker
COPY src/Application ./src/Application
COPY src/Domain ./src/Domain
COPY src/Infra ./src/Infra/

# Build and publish a release
RUN dotnet restore "src/Worker/Worker.csproj"
RUN dotnet publish "src/Worker/Worker.csproj" -c Release -o out --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8000/tcp
ENV DOTNET_URLS http://*:8000;
ENTRYPOINT ["dotnet", "Worker.dll"]
