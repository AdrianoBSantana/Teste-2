namespace ServicoEstoque.Contratos;

/// <summary>
/// DTO de resposta de produto para retornos da API.
/// </summary>
public class ProdutoRespostaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEmEstoque { get; set; }
}
