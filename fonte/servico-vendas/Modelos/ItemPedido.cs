namespace ServicoVendas.Modelos;

/// <summary>
/// Item de um pedido.
/// </summary>
public class ItemPedido
{
    /// <summary>
    /// Identificador do item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do pedido ao qual este item pertence.
    /// </summary>
    public Guid PedidoId { get; set; }

    /// <summary>
    /// Identificador do produto vendido (referência ao serviço de estoque).
    /// </summary>
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Quantidade do produto neste item.
    /// </summary>
    public int Quantidade { get; set; }

    /// <summary>
    /// Preço unitário do produto no momento da venda.
    /// </summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Navegação para o pedido.
    /// </summary>
    public Pedido? Pedido { get; set; }
}
