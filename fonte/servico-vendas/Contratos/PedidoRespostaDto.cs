namespace ServicoVendas.Contratos;

/// <summary>
/// DTO de resposta de pedido.
/// </summary>
public class PedidoRespostaDto
{
    public Guid Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public List<ItemPedidoRespostaDto> Itens { get; set; } = new();
}

public class ItemPedidoRespostaDto
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
