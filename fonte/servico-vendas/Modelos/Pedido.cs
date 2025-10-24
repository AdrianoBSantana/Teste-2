namespace ServicoVendas.Modelos;

/// <summary>
/// Entidade que representa um pedido de venda.
/// </summary>
public class Pedido
{
    /// <summary>
    /// Identificador único do pedido.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Data/hora de criação do pedido (UTC).
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status do pedido (ex.: Criado, Confirmado, Cancelado).
    /// </summary>
    public string Status { get; set; } = "Criado";

    /// <summary>
    /// Valor total do pedido.
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}
