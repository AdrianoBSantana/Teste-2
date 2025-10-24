# Plataforma de E-commerce

Este projeto é uma plataforma de e-commerce desenvolvida em .NET 8, utilizando uma arquitetura de micro-serviços. A plataforma é composta por um gateway de API, um serviço de estoque e um serviço de vendas, que se comunicam de forma síncrona e assíncrona.

## Arquitetura

A plataforma é composta pelos seguintes serviços:

*   **API Gateway:** Ponto de entrada único para todas as requisições da plataforma. É responsável por rotear as requisições para os serviços corretos, além de lidar com a autenticação e autorização.
*   **Serviço de Estoque:** Responsável por gerenciar o estoque de produtos.
*   **Serviço de Vendas:** Responsável por gerenciar as vendas de produtos.
*   **Compartilhado:** Projeto que contém código compartilhado entre os serviços, como as configurações de JWT e os eventos de domínio.
*   **Testes:** Projeto que contém os testes unitários e de integração para os serviços.

A comunicação entre os serviços é feita da seguinte forma:

*   **Síncrona:** O serviço de vendas se comunica com o serviço de estoque via HTTP para verificar a disponibilidade de produtos.
*   **Assíncrona:** O serviço de vendas publica um evento de "Venda Confirmada" em uma fila do RabbitMQ. O serviço de estoque consome este evento para baixar a quantidade de produtos do estoque.

## Tecnologias Utilizadas

*   **.NET 8:** Framework para desenvolvimento de aplicações web.
*   **ASP.NET Core:** Framework para desenvolvimento de aplicações web em .NET.
*   **Entity Framework Core:** ORM para acesso a dados.
*   **SQLite:** Banco de dados relacional embarcado.
*   **RabbitMQ:** Message broker para comunicação assíncrona.
*   **YARP (Yet Another Reverse Proxy):** Proxy reverso para o API Gateway.
*   **JWT (JSON Web Tokens):** Padrão para autenticação e autorização.
*   **Polly (Microsoft.Extensions.Http.Polly):** Resiliência de HttpClient (retry exponencial + circuit breaker) entre serviços.
*   **xUnit:** Framework para testes unitários.
*   **Moq:** Framework para mock de objetos em testes.

## Como Configurar e Executar o Projeto

### Pré-requisitos

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Docker](https://www.docker.com/products/docker-desktop) (para executar o RabbitMQ)

### 1. Executar o RabbitMQ

Execute o RabbitMQ em um contêiner Docker:

```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
```

### 2. Configurar as Conexões

Cada serviço (Estoque e Vendas) utiliza um banco de dados SQLite. Os arquivos de banco de dados (`estoque.db` e `vendas.db`) já estão na raiz de cada projeto de serviço.

As configurações de conexão com o RabbitMQ e as URLs dos serviços estão nos arquivos `appsettings.json` de cada projeto. Por padrão, os serviços estão configurados para serem executados nas seguintes portas:

*   **API Gateway:** `http://localhost:5154`
*   **Serviço de Estoque:** `http://localhost:5290`
*   **Serviço de Vendas:** `http://localhost:5156`

Observação: as migrações do EF Core são aplicadas automaticamente na inicialização de cada serviço (`Database.Migrate()`). Não é necessário executar comandos de migração manualmente para criar as tabelas.

## Como Configurar e Executar o Projeto

Abra um terminal para cada serviço e execute o seguinte comando:

**API Gateway:**

```bash
Token (DEV):

## Executando com Docker Compose (opcional)
### Guia de Execução Manual Passo a Passo

As instruções abaixo assumem Windows PowerShell. Para bash, ajuste a sintaxe de exportação de variáveis e comandos `curl` conforme necessário.

#### 1. Preparação do ambiente

```powershell
# (Opcional) clonar o repositório
git clone <url-do-repo>
cd plataforma-ecommerce

# Restaurar dependências
dotnet restore .\plataforma-ecommerce.sln
```

Os projetos usam bancos SQLite locais (`estoque.db` e `vendas.db`) criados automaticamente na inicialização. Não há dependências externas além do RabbitMQ.

#### 2. Subir o RabbitMQ

```powershell
docker run -d `
  --hostname rabbitmq-ecom `
  --name rabbitmq-ecom `
  -p 5672:5672 `
  -p 15672:15672 `
  rabbitmq:3-management
```

*Painel de administração:* http://localhost:15672 (usuário: `guest`, senha: `guest`).

Para reiniciar posteriormente:

```powershell
docker stop rabbitmq-ecom
docker start rabbitmq-ecom
```

#### 3. Configurações de cada serviço

* As strings de conexão e credenciais do RabbitMQ estão em `appsettings.Development.json`.
* Portas padrão em desenvolvimento:
  * API Gateway: http://localhost:5154
  * Serviço de Estoque: http://localhost:5290
  * Serviço de Vendas: http://localhost:5156
* As migrações do EF Core são aplicadas automaticamente (`Database.Migrate()`), portanto nenhum passo extra é necessário para provisionar os bancos.

#### 4. Executar os microserviços manualmente

Abra três janelas do PowerShell e execute os comandos abaixo (um por janela). Mantenha cada processo em execução.

```powershell
# Janela 1 - API Gateway
cd "c:\Users\...\plataforma-ecommerce\fonte\api-gateway"

```

```powershell
# Janela 2 - Serviço de Estoque
cd "c:\Users\...\plataforma-ecommerce\fonte\servico-estoque"
- Usamos `ASPNETCORE_ENVIRONMENT=Docker` e os arquivos `appsettings.Docker.json` para configurar os destinos internos (serviços e RabbitMQ) via rede do Compose.
```

```powershell
# Janela 3 - Serviço de Vendas
cd "c:\Users\...\plataforma-ecommerce\fonte\servico-vendas"
- O `docker-compose.yml` possui healthchecks nos serviços e no RabbitMQ, e usa `depends_on` com `condition: service_healthy` para orquestrar a ordem de subida.
```

> Dica: defina `ASPNETCORE_ENVIRONMENT=Development` (padrão) caso deseje logs mais verbosos.

#### 5. Verificar se todos os serviços estão saudáveis

Em uma nova janela, verifique os endpoints de saúde (não requer token):

```powershell
curl http://localhost:5154/healthz
curl http://localhost:5290/healthz
curl http://localhost:5156/healthz
```

Cada chamada deve retornar `{"status":"ok"}`.

#### 6. Fluxo manual completo via API Gateway

Todas as chamadas abaixo devem incluir o token JWT emitido pelo gateway.

1. **Gerar token:**

	```powershell
	$loginBody = @{ Usuario = 'admin'; Senha = 'admin' } | ConvertTo-Json -Compress
	$token = (Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/autenticacao/token' -ContentType 'application/json' -Body $loginBody).token
	$headers = @{ Authorization = "Bearer $token" }
	$token
	```

2. **Cadastrar produto no serviço de estoque:**

	```powershell
	$produtoBody = @{ nome = 'Camiseta Azul'; descricao = '100% algodao'; preco = 59.9; quantidadeEmEstoque = 10 } | ConvertTo-Json -Compress
	$produto = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/estoque/produtos' -Headers $headers -ContentType 'application/json' -Body $produtoBody
	$produto
	```

3. **Consultar catálogo de produtos:**

	```powershell
	Invoke-RestMethod -Method Get -Uri 'http://localhost:5154/estoque/produtos' -Headers $headers
	```

4. **Criar pedido de venda validando estoque:**

	```powershell
	$pedidoBody = @{ itens = @(@{ produtoId = $produto.id; quantidade = 2 }) } | ConvertTo-Json -Compress
	$pedido = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/vendas/pedidos' -Headers $headers -ContentType 'application/json' -Body $pedidoBody
	$pedido
	```

5. **Consultar pedido criado:**

	```powershell
	Invoke-RestMethod -Method Get -Uri ('http://localhost:5154/vendas/pedidos/' + $pedido.id) -Headers $headers
	```

6. **Confirmar baixa de estoque causada pelo evento RabbitMQ:**

	```powershell
	Invoke-RestMethod -Method Get -Uri ('http://localhost:5154/estoque/produtos/' + $produto.id) -Headers $headers
	```

	A quantidade em estoque deve ser reduzida (ex.: de 10 para 8 após o pedido).

#### 7. Logs e monitoramento

* **RabbitMQ:** painel em http://localhost:15672 permite acompanhar filas e mensagens.
* **Aplicações .NET:** os logs padrão são enviados ao console; utilize `dotnet run --verbosity detailed` para mais informações.
* **Health checks:** `/healthz` em cada serviço (gateway incluso) podem ser monitorados por ferramentas externas.

#### 8. Encerrar o ambiente manual

1. Encerrar cada serviço com `Ctrl + C` nos terminais onde `dotnet run` está ativo.
2. Parar o RabbitMQ:

	```powershell
	docker stop rabbitmq-ecom
	docker rm rabbitmq-ecom   # opcional, remove o contêiner
	```

3. Apagar bancos locais (opcional): delete `estoque.db` e `vendas.db` nas pastas dos serviços para começar do zero.
- Dentro dos containers, o Serviço de Vendas chama o Serviço de Estoque diretamente via DNS do Compose (`http://servico-estoque:8080/`) para maior estabilidade, enquanto clientes externos usam o API Gateway.
- O script `scripts/run-e2e.ps1` aguarda os `/healthz` ficarem OK com polling, cria token/produto/pedido, usa retry com backoff exponencial para criar o pedido e verifica a baixa de estoque através de polling do produto.