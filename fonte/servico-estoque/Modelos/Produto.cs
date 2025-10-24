namespace ServicoEstoque.Modelos;

/// <summary>
/// Entidade que representa um produto no catálogo e seu estoque atual.
/// </summary>
public class Produto
{
    /// <summary>
    /// Identificador único do produto.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nome do produto.
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do produto.
    /// </summary>
    public string? Descricao { get; set; }

    /// <summary>
    /// Preço unitário atual do produto.
    /// </summary>
    public decimal Preco { get; set; }

    /// <summary>
    /// Quantidade disponível em estoque.
    /// </summary>
    public int QuantidadeEmEstoque { get; set; }

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
