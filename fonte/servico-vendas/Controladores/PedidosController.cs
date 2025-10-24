using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicoVendas.Contratos;
using ServicoVendas.Dados;
using ServicoVendas.Modelos;
using ServicoVendas.Servicos;
using Compartilhado.Eventos;
using MassTransit;

namespace ServicoVendas.Controladores;

/// <summary>
/// Controlador responsável por gerenciar pedidos de venda.
/// </summary>
[ApiController]
[Route("vendas/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly VendasDbContext _db;
    private readonly IClienteEstoque _clienteEstoque;
    private readonly ILogger<PedidosController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public PedidosController(VendasDbContext db, IClienteEstoque clienteEstoque, ILogger<PedidosController> logger, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _clienteEstoque = clienteEstoque;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Cria um novo pedido de venda validando o estoque.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoRespostaDto>> Criar([FromBody] PedidoCriacaoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (dto.Itens is null || dto.Itens.Count == 0)
            return BadRequest(new { mensagem = "Informe ao menos um item." });

        // Token do usuário para repassar na chamada ao serviço de estoque
        var bearer = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        var itensValidados = new List<(ItemPedidoCriacaoDto Requisicao, ClienteEstoque.ProdutoEstoqueDto Produto)>();

        foreach (var item in dto.Itens)
        {
            var produto = await _clienteEstoque.ObterProdutoAsync(item.ProdutoId, bearer, ct);
            if (produto is null)
                return BadRequest(new { mensagem = $"Produto {item.ProdutoId} não encontrado." });

            if (item.Quantidade > produto.QuantidadeEmEstoque)
                return BadRequest(new { mensagem = $"Estoque insuficiente para o produto '{produto.Nome}'. Disponível: {produto.QuantidadeEmEstoque}." });

            itensValidados.Add((item, produto));
        }

        var pedido = new Pedido
        {
            Status = "Confirmado"
        };

        foreach (var (reqItem, produto) in itensValidados)
        {
            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = reqItem.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        pedido.ValorTotal = pedido.Itens.Sum(i => i.PrecoUnitario * i.Quantidade);

    _db.Pedidos.Add(pedido);
    await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Pedido criado: {PedidoId} com {QtdItens} itens e total {Total}", pedido.Id, pedido.Itens.Count, pedido.ValorTotal);

        var resposta = new PedidoRespostaDto
        {
            Id = pedido.Id,
            DataCriacao = pedido.DataCriacao,
            Status = pedido.Status,
            ValorTotal = pedido.ValorTotal,
            Itens = pedido.Itens.Select(i => new ItemPedidoRespostaDto
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        };

        // Publicar evento de venda confirmada para baixar o estoque de forma assíncrona
        var evento = new VendaConfirmada
        {
            PedidoId = pedido.Id,
            DataConfirmacaoUtc = DateTime.UtcNow,
            Itens = pedido.Itens.Select(i => new ItemVendaConfirmada
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade
            }).ToList()
        };

        await _publishEndpoint.Publish(evento, ct);
        _logger.LogInformation("Evento VendaConfirmada publicado para Pedido {PedidoId}", pedido.Id);
        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, resposta);
    }

    /// <summary>
    /// Lista todos os pedidos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PedidoRespostaDto>>> Listar(CancellationToken ct)
    {
        var pedidos = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync(ct);

        var resposta = pedidos.Select(p => new PedidoRespostaDto
        {
            Id = p.Id,
            DataCriacao = p.DataCriacao,
            Status = p.Status,
            ValorTotal = p.ValorTotal,
            Itens = p.Itens.Select(i => new ItemPedidoRespostaDto
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        }).ToList();

        return Ok(resposta);
    }

    /// <summary>
    /// Obtém um pedido por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PedidoRespostaDto>> ObterPorId([FromRoute] Guid id, CancellationToken ct)
    {
        var p = await _db.Pedidos.AsNoTracking().Include(x => x.Itens).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null)
            return NotFound(new { mensagem = "Pedido não encontrado." });

        var resposta = new PedidoRespostaDto
        {
            Id = p.Id,
            DataCriacao = p.DataCriacao,
            Status = p.Status,
            ValorTotal = p.ValorTotal,
            Itens = p.Itens.Select(i => new ItemPedidoRespostaDto
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        };

        return Ok(resposta);
    }
}
