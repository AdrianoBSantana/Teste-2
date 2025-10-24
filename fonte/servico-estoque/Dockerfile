# ServicoEstoque Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./plataforma-ecommerce.sln ./
COPY ./fonte ./fonte

RUN dotnet restore ./fonte/servico-estoque/ServicoEstoque.csproj
RUN dotnet build -c Release ./fonte/servico-estoque/ServicoEstoque.csproj -o /app/build
RUN dotnet publish -c Release ./fonte/servico-estoque/ServicoEstoque.csproj -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
# Healthcheck tooling
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","ServicoEstoque.dll"]
