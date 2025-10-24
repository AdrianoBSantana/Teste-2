using System.ComponentModel.DataAnnotations;

namespace ServicoVendas.Contratos;

/// <summary>
/// DTO para criação de pedido de venda.
/// </summary>
public class PedidoCriacaoDto
{
    [Required]
    [MinLength(1, ErrorMessage = "É necessário informar pelo menos um item.")]
    public List<ItemPedidoCriacaoDto> Itens { get; set; } = new();
}

public class ItemPedidoCriacaoDto
{
    [Required]
    public Guid ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
    public int Quantidade { get; set; }
}
