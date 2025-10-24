using System.ComponentModel.DataAnnotations;

namespace ServicoEstoque.Contratos;

/// <summary>
/// DTO para criação de produto.
/// </summary>
public class ProdutoCriacaoDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser zero ou positiva.")]
    public int QuantidadeEmEstoque { get; set; }
}
