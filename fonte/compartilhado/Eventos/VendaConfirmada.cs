namespace Compartilhado.Eventos;

/// <summary>
/// Evento publicado quando um pedido é confirmado no serviço de vendas.
/// Consumido pelo serviço de estoque para baixar a quantidade.
/// </summary>
public class VendaConfirmada
{
    public Guid PedidoId { get; set; }
    public DateTime DataConfirmacaoUtc { get; set; } = DateTime.UtcNow;
    public List<ItemVendaConfirmada> Itens { get; set; } = new();
}

public class ItemVendaConfirmada
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
