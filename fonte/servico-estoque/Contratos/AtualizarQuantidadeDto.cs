using System.ComponentModel.DataAnnotations;

namespace ServicoEstoque.Contratos;

/// <summary>
/// DTO para atualização de quantidade de estoque.
/// </summary>
public class AtualizarQuantidadeDto
{
    /// <summary>
    /// Nova quantidade absoluta a ser definida no estoque.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser zero ou positiva.")]
    public int NovaQuantidade { get; set; }
}
