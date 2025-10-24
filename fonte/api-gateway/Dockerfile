# ApiGateway Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solução e fontes
COPY ./plataforma-ecommerce.sln ./
COPY ./fonte ./fonte

# Restore apenas do projeto do gateway (restaura árvore inteira por referências)
RUN dotnet restore ./fonte/api-gateway/ApiGateway.csproj

# Build
RUN dotnet build -c Release ./fonte/api-gateway/ApiGateway.csproj -o /app/build

# Publish
RUN dotnet publish -c Release ./fonte/api-gateway/ApiGateway.csproj -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
# Healthcheck tooling
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","ApiGateway.dll"]
