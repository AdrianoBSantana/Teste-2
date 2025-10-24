# Plataforma de E-commerce

## O que é este projeto?

Este projeto é um protótipo de uma plataforma de comércio eletrônico. Ele simula o funcionamento dos bastidores de uma loja online, desde o cadastro e consulta de produtos até a realização de uma venda e a correspondente atualização do estoque.

O sistema foi construído para demonstrar uma arquitetura de software moderna, distribuída em diferentes módulos (micro-serviços) que se comunicam entre si.

## Visão Geral da Arquitetura

Para entender como o sistema funciona, podemos usar uma analogia com uma loja física:

*   **API Gateway (O Recepcionista):** É a porta de entrada do sistema. Todas as solicitações (como consultar um produto ou fazer um pedido) chegam primeiro a ele, que as direciona para o departamento correto. Ele também atua como segurança, validando quem pode entrar.

*   **Serviço de Estoque (O Almoxarifado):** Este departamento controla o inventário. Ele é responsável por adicionar novos produtos e dar baixa nos itens que foram vendidos.

*   **Serviço de Vendas (O Caixa):** É onde os pedidos de compra são processados.

### Como os departamentos se comunicam:

A comunicação entre eles acontece de duas formas:

1.  **Comunicação Direta (Síncrona):** Quando um cliente quer comprar um item, o **Caixa** (Serviço de Vendas) liga para o **Almoxarifado** (Serviço de Estoque) para verificar se o produto está disponível. A venda só prossegue se o almoxarifado confirmar.
2.  **Comunicação por Memorando (Assíncrona):** Após a venda ser confirmada, o **Caixa** envia um memorando (um evento `VendaConfirmada` via RabbitMQ) para o **Almoxarifado**, informando que o produto foi vendido. Ao receber este memorando, o almoxarifado atualiza seus registros e retira o item do inventário.

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
