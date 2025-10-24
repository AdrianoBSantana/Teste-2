using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ServicoVendas.Contratos;
using ServicoVendas.Controladores;
using ServicoVendas.Dados;
using ServicoVendas.Servicos;
using Xunit;

namespace Testes.Vendas;

public class PedidosControllerTests
{
    private static VendasDbContext CriarDbContext()
    {
        var opts = new DbContextOptionsBuilder<VendasDbContext>()
            .UseInMemoryDatabase(databaseName: $"VendasTests_{Guid.NewGuid()}")
            .Options;
        var ctx = new VendasDbContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static DefaultHttpContext CriarHttpContextComBearer(string token = "dummy-token")
    {
        var http = new DefaultHttpContext();
        http.Request.Headers["Authorization"] = $"Bearer {token}";
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "teste") }, "TestAuth"));
        return http;
    }

    /// <summary>
    /// Testa a criação de um pedido válido com estoque suficiente, verificando persistência e publicação do evento de venda confirmada.
    /// </summary>
    [Fact]
    public async Task Criar_HappyPath_DevePersistirEPublicarEvento()
    {
        // Arrange
        using var ctx = CriarDbContext();

    var mockCliente = new Mock<IClienteEstoque>();
        var produtoId = Guid.NewGuid();
        mockCliente
            .Setup(c => c.ObterProdutoAsync(produtoId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClienteEstoque.ProdutoEstoqueDto(produtoId, "Camiseta", null, 50m, 10));

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish
            .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    var controller = new PedidosController(ctx, mockCliente.Object, NullLogger<PedidosController>.Instance, mockPublish.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CriarHttpContextComBearer()
            }
        };

        var dto = new PedidoCriacaoDto
        {
            Itens =
            [
                new ItemPedidoCriacaoDto { ProdutoId = produtoId, Quantidade = 2 }
            ]
        };

        // Act
        var resultado = await controller.Criar(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var resposta = Assert.IsType<PedidoRespostaDto>(created.Value);
        Assert.Single(resposta.Itens);
        Assert.Equal(100m, resposta.ValorTotal); // 2 * 50

        // Persistência
        var salvo = await ctx.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == resposta.Id);
        Assert.NotNull(salvo);
        Assert.Single(salvo!.Itens);

        // Publicação de evento
    mockPublish.Verify(p => p.Publish(It.IsAny<Compartilhado.Eventos.VendaConfirmada>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Testa a criação de um pedido com estoque insuficiente, verificando se retorna BadRequest e não publica evento.
    /// </summary>
    [Fact]
    public async Task Criar_EstoqueInsuficiente_DeveRetornarBadRequest()
    {
        using var ctx = CriarDbContext();

    var mockCliente = new Mock<IClienteEstoque>();
        var produtoId = Guid.NewGuid();
        mockCliente
            .Setup(c => c.ObterProdutoAsync(produtoId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClienteEstoque.ProdutoEstoqueDto(produtoId, "Camiseta", null, 50m, 1));

        var mockPublish = new Mock<IPublishEndpoint>();

        var controller = new PedidosController(ctx, mockCliente.Object, NullLogger<PedidosController>.Instance, mockPublish.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CriarHttpContextComBearer()
            }
        };

        var dto = new PedidoCriacaoDto
        {
            Itens =
            [
                new ItemPedidoCriacaoDto { ProdutoId = produtoId, Quantidade = 2 }
            ]
        };

        var resultado = await controller.Criar(dto, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Contains("Estoque insuficiente", bad.Value!.ToString());

        mockPublish.Verify(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
