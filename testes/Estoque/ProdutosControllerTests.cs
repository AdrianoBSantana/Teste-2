using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ServicoEstoque.Contratos;
using ServicoEstoque.Controladores;
using ServicoEstoque.Dados;
using ServicoEstoque.Modelos;
using Xunit;

namespace Testes.Estoque;

public class ProdutosControllerTests
{
    private static EstoqueDbContext CriarDbContext()
    {
        var opts = new DbContextOptionsBuilder<EstoqueDbContext>()
            .UseInMemoryDatabase(databaseName: $"EstoqueTests_{Guid.NewGuid()}")
            .Options;
        var ctx = new EstoqueDbContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Cadastrar_DeveCriarProdutoERetornarCreated()
    {
        // Arrange
        using var ctx = CriarDbContext();
        var controller = new ProdutosController(ctx, NullLogger<ProdutosController>.Instance);
        var dto = new ProdutoCriacaoDto
        {
            Nome = "Camiseta",
            Descricao = "Camiseta 100% algodão",
            Preco = 49.90m,
            QuantidadeEmEstoque = 100
        };

        // Act
        var resultado = await controller.Cadastrar(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var resposta = Assert.IsType<ProdutoRespostaDto>(created.Value);
        Assert.Equal(dto.Nome, resposta.Nome);
        Assert.Equal(dto.QuantidadeEmEstoque, resposta.QuantidadeEmEstoque);

        // Confirma persistência
        var salvo = await ctx.Produtos.FindAsync(resposta.Id);
        Assert.NotNull(salvo);
        Assert.Equal("Camiseta", salvo!.Nome);
    }

    [Fact]
    public async Task ObterPorId_Inexistente_DeveRetornarNotFound()
    {
        using var ctx = CriarDbContext();
        var controller = new ProdutosController(ctx, NullLogger<ProdutosController>.Instance);

        var resposta = await controller.ObterPorId(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(resposta.Result);
    }
}
