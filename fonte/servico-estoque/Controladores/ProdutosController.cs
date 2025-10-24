using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Contratos;
using ServicoEstoque.Dados;
using ServicoEstoque.Modelos;

namespace ServicoEstoque.Controladores;

/// <summary>
/// Controlador responsável por gerenciar produtos e estoque.
/// </summary>
[ApiController]
[Route("estoque/produtos")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _db;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(EstoqueDbContext db, ILogger<ProdutosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra um novo produto.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoRespostaDto>> Cadastrar([FromBody] ProdutoCriacaoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var produto = new Produto
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            Preco = dto.Preco,
            QuantidadeEmEstoque = dto.QuantidadeEmEstoque
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Produto criado: {ProdutoId} - {Nome}", produto.Id, produto.Nome);

        var resposta = new ProdutoRespostaDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Preco = produto.Preco,
            QuantidadeEmEstoque = produto.QuantidadeEmEstoque
        };

        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, resposta);
    }

    /// <summary>
    /// Lista todos os produtos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoRespostaDto>>> Listar(CancellationToken ct)
    {
        var itens = await _db.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoRespostaDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                Preco = p.Preco,
                QuantidadeEmEstoque = p.QuantidadeEmEstoque
            })
            .ToListAsync(ct);

        return Ok(itens);
    }

    /// <summary>
    /// Obtém um produto por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProdutoRespostaDto>> ObterPorId([FromRoute] Guid id, CancellationToken ct)
    {
        var p = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null)
        {
            return NotFound(new { mensagem = "Produto não encontrado." });
        }

        return Ok(new ProdutoRespostaDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            Preco = p.Preco,
            QuantidadeEmEstoque = p.QuantidadeEmEstoque
        });
    }

    /// <summary>
    /// Atualiza a quantidade de estoque (valor absoluto).
    /// </summary>
    [HttpPut("{id:guid}/quantidade")]
    public async Task<ActionResult> AtualizarQuantidade([FromRoute] Guid id, [FromBody] AtualizarQuantidadeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produto is null)
        {
            return NotFound(new { mensagem = "Produto não encontrado." });
        }

        var anterior = produto.QuantidadeEmEstoque;
        produto.QuantidadeEmEstoque = dto.NovaQuantidade;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Quantidade atualizada para produto {ProdutoId}: {Anterior} -> {Atual}", produto.Id, anterior, produto.QuantidadeEmEstoque);

        return NoContent();
    }
}
