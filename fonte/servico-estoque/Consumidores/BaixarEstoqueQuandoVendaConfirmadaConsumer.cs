using Compartilhado.Eventos;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Dados;

namespace ServicoEstoque.Consumidores;

/// <summary>
/// Consumer do evento VendaConfirmada: baixa a quantidade do estoque para cada item vendido.
/// </summary>
public class BaixarEstoqueQuandoVendaConfirmadaConsumer : IConsumer<VendaConfirmada>
{
    private readonly EstoqueDbContext _db;
    private readonly ILogger<BaixarEstoqueQuandoVendaConfirmadaConsumer> _logger;

    public BaixarEstoqueQuandoVendaConfirmadaConsumer(EstoqueDbContext db, ILogger<BaixarEstoqueQuandoVendaConfirmadaConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VendaConfirmada> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Recebido VendaConfirmada para Pedido {PedidoId} com {QtdItens} itens", msg.PedidoId, msg.Itens.Count);

        // Carregar somente os produtos envolvidos para reduzir round trips
        var ids = msg.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _db.Produtos.Where(p => ids.Contains(p.Id)).ToListAsync(context.CancellationToken);

        foreach (var item in msg.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto is null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para baixar estoque", item.ProdutoId);
                continue;
            }

            var anterior = produto.QuantidadeEmEstoque;
            var nova = anterior - item.Quantidade;
            if (nova < 0)
            {
                _logger.LogWarning("Estoque insuficiente ao baixar: Produto {ProdutoId} tinha {Anterior}, tentativa de baixar {Qtd}", produto.Id, anterior, item.Quantidade);
                nova = 0;
            }

            produto.QuantidadeEmEstoque = nova;
            _logger.LogInformation("Produto {ProdutoId}: estoque {Anterior} -> {Atual}", produto.Id, anterior, nova);
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
