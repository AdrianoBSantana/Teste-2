# 🚀 Plataforma de E-commerce

Bem-vindo ao projeto da Plataforma de E-commerce! Este é um protótipo funcional que simula os bastidores de uma loja online, demonstrando uma arquitetura de micro-serviços moderna e robusta.

## 🎯 O que é este projeto?

Este projeto simula o fluxo completo de uma venda online:
1.  **Cadastro de produtos**: Adicionar itens ao catálogo.
2.  **Consulta de estoque**: Verificar a disponibilidade de um produto.
3.  **Realização de uma venda**: Criar um pedido de compra.
4.  **Atualização de estoque**: Dar baixa automática nos itens vendidos.

O sistema foi construído para ser um exemplo prático de uma arquitetura distribuída, onde diferentes módulos (micro-serviços) colaboram para realizar uma tarefa complexa.

## 🏗️ Visão Geral da Arquitetura

Para facilitar o entendimento, imagine uma loja física com diferentes departamentos:

*   **API Gateway (O Recepcionista 🤵):** É a porta de entrada do sistema. Todas as solicitações (como consultar um produto ou fazer um pedido) chegam primeiro a ele, que as direciona para o departamento correto. Ele também atua como segurança, validando quem pode entrar.

*   **Serviço de Estoque (O Almoxarifado 📦):** Este departamento controla o inventário. Ele é responsável por adicionar novos produtos e dar baixa nos itens que foram vendidos.

*   **Serviço de Vendas (O Caixa 🛒):** É onde os pedidos de compra são processados.

### Como os departamentos se comunicam:

A comunicação entre eles acontece de duas formas inteligentes:

1.  **Comunicação Direta (Síncrona 📞):** Quando um cliente quer comprar um item, o **Caixa** (Serviço de Vendas) liga para o **Almoxarifado** (Serviço de Estoque) para verificar se o produto está disponível. A venda só prossegue se o almoxarifado confirmar.

2.  **Comunicação por Memorando (Assíncrona 📝):** Após a venda ser confirmada, o **Caixa** envia um memorando (um evento `VendaConfirmada` via RabbitMQ) para o **Almoxarifado**, informando que o produto foi vendido. Ao receber este memorando, o almoxarifado atualiza seus registros e retira o item do inventário.

---

## 🛠️ Guia para Desenvolvedores

Esta seção contém as informações técnicas para configurar, executar e testar o projeto.

### ✨ Tecnologias Utilizadas
<div align="center">
  <img src="https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-8-512BD4?style=for-the-badge&logo=dotnet" alt="ASP.NET Core 8">
  <img src="https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=dotnet" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/SQLite-3-003B57?style=for-the-badge&logo=sqlite" alt="SQLite">
  <img src="https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq" alt="RabbitMQ">
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker" alt="Docker">
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens" alt="JWT">
  <img src="https://img.shields.io/badge/Polly-resilience-green?style=for-the-badge" alt="Polly">
  <img src="https://img.shields.io/badge/xUnit-tests-blue?style=for-the-badge" alt="xUnit">
  <img src="https://img.shields.io/badge/YARP-gateway-purple?style=for-the-badge" alt="YARP">
</div>

*   **.NET 8 / ASP.NET Core:** Framework para desenvolvimento da aplicação.
*   **Entity Framework Core:** ORM para acesso aos bancos de dados.
*   **SQLite:** Banco de dados relacional leve, utilizado em cada serviço.
*   **YARP (Yet Another Reverse Proxy):** Utilizado para implementar o API Gateway de forma eficiente.
*   **RabbitMQ:** Message broker para a comunicação assíncrona (por "memorandos").
*   **JWT (JSON Web Tokens):** Padrão de mercado para autenticação e autorização segura.
*   **Polly:** Biblioteca para resiliência, garantindo que a comunicação entre serviços se recupere de falhas temporárias (ex: tentativas automáticas e circuit breaker).
*   **xUnit & Moq:** Ferramentas para a criação de testes automatizados.
*   **Docker:** Utilizado para executar o RabbitMQ em um contêiner isolado.

### ⚙️ Como Configurar e Executar o Projeto

#### Pré-requisitos

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)

#### 1. Iniciar o RabbitMQ com Docker

Execute o comando abaixo para iniciar o RabbitMQ. O painel de administração estará disponível em `http://localhost:15672` (usuário: `guest`, senha: `guest`).

```powershell
docker run -d --hostname rabbitmq-ecom --name rabbitmq-ecom -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

#### 2. Restaurar Dependências do Projeto

Navegue até a raiz do projeto e execute:

```powershell
dotnet restore .\plataforma-ecommerce.sln
```

#### 3. Executar os Micro-serviços

Abra **três terminais separados**, um para cada serviço, e execute os comandos abaixo. Mantenha os processos em execução.

*   **Terminal 1 - API Gateway:**
    ```powershell
    cd .\fonte\api-gateway
    dotnet run
    ```
*   **Terminal 2 - Serviço de Estoque:**
    ```powershell
    cd .\fonte\servico-estoque
    dotnet run
    ```
*   **Terminal 3 - Serviço de Vendas:**
    ```powershell
    cd .\fonte\servico-vendas
    dotnet run
    ```

**Observações Importantes:**
*   Os bancos de dados SQLite (`estoque.db` e `vendas.db`) são criados e as migrações são aplicadas automaticamente na inicialização de cada serviço.
*   As portas padrão são:
    *   API Gateway: `http://localhost:5154`
    *   Serviço de Estoque: `http://localhost:5290`
    *   Serviço de Vendas: `http://localhost:5156`

---

### 🧪 Testando o Projeto

Existem duas formas principais de testar o fluxo completo: via **PowerShell** (linha de comando) ou **Swagger** (interface gráfica).

#### Opção 1: Testando via PowerShell

1.  **Gerar Token de Autenticação:**
    ```powershell
    $loginBody = @{ Usuario = 'admin'; Senha = 'admin' } | ConvertTo-Json -Compress
    $token = (Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/autenticacao/token' -ContentType 'application/json' -Body $loginBody).token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "Token gerado com sucesso!"
    ```

2.  **Cadastrar um Novo Produto:**
    ```powershell
    $produtoBody = @{ nome = 'Camiseta Azul'; descricao = '100% algodao'; preco = 59.9; quantidadeEmEstoque = 10 } | ConvertTo-Json -Compress
    $produto = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/estoque/produtos' -Headers $headers -ContentType 'application/json' -Body $produtoBody
    $produto
    ```

3.  **Criar um Pedido de Venda:**
    ```powershell
    $pedidoBody = @{ itens = @(@{ produtoId = $produto.id; quantidade = 2 }) } | ConvertTo-Json -Compress
    $pedido = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/vendas/pedidos' -Headers $headers -ContentType 'application/json' -Body $pedidoBody
    $pedido
    ```

4.  **Verificar a Baixa de Estoque:**
    Aguarde alguns segundos para o "memorando" ser processado e verifique o produto novamente.
    ```powershell
    Invoke-RestMethod -Method Get -Uri "http://localhost:5154/estoque/produtos/$($produto.id)" -Headers $headers
    ```
    *(A `quantidadeEmEstoque` deve ter mudado de 10 para 8).*

#### Opção 2: Testando via Swagger (Interface Gráfica)

O Swagger fornece uma interface visual e interativa para testar os endpoints.

**URLs dos Swaggers:**
- **API Gateway**: http://localhost:5154/swagger (recomendado para o fluxo completo)
- **Serviço de Estoque**: http://localhost:5290/swagger
- **Serviço de Vendas**: http://localhost:5156/swagger

**Passo a Passo no Swagger do API Gateway:**

1. **Gerar Token de Autenticação:**
   - Expanda `POST /autenticacao/token`.
   - Clique em "Try it out".
   - Corpo da requisição:
     ```json
     {
       "usuario": "admin",
       "senha": "admin"
     }
     ```
   - Execute e copie o `token` retornado.
   - No topo da página, clique em "Authorize" e insira `Bearer <token>` para autenticar as próximas requisições.

2. **Cadastrar um Novo Produto:**
   - Expanda `POST /estoque/produtos`.
   - Clique em "Try it out".
   - Corpo da requisição (exemplo):
     ```json
     {
       "nome": "Camiseta Azul",
       "descricao": "100% algodão",
       "preco": 59.90,
       "quantidadeEmEstoque": 10
     }
     ```
   - Execute e copie o `id` do produto criado.

3. **Criar um Pedido de Venda:**
   - Expanda `POST /vendas/pedidos`.
   - Clique em "Try it out".
   - Corpo da requisição (use o ID do produto):
     ```json
     {
       "itens": [
         {
           "produtoId": "<ID_DO_PRODUTO>",
           "quantidade": 2
         }
       ]
     }
     ```
   - Execute. O pedido será criado e o estoque será baixado automaticamente via evento.

4. **Verificar a Baixa de Estoque:**
   - Expanda `GET /estoque/produtos/{id}`.
   - Insira o ID do produto e execute.
   - Verifique se `quantidadeEmEstoque` diminuiu (ex.: 10 → 8).

---

### 🐳 Executando com Docker Compose (Alternativa)

Para uma experiência mais automatizada, o `docker-compose.yml` orquestra a subida de todos os serviços.

*   O `docker-compose.yml` possui healthchecks e `depends_on` para garantir que os serviços subam na ordem correta.
*   Dentro dos contêineres, os serviços se comunicam usando a rede interna do Docker (ex: `http://servico-estoque:8080`), enquanto o acesso externo continua via API Gateway.

### 🛑 Encerrando o Ambiente

1.  Pressione `Ctrl + C` em cada terminal onde os serviços estão rodando.
2.  Para parar e remover o contêiner do RabbitMQ:
    ```powershell
    docker stop rabbitmq-ecom
    docker rm rabbitmq-ecom
    ```
````
    ```

`````

---

## Guia para Desenvolvedores

Esta seção contém as informações técnicas para configurar, executar e entender a implementação do projeto.

### Tecnologias Utilizadas

*   **.NET 8 / ASP.NET Core:** Framework para desenvolvimento da aplicação.
*   **Entity Framework Core:** ORM para acesso aos bancos de dados.
*   **SQLite:** Banco de dados relacional utilizado em cada serviço.
*   **YARP (Yet Another Reverse Proxy):** Utilizado para implementar o API Gateway.
*   **RabbitMQ:** Message broker para a comunicação assíncrona (por "memorandos").
*   **JWT (JSON Web Tokens):** Padrão de mercado para autenticação e autorização.
*   **Polly:** Biblioteca para resiliência, garantindo que a comunicação entre serviços possa se recuperar de falhas temporárias (ex: tentativas automáticas e circuit breaker).
*   **xUnit & Moq:** Ferramentas para a criação de testes automatizados.
*   **Docker:** Utilizado para executar o RabbitMQ em um contêiner isolado.

### Como Configurar e Executar o Projeto

#### Pré-requisitos

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)

#### 1. Iniciar o RabbitMQ com Docker

Execute o comando abaixo para iniciar o RabbitMQ. O painel de administração estará disponível em `http://localhost:15672` (usuário: `guest`, senha: `guest`).

```powershell
docker run -d --hostname rabbitmq-ecom --name rabbitmq-ecom -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

#### 2. Restaurar Dependências do Projeto

Navegue até a raiz do projeto e execute:

```powershell
dotnet restore .\plataforma-ecommerce.sln
```

#### 3. Executar os Micro-serviços

Abra **três terminais separados**, um para cada serviço, e execute os comandos abaixo. Mantenha os processos em execução.

*   **Terminal 1 - API Gateway:**
    ```powershell
    cd .\fonte\api-gateway
    dotnet run
    ```
*   **Terminal 2 - Serviço de Estoque:**
    ```powershell
    cd .\fonte\servico-estoque
    dotnet run
    ```
*   **Terminal 3 - Serviço de Vendas:**
    ```powershell
    cd .\fonte\servico-vendas
    dotnet run
    ```

**Observações Importantes:**
*   Os bancos de dados SQLite (`estoque.db` e `vendas.db`) são criados e as migrações são aplicadas automaticamente na inicialização de cada serviço.
*   As portas padrão são:
    *   API Gateway: `http://localhost:5154`
    *   Serviço de Estoque: `http://localhost:5290`
    *   Serviço de Vendas: `http://localhost:5156`

#### 4. Testando o Fluxo Completo via PowerShell

O guia abaixo mostra como simular uma interação completa com a plataforma.

1.  **Gerar Token de Autenticação:**
    ```powershell
    $loginBody = @{ Usuario = 'admin'; Senha = 'admin' } | ConvertTo-Json -Compress
    $token = (Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/autenticacao/token' -ContentType 'application/json' -Body $loginBody).token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "Token gerado: $token"
    ```

2.  **Cadastrar um Novo Produto:**
    ```powershell
    $produtoBody = @{ nome = 'Camiseta Azul'; descricao = '100% algodao'; preco = 59.9; quantidadeEmEstoque = 10 } | ConvertTo-Json -Compress
    $produto = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/estoque/produtos' -Headers $headers -ContentType 'application/json' -Body $produtoBody
    $produto
    ```

3.  **Criar um Pedido de Venda:**
    ```powershell
    $pedidoBody = @{ itens = @(@{ produtoId = $produto.id; quantidade = 2 }) } | ConvertTo-Json -Compress
    $pedido = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/vendas/pedidos' -Headers $headers -ContentType 'application/json' -Body $pedidoBody
    $pedido
    ```

4.  **Verificar a Baixa de Estoque:**
    Aguarde alguns segundos para o "memorando" ser processado e verifique o produto novamente. A quantidade em estoque deverá ter sido reduzida.
    ```powershell
    Invoke-RestMethod -Method Get -Uri "http://localhost:5154/estoque/produtos/$($produto.id)" -Headers $headers
    ```
    *(A `quantidadeEmEstoque` deve ter mudado de 10 para 8).*

#### 5. Executando com Docker Compose (Alternativa)

O projeto também pode ser executado com Docker Compose. O arquivo `docker-compose.yml` orquestra a subida de todos os serviços.

*   O `docker-compose.yml` possui healthchecks e `depends_on` para garantir que os serviços subam na ordem correta.
*   Dentro dos contêineres, os serviços se comunicam usando a rede interna do Docker (ex: `http://servico-estoque:8080`), enquanto o acesso externo continua via API Gateway.

#### 6. Encerrando o Ambiente

1.  Pressione `Ctrl + C` em cada terminal onde os serviços estão rodando.
2.  Para parar e remover o contêiner do RabbitMQ:
    ```powershell
    docker stop rabbitmq-ecom
    docker rm rabbitmq-ecom
    ```

#### 7. Testando o Fluxo Completo via Swagger

O Swagger permite uma interface visual interativa para testar os endpoints. Cada serviço tem seu próprio Swagger habilitado.

**URLs dos Swaggers:**
- **API Gateway** (porta 5154): http://localhost:5154/swagger (endpoints roteados para todos os serviços).
- **Serviço de Estoque** (porta 5290): http://localhost:5290/swagger (endpoints diretos de produtos).
- **Serviço de Vendas** (porta 5156): http://localhost:5156/swagger (endpoints diretos de pedidos).

**Passo a Passo no Swagger do API Gateway:**

1. **Gerar Token de Autenticação:**
   - Expanda `POST /autenticacao/token`.
   - Clique em "Try it out".
   - Corpo da requisição:
     ```json
     {
       "usuario": "admin",
       "senha": "admin"
     }
     ```
   - Execute e copie o `token` retornado.
   - Clique no botão "Authorize" (cadeado) e insira `Bearer <token>` para endpoints protegidos.

2. **Cadastrar um Novo Produto:**
   - Expanda `POST /estoque/produtos`.
   - Clique em "Try it out".
   - Corpo da requisição (exemplo):
     ```json
     {
       "nome": "Camiseta Azul",
       "descricao": "100% algodão",
       "preco": 59.90,
       "quantidadeEmEstoque": 10
     }
     ```
   - Execute. Copie o `id` do produto criado.

3. **Criar um Pedido de Venda:**
   - Expanda `POST /vendas/pedidos`.
   - Clique em "Try it out".
   - Corpo da requisição (use o ID do produto):
     ```json
     {
       "itens": [
         {
           "produtoId": "<ID_DO_PRODUTO>",
           "quantidade": 2
         }
       ]
     }
     ```
   - Execute. O pedido será criado e o estoque será baixado automaticamente via evento RabbitMQ.

4. **Verificar a Baixa de Estoque:**
   - Expanda `GET /estoque/produtos/{id}`.
   - Insira o ID do produto e execute.
   - Verifique se `quantidadeEmEstoque` diminuiu (ex.: 10 → 8).

**Dicas:**
- Aguarde 1-2 segundos após criar o pedido para o processamento assíncrono.
- Use o painel RabbitMQ (http://localhost:15672) para monitorar mensagens.
- Para debug direto, acesse os Swaggers individuais dos serviços.

````
    ```
